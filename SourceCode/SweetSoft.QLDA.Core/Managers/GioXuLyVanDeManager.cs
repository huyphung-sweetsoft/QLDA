using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using SweetSoft.QLDA.Core.Respositories;

namespace SweetSoft.QLDA.Core.Managers
{
    public sealed class KetQuaGioXuLyVanDe
    {
        public Guid IdPhien { get; set; }
        public bool DangChay { get; set; }
        public long TongGiayDaLuu { get; set; }
        public string ThongBao { get; set; }
    }

    public sealed class ThongTinGioXuLyVanDe
    {
        public Guid IdVanDe { get; set; }
        public string MaVanDe { get; set; }
        public string TenVanDe { get; set; }
        public byte TrangThai { get; set; }
        public byte TrangThaiDuyetGio { get; set; }
        public bool DaDuocPhanCong { get; set; }
        public bool LaQuanLy { get; set; }
        public bool DangChay { get; set; }
        public bool DaTungXuLy { get; set; }
        public bool DaBaoPhanViec { get; set; }
        public bool DaRutBaoGanNhat { get; set; }
        public bool CoPhienMo { get; set; }
        public Guid? NguoiGuiDuyet { get; set; }
        public DateTime? NgayGuiDuyetUtc { get; set; }
        public string LyDoTuChoi { get; set; }
        public long TongGiayDaLuu { get; set; }
        public long TongGiayCuaToi { get; set; }
        public long GiayThuongCuaToi { get; set; }
        public long GiayTangCaCuaToi { get; set; }
        public long GiayThuongNhom { get; set; }
        public long GiayTangCaNhom { get; set; }
        public bool DangChayTangCa { get; set; }
        public DateTime? BatDauPhienCuaToiUtc { get; set; }
        public List<LichSuGioXuLyVanDe> LichSu { get; set; }
    }
    public sealed class LichSuGioXuLyVanDe
    {
        public Guid IdPhien { get; set; }
        public string NhanVien { get; set; }
        public DateTime BatDauUtc { get; set; }
        public DateTime? KetThucUtc { get; set; }
        public long SoGiay { get; set; }
        public bool LaTangCa { get; set; }
        public string HienThiBatDau { get { return BatDauUtc.AddHours(7).ToString("dd/MM/yyyy HH:mm:ss"); } }
        public string HienThiKetThuc { get { return KetThucUtc.HasValue ? KetThucUtc.Value.AddHours(7).ToString("dd/MM/yyyy HH:mm:ss") : "Đang chạy"; } }
        public string HienThiThoiGian { get { return GioXuLyVanDeManager.DinhDangGio(SoGiay); } }
    }

    /// <summary>
    /// Phan xu ly yeu cau hoan thanh Issue. Khong dung luong duyet Task.
    /// Moi thay doi va thong bao duoc ghi trong cung giao dich voi viec dong dong ho nhom.
    /// </summary>

    /// <summary>
    /// Dong ho xu ly van de: khong dung ngay ke hoach Task, khong tu khoi dong nguoi khac.
    /// Quan ly phien gio va quy trinh hoan thanh/duyet Issue tren cung mot manager.
    /// Giao dich gui duyet dong tat ca dong ho dang chay, bao toan gio da luu.
    /// </summary>
    public sealed class GioXuLyVanDeManager
    {
        private readonly GioXuLyVanDeRepository _repo = new GioXuLyVanDeRepository();
        private static readonly TimeSpan GioVietNam = TimeSpan.FromHours(7);

        private static DateTime GioUtcHienTai()
        {
            DateTime utc=DateTime.UtcNow;
            return new DateTime(utc.Ticks - (utc.Ticks % TimeSpan.TicksPerSecond), DateTimeKind.Utc);
        }

        // Lich ca duy nhat dung chung voi Task KBG20. Khong tao lich rieng cho Issue.
        // Tat ca thoi gian tu manager nay va tu TblKhaiBaoGioPhien la UTC.
        private static List<KeyValuePair<DateTime,DateTime>> CaUtc(DateTime ngayVietNam)
        {
            var result = new List<KeyValuePair<DateTime,DateTime>>();
            var lich = new KhaiBaoGio20Manager();
            // Xem ca cua ngay truoc de xu ly ca dem qua 00:00.
            foreach(var ngay in new [] { ngayVietNam.AddDays(-1), ngayVietNam })
                foreach(var ca in lich.LayLichCaTrongNgay(ngay))
                {
                    var start = ca.BatDau.Subtract(GioVietNam);
                    var end = ca.KetThuc.Subtract(GioVietNam);
                    if(end > start) result.Add(new KeyValuePair<DateTime,DateTime>(start,end));
                }
            return result.OrderBy(x=>x.Key).ToList();
        }

        private static bool TrongCa(DateTime utc)
        {
            return CaUtc(utc.Add(GioVietNam).Date).Any(x=>utc>=x.Key && utc<x.Value);
        }

        // Tach giao cac khoang ca, tron cac ca giao nhau de khong ghi cong hai lan.
        private static List<KeyValuePair<DateTime,DateTime>> KhoangHopLe(DateTime batDauUtc, DateTime ketThucUtc, bool tangCa)
        {
            var result = new List<KeyValuePair<DateTime,DateTime>>();
            if(ketThucUtc<=batDauUtc) return result;
            DateTime cursor=batDauUtc;
            while(cursor<ketThucUtc)
            {
                DateTime ngay=cursor.Add(GioVietNam).Date;
                DateTime endOfDay=ngay.AddDays(1).Subtract(GioVietNam);
                DateTime right=ketThucUtc<endOfDay?ketThucUtc:endOfDay;
                var intervals=new List<KeyValuePair<DateTime,DateTime>>();
                foreach(var ca in CaUtc(ngay))
                {
                    var a=ca.Key>cursor?ca.Key:cursor;
                    var b=ca.Value<right?ca.Value:right;
                    if(b>a)intervals.Add(new KeyValuePair<DateTime,DateTime>(a,b));
                }
                intervals=intervals.OrderBy(x=>x.Key).ToList();
                var merged=new List<KeyValuePair<DateTime,DateTime>>();
                foreach(var piece in intervals)
                {
                    if(merged.Count>0 && piece.Key<=merged[merged.Count-1].Value)
                    {
                        var last=merged[merged.Count-1];
                        merged[merged.Count-1]=new KeyValuePair<DateTime,DateTime>(last.Key,piece.Value>last.Value?piece.Value:last.Value);
                    }
                    else merged.Add(piece);
                }
                if(!tangCa)result.AddRange(merged);
                else
                {
                    DateTime p=cursor;
                    foreach(var ca in merged)
                    {
                        if(ca.Key>p)result.Add(new KeyValuePair<DateTime,DateTime>(p,ca.Key));
                        if(ca.Value>p)p=ca.Value;
                    }
                    if(right>p)result.Add(new KeyValuePair<DateTime,DateTime>(p,right));
                }
                cursor=right;
            }
            return result;
        }

        // Giong KBG20 Task: tang ca tu ngat khi bat dau ca lam viec tiep theo.
        // Issue KHONG co deadline Task, vi vay khong ap dung MocHetHanUtc cua Task.
        private static DateTime MocNgatTangCaUtc(DateTime batDauUtc)
        {
            DateTime batDauDiaPhuong = batDauUtc.Add(GioVietNam);
            var lich = new KhaiBaoGio20Manager();
            for (int i = 0; i <= 730; i++)
            {
                foreach (var ca in lich.LayLichCaTrongNgay(batDauDiaPhuong.Date.AddDays(i))
                    .OrderBy(x => x.BatDau))
                {
                    if (ca.BatDau > batDauDiaPhuong)
                        return ca.BatDau.Subtract(GioVietNam);
                }
            }
            throw new InvalidOperationException("Không xác định được ca tiếp theo để tự ngắt tăng ca vấn đề.");
        }

        // Hien thi live chinh xac: dung cung phep tinh voi ket toan luc Stop.
        public static long GiayPhienChuaKetToan(DateTime batDauUtc, DateTime denUtc, bool tangCa)
        {
            return KhoangHopLe(batDauUtc,denUtc,tangCa)
                .Sum(x=>(long)(x.Value-x.Key).TotalSeconds);
        }

        public KetQuaGioXuLyVanDe BatDau(Guid nhanVien, Guid duAn, Guid vanDe, Guid idThaoTac, bool tangCa = false)
        {
            if (nhanVien == Guid.Empty || duAn == Guid.Empty || vanDe == Guid.Empty || idThaoTac == Guid.Empty)
                throw new ArgumentException("Thiếu thông tin bắt đầu xử lý vấn đề.");
            using (SqlConnection con = _repo.MoKetNoi())
            using (SqlTransaction tx=con.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                _repo.KhoaDuAn(con,tx,duAn);
                // Idempotency: mot lan bam Start khong the tao lai phien sau khi Stop.
                Guid? daTao=_repo.LayPhienTheoThaoTac(con,tx,duAn,vanDe,nhanVien,idThaoTac);
                if(daTao.HasValue)
                {
                    var phienMo=_repo.LayPhienMo(con,tx,duAn,vanDe,nhanVien);
                    var kq=new KetQuaGioXuLyVanDe { IdPhien=daTao.Value,
                        DangChay=phienMo!=null && phienMo.IdPhien==daTao.Value,
                        TongGiayDaLuu=_repo.LayTongGiay(con,tx,duAn,vanDe,nhanVien),
                        ThongBao="Yêu cầu bắt đầu này đã được xử lý." };
                    tx.Commit(); return kq;
                }
                if (!_repo.DuocPhepXuLy(con,tx,duAn,vanDe,nhanVien))
                    throw new InvalidOperationException("Bạn chưa được phân công hoặc vấn đề không còn ở trạng thái đang xử lý.");
                PhienGioVanDe dangMo=_repo.LayPhienMo(con,tx,duAn,vanDe,nhanVien);
                if(dangMo!=null)
                {
                    // Mot nhan vien chi co mot phien Issue dang mo. Khong doi loai ngam va khong bao thanh cong gia.
                    if(dangMo.LaTangCa!=tangCa)
                        throw new InvalidOperationException("Bạn đang bấm giờ loại khác. Hãy dừng đồng hồ hiện tại trước khi bắt đầu loại giờ mới.");
                    var kq=new KetQuaGioXuLyVanDe {IdPhien=dangMo.IdPhien,DangChay=true,
                        TongGiayDaLuu=_repo.LayTongGiay(con,tx,duAn,vanDe,nhanVien),
                        ThongBao=dangMo.LaTangCa==tangCa?"Đồng hồ xử lý vấn đề đang chạy.":"Hãy dừng đồng hồ hiện tại trước khi chuyển giữa giờ thường và tăng ca."};
                    tx.Commit();return kq;
                }
                // Kiem tra tai server, khong tin trang thai nut tren client.
                DateTime batDauUtc=GioUtcHienTai();
                bool dangTrongCa=TrongCa(batDauUtc);
                if(!tangCa && !dangTrongCa)
                    throw new InvalidOperationException("Đã ngoài giờ làm việc theo lịch. Không thể bắt đầu đồng hồ giờ thường; hãy chọn giờ tăng ca nếu cần.");
                if(tangCa && dangTrongCa)
                    throw new InvalidOperationException("Đang trong ca làm việc. Chỉ bắt đầu tăng ca ngoài giờ theo lịch.");
                Guid idPhien=_repo.TaoPhien(con,tx,duAn,vanDe,nhanVien,idThaoTac,batDauUtc,tangCa);
                var result=new KetQuaGioXuLyVanDe {IdPhien=idPhien,DangChay=true,
                    TongGiayDaLuu=_repo.LayTongGiay(con,tx,duAn,vanDe,nhanVien),
                    ThongBao=tangCa?"Đã bắt đầu tính giờ tăng ca xử lý vấn đề.":"Đã bắt đầu tính giờ thường xử lý vấn đề."};
                tx.Commit(); return result;
            }
        }

        public KetQuaGioXuLyVanDe Dung(Guid nhanVien, Guid duAn, Guid vanDe)
        {
            if(nhanVien==Guid.Empty || duAn==Guid.Empty || vanDe==Guid.Empty)
                throw new ArgumentException("Thiếu thông tin dừng giờ xử lý vấn đề.");
            using(SqlConnection con=_repo.MoKetNoi())
            using(SqlTransaction tx=con.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                _repo.KhoaDuAn(con,tx,duAn);
                PhienGioVanDe phien=_repo.LayPhienMo(con,tx,duAn,vanDe,nhanVien);
                if(phien==null)
                {
                    var kq=new KetQuaGioXuLyVanDe {DangChay=false,
                        TongGiayDaLuu=_repo.LayTongGiay(con,tx,duAn,vanDe,nhanVien),
                        ThongBao="Không có đồng hồ xử lý vấn đề đang chạy."};
                    tx.Commit();return kq;
                }
                DateTime now=GioUtcHienTai();
                KetToanVaDong(con,tx,phien,now,1); // 1 = nhan vien tu dung
                long tong=_repo.LayTongGiay(con,tx,duAn,vanDe,nhanVien);
                tx.Commit();
                return new KetQuaGioXuLyVanDe {IdPhien=phien.IdPhien,DangChay=false,
                    TongGiayDaLuu=tong,ThongBao="Đã dừng và lưu giờ xử lý vấn đề."};
            }
        }

        // Diem ket noi dot 2: trong transaction va app lock du an do caller quan ly,
        // dong TOAN BO phien Issue truoc khi ghi trang thai cho duyet va phat thong bao.
        // KHONG goi thu cong rieng le ngoai giao dich gui duyet.
        internal int DongTatCaPhienTrongGiaoDich(SqlConnection con, SqlTransaction tx,
                                                  Guid duAn, Guid vanDe, DateTime ketThucUtc)
        {
            if(con==null || tx==null || duAn==Guid.Empty || vanDe==Guid.Empty)
                throw new ArgumentException("Thiếu giao dịch dừng đồng hồ nhóm.");
            DateTime now = DateTime.SpecifyKind(ketThucUtc,DateTimeKind.Utc);
            now=new DateTime(now.Ticks - (now.Ticks % TimeSpan.TicksPerSecond),DateTimeKind.Utc);
            var phien=_repo.LayTatCaPhienMo(con,tx,duAn,vanDe);
            foreach(var p in phien) KetToanVaDong(con,tx,p,now,3); // 3 = gui duyet
            return phien.Count;
        }

        private void KetToanVaDong(SqlConnection con, SqlTransaction tx, PhienGioVanDe phien,
                                   DateTime nowUtc, byte lyDo)
        {
            DateTime tu=phien.DaTinhDenUtc ?? phien.BatDauUtc;
            if(phien.LaTangCa)
            {
                // Khong cong them sau moc tu ngat cua Task, ke ca khi nguoi dung Stop tre.
                DateTime mocNgat = MocNgatTangCaUtc(phien.BatDauUtc);
                if(nowUtc > mocNgat) nowUtc = mocNgat;
                // Lich su cu co watermark vuot moc cat: khong ghi de/sua lai khoang da luu.
                if(nowUtc < tu) nowUtc = tu;
            }
            if(nowUtc < tu) throw new InvalidOperationException("Mốc dừng nhỏ hơn mốc đã tính giờ.");
            // Chia ngay va CAT THEO CA THUC TE nhu Task: gio thuong chi tinh trong ca,
            // tang ca chi tinh ngoai ca. Khong thay doi cac khoang da ket toan truoc do.
            foreach(var khoang in KhoangHopLe(tu,nowUtc,phien.LaTangCa))
                _repo.GhiKhoang(con,tx,phien.IdPhien,khoang.Key.Add(GioVietNam).Date,khoang.Key,khoang.Value);
            _repo.DongPhien(con,tx,phien.IdPhien,nowUtc,lyDo);
        }

        // Dot 3A: Hangfire chay trong KHAIBAOGIO-CHOT-SO hien co, khong tao recurring job moi.
        // Chi xu ly phien Issue (LoaiGio=3); khong cham phien Task, khong sinh bao cao PM.
        // Moi phien duoc khoa theo du an, ghi khoang + watermark TRONG CUNG transaction.
        public int KetToanPhienMo(DateTime utcNow, int soLuong = 100)
        {
            DateTime now = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
            now = new DateTime(now.Ticks - (now.Ticks % TimeSpan.TicksPerSecond), DateTimeKind.Utc);
            int limit = Math.Max(1, Math.Min(soLuong, 500));
            var canXuLy = new List<KeyValuePair<Guid, Guid>>();

            using (SqlConnection con = _repo.MoKetNoi())
            using (var cmd = new SqlCommand(@"SELECT TOP (@Limit) p.IdPhien, p.IdDuAn
                FROM dbo.TblKhaiBaoGioPhien p
                WHERE p.LoaiGio=3 AND p.IdVanDe IS NOT NULL AND p.IdCongViec IS NULL
                  AND p.KetThucLuc IS NULL
                  AND COALESCE(p.DaTinhDenLuc,p.BatDauLuc) < @Now
                ORDER BY COALESCE(p.DaTinhDenLuc,p.BatDauLuc),p.IdPhien", con))
            {
                Param(cmd, "@Limit", SqlDbType.Int, limit);
                Param(cmd, "@Now", SqlDbType.DateTime2, now);
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        canXuLy.Add(new KeyValuePair<Guid, Guid>(r.GetGuid(0), r.GetGuid(1)));
            }

            int thanhCong = 0;
            foreach (var item in canXuLy)
            {
                try
                {
                    using (SqlConnection con = _repo.MoKetNoi())
                    using (SqlTransaction tx = con.BeginTransaction(IsolationLevel.ReadCommitted))
                    {
                        _repo.KhoaDuAn(con, tx, item.Value);
                        PhienGioVanDe phien = null;
                        using (var cmd = new SqlCommand(@"SELECT IdPhien,IdNhanVien,IdVanDe,IdDuAn,
                            BatDauLuc,DaTinhDenLuc,ISNULL(LaTangCaVanDe,0)
                            FROM dbo.TblKhaiBaoGioPhien WITH (UPDLOCK,HOLDLOCK)
                            WHERE IdPhien=@I AND IdDuAn=@D AND IdCongViec IS NULL
                              AND IdVanDe IS NOT NULL AND LoaiGio=3 AND KetThucLuc IS NULL",con,tx))
                        {
                            Param(cmd,"@I",SqlDbType.UniqueIdentifier,item.Key);
                            Param(cmd,"@D",SqlDbType.UniqueIdentifier,item.Value);
                            using (var r=cmd.ExecuteReader())
                            {
                                if (r.Read()) phien = new PhienGioVanDe {
                                    IdPhien = r.GetGuid(0), IdNhanVien = r.GetGuid(1),
                                    IdVanDe = r.GetGuid(2), IdDuAn = r.GetGuid(3),
                                    BatDauUtc = r.GetDateTime(4),
                                    DaTinhDenUtc = r.IsDBNull(5) ? (DateTime?)null : r.GetDateTime(5),
                                    LaTangCa = Convert.ToBoolean(r.GetValue(6))
                                };
                            }
                        }
                        if (phien == null) { tx.Commit(); continue; }
                        DateTime tu = phien.DaTinhDenUtc ?? phien.BatDauUtc;
                        DateTime den = now;
                        DateTime? mocNgat = phien.LaTangCa
                            ? (DateTime?)MocNgatTangCaUtc(phien.BatDauUtc) : null;
                        if (mocNgat.HasValue && den > mocNgat.Value) den = mocNgat.Value;
                        if (den < tu) den = tu; // Khong tua nguoc watermark du lieu da ket toan.

                        if (mocNgat.HasValue && now >= mocNgat.Value)
                        {
                            // Giong Task: tu dong DONG OT o dau ca ke tiep, khong dong gio thuong.
                            KetToanVaDong(con,tx,phien,den,(byte)Kbg20LyDoKetThuc.TuNgatTangCa);
                        }
                        else if (den > tu)
                        {
                            foreach (var khoang in KhoangHopLe(tu,den,phien.LaTangCa))
                                _repo.GhiKhoang(con,tx,phien.IdPhien,
                                    khoang.Key.Add(GioVietNam).Date,khoang.Key,khoang.Value);
                            using (var cmd = new SqlCommand(@"UPDATE dbo.TblKhaiBaoGioPhien
                                SET DaTinhDenLuc=@Den,NgayCapNhat=SYSUTCDATETIME(),
                                    NguoiCapNhat=N'GioXuLyVanDe-Hangfire'
                                WHERE IdPhien=@I AND IdDuAn=@D AND IdVanDe IS NOT NULL
                                  AND IdCongViec IS NULL AND LoaiGio=3 AND KetThucLuc IS NULL
                                  AND ((DaTinhDenLuc=@Tu) OR (DaTinhDenLuc IS NULL AND BatDauLuc=@Tu))",con,tx))
                            {
                                Param(cmd,"@I",SqlDbType.UniqueIdentifier,phien.IdPhien);
                                Param(cmd,"@D",SqlDbType.UniqueIdentifier,phien.IdDuAn);
                                Param(cmd,"@Tu",SqlDbType.DateTime2,tu);
                                Param(cmd,"@Den",SqlDbType.DateTime2,den);
                                if(cmd.ExecuteNonQuery()!=1)
                                    throw new InvalidOperationException("Phiên Issue đã đổi watermark trong lúc kết toán.");
                            }
                        }
                        tx.Commit();
                        thanhCong++;
                    }
                }
                catch (Exception ex)
                {
                    // Tuong tu Task: mot phien loi khong chan ca batch, retry vao phut sau.
                    SweetSoft.QLDA.Core.SysManager.SysLogger.LogError(ex,
                        "Issue Hangfire KetToanPhienMo " + item.Key.ToString("D"));
                }
            }
            return thanhCong;
        }

        // Dot 3B: DTO chi doc cho Floating Timer V2, cung y nghia voi Kbg20DongHo cua Task.
        // Khong phat sinh Start/Stop, khong thay trang thai hay ghi DB.
        public sealed class DongHoNoiVanDe
        {
            public Guid IdPhien { get; set; }
            public Guid IdVanDe { get; set; }
            public Guid IdDuAn { get; set; }
            public string MaVanDe { get; set; }
            public string TenVanDe { get; set; }
            public string MaDuAn { get; set; }
            public string TenDuAn { get; set; }
            public bool LaTangCa { get; set; }
            public long SoGiayHienTai { get; set; }
            public bool DangCongGio { get; set; }
            public DateTime? MocDungCongLucUtc { get; set; }
        }

        public List<DongHoNoiVanDe> LayDongHoNoi(Guid idNhanVien, DateTime utcNow)
        {
            var result = new List<DongHoNoiVanDe>();
            if (idNhanVien == Guid.Empty) return result;
            DateTime now = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
            now = new DateTime(now.Ticks - (now.Ticks % TimeSpan.TicksPerSecond), DateTimeKind.Utc);
            // Chi lay phien cua nhan vien; nhom du lieu project tren client nhu Task.
            var phienMo = _repo.LayPhienMoChoDongHoNoi(idNhanVien);
            // CaUtc duoc dung chung voi cac phep ket toan Issue, khong suy dien ca tren JS.
            var caHienTai = CaUtc(now.Add(GioVietNam).Date);
            foreach (var phien in phienMo)
            {
                DateTime tu = phien.DaTinhDenUtc ?? phien.BatDauUtc;
                if (tu < phien.BatDauUtc) tu = phien.BatDauUtc;
                DateTime den = now;
                DateTime? mocNgat = null;
                if (phien.LaTangCa)
                {
                    // Dung HAM moc tu ngat cua Hangfire 3A; khong tao cong thuc deadline Task.
                    mocNgat = MocNgatTangCaUtc(phien.BatDauUtc);
                    if (den > mocNgat.Value) den = mocNgat.Value;
                }
                if (den < tu) den = tu;
                long giayChuaKetToan = KhoangHopLe(tu, den, phien.LaTangCa)
                    .Sum(x => (long)(x.Value - x.Key).TotalSeconds);
                bool dangCong = false;
                DateTime? boundary = null;
                if (now >= phien.BatDauUtc)
                {
                    if (phien.LaTangCa)
                    {
                        dangCong = now < mocNgat.Value && !caHienTai.Any(x => now >= x.Key && now < x.Value);
                        boundary = mocNgat;
                    }
                    else
                    {
                        foreach (var ca in caHienTai)
                        {
                            if (now >= ca.Key && now < ca.Value)
                            {
                                dangCong = true;
                                boundary = ca.Value;
                                break;
                            }
                        }
                    }
                }
                result.Add(new DongHoNoiVanDe {
                    IdPhien=phien.IdPhien,IdVanDe=phien.IdVanDe,IdDuAn=phien.IdDuAn,
                    MaVanDe=phien.MaVanDe,TenVanDe=phien.TenVanDe,
                    MaDuAn=phien.MaDuAn,TenDuAn=phien.TenDuAn,
                    LaTangCa=phien.LaTangCa,
                    SoGiayHienTai=phien.SoGiayDaKetToan+giayChuaKetToan,
                    DangCongGio=dangCong,MocDungCongLucUtc=boundary
                });
            }
            return result;
        }

        // Doc danh sach / lich su / gui hoan thanh / PM duyet, cung manager xu ly gio.


        // ========== DOI SOAT ISSUE: chung TblKhaiBaoGioDoiSoat, rieng nghiep vu ==========
        public sealed class BaoCaoGioVanDe
        {
            public Guid IdNhanVien { get; set; }
            public string TenNhanVien { get; set; }
            public long GiayHeThong { get; set; }
            public long GiayThuongHeThong { get; set; }
            public long GiayTangCaHeThong { get; set; }
            // Phan biet nhan vien chua tung bat dau Issue va nhan vien co phien nhung chua bao.
            public bool DaTungXuLy { get; set; }
            public int? PhutThuongTuKhai { get; set; }
            public int? PhutTangCaTuKhai { get; set; }
            public int? PhutThuongPmChot { get; set; }
            public int? PhutTangCaPmChot { get; set; }
            public string GhiChu { get; set; }
            public bool DaBao { get; set; }
            public bool DangChay { get; set; }
            public bool DaChot { get; set; }
            public int Lan { get; set; }
            public int PhienBan { get; set; }
            public byte SuKien { get; set; }
        }
        public sealed class GioVanDeNgay
        {
            public long GiayThuong { get; set; }
            public long GiayTangCa { get; set; }
            public Guid IdVanDe { get; set; }
            public string MaVanDe { get; set; }
            public string TenVanDe { get; set; }
            public long Giay { get; set; }
            public bool DangChay { get; set; }
            public bool DaBao { get; set; }
        }
        public sealed class LichSuBaoCaoVanDe
        {
            public string NhanVien { get; set; }
            public string NguoiThaoTac { get; set; }
            public int Lan { get; set; }
            public int PhienBan { get; set; }
            public byte LoaiSuKien { get; set; }
            public int? PhutThuong { get; set; }
            public int? PhutTangCa { get; set; }
            public int? PhutThuongPm { get; set; }
            public int? PhutTangCaPm { get; set; }
            public string LyDo { get; set; }
            public DateTime NgayTaoUtc { get; set; }
        }
        public List<LichSuBaoCaoVanDe> LayLichSuBaoCao(Guid duAn,Guid vanDe,Guid nguoiXem,bool admin)
        {
            LayThongTin(duAn,vanDe,nguoiXem,admin); // Phan quyen va du an
            var list=new List<LichSuBaoCaoVanDe>();
            using(SqlConnection con=_repo.MoKetNoi())
            using(var cmd=new SqlCommand(@"SELECT ISNULL(NULLIF(u.UserName,N''),N'Nhân viên'),
                  ISNULL(NULLIF(a.UserName,N''),N'Hệ thống'),s.LanDoiSoat,s.PhienBan,s.LoaiSuKien,
                  s.SoPhutThuongTuKhai,s.SoPhutTangCaTuKhai,
                  s.SoPhutThuongPmChot,s.SoPhutTangCaPmChot,s.LyDoDieuChinh,s.NgayTao
                FROM dbo.TblKhaiBaoGioDoiSoat s
                LEFT JOIN dbo.aspnet_Users u ON u.UserId=s.IdNhanVien
                LEFT JOIN dbo.aspnet_Users a ON a.UserId=s.IdNguoiThaoTacVanDe
                WHERE s.IdDuAn=@D AND s.IdVanDe=@V AND s.IdCongViec IS NULL
                ORDER BY s.NgayTao DESC,s.PhienBan DESC",con))
            {
                Param(cmd,"@D",SqlDbType.UniqueIdentifier,duAn);
                Param(cmd,"@V",SqlDbType.UniqueIdentifier,vanDe);
                using(var r=cmd.ExecuteReader())while(r.Read())list.Add(new LichSuBaoCaoVanDe{
                  NhanVien=r.GetString(0),NguoiThaoTac=r.GetString(1),
                  Lan=Convert.ToInt32(r.GetValue(2)),PhienBan=Convert.ToInt32(r.GetValue(3)),
                  LoaiSuKien=Convert.ToByte(r.GetValue(4)),
                  PhutThuong=r.IsDBNull(5)?(int?)null:Convert.ToInt32(r.GetValue(5)),
                  PhutTangCa=r.IsDBNull(6)?(int?)null:Convert.ToInt32(r.GetValue(6)),
                  PhutThuongPm=r.IsDBNull(7)?(int?)null:Convert.ToInt32(r.GetValue(7)),
                  PhutTangCaPm=r.IsDBNull(8)?(int?)null:Convert.ToInt32(r.GetValue(8)),
                  LyDo=r.IsDBNull(9)?null:r.GetString(9),NgayTaoUtc=r.GetDateTime(10)
                });
            }
            return list;
        }
        public sealed class GioPmChotDong
        {
            public Guid IdNhanVien { get; set; }
            public int PhutThuong { get; set; }
            public int PhutTangCa { get; set; }
            public string LyDo { get; set; }
            // Optional optimistic concurrency for the new PM Issue WebForms control.
            public int? LanKyVong { get; set; }
            public int? PhienBanKyVong { get; set; }
            public long? GiayHeThongKyVong { get; set; }
        }
        private static string BaoCaoLatestSql = @"
            SELECT TOP 1 LanDoiSoat,PhienBan,LoaiSuKien,
                SoPhutThuongTuKhai,SoPhutTangCaTuKhai,
                SoPhutThuongPmChot,SoPhutTangCaPmChot,LyDoDieuChinh
            FROM dbo.TblKhaiBaoGioDoiSoat WITH (UPDLOCK,HOLDLOCK)
            WHERE IdDuAn=@D AND IdVanDe=@V AND IdNhanVien=@U AND LaBanHienTai=1
              AND LanDoiSoat=(SELECT CASE WHEN v.TrangThaiDuyetGio=3 THEN v.LanGuiDuyetGio+1
                                     WHEN v.LanGuiDuyetGio<1 THEN 1 ELSE v.LanGuiDuyetGio END
                               FROM dbo.TblVanDe v WHERE v.IdVanDe=@V AND v.IdDuAn=@D)
            ORDER BY LanDoiSoat DESC,PhienBan DESC";
        private static int LanHienTai(SqlConnection con,SqlTransaction tx,Guid duAn,Guid vanDe)
        {
            using(var c=new SqlCommand(@"SELECT CASE WHEN TrangThaiDuyetGio=3 THEN LanGuiDuyetGio+1
                                WHEN LanGuiDuyetGio<1 THEN 1 ELSE LanGuiDuyetGio END
                 FROM dbo.TblVanDe WITH (UPDLOCK,HOLDLOCK)
                 WHERE IdVanDe=@V AND IdDuAn=@D AND ISNULL(DaXoa,0)=0",con,tx))
            {
                Param(c,"@D",SqlDbType.UniqueIdentifier,duAn);
                Param(c,"@V",SqlDbType.UniqueIdentifier,vanDe);
                object x=c.ExecuteScalar();
                if(x==null||x==DBNull.Value)throw new InvalidOperationException("Vấn đề không còn tồn tại trong dự án.");
                return Convert.ToInt32(x);
            }
        }
        private static int SuKienCu(SqlConnection con,SqlTransaction tx,Guid duAn,Guid vanDe,Guid user)
        {
            using(var cmd=new SqlCommand(BaoCaoLatestSql,con,tx))
            {
                Param(cmd,"@D",SqlDbType.UniqueIdentifier,duAn);
                Param(cmd,"@V",SqlDbType.UniqueIdentifier,vanDe);
                Param(cmd,"@U",SqlDbType.UniqueIdentifier,user);
                using(var r=cmd.ExecuteReader())return r.Read()?Convert.ToInt32(r.GetValue(2)):0;
            }
        }
        private static void GhiBanDoiSoat(SqlConnection con,SqlTransaction tx,Guid duAn,Guid vanDe,
             Guid nhanVien,Guid nguoiThaoTac,int lan,byte suKien,int? phutThuong,int? phutTangCa,
             int? phutThuongPm,int? phutTangCaPm,string lyDo,long heThongGiay)
        {
            using(var cmd=new SqlCommand(@"
                UPDATE dbo.TblKhaiBaoGioDoiSoat SET LaBanHienTai=0
                WHERE IdNhanVien=@U AND IdDuAn=@D AND IdVanDe=@V AND LanDoiSoat=@L AND LaBanHienTai=1;
                DECLARE @PhienBan int=(SELECT ISNULL(MAX(PhienBan),0)+1 FROM dbo.TblKhaiBaoGioDoiSoat
                  WHERE IdNhanVien=@U AND IdVanDe=@V AND IdDuAn=@D AND LanDoiSoat=@L);
                INSERT INTO dbo.TblKhaiBaoGioDoiSoat
                 (IdDoiSoat,IdNhanVien,IdCongViec,IdVanDe,IdDuAn,LanDoiSoat,PhienBan,LaBanHienTai,
                  SoGiayThuongHeThong,SoGiayTangCaHeThong,SoPhutThuongTuKhai,SoPhutTangCaTuKhai,
                  SoPhutThuongPmChot,SoPhutTangCaPmChot,LoaiSuKien,IdNguoiThaoTacVanDe,
                  LyDoDieuChinh,ChotGioHeThongLuc,NguoiTao,NgayTao)
                VALUES(NEWID(),@U,NULL,@V,@D,@L,@PhienBan,1,
                    @Seconds-ISNULL((SELECT SUM(k.SoGiay) FROM dbo.TblKhaiBaoGioPhien p
                         JOIN dbo.TblKhaiBaoGioKhoangTinh k ON k.IdPhien=p.IdPhien
                         WHERE p.IdVanDe=@V AND p.IdNhanVien=@U AND p.IdDuAn=@D
                           AND p.LoaiGio=3 AND ISNULL(p.LaTangCaVanDe,0)=1),0),
                    ISNULL((SELECT SUM(k.SoGiay) FROM dbo.TblKhaiBaoGioPhien p
                         JOIN dbo.TblKhaiBaoGioKhoangTinh k ON k.IdPhien=p.IdPhien
                         WHERE p.IdVanDe=@V AND p.IdNhanVien=@U AND p.IdDuAn=@D
                           AND p.LoaiGio=3 AND ISNULL(p.LaTangCaVanDe,0)=1),0),
                    @R,@O,@PR,@PO,@Event,@Actor,@Note,SYSUTCDATETIME(),N'GioXuLyVanDe',SYSUTCDATETIME());",con,tx))
            {
                Param(cmd,"@D",SqlDbType.UniqueIdentifier,duAn);
                Param(cmd,"@V",SqlDbType.UniqueIdentifier,vanDe);
                Param(cmd,"@U",SqlDbType.UniqueIdentifier,nhanVien);
                Param(cmd,"@Actor",SqlDbType.UniqueIdentifier,nguoiThaoTac);
                Param(cmd,"@L",SqlDbType.Int,lan);
                Param(cmd,"@Event",SqlDbType.TinyInt,suKien);
                Param(cmd,"@R",SqlDbType.Int,phutThuong.HasValue?(object)phutThuong.Value:DBNull.Value);
                Param(cmd,"@O",SqlDbType.Int,phutTangCa.HasValue?(object)phutTangCa.Value:DBNull.Value);
                Param(cmd,"@PR",SqlDbType.Int,phutThuongPm.HasValue?(object)phutThuongPm.Value:DBNull.Value);
                Param(cmd,"@PO",SqlDbType.Int,phutTangCaPm.HasValue?(object)phutTangCaPm.Value:DBNull.Value);
                Param(cmd,"@Note",SqlDbType.NVarChar,string.IsNullOrWhiteSpace(lyDo)?(object)DBNull.Value:lyDo);
                Param(cmd,"@Seconds",SqlDbType.BigInt,heThongGiay);
                cmd.ExecuteNonQuery();
            }
        }
        public void BaoPhanViec(Guid duAn,Guid vanDe,Guid nhanVien,int thuong,int tangCa,string lyDo,bool rut,
                                int? lanKyVong=null,int? phienBanKyVong=null)
        {
            if(duAn==Guid.Empty||vanDe==Guid.Empty||nhanVien==Guid.Empty)throw new ArgumentException("Thiếu thông tin báo phần việc.");
            if(thuong<0||tangCa<0||thuong>525600||tangCa>525600)throw new ArgumentOutOfRangeException("Số phút khai báo không hợp lệ.");
            lyDo=(lyDo??string.Empty).Trim();if(lyDo.Length>2000)throw new ArgumentException("Ghi chú dài tối đa 2000 ký tự.");
            using(SqlConnection con=_repo.MoKetNoi())
            using(SqlTransaction tx=con.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                _repo.KhoaDuAn(con,tx,duAn);
                int lan=LanHienTai(con,tx,duAn,vanDe);
                using(var cmd=new SqlCommand(@"SELECT CASE WHEN EXISTS(
                    SELECT 1 FROM dbo.TblVanDe v WITH(UPDLOCK,HOLDLOCK)
                    WHERE v.IdVanDe=@V AND v.IdDuAn=@D AND ISNULL(v.DaXoa,0)=0
                    AND v.TrangThaiDuyetGio IN(0,1,2,3)
                    AND EXISTS(SELECT 1 FROM dbo.TblVanDe_NhanVien a WHERE a.IdVanDe=v.IdVanDe AND a.IdNhanVien=@U)
                    AND EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioPhien p WHERE p.IdVanDe=@V AND p.IdNhanVien=@U AND p.LoaiGio=3)
                    AND NOT EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioDoiSoat s WHERE s.IdVanDe=@V AND s.IdNhanVien=@U
                      AND s.LoaiSuKien IN(4,6))
                ) THEN 1 ELSE 0 END",con,tx))
                {
                    Param(cmd,"@V",SqlDbType.UniqueIdentifier,vanDe);Param(cmd,"@D",SqlDbType.UniqueIdentifier,duAn);
                    Param(cmd,"@U",SqlDbType.UniqueIdentifier,nhanVien);
                    if(Convert.ToInt32(cmd.ExecuteScalar())!=1)
                        throw new InvalidOperationException("Bạn cần có phiên xử lý được phân công và chưa bị PM chốt giờ để báo phần việc.");
                }
                if(lanKyVong.HasValue || phienBanKyVong.HasValue)
                {
                    using(var versionQuery=new SqlCommand(BaoCaoLatestSql,con,tx))
                    {
                        Param(versionQuery,"@D",SqlDbType.UniqueIdentifier,duAn);
                        Param(versionQuery,"@V",SqlDbType.UniqueIdentifier,vanDe);
                        Param(versionQuery,"@U",SqlDbType.UniqueIdentifier,nhanVien);
                        int currentRound=0,currentVersion=0;
                        using(var versionReader=versionQuery.ExecuteReader())
                            if(versionReader.Read())
                            {
                                currentRound=Convert.ToInt32(versionReader.GetValue(0));
                                currentVersion=Convert.ToInt32(versionReader.GetValue(1));
                            }
                        if((lanKyVong.HasValue && lanKyVong.Value!=currentRound)
                           || (phienBanKyVong.HasValue && phienBanKyVong.Value!=currentVersion))
                            throw new DBConcurrencyException("Giờ tự khai vừa được cập nhật ở nơi khác. Hãy mở lại popup để xem bản mới nhất.");
                    }
                }
                int old=SuKienCu(con,tx,duAn,vanDe,nhanVien);
                if(rut && old!=1 && old!=3)throw new InvalidOperationException("Chưa có báo cáo phần việc để rút.");
                if(rut && string.IsNullOrWhiteSpace(lyDo))throw new InvalidOperationException("Cần nhập lý do rút báo phần việc.");
                if(rut)
                {
                    using(var check=new SqlCommand(@"SELECT TrangThaiDuyetGio FROM dbo.TblVanDe WHERE IdDuAn=@D AND IdVanDe=@V",con,tx))
                    {Param(check,"@D",SqlDbType.UniqueIdentifier,duAn);Param(check,"@V",SqlDbType.UniqueIdentifier,vanDe);
                     int status=Convert.ToInt32(check.ExecuteScalar());
                     if(status==1||status==2)
                         throw new InvalidOperationException("Vấn đề đang chờ duyệt hoặc đã duyệt, không thể rút báo; bạn vẫn có thể cập nhật giờ trước khi PM chốt.");}
                }
                var phien=_repo.LayPhienMo(con,tx,duAn,vanDe,nhanVien);
                if(phien!=null) KetToanVaDong(con,tx,phien,GioUtcHienTai(),1);
                long giay=_repo.LayTongGiay(con,tx,duAn,vanDe,nhanVien);
                if(!rut && giay==0 && thuong==0 && tangCa==0)
                    throw new InvalidOperationException("Chưa có giờ xử lý để báo phần việc.");
                byte suKienMoi = rut?(byte)2:(old==1||old==3?(byte)3:(byte)1);
                GhiBanDoiSoat(con,tx,duAn,vanDe,nhanVien,nhanVien,lan,
                     suKienMoi,rut?(int?)null:thuong,rut?(int?)null:tangCa,null,null,lyDo,giay);
                // Thong bao cho PM ve tung lan bao / rut bao / sua gio ca nhan,
                // nhu luong Task. Ghi TRONG giao dich; publish chuong SAU COMMIT.
                var nguoiNhanThongBao = new HashSet<Guid>();
                Tuple<Guid,string,string> pmThongBao=null;
                using(var pmQuery=new SqlCommand(@"SELECT d.IdNhanVienQuanLy,v.MaVanDe,v.TenVanDe,
                      ISNULL(NULLIF(u.UserName,N''),N'Nhân viên')
                      FROM dbo.TblDuAn d JOIN dbo.TblVanDe v ON v.IdDuAn=d.IdDuAn
                      LEFT JOIN dbo.aspnet_Users u ON u.UserId=@U
                      WHERE d.IdDuAn=@D AND v.IdVanDe=@V",con,tx))
                {
                    Param(pmQuery,"@D",SqlDbType.UniqueIdentifier,duAn);
                    Param(pmQuery,"@V",SqlDbType.UniqueIdentifier,vanDe);
                    Param(pmQuery,"@U",SqlDbType.UniqueIdentifier,nhanVien);
                    using(var pmReader=pmQuery.ExecuteReader())
                    {
                        Guid quanLy=Guid.Empty;string nhanVanDe="vấn đề";
                        string tenNguoi="Nhân viên";
                        if(pmReader.Read())
                        {
                            quanLy=pmReader.IsDBNull(0)?Guid.Empty:pmReader.GetGuid(0);
                            string ma=pmReader.IsDBNull(1)?"":Convert.ToString(pmReader.GetValue(1));
                            string ten=pmReader.IsDBNull(2)?"":Convert.ToString(pmReader.GetValue(2));
                            tenNguoi=pmReader.IsDBNull(3)?"Nhân viên":Convert.ToString(pmReader.GetValue(3));
                            nhanVanDe=(string.IsNullOrWhiteSpace(ma)?"":"["+ma+"] ")+ten;
                        }
                        // Khong goi GhiThongBao khi DataReader chua dong.
                        if(quanLy!=Guid.Empty && quanLy!=nhanVien)
                            pmThongBao = new Tuple<Guid,string,string>(quanLy,nhanVanDe,tenNguoi);
                    }
                }
                if(pmThongBao!=null)
                {
                    string tacVu=suKienMoi==2?"rút báo hoàn thành phần việc":
                                  suKienMoi==3?"sửa giờ tự khai":"báo hoàn thành phần việc";
                    string noiDung=pmThongBao.Item3+" đã "+tacVu+" cho "+pmThongBao.Item2+". "+
                        (rut?"Giờ đã ghi được giữ nguyên. ":
                        "Giờ tự khai: "+(thuong/60)+":"+(thuong%60).ToString("00")+
                        " làm theo lịch, "+(tangCa/60)+":"+(tangCa%60).ToString("00")+" tăng ca. ")+
                        (string.IsNullOrWhiteSpace(lyDo)?"":"Ghi chú/lý do: "+lyDo+". ")+
                        "Đây là báo cáo cá nhân, không phải yêu cầu duyệt hoàn thành toàn vấn đề.";
                    GhiThongBao(con,tx,pmThongBao.Item1,duAn,vanDe,
                        pmThongBao.Item3+" "+tacVu+" · "+pmThongBao.Item2,noiDung,"pm",nguoiNhanThongBao);
                }
                tx.Commit();
                PhatThongBaoIssueSauCommit(nguoiNhanThongBao);
            }
        }
        public List<BaoCaoGioVanDe> LayBaoCao(Guid duAn,Guid vanDe,Guid nguoiXem,bool admin)
        {
            // Shared role/assignment validation from the existing detail endpoint.
            ThongTinGioXuLyVanDe info=LayThongTin(duAn,vanDe,nguoiXem,admin);
            var list=new List<BaoCaoGioVanDe>();
            using(SqlConnection con=_repo.MoKetNoi())
            using(var cmd=new SqlCommand(@"
                SELECT a.IdNhanVien,ISNULL(NULLIF(u.UserName,N''),N'Nhân viên'),ISNULL(g.Giay,0),
                 b.LanDoiSoat,b.PhienBan,b.LoaiSuKien,b.SoPhutThuongTuKhai,b.SoPhutTangCaTuKhai,
                 b.SoPhutThuongPmChot,b.SoPhutTangCaPmChot,b.LyDoDieuChinh,
                 CASE WHEN op.IdPhien IS NULL THEN 0 ELSE 1 END,
                 ISNULL(g.GiayThuong,0),ISNULL(g.GiayTangCa,0),
                 CASE WHEN EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioPhien worked
                        WHERE worked.IdDuAn=@D AND worked.IdVanDe=@V AND worked.IdNhanVien=a.IdNhanVien
                          AND worked.IdCongViec IS NULL AND worked.LoaiGio=3) THEN 1 ELSE 0 END
                FROM (SELECT IdNhanVien FROM dbo.TblVanDe_NhanVien WHERE IdVanDe=@V
                      UNION SELECT IdNhanVien FROM dbo.TblKhaiBaoGioPhien WHERE IdVanDe=@V AND IdDuAn=@D AND LoaiGio=3) a
                CROSS JOIN dbo.TblVanDe v
                LEFT JOIN dbo.aspnet_Users u ON u.UserId=a.IdNhanVien
                OUTER APPLY(SELECT SUM(k.SoGiay) AS Giay,
                    SUM(CASE WHEN ISNULL(p.LaTangCaVanDe,0)=0 THEN k.SoGiay ELSE 0 END) GiayThuong,
                    SUM(CASE WHEN ISNULL(p.LaTangCaVanDe,0)=1 THEN k.SoGiay ELSE 0 END) GiayTangCa
                  FROM dbo.TblKhaiBaoGioPhien p
                  JOIN dbo.TblKhaiBaoGioKhoangTinh k ON k.IdPhien=p.IdPhien
                  WHERE p.IdVanDe=@V AND p.IdDuAn=@D AND p.IdNhanVien=a.IdNhanVien AND p.LoaiGio=3) g
                OUTER APPLY(SELECT TOP 1 LanDoiSoat,PhienBan,LoaiSuKien,
                  SoPhutThuongTuKhai,SoPhutTangCaTuKhai,SoPhutThuongPmChot,SoPhutTangCaPmChot,LyDoDieuChinh
                  FROM dbo.TblKhaiBaoGioDoiSoat s WHERE s.IdVanDe=@V AND s.IdDuAn=@D AND s.IdNhanVien=a.IdNhanVien
                    AND s.LaBanHienTai=1
                    AND s.LanDoiSoat=(CASE WHEN v.TrangThaiDuyetGio=3 THEN v.LanGuiDuyetGio+1
                                         WHEN v.LanGuiDuyetGio<1 THEN 1 ELSE v.LanGuiDuyetGio END)
                    ORDER BY LanDoiSoat DESC,PhienBan DESC) b
                OUTER APPLY(SELECT TOP 1 IdPhien FROM dbo.TblKhaiBaoGioPhien p WHERE p.IdVanDe=@V AND p.IdDuAn=@D
                  AND p.IdNhanVien=a.IdNhanVien AND p.LoaiGio=3 AND p.KetThucLuc IS NULL) op
                WHERE v.IdVanDe=@V AND v.IdDuAn=@D
                ORDER BY ISNULL(g.Giay,0) DESC,u.UserName",con))
            {
                Param(cmd,"@D",SqlDbType.UniqueIdentifier,duAn);Param(cmd,"@V",SqlDbType.UniqueIdentifier,vanDe);
                using(var r=cmd.ExecuteReader())while(r.Read())
                {
                    int kind=r.IsDBNull(5)?0:Convert.ToInt32(r.GetValue(5));
                    list.Add(new BaoCaoGioVanDe{IdNhanVien=r.GetGuid(0),TenNhanVien=r.GetString(1),
                     GiayHeThong=Convert.ToInt64(r.GetValue(2)),
                     GiayThuongHeThong=Convert.ToInt64(r.GetValue(12)),
                     GiayTangCaHeThong=Convert.ToInt64(r.GetValue(13)),
                     DaTungXuLy=Convert.ToInt32(r.GetValue(14))!=0,Lan=r.IsDBNull(3)?0:Convert.ToInt32(r.GetValue(3)),
                     PhienBan=r.IsDBNull(4)?0:Convert.ToInt32(r.GetValue(4)),SuKien=(byte)kind,
                     PhutThuongTuKhai=r.IsDBNull(6)?(int?)null:Convert.ToInt32(r.GetValue(6)),
                     PhutTangCaTuKhai=r.IsDBNull(7)?(int?)null:Convert.ToInt32(r.GetValue(7)),
                     PhutThuongPmChot=r.IsDBNull(8)?(int?)null:Convert.ToInt32(r.GetValue(8)),
                     PhutTangCaPmChot=r.IsDBNull(9)?(int?)null:Convert.ToInt32(r.GetValue(9)),
                     GhiChu=r.IsDBNull(10)?null:r.GetString(10),DangChay=Convert.ToInt32(r.GetValue(11))!=0,
                     DaBao=kind==1||kind==3||kind==4||kind==6,DaChot=kind==4||kind==6});
                }
            }
            return list;
        }
        // Mot truy van doi soat CA NHAN cho toan du an, thay vi moi Issue goi LayBaoCao
        // (ham do lai goi LayThongTin, gay N+1 SQL request khi vao Tab 3).
        // Quyen project + tham gia hoac PM giong LayDanhSach, khong doc gio cua nguoi khac.
        public Dictionary<Guid,BaoCaoGioVanDe> LayBangKeCuaToi(Guid duAn,Guid nhanVien,bool admin)
        {
            if(duAn==Guid.Empty || nhanVien==Guid.Empty)
                throw new ArgumentException("Thiếu thông tin bảng kê giờ vấn đề.");
            var result=new Dictionary<Guid,BaoCaoGioVanDe>();
            using(SqlConnection con=_repo.MoKetNoi())
            using(var cmd=new SqlCommand(@"
                SELECT v.IdVanDe,ISNULL(g.Giay,0),ISNULL(g.GiayThuong,0),ISNULL(g.GiayTangCa,0),
                       s.LanDoiSoat,s.PhienBan,s.LoaiSuKien,s.SoPhutThuongTuKhai,s.SoPhutTangCaTuKhai,
                       s.SoPhutThuongPmChot,s.SoPhutTangCaPmChot,s.LyDoDieuChinh,
                       CASE WHEN EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioPhien p
                           WHERE p.IdDuAn=@D AND p.IdVanDe=v.IdVanDe AND p.IdNhanVien=@U
                             AND p.IdCongViec IS NULL AND p.LoaiGio=3) THEN 1 ELSE 0 END
                FROM dbo.TblVanDe v
                INNER JOIN dbo.TblDuAn d ON d.IdDuAn=v.IdDuAn
                OUTER APPLY(SELECT ISNULL(SUM(k.SoGiay),0) Giay,
                      ISNULL(SUM(CASE WHEN ISNULL(p.LaTangCaVanDe,0)=0 THEN k.SoGiay ELSE 0 END),0) GiayThuong,
                      ISNULL(SUM(CASE WHEN ISNULL(p.LaTangCaVanDe,0)=1 THEN k.SoGiay ELSE 0 END),0) GiayTangCa
                     FROM dbo.TblKhaiBaoGioPhien p
                     JOIN dbo.TblKhaiBaoGioKhoangTinh k ON k.IdPhien=p.IdPhien
                     WHERE p.IdDuAn=@D AND p.IdVanDe=v.IdVanDe AND p.IdNhanVien=@U
                       AND p.IdCongViec IS NULL AND p.LoaiGio=3) g
                OUTER APPLY(SELECT TOP(1) s.LanDoiSoat,s.PhienBan,s.LoaiSuKien,
                     s.SoPhutThuongTuKhai,s.SoPhutTangCaTuKhai,
                     s.SoPhutThuongPmChot,s.SoPhutTangCaPmChot,s.LyDoDieuChinh
                     FROM dbo.TblKhaiBaoGioDoiSoat s
                     WHERE s.IdDuAn=@D AND s.IdVanDe=v.IdVanDe AND s.IdNhanVien=@U
                       AND s.LaBanHienTai=1 AND s.LanDoiSoat=(CASE WHEN v.TrangThaiDuyetGio=3
                           THEN v.LanGuiDuyetGio+1 WHEN v.LanGuiDuyetGio<1 THEN 1 ELSE v.LanGuiDuyetGio END)
                     ORDER BY s.PhienBan DESC) s
                WHERE v.IdDuAn=@D AND ISNULL(v.DaXoa,0)=0 AND ISNULL(d.DaXoa,0)=0
                   AND (@Admin=1 OR d.IdNhanVienQuanLy=@U
                        OR EXISTS(SELECT 1 FROM dbo.TblVanDe_NhanVien a
                               WHERE a.IdVanDe=v.IdVanDe AND a.IdNhanVien=@U))",con))
            {
                Param(cmd,"@D",SqlDbType.UniqueIdentifier,duAn);
                Param(cmd,"@U",SqlDbType.UniqueIdentifier,nhanVien);
                Param(cmd,"@Admin",SqlDbType.Bit,admin);
                using(var r=cmd.ExecuteReader())while(r.Read())
                {
                    int kind=r.IsDBNull(6)?0:Convert.ToInt32(r.GetValue(6));
                    result[r.GetGuid(0)]=new BaoCaoGioVanDe {
                        IdNhanVien=nhanVien,GiayHeThong=Convert.ToInt64(r.GetValue(1)),
                        GiayThuongHeThong=Convert.ToInt64(r.GetValue(2)),
                        GiayTangCaHeThong=Convert.ToInt64(r.GetValue(3)),
                        Lan=r.IsDBNull(4)?0:Convert.ToInt32(r.GetValue(4)),
                        PhienBan=r.IsDBNull(5)?0:Convert.ToInt32(r.GetValue(5)),
                        SuKien=(byte)kind,
                        PhutThuongTuKhai=r.IsDBNull(7)?(int?)null:Convert.ToInt32(r.GetValue(7)),
                        PhutTangCaTuKhai=r.IsDBNull(8)?(int?)null:Convert.ToInt32(r.GetValue(8)),
                        PhutThuongPmChot=r.IsDBNull(9)?(int?)null:Convert.ToInt32(r.GetValue(9)),
                        PhutTangCaPmChot=r.IsDBNull(10)?(int?)null:Convert.ToInt32(r.GetValue(10)),
                        GhiChu=r.IsDBNull(11)?null:r.GetString(11),
                        DaTungXuLy=Convert.ToInt32(r.GetValue(12))==1,
                        DaBao=kind==1||kind==3||kind==4||kind==6,
                        DaChot=kind==4||kind==6
                    };
                }
            }
            return result;
        }

        public List<GioVanDeNgay> LayNhatKy(Guid duAn,Guid nhanVien,DateTime ngay)
        {
            var list=new List<GioVanDeNgay>();
            // Tinh theo k.NgayLam, khong theo ngay bat dau cua phien.
            DateTime endUtc=ngay.Date.AddDays(1).AddHours(-7);
            using(SqlConnection con=_repo.MoKetNoi())
            using(var cmd=new SqlCommand(@"
               SELECT v.IdVanDe,v.MaVanDe,v.TenVanDe,ISNULL(t.Giay,0),
                 CASE WHEN p.IdPhien IS NULL THEN 0 ELSE 1 END,
                 ISNULL(t.GiayThuong,0),ISNULL(t.GiayTangCa,0)
               FROM (
                   SELECT DISTINCT q.IdVanDe FROM dbo.TblKhaiBaoGioPhien q
                   JOIN dbo.TblKhaiBaoGioKhoangTinh k ON k.IdPhien=q.IdPhien
                   WHERE q.IdDuAn=@D AND q.IdNhanVien=@U AND q.LoaiGio=3 AND k.NgayLam=@Day
                   UNION
                   SELECT DISTINCT q.IdVanDe FROM dbo.TblKhaiBaoGioPhien q
                   WHERE q.IdDuAn=@D AND q.IdNhanVien=@U AND q.LoaiGio=3
                     AND q.KetThucLuc IS NULL AND q.BatDauLuc<@EndUtc
               ) ids
               JOIN dbo.TblVanDe v ON v.IdVanDe=ids.IdVanDe AND v.IdDuAn=@D
               OUTER APPLY(SELECT SUM(k.SoGiay) Giay,
                    SUM(CASE WHEN ISNULL(q.LaTangCaVanDe,0)=0 THEN k.SoGiay ELSE 0 END) GiayThuong,
                    SUM(CASE WHEN ISNULL(q.LaTangCaVanDe,0)=1 THEN k.SoGiay ELSE 0 END) GiayTangCa
                    FROM dbo.TblKhaiBaoGioKhoangTinh k
                    JOIN dbo.TblKhaiBaoGioPhien q ON q.IdPhien=k.IdPhien
                    WHERE q.IdVanDe=v.IdVanDe AND q.IdDuAn=@D AND q.IdNhanVien=@U
                      AND q.LoaiGio=3 AND k.NgayLam=@Day) t
               OUTER APPLY(SELECT TOP 1 q.IdPhien FROM dbo.TblKhaiBaoGioPhien q
                    WHERE q.IdVanDe=v.IdVanDe AND q.IdDuAn=@D AND q.IdNhanVien=@U
                      AND q.LoaiGio=3 AND q.KetThucLuc IS NULL AND q.BatDauLuc<@EndUtc) p
               ORDER BY v.MaVanDe",con))
            {
                Param(cmd,"@D",SqlDbType.UniqueIdentifier,duAn);Param(cmd,"@U",SqlDbType.UniqueIdentifier,nhanVien);
                Param(cmd,"@Day",SqlDbType.Date,ngay.Date);Param(cmd,"@EndUtc",SqlDbType.DateTime2,endUtc);
                using(var r=cmd.ExecuteReader())while(r.Read())list.Add(new GioVanDeNgay{
                    IdVanDe=r.GetGuid(0),MaVanDe=r.IsDBNull(1)?"":r.GetString(1),
                    TenVanDe=r.IsDBNull(2)?"":r.GetString(2),Giay=Convert.ToInt64(r.GetValue(3)),
                    DangChay=Convert.ToInt32(r.GetValue(4))!=0,
                    GiayThuong=Convert.ToInt64(r.GetValue(5)),GiayTangCa=Convert.ToInt64(r.GetValue(6))});
            }
            return list;
        }
        public sealed class PhienVanDeNgay
        {
            public DateTime BatDauUtc { get; set; }
            public DateTime? KetThucUtc { get; set; }
            public bool LaTangCa { get; set; }
            public long GiayDaKetToanTrongNgay { get; set; }
        }
        public List<PhienVanDeNgay> LayPhienTrongNgay(Guid duAn, Guid vanDe, Guid nhanVien, bool admin, DateTime ngay)
        {
            // Dong nhat Tab 2 voi Task: chi doc phien cua chinh tai khoan, gio tinh theo k.NgayLam.
            // Xac thuc tren Issue TRUOC khi doc du lieu, khong dua vao vanDeId do client goi.
            LayThongTin(duAn,vanDe,nhanVien,admin);
            DateTime start=ngay.Date.AddHours(-7),end=start.AddDays(1);
            var list=new List<PhienVanDeNgay>();
            using(SqlConnection con=_repo.MoKetNoi())
            using(var cmd=new SqlCommand(@"SELECT p.BatDauLuc,p.KetThucLuc,ISNULL(p.LaTangCaVanDe,0),
                ISNULL(SUM(CASE WHEN k.NgayLam=@Day THEN k.SoGiay ELSE 0 END),0)
                FROM dbo.TblKhaiBaoGioPhien p
                LEFT JOIN dbo.TblKhaiBaoGioKhoangTinh k ON k.IdPhien=p.IdPhien
                WHERE p.IdDuAn=@D AND p.IdVanDe=@V AND p.IdNhanVien=@U AND p.LoaiGio=3
                  AND p.BatDauLuc<@EndUtc AND (p.KetThucLuc IS NULL OR p.KetThucLuc>@StartUtc)
                GROUP BY p.IdPhien,p.BatDauLuc,p.KetThucLuc,p.LaTangCaVanDe
                ORDER BY p.BatDauLuc DESC",con))
            {
                Param(cmd,"@D",SqlDbType.UniqueIdentifier,duAn);Param(cmd,"@V",SqlDbType.UniqueIdentifier,vanDe);
                Param(cmd,"@U",SqlDbType.UniqueIdentifier,nhanVien);Param(cmd,"@Day",SqlDbType.Date,ngay.Date);
                Param(cmd,"@StartUtc",SqlDbType.DateTime2,start);Param(cmd,"@EndUtc",SqlDbType.DateTime2,end);
                using(var r=cmd.ExecuteReader())while(r.Read())list.Add(new PhienVanDeNgay {
                    BatDauUtc=r.GetDateTime(0),KetThucUtc=r.IsDBNull(1)?(DateTime?)null:r.GetDateTime(1),
                    LaTangCa=Convert.ToBoolean(r.GetValue(2)),GiayDaKetToanTrongNgay=Convert.ToInt64(r.GetValue(3))
                });
            }
            return list;
        }
        public void ChotCong(Guid duAn,Guid vanDe,Guid pm,bool admin,IList<GioPmChotDong> danhSach)
        {
            if(danhSach==null||danhSach.Count==0)throw new ArgumentException("Chưa có nhân viên để chốt công.");
            using(SqlConnection con=_repo.MoKetNoi())
            using(SqlTransaction tx=con.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                _repo.KhoaDuAn(con,tx,duAn);
                var notifyAfterCommit = new HashSet<Guid>();
                using(var q=new SqlCommand(@"SELECT CASE WHEN EXISTS(SELECT 1 FROM dbo.TblVanDe v WITH(UPDLOCK,HOLDLOCK)
                       JOIN dbo.TblDuAn d ON d.IdDuAn=v.IdDuAn
                       WHERE v.IdDuAn=@D AND v.IdVanDe=@V AND v.TrangThaiDuyetGio=2 AND v.TrangThai=1
                         AND (d.IdNhanVienQuanLy=@U OR @A=1)) THEN 1 ELSE 0 END",con,tx))
                {
                    Param(q,"@D",SqlDbType.UniqueIdentifier,duAn);Param(q,"@V",SqlDbType.UniqueIdentifier,vanDe);
                    Param(q,"@U",SqlDbType.UniqueIdentifier,pm);Param(q,"@A",SqlDbType.Bit,admin);
                    if(Convert.ToInt32(q.ExecuteScalar())!=1)throw new UnauthorizedAccessException("Chỉ PM được chốt giờ sau khi duyệt Issue.");
                }
                // Participants who ever recorded Issue hours MUST have a current personal report.
                var actual=new Dictionary<Guid,long>();
                using(var q=new SqlCommand(@"SELECT p.IdNhanVien,ISNULL(SUM(k.SoGiay),0) FROM dbo.TblKhaiBaoGioPhien p
                    LEFT JOIN dbo.TblKhaiBaoGioKhoangTinh k ON k.IdPhien=p.IdPhien
                    WHERE p.IdDuAn=@D AND p.IdVanDe=@V AND p.LoaiGio=3
                    GROUP BY p.IdNhanVien
                    HAVING COUNT(p.IdPhien)>0 OR EXISTS (
                        SELECT 1 FROM dbo.TblKhaiBaoGioDoiSoat s
                        WHERE s.IdDuAn=@D AND s.IdVanDe=@V AND s.IdNhanVien=p.IdNhanVien
                          AND s.LaBanHienTai=1 AND s.LoaiSuKien IN(1,3,4,6)
                          AND s.LanDoiSoat=(SELECT CASE WHEN v.TrangThaiDuyetGio=3
                                    THEN v.LanGuiDuyetGio+1 WHEN v.LanGuiDuyetGio<1 THEN 1
                                    ELSE v.LanGuiDuyetGio END
                                    FROM dbo.TblVanDe v WHERE v.IdVanDe=@V AND v.IdDuAn=@D)
                    )",con,tx))
                {
                    Param(q,"@D",SqlDbType.UniqueIdentifier,duAn);Param(q,"@V",SqlDbType.UniqueIdentifier,vanDe);
                    using(var r=q.ExecuteReader())while(r.Read())actual[r.GetGuid(0)]=Convert.ToInt64(r.GetValue(1));
                }
                if(actual.Count!=danhSach.Count || actual.Keys.Except(danhSach.Select(x=>x.IdNhanVien)).Any()
                   || danhSach.GroupBy(x=>x.IdNhanVien).Any(g=>g.Count()>1))
                    throw new InvalidOperationException("Danh sách chốt không khớp với nhân viên đã tham gia.");
                int lan=LanHienTai(con,tx,duAn,vanDe);
                foreach(var x in danhSach)
                {
                    long currentSeconds;
                    if(!actual.TryGetValue(x.IdNhanVien,out currentSeconds))
                        throw new InvalidOperationException("Nhân viên đã thay đổi trong lúc PM chốt giờ.");
                    if(x.GiayHeThongKyVong.HasValue && currentSeconds!=x.GiayHeThongKyVong.Value)
                        throw new DBConcurrencyException("Giờ hệ thống đã thay đổi. Vui lòng làm mới trước khi chốt công.");
                }
                foreach(var x in danhSach)
                {
                    if(x.PhutThuong<0||x.PhutTangCa<0||x.PhutThuong>525600||x.PhutTangCa>525600)
                        throw new ArgumentException("Số phút PM chốt không hợp lệ.");
                    if(string.IsNullOrWhiteSpace(x.LyDo)==false && x.LyDo.Length>2000)
                        throw new ArgumentException("Ghi chú PM quá dài.");
                    using(var q=new SqlCommand(BaoCaoLatestSql.Replace("WITH (UPDLOCK,HOLDLOCK)","WITH (UPDLOCK,HOLDLOCK)"),con,tx))
                    {
                        Param(q,"@D",SqlDbType.UniqueIdentifier,duAn);Param(q,"@V",SqlDbType.UniqueIdentifier,vanDe);
                        Param(q,"@U",SqlDbType.UniqueIdentifier,x.IdNhanVien);
                        using(var r=q.ExecuteReader())
                        {
                            if(!r.Read()||r.IsDBNull(2)||Convert.ToInt32(r.GetValue(2))==2)
                                throw new InvalidOperationException("Nhân viên còn chưa báo hoàn thành phần việc.");
                            if((x.LanKyVong.HasValue && Convert.ToInt32(r.GetValue(0))!=x.LanKyVong.Value)
                               || (x.PhienBanKyVong.HasValue && Convert.ToInt32(r.GetValue(1))!=x.PhienBanKyVong.Value))
                                throw new DBConcurrencyException("Phiên bản khai giờ đã thay đổi. Vui lòng làm mới trước khi chốt công.");
                            if(Convert.ToInt32(r.GetValue(2))>=4)throw new InvalidOperationException("Giờ Issue đã được chốt; dùng điều chỉnh sau chốt.");
                            int tr=r.IsDBNull(3)?0:Convert.ToInt32(r.GetValue(3));
                            int tc=r.IsDBNull(4)?0:Convert.ToInt32(r.GetValue(4));
                            if((tr!=x.PhutThuong||tc!=x.PhutTangCa)&&string.IsNullOrWhiteSpace(x.LyDo))
                                throw new InvalidOperationException("PM phải nhập lý do khi điều chỉnh số phút đã báo.");
                        }
                    }
                }
                foreach(var x in danhSach)
                {
                    int tr=0,tc=0;
                    using(var q=new SqlCommand(BaoCaoLatestSql,con,tx))
                    {Param(q,"@D",SqlDbType.UniqueIdentifier,duAn);Param(q,"@V",SqlDbType.UniqueIdentifier,vanDe);
                     Param(q,"@U",SqlDbType.UniqueIdentifier,x.IdNhanVien);
                     using(var r=q.ExecuteReader())if(r.Read()){
                        tr=r.IsDBNull(3)?0:Convert.ToInt32(r.GetValue(3));tc=r.IsDBNull(4)?0:Convert.ToInt32(r.GetValue(4));}}
                    GhiBanDoiSoat(con,tx,duAn,vanDe,x.IdNhanVien,pm,lan,4,tr,tc,
                         x.PhutThuong,x.PhutTangCa,x.LyDo,actual[x.IdNhanVien]);
                    // Luu gia tri PM chot CUA LAN NAY vao thong bao, khong doc lai ban chot sau nay.
                    string pmChotNoiDung = "Kết quả: PM ĐÃ CHỐT GIỜ\r\n"
                        + "Giờ làm theo lịch PM chốt: " + HienThiPhutPm(x.PhutThuong) + "\r\n"
                        + "Giờ tăng ca PM chốt: " + HienThiPhutPm(x.PhutTangCa) + "\r\n"
                        + "Tổng giờ PM chốt: " + HienThiPhutPm((long)x.PhutThuong+x.PhutTangCa) + "\r\n"
                        + "Lượt đối soát: " + lan + "\r\n"
                        + (string.IsNullOrWhiteSpace(x.LyDo) ? "" : "Ghi chú PM: " + x.LyDo + "\r\n")
                        + "Có thể xem bảng kê và lịch sử giờ công của vấn đề.";
                    GhiThongBao(con,tx,x.IdNhanVien,duAn,vanDe,"Quản lý đã chốt giờ xử lý vấn đề",
                        pmChotNoiDung,"pm-final",notifyAfterCommit);
                }
                tx.Commit();
                PhatThongBaoIssueSauCommit(notifyAfterCommit);
            }
        }
        public void DieuChinhSauChot(Guid duAn,Guid vanDe,Guid pm,bool admin,Guid nhanVien,
                                      int phutThuong,int phutTangCa,string lyDo,
                                      int? lanKyVong=null,int? phienBanKyVong=null)
        {
            if(phutThuong<0||phutTangCa<0||string.IsNullOrWhiteSpace(lyDo)||lyDo.Trim().Length>2000)
                throw new ArgumentException("Điều chỉnh sau chốt cần giờ hợp lệ và lý do.");
            using(SqlConnection con=_repo.MoKetNoi())using(SqlTransaction tx=con.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                _repo.KhoaDuAn(con,tx,duAn);
                var notifyAfterCommit = new HashSet<Guid>();
                using(var q=new SqlCommand(@"SELECT CASE WHEN EXISTS(SELECT 1 FROM dbo.TblVanDe v
                  JOIN dbo.TblDuAn d ON d.IdDuAn=v.IdDuAn WHERE v.IdDuAn=@D AND v.IdVanDe=@V
                    AND v.TrangThaiDuyetGio=2 AND (d.IdNhanVienQuanLy=@U OR @A=1)) THEN 1 ELSE 0 END",con,tx))
                {Param(q,"@D",SqlDbType.UniqueIdentifier,duAn);Param(q,"@V",SqlDbType.UniqueIdentifier,vanDe);
                 Param(q,"@U",SqlDbType.UniqueIdentifier,pm);Param(q,"@A",SqlDbType.Bit,admin);
                 if(Convert.ToInt32(q.ExecuteScalar())!=1)throw new UnauthorizedAccessException("Không có quyền chỉnh giờ sau chốt.");}
                int lan=LanHienTai(con,tx,duAn,vanDe),rpt=0,ot=0;
                using(var q=new SqlCommand(BaoCaoLatestSql,con,tx))
                {Param(q,"@D",SqlDbType.UniqueIdentifier,duAn);Param(q,"@V",SqlDbType.UniqueIdentifier,vanDe);
                 Param(q,"@U",SqlDbType.UniqueIdentifier,nhanVien);
                 using(var r=q.ExecuteReader())
                 {if(!r.Read()||r.IsDBNull(2)||Convert.ToInt32(r.GetValue(2))<4)
                     throw new InvalidOperationException("Giờ của nhân viên chưa được chốt.");
                   if((lanKyVong.HasValue && Convert.ToInt32(r.GetValue(0))!=lanKyVong.Value)
                      || (phienBanKyVong.HasValue && Convert.ToInt32(r.GetValue(1))!=phienBanKyVong.Value))
                       throw new DBConcurrencyException("Bản chốt đã thay đổi. Vui lòng làm mới popup trước khi điều chỉnh.");
                   rpt=r.IsDBNull(3)?0:Convert.ToInt32(r.GetValue(3));ot=r.IsDBNull(4)?0:Convert.ToInt32(r.GetValue(4));}}
                long secs=_repo.LayTongGiay(con,tx,duAn,vanDe,nhanVien);
                GhiBanDoiSoat(con,tx,duAn,vanDe,nhanVien,pm,lan,6,rpt,ot,phutThuong,phutTangCa,lyDo.Trim(),secs);
                // Chot gia tri ngay tai thoi diem ghi su kien 6. Lich su thong bao khong doi theo cac lan sua sau.
                string pmDieuChinhNoiDung = "Kết quả: PM ĐIỀU CHỈNH SAU CHỐT\r\n"
                    + "Giờ làm theo lịch PM chốt: " + HienThiPhutPm(phutThuong) + "\r\n"
                    + "Giờ tăng ca PM chốt: " + HienThiPhutPm(phutTangCa) + "\r\n"
                    + "Tổng giờ PM chốt: " + HienThiPhutPm((long)phutThuong+phutTangCa) + "\r\n"
                    + "Lượt đối soát: " + lan + "\r\n"
                    + "Lý do điều chỉnh: " + lyDo.Trim();
                GhiThongBao(con,tx,nhanVien,duAn,vanDe,"Quản lý đã điều chỉnh giờ xử lý sau chốt",
                    pmDieuChinhNoiDung,"pm-final",notifyAfterCommit);
                tx.Commit();
                PhatThongBaoIssueSauCommit(notifyAfterCommit);
            }
        }

        // Snapshot PM cua CHINH nguoi nhan thong bao: khong lay du lieu cua nhan vien khac.
        // Giu gio he thong da ghi trong ban doi soat tai thoi diem PM chot, khong dung tong hien tai.
        public sealed class KetQuaPmVanDeTheoLan
        {
            public DateTime NgayTaoUtc { get; set; }
            public byte LoaiSuKien { get; set; }
            public long GiayThuongHeThong { get; set; }
            public long GiayTangCaHeThong { get; set; }
            public int? PhutThuongTuKhai { get; set; }
            public int? PhutTangCaTuKhai { get; set; }
            public int PhutThuongPmChot { get; set; }
            public int PhutTangCaPmChot { get; set; }
            public string LyDo { get; set; }
        }
        public List<KetQuaPmVanDeTheoLan> LayLichSuPmCaNhan(Guid duAn,Guid vanDe,Guid nguoiNhan,bool admin)
        {
            LayThongTin(duAn,vanDe,nguoiNhan,admin);
            var list=new List<KetQuaPmVanDeTheoLan>();
            using(SqlConnection con=_repo.MoKetNoi())
            using(var q=new SqlCommand(@"SELECT NgayTao,LoaiSuKien,
                    ISNULL(SoGiayThuongHeThong,0),ISNULL(SoGiayTangCaHeThong,0),
                    SoPhutThuongTuKhai,SoPhutTangCaTuKhai,
                    SoPhutThuongPmChot,SoPhutTangCaPmChot,LyDoDieuChinh
                FROM dbo.TblKhaiBaoGioDoiSoat
                WHERE IdDuAn=@D AND IdVanDe=@V AND IdNhanVien=@U AND IdCongViec IS NULL
                  AND LoaiSuKien IN (4,6) AND SoPhutThuongPmChot IS NOT NULL
                  AND SoPhutTangCaPmChot IS NOT NULL
                ORDER BY NgayTao DESC,PhienBan DESC",con))
            {
                Param(q,"@D",SqlDbType.UniqueIdentifier,duAn);
                Param(q,"@V",SqlDbType.UniqueIdentifier,vanDe);
                Param(q,"@U",SqlDbType.UniqueIdentifier,nguoiNhan);
                using(var r=q.ExecuteReader())while(r.Read())list.Add(new KetQuaPmVanDeTheoLan {
                    NgayTaoUtc=r.GetDateTime(0),LoaiSuKien=Convert.ToByte(r.GetValue(1)),
                    GiayThuongHeThong=Convert.ToInt64(r.GetValue(2)),GiayTangCaHeThong=Convert.ToInt64(r.GetValue(3)),
                    PhutThuongTuKhai=r.IsDBNull(4)?(int?)null:Convert.ToInt32(r.GetValue(4)),
                    PhutTangCaTuKhai=r.IsDBNull(5)?(int?)null:Convert.ToInt32(r.GetValue(5)),
                    PhutThuongPmChot=Convert.ToInt32(r.GetValue(6)),
                    PhutTangCaPmChot=Convert.ToInt32(r.GetValue(7)),
                    LyDo=r.IsDBNull(8)?null:Convert.ToString(r.GetValue(8))
                });
            }
            return list;
        }

        // Chi cho thong bao CU khong luu chi tiet: tim DUNG mot su kien PM cua nhan vien
        // cung van de, dung loai su kien, va phat sinh gan nhu dong thoi voi thong bao.
        // Khong lay BAN HIEN TAI vi no co the da bi PM chinh sau do.
        public Tuple<int,int> LayGioPmTuThongBaoCu(Guid duAn,Guid vanDe,Guid nguoiNhan,bool admin,
                                                     DateTime thoiDiemThongBao,bool laDieuChinh)
        {
            LayThongTin(duAn,vanDe,nguoiNhan,admin); // Phan quyen du an + nguoi tham gia / PM.
            var matches=new List<Tuple<int,int>>();
            using(SqlConnection con=_repo.MoKetNoi())
            using(var q=new SqlCommand(@"SELECT SoPhutThuongPmChot,SoPhutTangCaPmChot,NgayTao
                FROM dbo.TblKhaiBaoGioDoiSoat
                WHERE IdDuAn=@D AND IdVanDe=@V AND IdNhanVien=@U AND IdCongViec IS NULL
                  AND LoaiSuKien=@E AND SoPhutThuongPmChot IS NOT NULL
                  AND SoPhutTangCaPmChot IS NOT NULL",con))
            {
                Param(q,"@D",SqlDbType.UniqueIdentifier,duAn);
                Param(q,"@V",SqlDbType.UniqueIdentifier,vanDe);
                Param(q,"@U",SqlDbType.UniqueIdentifier,nguoiNhan);
                Param(q,"@E",SqlDbType.TinyInt,laDieuChinh?6:4);
                using(var rd=q.ExecuteReader())while(rd.Read())
                {
                    DateTime createdUtc=rd.GetDateTime(2);
                    // TblThongBao.NgayTao su dung GETDATE() (gio may chu),
                    // doi soat NgayTao su dung SYSUTCDATETIME(); chap nhan ca UTC va VN (+07).
                    double delta=Math.Min(Math.Abs((thoiDiemThongBao-createdUtc).TotalSeconds),
                               Math.Abs((thoiDiemThongBao-createdUtc.AddHours(7)).TotalSeconds));
                    if(delta<=30)
                        matches.Add(Tuple.Create(Convert.ToInt32(rd.GetValue(0)),Convert.ToInt32(rd.GetValue(1))));
                }
            }
            return matches.Count==1?matches[0]:null;
        }

        private static string HienThiPhutPm(long phut)
        {
            return (phut/60).ToString(System.Globalization.CultureInfo.InvariantCulture) + ":"
                + (phut%60).ToString("00",System.Globalization.CultureInfo.InvariantCulture);
        }
        public static string DinhDangGio(long giay)
        {
            if (giay < 0) giay = 0;
            return string.Format("{0:00}:{1:00}:{2:00}", giay / 3600, (giay / 60) % 60, giay % 60);
        }
        private static void Param(SqlCommand c,string name,SqlDbType type,object value)
        {
            c.Parameters.Add(name,type).Value=value??DBNull.Value;
        }
        private static Guid? NullableGuid(IDataRecord r,int index)
        {
            return r.IsDBNull(index)?(Guid?)null:r.GetGuid(index);
        }

        /// <summary>Danh sach van de duoc phan cong hoac duoc PM xem, chi trong du an.</summary>
        public List<ThongTinGioXuLyVanDe> LayDanhSach(Guid duAn, Guid nguoiXem, bool laQuanTri)
        {
            if (duAn==Guid.Empty || nguoiXem==Guid.Empty) throw new ArgumentException("Thiếu dự án hoặc nhân viên.");
            var items=new List<ThongTinGioXuLyVanDe>();
            using (SqlConnection con=_repo.MoKetNoi())
            using (var cmd=new SqlCommand(@"SELECT v.IdVanDe,v.MaVanDe,v.TenVanDe,v.TrangThai,
                      v.TrangThaiDuyetGio,
                      CASE WHEN a.IdNhanVien IS NULL THEN 0 ELSE 1 END AS DuocPhanCong,
                      CASE WHEN @Admin=1 OR d.IdNhanVienQuanLy=@U THEN 1 ELSE 0 END AS LaQuanLy,
                      ISNULL(g.SoGiay,0) AS GioNhom,ISNULL(c.SoGiay,0) AS GioToi,
                      CASE WHEN t.IdPhien IS NULL THEN 0 ELSE 1 END AS DangChay,
                      t.BatDauLuc,
                      CASE WHEN EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioPhien q
                           WHERE q.IdVanDe=v.IdVanDe AND q.IdNhanVien=@U AND q.LoaiGio=3)
                           THEN 1 ELSE 0 END AS DaXuLy,
                      CASE WHEN EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioDoiSoat s
                           WHERE s.IdVanDe=v.IdVanDe AND s.IdNhanVien=@U
                             AND s.LaBanHienTai=1 AND s.LoaiSuKien IN(1,3,4,6)
                             AND s.LanDoiSoat=(CASE WHEN v.TrangThaiDuyetGio=3 THEN v.LanGuiDuyetGio+1
                                             WHEN v.LanGuiDuyetGio<1 THEN 1 ELSE v.LanGuiDuyetGio END)
                             AND NOT EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioDoiSoat n
                               WHERE n.IdVanDe=s.IdVanDe AND n.IdNhanVien=@U AND n.LaBanHienTai=1
                                 AND n.LanDoiSoat>s.LanDoiSoat))
                           THEN 1 ELSE 0 END AS DaBao,
                      ISNULL(c.GiayThuong,0),ISNULL(c.GiayTangCa,0),
                      ISNULL(g.GiayThuong,0),ISNULL(g.GiayTangCa,0),
                      ISNULL(t.LaTangCaVanDe,0),
                      CASE WHEN lastEvent.LoaiSuKien=2 THEN 1 ELSE 0 END AS DaRutBaoGanNhat
                    FROM dbo.TblVanDe v
                    INNER JOIN dbo.TblDuAn d ON d.IdDuAn=v.IdDuAn
                    LEFT JOIN dbo.TblVanDe_NhanVien a ON a.IdVanDe=v.IdVanDe AND a.IdNhanVien=@U
                    OUTER APPLY(SELECT SUM(k.SoGiay) AS SoGiay,
                         SUM(CASE WHEN ISNULL(p.LaTangCaVanDe,0)=0 THEN k.SoGiay ELSE 0 END) GiayThuong,
                         SUM(CASE WHEN ISNULL(p.LaTangCaVanDe,0)=1 THEN k.SoGiay ELSE 0 END) GiayTangCa
                         FROM dbo.TblKhaiBaoGioPhien p
                        INNER JOIN dbo.TblKhaiBaoGioKhoangTinh k ON k.IdPhien=p.IdPhien
                        WHERE p.IdVanDe=v.IdVanDe AND p.IdCongViec IS NULL AND p.LoaiGio=3) g
                    OUTER APPLY(SELECT SUM(k.SoGiay) AS SoGiay,
                         SUM(CASE WHEN ISNULL(p.LaTangCaVanDe,0)=0 THEN k.SoGiay ELSE 0 END) GiayThuong,
                         SUM(CASE WHEN ISNULL(p.LaTangCaVanDe,0)=1 THEN k.SoGiay ELSE 0 END) GiayTangCa
                         FROM dbo.TblKhaiBaoGioPhien p
                        INNER JOIN dbo.TblKhaiBaoGioKhoangTinh k ON k.IdPhien=p.IdPhien
                        WHERE p.IdVanDe=v.IdVanDe AND p.IdNhanVien=@U AND p.IdCongViec IS NULL AND p.LoaiGio=3) c
                    OUTER APPLY(SELECT TOP 1 s.LoaiSuKien FROM dbo.TblKhaiBaoGioDoiSoat s
                        WHERE s.IdDuAn=@D AND s.IdVanDe=v.IdVanDe AND s.IdNhanVien=@U
                          AND s.LaBanHienTai=1
                          AND s.LanDoiSoat=(CASE WHEN v.TrangThaiDuyetGio=3 THEN v.LanGuiDuyetGio+1
                                          WHEN v.LanGuiDuyetGio<1 THEN 1 ELSE v.LanGuiDuyetGio END)
                        ORDER BY s.PhienBan DESC) lastEvent
                    -- Tab 1 Issue hien base=giay da ket toan + live=giay chua ket toan.
                    -- MOC tick cua list phai la DaTinhDenLuc; neu dung BatDauLuc sau Hangfire se cong trung!
                    OUTER APPLY(SELECT TOP 1 p.IdPhien,
                         COALESCE(p.DaTinhDenLuc,p.BatDauLuc) AS BatDauLuc,p.LaTangCaVanDe
                        FROM dbo.TblKhaiBaoGioPhien p
                        WHERE p.IdVanDe=v.IdVanDe AND p.IdNhanVien=@U
                          AND p.IdCongViec IS NULL AND p.LoaiGio=3 AND p.KetThucLuc IS NULL
                        ORDER BY p.BatDauLuc DESC) t
                    WHERE v.IdDuAn=@D AND ISNULL(v.DaXoa,0)=0 AND ISNULL(d.DaXoa,0)=0
                      AND (a.IdNhanVien IS NOT NULL OR d.IdNhanVienQuanLy=@U OR @Admin=1)
                    -- Thu tu on dinh khi Start/Stop, khong day hang dang chay len dau.
                    ORDER BY v.MaVanDe,v.IdVanDe",con))
            {
                Param(cmd,"@D",SqlDbType.UniqueIdentifier,duAn);
                Param(cmd,"@U",SqlDbType.UniqueIdentifier,nguoiXem);
                Param(cmd,"@Admin",SqlDbType.Bit,laQuanTri);
                using(var r=cmd.ExecuteReader()) while(r.Read())
                    items.Add(new ThongTinGioXuLyVanDe {
                        IdVanDe=r.GetGuid(0),MaVanDe=r.IsDBNull(1)?"":r.GetString(1),
                        TenVanDe=r.IsDBNull(2)?"":r.GetString(2),TrangThai=Convert.ToByte(r.GetValue(3)),
                        TrangThaiDuyetGio=Convert.ToByte(r.GetValue(4)),
                        DaDuocPhanCong=Convert.ToInt32(r.GetValue(5))==1,
                        LaQuanLy=Convert.ToInt32(r.GetValue(6))==1,
                        TongGiayDaLuu=Convert.ToInt64(r.GetValue(7)),TongGiayCuaToi=Convert.ToInt64(r.GetValue(8)),
                        DangChay=Convert.ToInt32(r.GetValue(9))==1,
                        BatDauPhienCuaToiUtc=r.IsDBNull(10)?(DateTime?)null:r.GetDateTime(10),
                        DaTungXuLy=Convert.ToInt32(r.GetValue(11))==1,
                        DaBaoPhanViec=Convert.ToInt32(r.GetValue(12))==1,
                        GiayThuongCuaToi=Convert.ToInt64(r.GetValue(13)),
                        GiayTangCaCuaToi=Convert.ToInt64(r.GetValue(14)),
                        GiayThuongNhom=Convert.ToInt64(r.GetValue(15)),
                        GiayTangCaNhom=Convert.ToInt64(r.GetValue(16)),
                        DangChayTangCa=Convert.ToBoolean(r.GetValue(17)),
                        DaRutBaoGanNhat=Convert.ToInt32(r.GetValue(18))==1,
                        LichSu=new List<LichSuGioXuLyVanDe>()
                    });
            }
            return items;
        }

        public ThongTinGioXuLyVanDe LayThongTin(Guid duAn,Guid vanDe,Guid nguoiXem,bool laQuanTri)
        {
            if(duAn==Guid.Empty || vanDe==Guid.Empty || nguoiXem==Guid.Empty)
                throw new ArgumentException("Thiếu dữ liệu xem giờ xử lý vấn đề.");
            using(SqlConnection con=_repo.MoKetNoi())
            {
                ThongTinGioXuLyVanDe info;
                using(var cmd=new SqlCommand(@"SELECT v.IdVanDe,v.MaVanDe,v.TenVanDe,v.TrangThai,v.TrangThaiDuyetGio,
                         v.IdNguoiGuiDuyetGio,v.NgayGuiDuyetGio,v.LyDoTuChoiGio,
                         CASE WHEN EXISTS(SELECT 1 FROM dbo.TblVanDe_NhanVien a
                                          WHERE a.IdVanDe=v.IdVanDe AND a.IdNhanVien=@U) THEN 1 ELSE 0 END,
                         CASE WHEN @Admin=1 OR d.IdNhanVienQuanLy=@U THEN 1 ELSE 0 END
                         FROM dbo.TblVanDe v JOIN dbo.TblDuAn d ON d.IdDuAn=v.IdDuAn
                         WHERE v.IdVanDe=@V AND v.IdDuAn=@D
                           AND ISNULL(v.DaXoa,0)=0 AND ISNULL(d.DaXoa,0)=0
                           AND (@Admin=1 OR d.IdNhanVienQuanLy=@U OR
                             EXISTS(SELECT 1 FROM dbo.TblVanDe_NhanVien a
                                    WHERE a.IdVanDe=v.IdVanDe AND a.IdNhanVien=@U))",con))
                {
                    Param(cmd,"@D",SqlDbType.UniqueIdentifier,duAn);
                    Param(cmd,"@V",SqlDbType.UniqueIdentifier,vanDe);
                    Param(cmd,"@U",SqlDbType.UniqueIdentifier,nguoiXem);
                    Param(cmd,"@Admin",SqlDbType.Bit,laQuanTri);
                    using(var r=cmd.ExecuteReader())
                    {
                        if(!r.Read()) throw new InvalidOperationException("Không tìm thấy vấn đề trong dự án.");
                        info=new ThongTinGioXuLyVanDe{
                            IdVanDe=r.GetGuid(0),MaVanDe=r.IsDBNull(1)?"":r.GetString(1),
                            TenVanDe=r.IsDBNull(2)?"":r.GetString(2),
                            TrangThai=Convert.ToByte(r.GetValue(3)),
                            TrangThaiDuyetGio=Convert.ToByte(r.GetValue(4)),
                            NguoiGuiDuyet=NullableGuid(r,5),
                            NgayGuiDuyetUtc=r.IsDBNull(6)?(DateTime?)null:r.GetDateTime(6),
                            LyDoTuChoi=r.IsDBNull(7)?null:r.GetString(7),
                            DaDuocPhanCong=Convert.ToInt32(r.GetValue(8))==1,
                            LaQuanLy=Convert.ToInt32(r.GetValue(9))==1,
                            LichSu=new List<LichSuGioXuLyVanDe>()
                        };
                    }
                }
                using(var cmd=new SqlCommand(@"SELECT p.IdPhien,COALESCE(NULLIF(u.UserName,N''),N'Nhân viên') AS NhanVien,
                              p.BatDauLuc,p.KetThucLuc,ISNULL(SUM(k.SoGiay),0) AS SoGiay,p.IdNhanVien,ISNULL(p.LaTangCaVanDe,0)
                     FROM dbo.TblKhaiBaoGioPhien p
                     LEFT JOIN dbo.aspnet_Users u ON u.UserId=p.IdNhanVien
                     LEFT JOIN dbo.TblKhaiBaoGioKhoangTinh k ON k.IdPhien=p.IdPhien
                     WHERE p.IdDuAn=@D AND p.IdVanDe=@V AND p.IdCongViec IS NULL AND p.LoaiGio=3
                     GROUP BY p.IdPhien,u.UserName,p.BatDauLuc,p.KetThucLuc,p.IdNhanVien,p.LaTangCaVanDe
                     ORDER BY p.BatDauLuc DESC,p.IdPhien DESC",con))
                {
                    Param(cmd,"@D",SqlDbType.UniqueIdentifier,duAn);
                    Param(cmd,"@V",SqlDbType.UniqueIdentifier,vanDe);
                    using(var r=cmd.ExecuteReader())
                    {
                        while(r.Read())
                        {
                            var item=new LichSuGioXuLyVanDe{
                                IdPhien=r.GetGuid(0),NhanVien=r.GetString(1),BatDauUtc=r.GetDateTime(2),
                                KetThucUtc=r.IsDBNull(3)?(DateTime?)null:r.GetDateTime(3),
                                SoGiay=Convert.ToInt64(r.GetValue(4)),LaTangCa=Convert.ToBoolean(r.GetValue(6))};
                            Guid nhanVien=r.GetGuid(5);
                            info.LichSu.Add(item);
                            info.TongGiayDaLuu+=item.SoGiay;
                            if(item.LaTangCa)info.GiayTangCaNhom+=item.SoGiay;
                            else info.GiayThuongNhom+=item.SoGiay;
                            if(nhanVien==nguoiXem)
                            {
                                info.DaTungXuLy=true;
                                info.TongGiayCuaToi+=item.SoGiay;
                                if(item.LaTangCa)info.GiayTangCaCuaToi+=item.SoGiay;
                                else info.GiayThuongCuaToi+=item.SoGiay;
                                if(!item.KetThucUtc.HasValue)
                                { info.DangChay=true; info.BatDauPhienCuaToiUtc=item.BatDauUtc; info.DangChayTangCa=item.LaTangCa; }
                            }
                            if(!item.KetThucUtc.HasValue) info.CoPhienMo=true;
                        }
                    }
                }
                // Thông tin báo phần việc được đọc theo bản hiện tại mới nhất.
                using(var cmd=new SqlCommand(@"SELECT TOP 1 LoaiSuKien FROM dbo.TblKhaiBaoGioDoiSoat
                    WHERE IdDuAn=@D AND IdVanDe=@V AND IdNhanVien=@U AND LaBanHienTai=1
                      AND LanDoiSoat=(SELECT CASE WHEN TrangThaiDuyetGio=3 THEN LanGuiDuyetGio+1
                                                 WHEN LanGuiDuyetGio<1 THEN 1 ELSE LanGuiDuyetGio END
                                      FROM dbo.TblVanDe WHERE IdVanDe=@V)
                    ORDER BY LanDoiSoat DESC,PhienBan DESC",con))
                {
                    Param(cmd,"@D",SqlDbType.UniqueIdentifier,duAn);
                    Param(cmd,"@V",SqlDbType.UniqueIdentifier,vanDe);
                    Param(cmd,"@U",SqlDbType.UniqueIdentifier,nguoiXem);
                    object kind=cmd.ExecuteScalar();int value=kind==null||kind==DBNull.Value?0:Convert.ToInt32(kind);
                    info.DaBaoPhanViec=value==1||value==3||value==4||value==6;
                    info.DaRutBaoGanNhat=value==2;
                }
                return info;
            }
        }

        public int GuiHoanThanh(Guid duAn,Guid vanDe,Guid nguoiGui,string ghiChu)
        {
            if(duAn==Guid.Empty || vanDe==Guid.Empty || nguoiGui==Guid.Empty)
                throw new ArgumentException("Thiếu thông tin gửi hoàn thành vấn đề.");
            ghiChu=(ghiChu??string.Empty).Trim();
            if(ghiChu.Length>2000) throw new InvalidOperationException("Nội dung xử lý dài tối đa 2.000 ký tự.");
            using(SqlConnection con=_repo.MoKetNoi())
            using(SqlTransaction tx=con.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                _repo.KhoaDuAn(con,tx,duAn);
                var notifyAfterCommit = new HashSet<Guid>();
                using(var cmd=new SqlCommand(@"SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM dbo.TblVanDe v WITH(UPDLOCK,HOLDLOCK)
                    WHERE v.IdVanDe=@V AND v.IdDuAn=@D AND ISNULL(v.DaXoa,0)=0
                      AND v.TrangThai=0 AND ISNULL(v.TrangThaiDuyetGio,0) IN(0,3)
                      AND EXISTS(SELECT 1 FROM dbo.TblVanDe_NhanVien a WHERE a.IdVanDe=@V AND a.IdNhanVien=@U)
                      AND EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioPhien p
                          WHERE p.IdVanDe=@V AND p.IdDuAn=@D AND p.IdNhanVien=@U AND p.LoaiGio=3)
                      AND EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioDoiSoat s
                          WHERE s.IdVanDe=@V AND s.IdDuAn=@D AND s.IdNhanVien=@U
                            AND s.LaBanHienTai=1 AND s.LoaiSuKien IN(1,3)
                            AND s.LanDoiSoat=(CASE WHEN v.TrangThaiDuyetGio=3 THEN v.LanGuiDuyetGio+1
                                              WHEN v.LanGuiDuyetGio<1 THEN 1 ELSE v.LanGuiDuyetGio END)
                            AND NOT EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioDoiSoat n
                              WHERE n.IdVanDe=s.IdVanDe AND n.IdNhanVien=s.IdNhanVien
                                AND n.LaBanHienTai=1 AND n.LanDoiSoat>s.LanDoiSoat))
                     ) THEN 1 ELSE 0 END",con,tx))
                {
                    Param(cmd,"@D",SqlDbType.UniqueIdentifier,duAn);
                    Param(cmd,"@V",SqlDbType.UniqueIdentifier,vanDe);
                    Param(cmd,"@U",SqlDbType.UniqueIdentifier,nguoiGui);
                    if(Convert.ToInt32(cmd.ExecuteScalar())!=1)
                        throw new InvalidOperationException("Bạn phải báo hoàn thành phần việc của mình trước khi gửi duyệt toàn vấn đề; vấn đề phải đang xử lý.");
                }
                DateTime now=GioUtcHienTai();
                var phienMo=_repo.LayTatCaPhienMo(con,tx,duAn,vanDe);
                int soDong=DongTatCaPhienTrongGiaoDich(con,tx,duAn,vanDe,now);
                using(var cmd=new SqlCommand(@"UPDATE dbo.TblVanDe SET
                         TrangThaiDuyetGio=1,IdNguoiGuiDuyetGio=@U,NgayGuiDuyetGio=@Now,
                         IdNguoiDuyetGio=NULL,NgayDuyetGio=NULL,LyDoTuChoiGio=NULL,
                         LanGuiDuyetGio=LanGuiDuyetGio+1,GhiChuGiaiQuyet=@Note,
                         NgayCapNhat=GETDATE(),NguoiCapNhat=N'GioXuLyVanDe'
                         WHERE IdVanDe=@V AND IdDuAn=@D AND TrangThai=0
                         AND ISNULL(TrangThaiDuyetGio,0) IN(0,3) AND ISNULL(DaXoa,0)=0",con,tx))
                {
                    Param(cmd,"@D",SqlDbType.UniqueIdentifier,duAn);
                    Param(cmd,"@V",SqlDbType.UniqueIdentifier,vanDe);
                    Param(cmd,"@U",SqlDbType.UniqueIdentifier,nguoiGui);
                    Param(cmd,"@Now",SqlDbType.DateTime2,now);
                    Param(cmd,"@Note",SqlDbType.NVarChar,string.IsNullOrEmpty(ghiChu)?(object)DBNull.Value:ghiChu);
                    if(cmd.ExecuteNonQuery()!=1) throw new InvalidOperationException("Vấn đề đã thay đổi trong quá trình gửi duyệt.");
                }
                // CHI khi gui duyet TOAN ISSUE moi thong bao B. Bao phan viec rieng khong dong cham B.
                var recipients=new List<Guid>();
                using(var cmd=new SqlCommand(@"SELECT DISTINCT p.IdNhanVien FROM dbo.TblKhaiBaoGioPhien p
                     WHERE p.IdDuAn=@D AND p.IdVanDe=@V AND p.LoaiGio=3",con,tx))
                {
                    Param(cmd,"@D",SqlDbType.UniqueIdentifier,duAn);
                    Param(cmd,"@V",SqlDbType.UniqueIdentifier,vanDe);
                    using(var r=cmd.ExecuteReader())while(r.Read())recipients.Add(r.GetGuid(0));
                }
                var stoppedUsers=new HashSet<Guid>(phienMo.ConvertAll(p=>p.IdNhanVien));
                foreach(var user in new HashSet<Guid>(recipients))
                {
                    if(user==nguoiGui)continue;
                    bool hasReported=false;
                    using(var cmd=new SqlCommand(@"SELECT TOP 1 LoaiSuKien FROM dbo.TblKhaiBaoGioDoiSoat
                         WHERE IdDuAn=@D AND IdVanDe=@V AND IdNhanVien=@U AND LaBanHienTai=1
                         AND LanDoiSoat=(SELECT LanGuiDuyetGio FROM dbo.TblVanDe WHERE IdVanDe=@V)
                         ORDER BY LanDoiSoat DESC,PhienBan DESC",con,tx))
                    {
                        Param(cmd,"@D",SqlDbType.UniqueIdentifier,duAn);
                        Param(cmd,"@V",SqlDbType.UniqueIdentifier,vanDe);
                        Param(cmd,"@U",SqlDbType.UniqueIdentifier,user);
                        object kind=cmd.ExecuteScalar();
                        int eventId=kind==null||kind==DBNull.Value?0:Convert.ToInt32(kind);
                        hasReported=eventId==1||eventId==3||eventId==4||eventId==6;
                    }
                    bool wasRunning=stoppedUsers.Contains(user);
                    string text=hasReported
                        ? "Một nhân viên đã gửi duyệt hoàn thành vấn đề. Phần việc của bạn đã được báo."
                        : wasRunning
                          ? "Một nhân viên đã gửi duyệt hoàn thành vấn đề. Đồng hồ của bạn đã tự dừng và lưu giờ. Vui lòng báo hoàn thành phần việc."
                          : "Một nhân viên đã gửi duyệt hoàn thành vấn đề. Vui lòng báo hoàn thành phần việc và xác nhận giờ đã thực hiện.";
                    GhiThongBao(con,tx,user,duAn,vanDe,
                        hasReported?"Vấn đề được gửi duyệt hoàn thành":"Yêu cầu báo hoàn thành phần việc vấn đề",
                        text,hasReported?"detail":"report",notifyAfterCommit);
                }
                // Thong bao PM theo cot quan ly du an hien co.
                // Neu nguoi gui cung la PM, chi gui THONG BAO CAN XU LY, khong tao hai notice.
                Guid pmDuAn=Guid.Empty;
                using(var cmd=new SqlCommand(@"SELECT IdNhanVienQuanLy FROM dbo.TblDuAn WHERE IdDuAn=@D",con,tx))
                {
                    Param(cmd,"@D",SqlDbType.UniqueIdentifier,duAn);
                    object pm=cmd.ExecuteScalar();
                    if(pm!=null && pm!=DBNull.Value && (Guid)pm!=Guid.Empty)
                    {
                        pmDuAn=(Guid)pm;
                        GhiThongBao(con,tx,pmDuAn,duAn,vanDe,"Vấn đề đang chờ duyệt hoàn thành",
                            "Một nhân viên đã gửi yêu cầu hoàn thành vấn đề. Vui lòng xét duyệt kết quả xử lý; " +
                            "chốt giờ sau khi các nhân viên đã báo phần việc.","review",notifyAfterCommit);
                    }
                }
                if(nguoiGui!=pmDuAn)
                    GhiThongBao(con,tx,nguoiGui,duAn,vanDe,
                        "Đã gửi yêu cầu duyệt hoàn thành vấn đề",
                        "Yêu cầu hoàn thành toàn vấn đề đã được gửi cho PM. Đồng hồ của nhóm đã dừng và giờ được bảo lưu. " +
                        "Việc gửi duyệt chưa phải PM duyệt hoàn thành hoặc chốt giờ công.",
                        "detail",notifyAfterCommit);
                tx.Commit();
                PhatThongBaoIssueSauCommit(notifyAfterCommit);
                return soDong;
            }
        }

        public void XuLyYeuCau(Guid duAn,Guid vanDe,Guid nguoiDuyet,bool laQuanTri,
                               bool duyet,string lyDo)
        {
            if(duAn==Guid.Empty || vanDe==Guid.Empty || nguoiDuyet==Guid.Empty)
                throw new ArgumentException("Thiếu thông tin duyệt vấn đề.");
            lyDo=(lyDo??"").Trim();
            if(!duyet && string.IsNullOrWhiteSpace(lyDo))
                throw new InvalidOperationException("Bạn cần nhập lý do từ chối.");
            if(lyDo.Length>1000) throw new InvalidOperationException("Lý do dài tối đa 1.000 ký tự.");
            using(SqlConnection con=_repo.MoKetNoi())
            using(SqlTransaction tx=con.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                _repo.KhoaDuAn(con,tx,duAn);
                var notifyAfterCommit = new HashSet<Guid>();
                using(var cmd=new SqlCommand(@"SELECT CASE WHEN EXISTS(
                    SELECT 1 FROM dbo.TblDuAn d
                    INNER JOIN dbo.TblVanDe v WITH(UPDLOCK,HOLDLOCK) ON v.IdDuAn=d.IdDuAn
                    WHERE d.IdDuAn=@D AND v.IdVanDe=@V AND v.TrangThai=0
                    AND ISNULL(v.DaXoa,0)=0 AND v.TrangThaiDuyetGio=1
                    AND NOT EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioPhien p
                        WHERE p.IdVanDe=v.IdVanDe AND p.LoaiGio=3 AND p.KetThucLuc IS NULL)
                    AND (d.IdNhanVienQuanLy=@U OR @Admin=1)
                    ) THEN 1 ELSE 0 END",con,tx))
                {
                    Param(cmd,"@D",SqlDbType.UniqueIdentifier,duAn);
                    Param(cmd,"@V",SqlDbType.UniqueIdentifier,vanDe);
                    Param(cmd,"@U",SqlDbType.UniqueIdentifier,nguoiDuyet);
                    Param(cmd,"@Admin",SqlDbType.Bit,laQuanTri);
                    if(Convert.ToInt32(cmd.ExecuteScalar())!=1)
                        throw new InvalidOperationException("Bạn không có quyền duyệt, hoặc vấn đề không còn chờ duyệt.");
                }
                using(var cmd=new SqlCommand(@"UPDATE dbo.TblVanDe SET
                      TrangThaiDuyetGio=@Status,TrangThai=@IssueStatus,
                      IdNguoiDuyetGio=@U,NgayDuyetGio=@Now,LyDoTuChoiGio=@Reason,
                      NgayCapNhat=GETDATE(),NguoiCapNhat=N'GioXuLyVanDe'
                      WHERE IdVanDe=@V AND IdDuAn=@D AND TrangThai=0 AND TrangThaiDuyetGio=1",con,tx))
                {
                    Param(cmd,"@V",SqlDbType.UniqueIdentifier,vanDe);
                    Param(cmd,"@D",SqlDbType.UniqueIdentifier,duAn);
                    Param(cmd,"@U",SqlDbType.UniqueIdentifier,nguoiDuyet);
                    Param(cmd,"@Now",SqlDbType.DateTime2,GioUtcHienTai());
                    Param(cmd,"@Status",SqlDbType.TinyInt,duyet?(byte)2:(byte)3);
                    Param(cmd,"@IssueStatus",SqlDbType.TinyInt,duyet?(byte)1:(byte)0);
                    Param(cmd,"@Reason",SqlDbType.NVarChar,duyet?(object)DBNull.Value:lyDo);
                    if(cmd.ExecuteNonQuery()!=1) throw new InvalidOperationException("Yêu cầu đã được xử lý ở nơi khác.");
                }
                // Bao cho nhan vien duoc giao hoac da co phien gio (bao gom nguoi gui).
                using(var cmd=new SqlCommand(@"SELECT DISTINCT IdNhanVien FROM dbo.TblVanDe_NhanVien
                       WHERE IdVanDe=@V UNION SELECT DISTINCT IdNhanVien
                       FROM dbo.TblKhaiBaoGioPhien WHERE IdVanDe=@V AND LoaiGio=3",con,tx))
                {
                    Param(cmd,"@V",SqlDbType.UniqueIdentifier,vanDe);
                    var users=new List<Guid>();
                    using(var r=cmd.ExecuteReader()) while(r.Read()) users.Add(r.GetGuid(0));
                    foreach(var user in new HashSet<Guid>(users))
                        GhiThongBao(con,tx,user,duAn,vanDe,
                            duyet?"Vấn đề đã được duyệt hoàn thành":"Yêu cầu hoàn thành vấn đề bị từ chối",
                            duyet?"Quản lý đã xác nhận vấn đề hoàn thành. Các phiên giờ được giữ nguyên để đối soát.":
                                   "Quản lý đã từ chối yêu cầu hoàn thành. Lý do: "+lyDo+". Bạn có thể chủ động bấm Start để tiếp tục xử lý.","detail",notifyAfterCommit);
                }
                // Nhu Task: sau khi PM DUYET, nhac nguoi DA TUNG XU LY nhung CHUA BAO.
                // Nguoi chi duoc phan cong ma chua Start KHONG bi nhac bao gio.
                if(duyet)
                {
                    using(var cmd=new SqlCommand(@"SELECT DISTINCT p.IdNhanVien
                        FROM dbo.TblKhaiBaoGioPhien p
                        WHERE p.IdVanDe=@V AND p.IdDuAn=@D AND p.LoaiGio=3
                          AND NOT EXISTS (
                            SELECT 1 FROM dbo.TblKhaiBaoGioDoiSoat s
                            WHERE s.IdDuAn=@D AND s.IdVanDe=@V AND s.IdNhanVien=p.IdNhanVien
                              AND s.LaBanHienTai=1 AND s.LoaiSuKien IN(1,3,4,6)
                              AND s.LanDoiSoat=(SELECT v.LanGuiDuyetGio
                                 FROM dbo.TblVanDe v WHERE v.IdVanDe=@V AND v.IdDuAn=@D))",con,tx))
                    {
                        Param(cmd,"@D",SqlDbType.UniqueIdentifier,duAn);
                        Param(cmd,"@V",SqlDbType.UniqueIdentifier,vanDe);
                        var canNhac=new List<Guid>();
                        using(var rd=cmd.ExecuteReader())while(rd.Read())canNhac.Add(rd.GetGuid(0));
                        foreach(var user in canNhac.Distinct())
                            GhiThongBao(con,tx,user,duAn,vanDe,
                                "Nhắc xác nhận giờ xử lý vấn đề sau khi PM duyệt",
                                "Vấn đề đã được PM duyệt hoàn thành, nhưng bạn đã có phiên xử lý và chưa báo hoàn thành " +
                                "phần việc của mình. Hãy kiểm tra giờ thường/tăng ca rồi xác nhận báo phần việc " +
                                "để PM có thể chốt giờ công.","report",notifyAfterCommit);
                    }
                }
                tx.Commit();
                PhatThongBaoIssueSauCommit(notifyAfterCommit);
            }
        }

        // Dispatch only after DB COMMIT; this retains atomic INSERT and avoids phantom bell events.
        private static void PhatThongBaoIssueSauCommit(IEnumerable<Guid> users)
        {
            var callback=ThongBaoManager.SauKhiThongBaoThayDoi;
            if(callback==null || users==null)return;
            foreach(var user in users.Distinct())
                try{ callback(user); }
                catch(Exception ex){System.Diagnostics.Trace.TraceError("Issue notification publish after commit: "+ex);}
        }
        private static void GhiThongBao(SqlConnection con,SqlTransaction tx,Guid user,Guid duAn,
                                        Guid vanDe,string title,string body,string target,
                                        ICollection<Guid> notifyAfterCommit)
        {
            if(user==Guid.Empty)return;
            using(var cmd=new SqlCommand(@"INSERT INTO dbo.TblThongBao
                    (IdThongBao,UserId,IdDuAn,IdCongViec,TieuDe,NoiDung,LoaiThongBao,
                     DuongDanLienKet,DaDoc,DaXoa,NguoiTao,NgayTao,NguoiCapNhat,NgayCapNhat)
                    VALUES(NEWID(),@U,@D,NULL,@Title,@Body,N'VAN_DE',@Link,0,0,
                     N'GioXuLyVanDe',GETDATE(),N'GioXuLyVanDe',GETDATE())",con,tx))
            {
                Param(cmd,"@U",SqlDbType.UniqueIdentifier,user);
                Param(cmd,"@D",SqlDbType.UniqueIdentifier,duAn);
                Param(cmd,"@Title",SqlDbType.NVarChar,title!=null&&title.Length>255?title.Substring(0,255):title);
                Param(cmd,"@Body",SqlDbType.NVarChar,body);
                Param(cmd,"@Link",SqlDbType.NVarChar,"GioXuLyVanDe:"+vanDe.ToString("D")+":"+target);
                if(cmd.ExecuteNonQuery()!=1) throw new InvalidOperationException("Không lưu được thông báo giờ xử lý vấn đề.");
                if(notifyAfterCommit!=null)notifyAfterCommit.Add(user);
            }
        }
    }
}
