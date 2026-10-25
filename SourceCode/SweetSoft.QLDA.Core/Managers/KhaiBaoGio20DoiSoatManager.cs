// KhaiBaoGio 2.0 - doi soat gio TU KHAI sau khi PM duyet.
// File moi. KHONG thay doi cac bang/timer V1, khong cap nhat gio he thong.
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using SweetSoft.QLDA.Core.Managers;
using System.Transactions;
using SweetSoft.QLDA.Core.Respositories;

namespace SweetSoft.QLDA.Core.Managers
{
    // LanThucHien thuoc TRUC TIEP Task, khong phu thuoc so phien bam gio
    // va khong doi nghia LanDoiSoat / PhienBan cua tung nhan vien.
    public static class Kbg20LuotThucHien
    {
        public static int LayLanHienTai(SqlConnection c, SqlTransaction tx, Guid project, Guid task)
        {
            using (var q = new SqlCommand(@"SELECT LanThucHien FROM dbo.TblCongViec
                WHERE IdDuAn=@D AND IdCongViec=@T AND ISNULL(DaXoa,0)=0", c, tx))
            {
                q.Parameters.Add("@D", SqlDbType.UniqueIdentifier).Value = project;
                q.Parameters.Add("@T", SqlDbType.UniqueIdentifier).Value = task;
                var result = q.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                    throw new InvalidOperationException("Khong tim thay Task de doc lan thuc hien.");
                int lan = Convert.ToInt32(result);
                if (lan < 1) throw new InvalidOperationException("LanThucHien cua Task khong hop le.");
                return lan;
            }
        }
        // Duoc goi trong TransactionScope da bao quanh task.Save() khi PM tu choi.
        public static void TangLuotTrongTransactionScope(SqlConnection c, Guid project, Guid task)
        {
            if (Transaction.Current == null)
                throw new InvalidOperationException("Tang luot Task phai nam trong transaction chung.");
            using (var q = new SqlCommand(@"UPDATE dbo.TblCongViec
                SET LanThucHien=LanThucHien+1
                WHERE IdDuAn=@D AND IdCongViec=@T AND TrangThai=1
                  AND ISNULL(DaXoa,0)=0 AND LanThucHien>=1 AND LanThucHien<2147483647", c))
            {
                q.Parameters.Add("@D", SqlDbType.UniqueIdentifier).Value = project;
                q.Parameters.Add("@T", SqlDbType.UniqueIdentifier).Value = task;
                if (q.ExecuteNonQuery() != 1)
                    throw new DBConcurrencyException("Khong tang duoc luot thuc hien; da rollback tu choi Task.");
            }
        }
    }

    public sealed class Kbg20BanDoiSoat
    {
        public Guid IdCongViec { get; set; }
        public Guid IdDuAn { get; set; }
        public string MaCongViec { get; set; }
        public string TenCongViec { get; set; }
        public int LanDoiSoat { get; set; }
        public int PhienBan { get; set; }
        public long GiayThuongHeThong { get; set; }
        public long GiayTangCaHeThong { get; set; }
        public int? PhutThuongTuKhai { get; set; }
        public int? PhutTangCaTuKhai { get; set; }
        public string LyDoDieuChinh { get; set; }
        public DateTime ChotGioHeThongLucUtc { get; set; }
        public DateTime NgayTaoUtc { get; set; }
        public bool LaBanHienTai { get; set; }
    }
    public sealed class Kbg20ChiTietDoiSoat
    {
        public Kbg20BanDoiSoat BanHienTai { get; set; }
        public List<Kbg20BanDoiSoat> LichSu { get; set; } = new List<Kbg20BanDoiSoat>();
    }
    public sealed class Kbg20DoiSoatManager
    {
        private static SqlCommand Cmd(SqlConnection c, string sql)
        {
            return new SqlCommand(sql, c) { CommandTimeout = 45 };
        }
        private static void P(SqlCommand cmd, string name, SqlDbType type, object value)
        {
            cmd.Parameters.Add(name, type).Value = value ?? DBNull.Value;
        }
        private static SqlConnection Open()
        {
            var repo = new KhaiBaoGio20Repository();
            var builder = new SqlConnectionStringBuilder(repo.ChuoiKetNoi);
            if (!builder.Enlist) throw new InvalidOperationException("KhaiBaoGio20.TenKetNoi phai co Enlist=true de dam bao rollback dong bo Task.");
            var con = new SqlConnection(builder.ConnectionString);
            con.Open();
            return con;
        }
        private static void ProjectLock(SqlConnection con, Guid projectId)
        {
            using (var cmd = Cmd(con, @"DECLARE @R INT;
                EXEC @R=sp_getapplock @Resource=@Key,@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=15000;
                IF @R<0 THROW 51020,N'Khong lay duoc khoa du an KBG20.',1;"))
            {
                P(cmd,"@Key",SqlDbType.NVarChar,"KBG20:DUAN:"+projectId.ToString("N"));
                cmd.ExecuteNonQuery();
            }
        }
        private static void EnsureEligible(SqlConnection c, Guid nv, Guid duAn, Guid cv)
        {
            using (var cmd = Cmd(c, @"SELECT TOP 1 1 FROM dbo.TblCongViec t WITH(HOLDLOCK)
                WHERE t.IdCongViec=@T AND t.IdDuAn=@D AND t.TrangThai IN (2,3)
                  AND (EXISTS (SELECT 1 FROM dbo.TblCongViec_NhanVien a
                    WHERE a.IdCongViec=t.IdCongViec AND a.IdNhanVien=@U)
                  OR EXISTS (SELECT 1 FROM dbo.TblKhaiBaoGioPhien p
                    WHERE p.IdCongViec=t.IdCongViec AND p.IdNhanVien=@U)
                  OR EXISTS (SELECT 1 FROM dbo.TblKhaiBaoGioDoiSoat s
                    WHERE s.IdCongViec=t.IdCongViec AND s.IdNhanVien=@U))"))
            {
                P(cmd,"@T",SqlDbType.UniqueIdentifier,cv);
                P(cmd,"@D",SqlDbType.UniqueIdentifier,duAn);
                P(cmd,"@U",SqlDbType.UniqueIdentifier,nv);
                if (cmd.ExecuteScalar() == null)
                    throw new UnauthorizedAccessException("Ban khong co quyen doi soat task nay hoac task chua duoc PM duyet.");
            }
        }
        private static Kbg20BanDoiSoat Read(IDataRecord r)
        {
            return new Kbg20BanDoiSoat {
                IdCongViec=r.GetGuid(0), IdDuAn=r.GetGuid(1),
                MaCongViec=r.IsDBNull(2)?"":Convert.ToString(r.GetValue(2)),
                TenCongViec=r.IsDBNull(3)?"":Convert.ToString(r.GetValue(3)),
                LanDoiSoat=r.GetInt32(4), PhienBan=r.GetInt32(5),
                GiayThuongHeThong=r.GetInt64(6), GiayTangCaHeThong=r.GetInt64(7),
                PhutThuongTuKhai=r.IsDBNull(8)?(int?)null:r.GetInt32(8),
                PhutTangCaTuKhai=r.IsDBNull(9)?(int?)null:r.GetInt32(9),
                LyDoDieuChinh=r.IsDBNull(10)?null:r.GetString(10),
                ChotGioHeThongLucUtc=DateTime.SpecifyKind(r.GetDateTime(11),DateTimeKind.Utc),
                NgayTaoUtc=DateTime.SpecifyKind(r.GetDateTime(12),DateTimeKind.Utc),
                LaBanHienTai=r.GetBoolean(13)
            };
        }
        private const string SelectReconcile = @"SELECT s.IdCongViec,s.IdDuAn,t.MaCongViec,t.TenCongViec,
                    s.LanDoiSoat,s.PhienBan,s.SoGiayThuongHeThong,s.SoGiayTangCaHeThong,
                    s.SoPhutThuongTuKhai,s.SoPhutTangCaTuKhai,s.LyDoDieuChinh,s.ChotGioHeThongLuc,
                    s.NgayTao,s.LaBanHienTai
                    FROM dbo.TblKhaiBaoGioDoiSoat s
                    INNER JOIN dbo.TblCongViec t ON t.IdCongViec=s.IdCongViec";

        public List<Kbg20BanDoiSoat> LayDanhSachDoiSoat(Guid nv, Guid project)
        {
            var result = new List<Kbg20BanDoiSoat>();
            using (var c=Open()) using (var cmd=Cmd(c, SelectReconcile+@"
                WHERE s.IdNhanVien=@U AND s.IdDuAn=@D AND s.LaBanHienTai=1 AND s.LoaiSuKien=0
                  AND s.LanDoiSoat=(SELECT MAX(x.LanDoiSoat) FROM dbo.TblKhaiBaoGioDoiSoat x
                    WHERE x.IdNhanVien=s.IdNhanVien AND x.IdCongViec=s.IdCongViec AND x.LoaiSuKien=0)
                ORDER BY s.NgayTao DESC"))
            {
                P(cmd,"@U",SqlDbType.UniqueIdentifier,nv);
                P(cmd,"@D",SqlDbType.UniqueIdentifier,project);
                using(var r=cmd.ExecuteReader()) while(r.Read()) result.Add(Read(r));
            }
            return result;
        }
        public Kbg20ChiTietDoiSoat LayChiTiet(Guid nv, Guid project, Guid task)
        {
            var result = new Kbg20ChiTietDoiSoat();
            using (var c=Open())
            {
                EnsureEligible(c,nv,project,task);
                using(var cmd=Cmd(c, SelectReconcile+@"
                    WHERE s.IdNhanVien=@U AND s.IdDuAn=@D AND s.IdCongViec=@T AND s.LoaiSuKien=0
                    ORDER BY s.LanDoiSoat DESC,s.PhienBan DESC"))
                {
                    P(cmd,"@U",SqlDbType.UniqueIdentifier,nv);
                    P(cmd,"@D",SqlDbType.UniqueIdentifier,project);
                    P(cmd,"@T",SqlDbType.UniqueIdentifier,task);
                    using(var r=cmd.ExecuteReader()) while(r.Read()) result.LichSu.Add(Read(r));
                }
                result.BanHienTai=result.LichSu.FirstOrDefault(x=>x.LaBanHienTai);
            }
            return result;
        }

        // Chi PM duyet hoan thanh moi goi. Phai nam TRONG TransactionScope cua CtrlFastComplete/Review.
        // Session bai ghi gio da duoc chot truoc khi goi ham nay.
        public void TaoDotSauKhiPmDuyetTrongTransactionScope(Guid project, Guid task, DateTime nowUtc)
        {
            if(Transaction.Current==null) throw new InvalidOperationException("Tao doi soat yeu cau transaction chung Task + KBG20.");
            using(var c=Open())
            {
                ProjectLock(c,project);
                using(var check=Cmd(c,@"SELECT 1 FROM dbo.TblCongViec WITH(HOLDLOCK)
                    WHERE IdCongViec=@T AND IdDuAn=@D AND TrangThai IN (2,3) AND ISNULL(DaXoa,0)=0"))
                {
                    P(check,"@T",SqlDbType.UniqueIdentifier,task);
                    P(check,"@D",SqlDbType.UniqueIdentifier,project);
                    if(check.ExecuteScalar()==null) throw new InvalidOperationException("Task chua duoc PM xac nhan hoan thanh.");
                }
                using(var cmd=Cmd(c,@";WITH U AS (
                      SELECT DISTINCT IdNhanVien FROM dbo.TblCongViec_NhanVien WHERE IdCongViec=@T
                      UNION SELECT DISTINCT IdNhanVien FROM dbo.TblKhaiBaoGioPhien WHERE IdCongViec=@T
                  ), S AS (
                      SELECT p.IdNhanVien,
                      ISNULL(SUM(CASE WHEN p.LoaiGio=1 THEN k.SoGiay ELSE 0 END),0) Thuong,
                      ISNULL(SUM(CASE WHEN p.LoaiGio=2 THEN k.SoGiay ELSE 0 END),0) TangCa
                      FROM dbo.TblKhaiBaoGioPhien p
                      LEFT JOIN dbo.TblKhaiBaoGioKhoangTinh k ON k.IdPhien=p.IdPhien
                      WHERE p.IdCongViec=@T GROUP BY p.IdNhanVien
                  )
                  INSERT dbo.TblKhaiBaoGioDoiSoat
                    (IdDoiSoat,IdNhanVien,IdCongViec,IdDuAn,LanDoiSoat,PhienBan,LaBanHienTai,
                     SoGiayThuongHeThong,SoGiayTangCaHeThong,SoPhutThuongTuKhai,SoPhutTangCaTuKhai,
                     ChotGioHeThongLuc,NguoiTao,LanThucHien)
                  SELECT NEWID(),u.IdNhanVien,@T,@D,
                     ISNULL((SELECT MAX(x.LanDoiSoat) FROM dbo.TblKhaiBaoGioDoiSoat x WITH(UPDLOCK,HOLDLOCK)
                              WHERE x.IdNhanVien=u.IdNhanVien AND x.IdCongViec=@T),0)+1,
                     1,1,ISNULL(s.Thuong,0),ISNULL(s.TangCa,0),NULL,NULL,@At,N'KBG20_PM_DUYET',
                     (SELECT LanThucHien FROM dbo.TblCongViec WHERE IdDuAn=@D AND IdCongViec=@T)
                  FROM U u LEFT JOIN S s ON s.IdNhanVien=u.IdNhanVien"))
                {
                    P(cmd,"@T",SqlDbType.UniqueIdentifier,task);
                    P(cmd,"@D",SqlDbType.UniqueIdentifier,project);
                    P(cmd,"@At",SqlDbType.DateTime2,nowUtc);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Chi nhan vien tu luu cho minh, khong co tham so id nhan vien tu browser.
        public int LuuTuKhai(Guid nv,Guid project,Guid task,int phutThuong,int phutTangCa,string lyDo,int expectedVersion)
        {
            if(phutThuong<0 || phutTangCa<0 || phutThuong>599999 || phutTangCa>599999)
                throw new ArgumentOutOfRangeException("phutThuong", "So phut phai hop le va khong am.");
            if(lyDo!=null && lyDo.Length>2000) throw new ArgumentException("Ly do dieu chinh qua 2000 ky tu.");
            using(var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel=System.Transactions.IsolationLevel.ReadCommitted,
                    Timeout=TimeSpan.FromMinutes(2) }))
            using(var c=Open())
            {
                ProjectLock(c,project);
                EnsureEligible(c,nv,project,task);
                // DOT 8: duong sua doi soat legacy cung phai bi khoa sau su kien PM chot (Loai=4).
                // Khong sua data cu: chi chan thao tac moi khi PM da chot cong.
                using(var lockPm=Cmd(c,@"SELECT TOP (1) 1 FROM dbo.TblKhaiBaoGioDoiSoat WITH(UPDLOCK,HOLDLOCK)
                    WHERE IdNhanVien=@U AND IdDuAn=@D AND IdCongViec=@T AND LoaiSuKien=4"))
                {
                    P(lockPm,"@U",SqlDbType.UniqueIdentifier,nv);
                    P(lockPm,"@D",SqlDbType.UniqueIdentifier,project);
                    P(lockPm,"@T",SqlDbType.UniqueIdentifier,task);
                    if(lockPm.ExecuteScalar()!=null)
                        throw new InvalidOperationException("Gio cong da duoc PM duyet, khong duoc sua gio tu khai.");
                }
                Guid id; int lan; int version; long regular;long ot; DateTime snap;
                using(var cmd=Cmd(c,@"SELECT TOP(1) IdDoiSoat,LanDoiSoat,PhienBan,
                         SoGiayThuongHeThong,SoGiayTangCaHeThong,ChotGioHeThongLuc
                      FROM dbo.TblKhaiBaoGioDoiSoat WITH(UPDLOCK,HOLDLOCK)
                      WHERE IdNhanVien=@U AND IdDuAn=@D AND IdCongViec=@T AND LaBanHienTai=1 AND LoaiSuKien=0
                      ORDER BY LanDoiSoat DESC,PhienBan DESC"))
                {
                    P(cmd,"@U",SqlDbType.UniqueIdentifier,nv);
                    P(cmd,"@D",SqlDbType.UniqueIdentifier,project);
                    P(cmd,"@T",SqlDbType.UniqueIdentifier,task);
                    using(var r=cmd.ExecuteReader())
                    {
                        if(!r.Read()) throw new InvalidOperationException("Chua co dot doi soat duoc PM duyet cho nhan vien nay.");
                        id=r.GetGuid(0);lan=r.GetInt32(1);version=r.GetInt32(2);
                        regular=r.GetInt64(3);ot=r.GetInt64(4);snap=r.GetDateTime(5);
                    }
                }
                if(version!=expectedVersion) throw new DBConcurrencyException("Ban doi soat da duoc cap nhat. Vui long tai lai truoc khi luu.");
                using(var cmd=Cmd(c,@"UPDATE dbo.TblKhaiBaoGioDoiSoat SET LaBanHienTai=0
                    WHERE IdDoiSoat=@I AND LaBanHienTai=1 AND PhienBan=@V"))
                {
                    P(cmd,"@I",SqlDbType.UniqueIdentifier,id);P(cmd,"@V",SqlDbType.Int,version);
                    if(cmd.ExecuteNonQuery()!=1) throw new DBConcurrencyException("Ban doi soat da thay doi.");
                }
                using(var cmd=Cmd(c,@"INSERT dbo.TblKhaiBaoGioDoiSoat
                    (IdDoiSoat,IdNhanVien,IdCongViec,IdDuAn,LanDoiSoat,PhienBan,LaBanHienTai,
                     SoGiayThuongHeThong,SoGiayTangCaHeThong,SoPhutThuongTuKhai,SoPhutTangCaTuKhai,
                     LyDoDieuChinh,ChotGioHeThongLuc,NguoiTao,LanThucHien)
                    VALUES (NEWID(),@U,@T,@D,@L,@V,1,@R,@O,@SR,@SO,@Reason,@At,@User,
                     (SELECT LanThucHien FROM dbo.TblCongViec WHERE IdDuAn=@D AND IdCongViec=@T))"))
                {
                    P(cmd,"@U",SqlDbType.UniqueIdentifier,nv);P(cmd,"@T",SqlDbType.UniqueIdentifier,task);
                    P(cmd,"@D",SqlDbType.UniqueIdentifier,project);P(cmd,"@L",SqlDbType.Int,lan);
                    P(cmd,"@V",SqlDbType.Int,version+1);P(cmd,"@R",SqlDbType.BigInt,regular);
                    P(cmd,"@O",SqlDbType.BigInt,ot);P(cmd,"@SR",SqlDbType.Int,phutThuong);
                    P(cmd,"@SO",SqlDbType.Int,phutTangCa);P(cmd,"@Reason",SqlDbType.NVarChar,lyDo);
                    P(cmd,"@At",SqlDbType.DateTime2,snap);P(cmd,"@User",SqlDbType.NVarChar,nv.ToString("D"));
                    cmd.ExecuteNonQuery();
                }
                scope.Complete();return version+1;
            }
        }
    }
}

namespace SweetSoft.QLDA.Core.Managers
{
    // Lop dieu phoi TUY CHON; khong dua nghiep vu tinh trang Task vao KBG20.
    // Action luuTask la toan bo doan .Save + AutoSet* NGUYEN VAN cua partner.
    public static class Kbg20TichHopTask
    {
        public static bool DuocKichHoat {
            get { return KhaiBaoGioManager.KhaiBaoGio20DaKichHoat() &&
                string.Equals(ConfigurationManager.AppSettings["KhaiBaoGio20.DongBoTask"],"true",StringComparison.OrdinalIgnoreCase); }
        }
        // DOT 7B.2: Doc trang thai Task tu SQL ngay trong transaction chung.
        // Khong tin ViewState / object SubSonic da load tu truoc, de tranh tab cu
        // gui lai/PM duyet lai va ghi de TrangThai khi du lieu da thay doi.
        private static int LayTrangThaiTaskTrongGiaoDich(SqlConnection con, Guid duAn, Guid task)
        {
            using (var cmd = new SqlCommand(@"SELECT TrangThai FROM dbo.TblCongViec WITH (UPDLOCK,HOLDLOCK)
                WHERE IdDuAn=@D AND IdCongViec=@T AND ISNULL(DaXoa,0)=0", con))
            {
                cmd.CommandTimeout = 45;
                cmd.Parameters.Add("@D", SqlDbType.UniqueIdentifier).Value = duAn;
                cmd.Parameters.Add("@T", SqlDbType.UniqueIdentifier).Value = task;
                var value = cmd.ExecuteScalar();
                if (value == null || value == DBNull.Value)
                    throw new InvalidOperationException("Cong viec khong ton tai, da bi xoa hoac khong thuoc du an.");
                return Convert.ToInt32(value);
            }
        }

        // Giu NGUYEN hai chu ky public cua partner. Chi them bao ve tai Core.
        public static void LuuHoanThanh(Guid duAn,Guid task,bool pmXacNhan,Action luuTask)
        {
            LuuHoanThanhNoiBo(duAn,task,pmXacNhan,false,luuTask);
        }
        public static void DuyetHoanThanh(Guid duAn,Guid task,Action luuTask)
        {
            LuuHoanThanhNoiBo(duAn,task,true,true,luuTask);
        }
        // Partner phai goi gateway nay thay vi task.Save() don le khi PM TU CHOI.
        public static void TuChoiHoanThanh(Guid duAn, Guid task, Action luuTask)
        {
            if (luuTask == null) throw new ArgumentNullException("luuTask");
            if (!DuocKichHoat) { luuTask(); return; }
            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                    Timeout = TimeSpan.FromMinutes(2) }))
            {
                var repo = new KhaiBaoGio20Repository();
                using (var con = repo.MoKetNoi())
                {
                    if (LayTrangThaiTaskTrongGiaoDich(con, duAn, task) != 4)
                        throw new InvalidOperationException("Task khong con o trang thai cho duyet.");
                    luuTask();
                    if (LayTrangThaiTaskTrongGiaoDich(con, duAn, task) != 1)
                        throw new InvalidOperationException("Tu choi Task khong chuyen ve Dang thuc hien.");
                    Kbg20LuotThucHien.TangLuotTrongTransactionScope(con, duAn, task);
                }
                scope.Complete();
            }
        }
        private static void LuuHoanThanhNoiBo(Guid duAn,Guid task,bool pmXacNhan,
            bool laDuyetTuChoDuyet, Action luuTask)
        {
            if (luuTask == null) throw new ArgumentNullException("luuTask");
            if (duAn == Guid.Empty || task == Guid.Empty)
                throw new ArgumentException("Thieu du an hoac cong viec can hoan thanh.");
            // Feature tat: giu NGUYEN duong TaskList hien huu, khong can thiep nghiep vu cu.
            if (!DuocKichHoat) { luuTask(); return; }
            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                    Timeout = TimeSpan.FromMinutes(2) }))
            {
                // Nguoi GUI YEU CAU hoan thanh buoc phai tu bao hoan thanh phan viec.
                // PM DUYET yeu cau da gui la mot duong khac (laDuyetTuChoDuyet=true).
                if (!laDuyetTuChoDuyet)
                {
                    var currentUser = SweetSoft.QLDA.Core.Infrastructure.SweetContext.Current;
                    Guid sender = currentUser != null ? currentUser.UserId : Guid.Empty;
                    if (sender == Guid.Empty)
                        throw new UnauthorizedAccessException("Khong xac dinh duoc nguoi gui yeu cau hoan thanh.");
                    if (!TaskManager.Instance.GetAssignedNhanVienIds(task).Contains(sender))
                        throw new UnauthorizedAccessException("Ban chua duoc phan cong truc tiep tren Task nay.");
                    var currentHours = new Kbg20PmChotCongTaskManager().LayBanDuyet(duAn, task);
                    var mine = currentHours.NhanVien.FirstOrDefault(x=>x.IdNhanVien==sender);
                    if (mine == null || !mine.DaXacNhanGio)
                        throw new InvalidOperationException("Ban phai Bao hoan thanh phan viec va khai gio hop le truoc khi gui yeu cau PM duyet.");
                }
                var clock = new KhaiBaoGio20Manager();
                // AppLock cua KBG20 + dong phien truc tiep task, TAT CA trong ambient transaction.
                // Neu check hoac callback that bai, SQL rollback ca dong phien lan Task.
                clock.ChotTatCaPhienTaskTrongTransactionScope(duAn, task);
                var repo = new KhaiBaoGio20Repository();
                using (var con = repo.MoKetNoi())
                {
                    int truoc = LayTrangThaiTaskTrongGiaoDich(con, duAn, task);
                    if (laDuyetTuChoDuyet)
                    {
                        if (truoc != 4)
                            throw new InvalidOperationException("Cong viec khong con o trang thai Cho duyet. Vui long tai lai.");
                    }
                    else if (truoc != 0 && truoc != 1)
                    {
                        throw new InvalidOperationException("Cong viec da thay doi trang thai, khong the gui/hoan thanh lai. Vui long tai lai.");
                    }

                    // Giu nguyen tat ca validation, task.Save() va AutoSet* cua partner.
                    luuTask();

                    int sau = LayTrangThaiTaskTrongGiaoDich(con, duAn, task);
                    if (pmXacNhan ? (sau != 2 && sau != 3) : sau != 4)
                        throw new InvalidOperationException("Luu trang thai cong viec khong dung quy trinh. Da huy thao tac va viec chot gio.");
                }
                // Event 0 (Task hoan thanh) khong phai event 4 PM CHOT CONG.
                if (pmXacNhan)
                    new Kbg20DoiSoatManager().TaoDotSauKhiPmDuyetTrongTransactionScope(
                        duAn, task, DateTime.UtcNow);
                scope.Complete();
            }
            {
                try { PhatNhacXacNhanGioSauPmDuyet(duAn,task); }
                catch(Exception ex) { System.Diagnostics.Trace.TraceError("KBG20 sau PM duyet nhac xac nhan gio: "+ex); }
            }
        }
        // Sau COMMIT: thong bao toi tat ca nhan vien duoc giao task truc tiep (1 thong bao/nguoi).
        // Khong dong timer cua task cha/con, khong gui email moi.
        public static void PhatThongBaoNhom(Guid duAn,Guid taskId,Guid nguoiThucHien,bool pmXacNhan)
        {
            if(!DuocKichHoat) return;
            var task=TaskManager.Instance.FetchById(taskId);
            if(task==null || task.IdDuAn!=duAn) return;
            var nguoi=SweetSoft.QLDA.DataAccess.AspnetUser.FetchByID(nguoiThucHien);
            string tenNguoi=nguoi!=null && !string.IsNullOrWhiteSpace(nguoi.DisplayName)
                ? nguoi.DisplayName : "Một thành viên";
            var idPm=DuAnManager.Instance.LayIdNhanVienQuanLy(duAn);
            var pm=idPm.HasValue ? SweetSoft.QLDA.DataAccess.AspnetUser.FetchByID(idPm.Value) : null;
            string tenPm=pm!=null && !string.IsNullOrWhiteSpace(pm.DisplayName) ? pm.DisplayName : "PM dự án";
            string nhanTask="["+task.MaCongViec+"] "+task.TenCongViec;
            foreach(Guid nv in TaskManager.Instance.GetAssignedNhanVienIds(taskId).Distinct())
            {
                if(nv==Guid.Empty)continue;
                try {
                    string title=pmXacNhan ? "Công việc đã hoàn thành: "+nhanTask
                        : "Công việc đã được gửi duyệt: "+nhanTask;
                    string desc=pmXacNhan
                        ? ("PM " + tenNguoi + " đã xác nhận hoàn thành " + nhanTask +
                           ". Đồng hồ trực tiếp trên công việc của nhóm đã được chốt. " +
                           "Nếu chưa báo hoàn thành phần việc, nhấn vào thông báo nhắc để mở biểu mẫu Báo hoàn thành; " +
                           "sau đó PM mới chốt giờ công chính thức.")
                        : ((nv==nguoiThucHien ? "Bạn" : tenNguoi) + " đã gửi yêu cầu duyệt hoàn thành " + nhanTask +
                           " đến " + tenPm + ". Đồng hồ trực tiếp trên công việc của nhóm đã được chốt; " +
                           "giờ đã ghi được bảo lưu. Nếu PM từ chối, bạn có thể tự Start lại.");
                    // Duong dan noi bo toi trang Tinh gio; PM hoan thanh truc tiep mo doi soat luon.
                    string basePath=(System.Web.HttpRuntime.AppDomainAppVirtualPath ?? "/").TrimEnd('/');
                    string link=basePath+"/fKhaiBaoGio/KhaiBaoGio.aspx?ProjectId="+duAn.ToString("D");
                    if(pmXacNhan) link+="&reconcile=1&taskId="+taskId.ToString("D");
                    var thongBao = ThongBaoManager.Instance.Create(userId:nv,tieuDe:title,noiDung:desc,
                        loaiThongBao:ThongBaoTypes.CongViec,
                        idCongViec:taskId,idDuAn:duAn,duongDanLienKet:link,guiEmail:false);
                    if (thongBao == null)
                        throw new InvalidOperationException("Khong tao duoc thong bao nhom sau commit.");
                } catch(Exception ex) {
                    try { SweetSoft.QLDA.Core.SysManager.SysLogger.LogError(ex,
                        "KBG20 thong bao nhom task=" + taskId + ", nv=" + nv); }
                    catch { System.Diagnostics.Trace.TraceError("KBG20 thong bao nhom task " + taskId + ": " + ex); }
                }
            }
        }
        // Duoc goi CHI SAU COMMIT gateway duyet Task (ke ca PM nhan truc tiep).
        // Khong phu thuoc popup partner hay trang duoc mo tu chuong thong bao.
        // Kết quả gửi để UI không báo thành công giả khi không có bản ghi thông báo.
        public sealed class KetQuaGuiNhacBao
        {
            public int CanNhac { get; set; }
            public int DaGui { get; set; }
            public int BoQuaTrung { get; set; }
            public int ThatBai { get; set; }
        }

        // Luồng tự động: gửi NGAY khi được gọi; 60 phút chỉ ngăn một thông báo trùng.
        // Giữ chữ ký cũ để không ảnh hưởng các control đã gọi từ trước.
        public static void PhatNhacXacNhanGioSauPmDuyet(Guid duAn, Guid taskId)
        {
            GuiNhacBaoHoanThanhNoiBo(duAn, taskId, false);
        }

        // PM bấm Gửi nhắc nhở: gửi NGAY, không dùng cooldown của luồng tự động.
        // Caller phải xác minh quyền PM/administrator và đúng project ở server.
        public static KetQuaGuiNhacBao GuiNhacBaoHoanThanhThuCong(Guid duAn, Guid taskId)
        {
            return GuiNhacBaoHoanThanhNoiBo(duAn, taskId, true);
        }

        private static KetQuaGuiNhacBao GuiNhacBaoHoanThanhNoiBo(Guid duAn, Guid taskId, bool thuCong)
        {
            var ketQua = new KetQuaGuiNhacBao();
            if (!DuocKichHoat)
            {
                if (thuCong) throw new InvalidOperationException("Chức năng KhaiBaoGio 2.0 chưa được kích hoạt.");
                return ketQua;
            }
            var task = TaskManager.Instance.FetchById(taskId);
            if (task == null || task.IdDuAn != duAn ||
                (task.TrangThai != 2 && task.TrangThai != 3 && task.TrangThai != 4))
            {
                if (thuCong) throw new InvalidOperationException("Task không thuộc dự án hoặc không ở trạng thái Chờ duyệt/Hoàn thành.");
                return ketQua;
            }
            var missing = new Kbg20BaoXongManager().LayNhanVienChuaXacNhan(duAn, taskId, false);
            ketQua.CanNhac = missing.Count;
            string taskName = "[" + task.MaCongViec + "] " + task.TenCongViec;
            foreach (var person in missing)
            {
                try
                {
                    var basePath = (System.Web.HttpRuntime.AppDomainAppVirtualPath ?? "/").TrimEnd('/');
                    var link = basePath + "/fKhaiBaoGio/KhaiBaoGio.aspx?ProjectId=" + duAn.ToString("D") +
                        "&reportTaskId=" + taskId.ToString("D");
                    // Chỉ tự động mới có giới hạn 60 phút. PM nhấn nút là yêu cầu gửi lại ngay.
                    if (!thuCong)
                    {
                        using (var checkC = new KhaiBaoGio20Repository().MoKetNoi())
                        using (var dupe = new SqlCommand(@"SELECT TOP 1 1 FROM dbo.TblThongBao
                            WHERE UserId=@U AND IdCongViec=@T AND IdDuAn=@D AND DuongDanLienKet=@Link
                              AND DaXoa=0 AND NgayTao>=DATEADD(MINUTE,-60,GETDATE())", checkC))
                        {
                            dupe.Parameters.Add("@U", SqlDbType.UniqueIdentifier).Value = person.IdNhanVien;
                            dupe.Parameters.Add("@T", SqlDbType.UniqueIdentifier).Value = taskId;
                            dupe.Parameters.Add("@D", SqlDbType.UniqueIdentifier).Value = duAn;
                            dupe.Parameters.Add("@Link", SqlDbType.NVarChar, 2048).Value = link;
                            if (dupe.ExecuteScalar() != null)
                            {
                                ketQua.BoQuaTrung++;
                                continue;
                            }
                        }
                    }
                    var n = ThongBaoManager.Instance.Create(userId:person.IdNhanVien,
                        tieuDe:"Bạn chưa báo hoàn thành phần việc: " + taskName,
                        noiDung:(task.TrangThai == 4 ? "Công việc " + taskName + " đang chờ duyệt hoàn thành" :
                            "Công việc " + taskName + " đã được PM duyệt hoàn thành") +
                            ", nhưng bạn chưa báo hoàn thành phần việc của mình. " +
                            "Nhấn thông báo để mở biểu mẫu Báo hoàn thành phần việc và khai báo giờ công. " +
                            "PM chỉ có thể chốt giờ công tập thể sau khi toàn bộ nhân viên đã báo.",
                        loaiThongBao:ThongBaoTypes.CongViec, idCongViec:taskId, idDuAn:duAn,
                        duongDanLienKet:link, guiEmail:false);
                    if (n == null) throw new InvalidOperationException("Không tạo được thông báo nhắc hoàn thành phần việc.");
                    ketQua.DaGui++;
                }
                catch (Exception ex)
                {
                    ketQua.ThatBai++;
                    System.Diagnostics.Trace.TraceError("KBG20 gửi nhắc hoàn thành Task {0}, nhân viên {1}: {2}",
                        taskId, person.IdNhanVien, ex);
                }
            }
            return ketQua;
        }
        public static void StartTaskVaGio(Guid duAn,Guid task,Guid nhanVien,Guid thaoTac,
            bool quaHan,bool phuThuoc,bool laTangCa,Action luuTask)
        {
            if(luuTask==null) throw new ArgumentNullException("luuTask");
            if(!DuocKichHoat) throw new InvalidOperationException("Chuc nang dong bo Task va KBG20 chua duoc bat tren TEST.");
            using(var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel=System.Transactions.IsolationLevel.ReadCommitted,
                    Timeout=TimeSpan.FromMinutes(2) }))
            {
                var clock=new KhaiBaoGio20Manager();
                using(clock.MoKhoaDuAnTrongTransactionScope(duAn))
                {
                    // Check lai ngay TRONG transaction, khong tin trang thai tren popup / ViewState.
                    var current=TaskManager.Instance.FetchById(task);
                    if(current==null || current.IdDuAn!=duAn || current.DaXoa==true || current.TrangThai!=0)
                        throw new InvalidOperationException("Task da thay doi trang thai trong khi mo popup. Vui long tai lai.");
                    // Cả giờ theo lịch và tăng ca đều xét Start sớm tại Manager trong
                    // cùng transaction; không chặn tăng ca theo DateTime.Today của IIS.
                    if(!TaskManager.Instance.GetAssignedNhanVienIds(task).Contains(nhanVien))
                        throw new UnauthorizedAccessException("Ban khong con duoc phan cong truc tiep vao task.");
                    luuTask(); // Partner CtrlStartTask: 0->1, update cha nhu TaskList.
                    if(laTangCa)
                        clock.BatDauTangCaTrongTransactionScope(nhanVien,duAn,task,thaoTac);
                    else
                        clock.BatDauGioThuongTrongTransactionScope(nhanVien,duAn,task,thaoTac,quaHan,phuThuoc);
                    scope.Complete();
                }
            }
        }
    }
}

// DOT 2: luong BAO XONG CA NHAN. Giu nguyen Kbg20DoiSoatManager legacy phuc vu TaskList.
// Su dung 5 cot DOT1 V2: LoaiSuKien=1 bao,2 rut,3 sua,4 PM chot (dot sau).
namespace SweetSoft.QLDA.Core.Managers
{
    public sealed class Kbg20BaoXongBanGhi
    {
        public Guid IdCongViec { get; set; }
        public string MaCongViec { get; set; }
        public string TenCongViec { get; set; }
        public int LoaiSuKien { get; set; }
        public int Lan { get; set; }
        public int PhienBan { get; set; }
        public int LanThucHien { get; set; }
        public long GiayThuong { get; set; }
        public long GiayTangCa { get; set; }
        public int? PhutTuKhaiThuong { get; set; }
        public int? PhutTuKhaiTangCa { get; set; }
        public string LyDo { get; set; }
        public DateTime NgayTaoUtc { get; set; }
        public bool DaPmChot { get; set; }
        // Event 3 co the la sua cua event 1 HOAC event 5; khong duoc danh dong.
        public bool CoGocXacNhanSauTask { get; set; }
        public bool DangBaoXong { get { return LoaiSuKien==1 || LoaiSuKien==3 || LoaiSuKien==5; } }
        public bool DaBaoXongPhanViec { get { return LoaiSuKien==1 || LoaiSuKien==3 || LoaiSuKien==5; } }
    }
    public sealed class Kbg20BaoXongChiTiet
    {
        public Kbg20BaoXongBanGhi HienTai { get; set; }
        public List<Kbg20BaoXongBanGhi> LichSu { get; set; } = new List<Kbg20BaoXongBanGhi>();
        public long GiayThuongHeThong { get; set; }
        public long GiayTangCaHeThong { get; set; }
        public bool ConPhanCong { get; set; }
        public bool KhoaPm { get; set; }
        public int TrangThaiTask { get; set; }
    }
    public sealed class Kbg20ViecChoXacNhanGio
    {
        public Guid IdCongViec { get; set; }
        public string MaCongViec { get; set; }
        public string TenCongViec { get; set; }
        public int TrangThai { get; set; }
        public long GiayHanhChinh { get; set; }
        public long GiayTangCa { get; set; }
    }
    public sealed class Kbg20NhanVienChuaXacNhan
    {
        public Guid IdNhanVien { get; set; }
        public string TenNhanVien { get; set; }
    }
    public sealed class Kbg20BaoXongManager
    {
        private readonly KhaiBaoGio20Repository repo=new KhaiBaoGio20Repository();
        private static SqlCommand Cmd(SqlConnection c,SqlTransaction tx,string sql)
        { return new SqlCommand(sql,c,tx) { CommandTimeout=45 }; }
        private static void P(SqlCommand cmd,string n,SqlDbType t,object v)
        { cmd.Parameters.Add(n,t).Value=v??DBNull.Value; }
        private const string Columns=@"s.IdCongViec,t.MaCongViec,t.TenCongViec,s.LoaiSuKien,
           s.LanDoiSoat,s.PhienBan,s.SoGiayThuongHeThong,s.SoGiayTangCaHeThong,
           s.SoPhutThuongTuKhai,s.SoPhutTangCaTuKhai,s.LyDoDieuChinh,s.NgayTao,
           CASE WHEN s.LoaiSuKien=5 OR (s.LoaiSuKien=3 AND EXISTS (
                SELECT 1 FROM dbo.TblKhaiBaoGioDoiSoat origin
                WHERE origin.IdNhanVien=s.IdNhanVien AND origin.IdDuAn=s.IdDuAn
                  AND origin.IdCongViec=s.IdCongViec AND origin.LanDoiSoat=s.LanDoiSoat
                  AND origin.LoaiSuKien=5)) THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS GocXacNhanSauTask,
           s.LanThucHien AS LanThucHien";
        private static Kbg20BaoXongBanGhi Read(IDataRecord r)
        {
            return new Kbg20BaoXongBanGhi {
                IdCongViec=r.GetGuid(0),MaCongViec=r.IsDBNull(1)?"":Convert.ToString(r.GetValue(1)),
                TenCongViec=r.IsDBNull(2)?"":Convert.ToString(r.GetValue(2)),
                LoaiSuKien=Convert.ToInt32(r.GetValue(3)),Lan=r.GetInt32(4),PhienBan=r.GetInt32(5),
                GiayThuong=Convert.ToInt64(r.GetValue(6)),GiayTangCa=Convert.ToInt64(r.GetValue(7)),
                PhutTuKhaiThuong=r.IsDBNull(8)?(int?)null:Convert.ToInt32(r.GetValue(8)),
                PhutTuKhaiTangCa=r.IsDBNull(9)?(int?)null:Convert.ToInt32(r.GetValue(9)),
                LyDo=r.IsDBNull(10)?null:Convert.ToString(r.GetValue(10)),
                NgayTaoUtc=DateTime.SpecifyKind(r.GetDateTime(11),DateTimeKind.Utc),
                CoGocXacNhanSauTask=r.FieldCount>12 && !r.IsDBNull(12) && Convert.ToBoolean(r.GetValue(12)),
                LanThucHien=r.FieldCount>13 && !r.IsDBNull(13) ? Convert.ToInt32(r.GetValue(13)) : 1,
                // LayTomTat them cot khoa PM SAU cot LanThucHien.
                DaPmChot=r.FieldCount>14 && !r.IsDBNull(14) && Convert.ToBoolean(r.GetValue(14))
            };
        }
        private static void Keys(SqlCommand cmd,Guid nv,Guid project,Guid task)
        {
            P(cmd,"@U",SqlDbType.UniqueIdentifier,nv);
            P(cmd,"@D",SqlDbType.UniqueIdentifier,project);
            P(cmd,"@T",SqlDbType.UniqueIdentifier,task);
        }
        private static void Valid(Guid nv,Guid project,Guid task)
        { if(nv==Guid.Empty||project==Guid.Empty||task==Guid.Empty)throw new ArgumentException("Thieu nhan vien, du an hoac cong viec."); }
        private static void ValidMinutes(int reg,int ot)
        { if(reg<0||ot<0||reg>599999||ot>599999)throw new ArgumentException("So phut tu khai khong hop le."); }
        private static void ValidReason(string reason,bool required)
        {
            if(reason!=null&&reason.Length>2000)throw new ArgumentException("Ghi chu qua 2000 ky tu.");
            if(required&&string.IsNullOrWhiteSpace(reason))throw new ArgumentException("Vui long nhap ly do rut bao.");
        }
        private void EnsureView(SqlConnection c,SqlTransaction tx,Guid nv,Guid project,Guid task)
        {
            using(var cmd=Cmd(c,tx,@"SELECT 1 FROM dbo.TblCongViec t WHERE t.IdCongViec=@T AND t.IdDuAn=@D
                AND (EXISTS(SELECT 1 FROM dbo.TblCongViec_NhanVien a WHERE a.IdCongViec=@T AND a.IdNhanVien=@U)
                  OR EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioPhien p WHERE p.IdCongViec=@T AND p.IdNhanVien=@U)
                  OR EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioDoiSoat s WHERE s.IdCongViec=@T AND s.IdNhanVien=@U))"))
            { Keys(cmd,nv,project,task);if(cmd.ExecuteScalar()==null)throw new UnauthorizedAccessException("Khong co quyen xem gio cong nay."); }
        }
        private static Kbg20BaoXongBanGhi Latest(SqlConnection c,SqlTransaction tx,Guid nv,Guid project,Guid task)
        {
            using(var cmd=Cmd(c,tx,@"SELECT TOP (1) "+Columns+@"
                FROM dbo.TblKhaiBaoGioDoiSoat s WITH(UPDLOCK,HOLDLOCK)
                INNER JOIN dbo.TblCongViec t ON t.IdCongViec=s.IdCongViec
                WHERE s.IdNhanVien=@U AND s.IdDuAn=@D AND s.IdCongViec=@T AND s.LoaiSuKien IN (1,2,3,4,5)
                  AND s.LanThucHien=t.LanThucHien
                ORDER BY s.LanDoiSoat DESC,s.PhienBan DESC,s.NgayTao DESC"))
            { Keys(cmd,nv,project,task);using(var r=cmd.ExecuteReader())return r.Read()?Read(r):null; }
        }
        private static bool LockedByPm(SqlConnection c,SqlTransaction tx,Guid nv,Guid project,Guid task)
        {
            using(var cmd=Cmd(c,tx,@"SELECT TOP(1) 1 FROM dbo.TblKhaiBaoGioDoiSoat WITH(UPDLOCK,HOLDLOCK)
                WHERE IdNhanVien=@U AND IdDuAn=@D AND IdCongViec=@T AND LoaiSuKien=4"))
            { Keys(cmd,nv,project,task);return cmd.ExecuteScalar()!=null; }
        }
        // Chon dung dong lich su theo request id khi phat thong bao sau commit.
        // Khong doc "latest" de tranh race voi mot thao tac sua/rut moi o tab khac.
        private Kbg20BaoXongBanGhi DocTheoMaThaoTac(Guid nv,Guid project,Guid task,Guid requestId)
        {
            using(var c=repo.MoKetNoi())
            using(var q=Cmd(c,null,@"SELECT "+Columns+@" FROM dbo.TblKhaiBaoGioDoiSoat s
                INNER JOIN dbo.TblCongViec t ON t.IdCongViec=s.IdCongViec
                WHERE s.IdNhanVien=@U AND s.IdDuAn=@D AND s.IdCongViec=@T AND s.IdThaoTac=@I"))
            {
                Keys(q,nv,project,task);P(q,"@I",SqlDbType.UniqueIdentifier,requestId);
                using(var rd=q.ExecuteReader())return rd.Read()?Read(rd):null;
            }
        }
        private static bool RequestDone(SqlConnection c,SqlTransaction tx,Guid requestId,Guid nv,Guid project,Guid task,int type)
        {
            using(var cmd=Cmd(c,tx,@"SELECT TOP (1) LoaiSuKien FROM dbo.TblKhaiBaoGioDoiSoat WITH(UPDLOCK,HOLDLOCK)
                WHERE IdThaoTac=@I AND IdNhanVien=@U AND IdDuAn=@D AND IdCongViec=@T"))
            { Keys(cmd,nv,project,task);P(cmd,"@I",SqlDbType.UniqueIdentifier,requestId);
              var x=cmd.ExecuteScalar();if(x==null)return false;
              if(Convert.ToInt32(x)!=type)throw new DBConcurrencyException("Ma thao tac da duoc su dung cho hanh dong khac.");return true; }
        }
        private static long[] TotalSeconds(SqlConnection c,SqlTransaction tx,Guid nv,Guid task)
        {
            using(var cmd=Cmd(c,tx,@"SELECT
                ISNULL(SUM(CASE WHEN p.LoaiGio=1 THEN CAST(k.SoGiay AS bigint) ELSE 0 END),0),
                ISNULL(SUM(CASE WHEN p.LoaiGio=2 THEN CAST(k.SoGiay AS bigint) ELSE 0 END),0)
                FROM dbo.TblKhaiBaoGioPhien p
                LEFT JOIN dbo.TblKhaiBaoGioKhoangTinh k ON k.IdPhien=p.IdPhien
                WHERE p.IdNhanVien=@U AND p.IdCongViec=@T"))
            { P(cmd,"@U",SqlDbType.UniqueIdentifier,nv);P(cmd,"@T",SqlDbType.UniqueIdentifier,task);
              using(var r=cmd.ExecuteReader()){r.Read();return new []{Convert.ToInt64(r.GetValue(0)),Convert.ToInt64(r.GetValue(1))};} }
        }
        private static int MaxRound(SqlConnection c,SqlTransaction tx,Guid nv,Guid project,Guid task)
        {
            using(var cmd=Cmd(c,tx,@"SELECT ISNULL(MAX(LanDoiSoat),0) FROM dbo.TblKhaiBaoGioDoiSoat WITH(UPDLOCK,HOLDLOCK)
                WHERE IdNhanVien=@U AND IdDuAn=@D AND IdCongViec=@T"))
            { Keys(cmd,nv,project,task);return Convert.ToInt32(cmd.ExecuteScalar()); }
        }
        private static void CancelCurrent(SqlConnection c,SqlTransaction tx,Guid nv,Guid project,Guid task,int lan,int version)
        {
            using(var cmd=Cmd(c,tx,@"UPDATE dbo.TblKhaiBaoGioDoiSoat SET LaBanHienTai=0
                WHERE IdNhanVien=@U AND IdDuAn=@D AND IdCongViec=@T
                  AND LanDoiSoat=@L AND PhienBan=@V AND LaBanHienTai=1 AND LoaiSuKien IN (1,2,3,5)"))
            { Keys(cmd,nv,project,task);P(cmd,"@L",SqlDbType.Int,lan);P(cmd,"@V",SqlDbType.Int,version);
              if(cmd.ExecuteNonQuery()!=1)throw new DBConcurrencyException("Lich su bao xong da thay doi. Vui long tai lai."); }
        }
        private static void Insert(SqlConnection c,SqlTransaction tx,Guid nv,Guid project,Guid task,
            Guid requestId,int type,int lan,int version,long[] system,int? regular,int? overtime,string note)
        {
            using(var cmd=Cmd(c,tx,@"INSERT dbo.TblKhaiBaoGioDoiSoat
                (IdDoiSoat,IdNhanVien,IdDuAn,IdCongViec,LanDoiSoat,PhienBan,LaBanHienTai,
                 SoGiayThuongHeThong,SoGiayTangCaHeThong,SoPhutThuongTuKhai,SoPhutTangCaTuKhai,
                 LyDoDieuChinh,ChotGioHeThongLuc,NguoiTao,NgayTao,LoaiSuKien,IdThaoTac,LanThucHien)
                VALUES(NEWID(),@U,@D,@T,@L,@V,1,@R,@O,@SR,@SO,@Reason,@Now,@Actor,@Now,@Type,@Id,
                       (SELECT LanThucHien FROM dbo.TblCongViec WHERE IdDuAn=@D AND IdCongViec=@T))"))
            {
                Keys(cmd,nv,project,task);
                P(cmd,"@L",SqlDbType.Int,lan);P(cmd,"@V",SqlDbType.Int,version);
                P(cmd,"@R",SqlDbType.BigInt,system[0]);P(cmd,"@O",SqlDbType.BigInt,system[1]);
                P(cmd,"@SR",SqlDbType.Int,regular);P(cmd,"@SO",SqlDbType.Int,overtime);
                P(cmd,"@Reason",SqlDbType.NVarChar,note);
                P(cmd,"@Now",SqlDbType.DateTime2,DateTime.UtcNow);
                P(cmd,"@Actor",SqlDbType.NVarChar,nv.ToString("D"));
                P(cmd,"@Type",SqlDbType.TinyInt,type);P(cmd,"@Id",SqlDbType.UniqueIdentifier,requestId);
                cmd.ExecuteNonQuery();
            }
        }
        public Dictionary<Guid,Kbg20BaoXongBanGhi> LayTomTat(Guid nv,Guid project)
        {
            var items=new Dictionary<Guid,Kbg20BaoXongBanGhi>();
            using(var c=repo.MoKetNoi())
            using(var cmd=Cmd(c,null,@"SELECT "+Columns+@",
              CASE WHEN EXISTS (SELECT 1 FROM dbo.TblKhaiBaoGioDoiSoat lockrow
                  WHERE lockrow.IdNhanVien=s.IdNhanVien AND lockrow.IdDuAn=s.IdDuAn
                    AND lockrow.IdCongViec=s.IdCongViec AND lockrow.LoaiSuKien=4)
                  THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS DaPmChot
              FROM dbo.TblKhaiBaoGioDoiSoat s
              JOIN dbo.TblCongViec t ON t.IdCongViec=s.IdCongViec
              WHERE s.IdNhanVien=@U AND s.IdDuAn=@D AND s.LoaiSuKien IN(1,2,3,4,5)
                AND s.LanThucHien=t.LanThucHien
                AND NOT EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioDoiSoat newer
                  WHERE newer.IdCongViec=s.IdCongViec AND newer.IdNhanVien=s.IdNhanVien AND newer.IdDuAn=s.IdDuAn
                    AND newer.LoaiSuKien IN(1,2,3,4,5) AND newer.LanThucHien=s.LanThucHien
                    AND (newer.LanDoiSoat>s.LanDoiSoat OR
                      (newer.LanDoiSoat=s.LanDoiSoat AND newer.PhienBan>s.PhienBan)))"))
            { P(cmd,"@U",SqlDbType.UniqueIdentifier,nv);P(cmd,"@D",SqlDbType.UniqueIdentifier,project);
              using(var r=cmd.ExecuteReader())while(r.Read()){var x=Read(r);items[x.IdCongViec]=x;} }
            return items;
        }
        // Doc theo lo (KHONG N+1): Task 4/2/3 can xac nhan gio, ke ca chua tung bao.
        // Chi nguoi duoc phan cong hoac co lich su gio/doi soat truc tiep moi duoc thay.
        // Ket qua doc 1 truy van, dung cho messagebox gui Task va thong bao nhac nhan vien.
        // Event 1/5 (va event 3 sua gio) deu co nghia: da bao hoan thanh trong luot hien tai.
        // Chung mot quy tac voi popup PM. Khong duoc tinh khac nhau tai TaskList/Tab1/PM.
        public List<Kbg20NhanVienChuaXacNhan> LayNhanVienChuaXacNhan(Guid project,Guid task,bool chiTinhBaoXong)
        {
            var ban=new Kbg20PmChotCongTaskManager().LayBanDuyet(project,task);
            return ban.NhanVien.Where(x=>!x.DaXacNhanGio).Select(x=>new Kbg20NhanVienChuaXacNhan {
                IdNhanVien=x.IdNhanVien,TenNhanVien=x.TenNhanVien
            }).ToList();
        }

        public List<Kbg20ViecChoXacNhanGio> LayViecCanXacNhan(Guid nv,Guid project)
        {
            var list=new List<Kbg20ViecChoXacNhanGio>();
            using(var c=repo.MoKetNoi())
            using(var q=Cmd(c,null,@"SELECT t.IdCongViec,t.MaCongViec,t.TenCongViec,t.TrangThai,
                ISNULL(g.Reg,0) Reg,ISNULL(g.Ot,0) Ot
                FROM dbo.TblCongViec t
                OUTER APPLY(SELECT
                  SUM(CASE WHEN p.LoaiGio=1 THEN CAST(ISNULL(k.SoGiay,0) AS bigint) ELSE CAST(0 AS bigint) END) Reg,
                  SUM(CASE WHEN p.LoaiGio=2 THEN CAST(ISNULL(k.SoGiay,0) AS bigint) ELSE CAST(0 AS bigint) END) Ot
                  FROM dbo.TblKhaiBaoGioPhien p LEFT JOIN dbo.TblKhaiBaoGioKhoangTinh k ON k.IdPhien=p.IdPhien
                  WHERE p.IdDuAn=@D AND p.IdCongViec=t.IdCongViec AND p.IdNhanVien=@U) g
                WHERE t.IdDuAn=@D AND ISNULL(t.DaXoa,0)=0 AND t.TrangThai IN(4,2,3)
                AND (EXISTS(SELECT 1 FROM dbo.TblCongViec_NhanVien a WHERE a.IdCongViec=t.IdCongViec AND a.IdNhanVien=@U)
                 OR EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioPhien p WHERE p.IdCongViec=t.IdCongViec AND p.IdNhanVien=@U)
                 OR EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioDoiSoat s WHERE s.IdCongViec=t.IdCongViec AND s.IdNhanVien=@U))"))
            {
                P(q,"@D",SqlDbType.UniqueIdentifier,project);P(q,"@U",SqlDbType.UniqueIdentifier,nv);
                using(var rd=q.ExecuteReader())while(rd.Read())list.Add(new Kbg20ViecChoXacNhanGio {
                    IdCongViec=rd.GetGuid(0),MaCongViec=rd.IsDBNull(1)?"":Convert.ToString(rd.GetValue(1)),
                    TenCongViec=rd.IsDBNull(2)?"":Convert.ToString(rd.GetValue(2)),
                    TrangThai=Convert.ToInt32(rd.GetValue(3)),GiayHanhChinh=Convert.ToInt64(rd.GetValue(4)),
                    GiayTangCa=Convert.ToInt64(rd.GetValue(5)) });
            }
            return list;
        }

        // Bao hoan thanh tren Task 4/2/3; event 5 la nguon lich su, NGHIEP VU la da bao.
        public Kbg20BaoXongBanGhi XacNhanGioSauTask(Guid nv,Guid project,Guid task,Guid requestId,
            int regular,int overtime,string note)
        {
            Valid(nv,project,task);ValidMinutes(regular,overtime);ValidReason(note,false);
            if(requestId==Guid.Empty)throw new ArgumentException("Thieu ma thao tac.");
            bool written=false;
            using(var c=repo.MoKetNoi()) using(var tx=c.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
            {
                repo.KhoaTheoDuAn(c,tx,project);
                if(RequestDone(c,tx,requestId,nv,project,task,5))
                {var same=Latest(c,tx,nv,project,task);tx.Commit();return same;}
                EnsureView(c,tx,nv,project,task);
                var target=repo.LayCongViec(c,tx,task);
                if(target==null || target.IdDuAn!=project ||
                   (target.TrangThai!=2 && target.TrangThai!=3 && target.TrangThai!=4))
                    throw new InvalidOperationException("Task phai o trang thai Cho duyet/Da hoan thanh moi bao phan viec theo luong nay.");
                if(LockedByPm(c,tx,nv,project,task))throw new InvalidOperationException("PM da chot gio; chi duoc xem.");
                var last=Latest(c,tx,nv,project,task);
                if(last!=null && last.LoaiSuKien!=2 && !last.CoGocXacNhanSauTask)
                    throw new DBConcurrencyException("Đã báo hoàn thành phần việc. Hãy sửa giờ đã báo thay vì xác nhận giờ trùng.");
                // Neu bao cu khong doi sau luc Task bi tu choi/lai phat sinh gio,
                // chi duoc sua gio; khong tao them mot dot xac nhan giong het.
                var currentSeconds=TotalSeconds(c,tx,nv,task);
                if(last!=null && last.CoGocXacNhanSauTask &&
                   last.GiayThuong==currentSeconds[0] && last.GiayTangCa==currentSeconds[1])
                    throw new DBConcurrencyException("Giờ hệ thống không thay đổi; hãy dùng Sửa giờ thay vì tạo đợt xác nhận mới.");
                // Dong ho da chot boi workflow Task. Tuyet doi khong tao gio moi o day.
                using(var q=Cmd(c,tx,@"SELECT TOP 1 1 FROM dbo.TblKhaiBaoGioPhien WITH(HOLDLOCK)
                    WHERE IdDuAn=@D AND IdCongViec=@T AND IdNhanVien=@U AND KetThucLuc IS NULL"))
                {Keys(q,nv,project,task);if(q.ExecuteScalar()!=null)
                    throw new InvalidOperationException("Van con dong ho mo tren Task; khong duoc xac nhan gio.");}
                var total=TotalSeconds(c,tx,nv,task);
                Insert(c,tx,nv,project,task,requestId,5,checked(MaxRound(c,tx,nv,project,task)+1),1,
                    total,regular,overtime,note);
                tx.Commit();written=true;
            }
            var saved=LayChiTiet(nv,project,task).HienTai;
            if(written)NotifyPm(project,task,nv,5,DocTheoMaThaoTac(nv,project,task,requestId));
            return saved;
        }

        public Kbg20BaoXongChiTiet LayChiTiet(Guid nv,Guid project,Guid task)
        {
            Valid(nv,project,task);var detail=new Kbg20BaoXongChiTiet();
            using(var c=repo.MoKetNoi())
            {
                EnsureView(c,null,nv,project,task);
                detail.ConPhanCong=repo.CoPhanCong(c,null,task,nv);
                using(var st=Cmd(c,null,@"SELECT TrangThai FROM dbo.TblCongViec WHERE IdCongViec=@T AND IdDuAn=@D"))
                { P(st,"@T",SqlDbType.UniqueIdentifier,task);P(st,"@D",SqlDbType.UniqueIdentifier,project);
                  var x=st.ExecuteScalar();if(x==null)throw new InvalidOperationException("Cong viec khong con hieu luc.");detail.TrangThaiTask=Convert.ToInt32(x); }
                detail.KhoaPm=LockedByPm(c,null,nv,project,task);
                using(var cmd=Cmd(c,null,@"SELECT "+Columns+@" FROM dbo.TblKhaiBaoGioDoiSoat s
                    JOIN dbo.TblCongViec t ON t.IdCongViec=s.IdCongViec
                    WHERE s.IdNhanVien=@U AND s.IdDuAn=@D AND s.IdCongViec=@T
                    AND s.LoaiSuKien IN (1,2,3,4,5)
                    ORDER BY s.LanDoiSoat DESC,s.PhienBan DESC,s.NgayTao DESC"))
                { Keys(cmd,nv,project,task);using(var r=cmd.ExecuteReader())while(r.Read())detail.LichSu.Add(Read(r)); }
                var lanHienTai=Kbg20LuotThucHien.LayLanHienTai(c,null,project,task);
                detail.HienTai=detail.LichSu.FirstOrDefault(x=>x.LanThucHien==lanHienTai);
                var total=TotalSeconds(c,null,nv,task);
                detail.GiayThuongHeThong=total[0];detail.GiayTangCaHeThong=total[1];
            }
            return detail;
        }
        public Kbg20BaoXongBanGhi BaoXong(Guid nv,Guid project,Guid task,Guid requestId,int regular,int overtime,string note)
        {
            Valid(nv,project,task);ValidMinutes(regular,overtime);ValidReason(note,false);
            if(requestId==Guid.Empty)throw new ArgumentException("Thieu ma thao tac.");
            // Cung mot hanh dong Bao hoan thanh: tren Task da cho duyet / hoan thanh
            // chi ghi bao ca nhan, khong Start/Stop dong ho, khong doi trang thai Task.
            using(var checkC=repo.MoKetNoi())
            using(var statusCmd=Cmd(checkC,null,@"SELECT TrangThai FROM dbo.TblCongViec WHERE IdDuAn=@D AND IdCongViec=@T"))
            {
                P(statusCmd,"@D",SqlDbType.UniqueIdentifier,project);
                P(statusCmd,"@T",SqlDbType.UniqueIdentifier,task);
                var state=statusCmd.ExecuteScalar();
                if(state!=null && (Convert.ToInt32(state)==4 || Convert.ToInt32(state)==2 || Convert.ToInt32(state)==3))
                    return XacNhanGioSauTask(nv,project,task,requestId,regular,overtime,note);
            }
            bool written=false;
            using(var c=repo.MoKetNoi()) using(var tx=c.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
            {
                repo.KhoaTheoDuAn(c,tx,project);
                if(RequestDone(c,tx,requestId,nv,project,task,1))
                {var saved=Latest(c,tx,nv,project,task);tx.Commit();return saved;}
                var target=repo.LayCongViec(c,tx,task);
                if(target==null||target.IdDuAn!=project||target.TrangThai!=1||target.ChoDuyet)
                    throw new InvalidOperationException("Task khong o trang thai dang thuc hien hoac dang cho duyet chung.");
                if(!repo.CoPhanCong(c,tx,task,nv))throw new UnauthorizedAccessException("Ban khong duoc phan cong truc tiep vao Task.");
                if(LockedByPm(c,tx,nv,project,task))throw new InvalidOperationException("PM da chot cong. Khong the bao xong lai.");
                var last=Latest(c,tx,nv,project,task);
                if(last!=null && last.LoaiSuKien!=2 && !last.CoGocXacNhanSauTask)
                    throw new DBConcurrencyException("Bạn đã báo hoàn thành phần việc. Vui lòng tải lại.");
                // Chu y: transaction CHUNG ket toan + dong phien rieng cua nv + snapshot + insert.
                new KhaiBaoGio20Manager().ChotPhienCaNhanTrongGiaoDich(c,tx,project,task,nv);
                var total=TotalSeconds(c,tx,nv,task);
                int lan=checked(MaxRound(c,tx,nv,project,task)+1);
                Insert(c,tx,nv,project,task,requestId,1,lan,1,total,regular,overtime,note);
                tx.Commit();written=true;
            }
            if(written)NotifyPm(project,task,nv,1,DocTheoMaThaoTac(nv,project,task,requestId));
            return LayChiTiet(nv,project,task).HienTai;
        }
        public Kbg20BaoXongBanGhi RutBao(Guid nv,Guid project,Guid task,Guid requestId,int expectedRound,int expectedVersion,string reason)
        {
            Valid(nv,project,task);ValidReason(reason,true);
            if(requestId==Guid.Empty)throw new ArgumentException("Thieu ma thao tac.");
            bool written=false;
            using(var c=repo.MoKetNoi()) using(var tx=c.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
            {
                repo.KhoaTheoDuAn(c,tx,project);
                if(RequestDone(c,tx,requestId,nv,project,task,2))
                {var same=Latest(c,tx,nv,project,task);tx.Commit();return same;}
                EnsureView(c,tx,nv,project,task);
                var target=repo.LayCongViec(c,tx,task);
                if(target==null || target.IdDuAn!=project || target.TrangThai!=1 || target.ChoDuyet)
                    throw new InvalidOperationException("Task dang cho duyet/da hoan thanh, khong duoc rut bao de lam tiep. Chi co the sua gio truoc khi PM chot.");
                if(LockedByPm(c,tx,nv,project,task))throw new InvalidOperationException("PM da chot cong. Khong the rut bao.");
                var latest=Latest(c,tx,nv,project,task);
                if(latest==null||!latest.DaBaoXongPhanViec||latest.Lan!=expectedRound||latest.PhienBan!=expectedVersion)
                    throw new DBConcurrencyException("Trang thai bao xong da thay doi. Hay tai lai.");
                CancelCurrent(c,tx,nv,project,task,latest.Lan,latest.PhienBan);
                Insert(c,tx,nv,project,task,requestId,2,latest.Lan,checked(latest.PhienBan+1),
                    new[]{latest.GiayThuong,latest.GiayTangCa},latest.PhutTuKhaiThuong,latest.PhutTuKhaiTangCa,reason);
                tx.Commit();written=true;
            }
            if(written)NotifyPm(project,task,nv,2,DocTheoMaThaoTac(nv,project,task,requestId));
            return LayChiTiet(nv,project,task).HienTai;
        }
        public Kbg20BaoXongBanGhi SuaTuKhai(Guid nv,Guid project,Guid task,Guid requestId,int expectedRound,int expectedVersion,int regular,int overtime,string reason)
        {
            Valid(nv,project,task);ValidMinutes(regular,overtime);ValidReason(reason,false);
            if(requestId==Guid.Empty)throw new ArgumentException("Thieu ma thao tac.");
            using(var c=repo.MoKetNoi()) using(var tx=c.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
            {
                repo.KhoaTheoDuAn(c,tx,project);
                if(RequestDone(c,tx,requestId,nv,project,task,3))
                {var same=Latest(c,tx,nv,project,task);tx.Commit();return same;}
                EnsureView(c,tx,nv,project,task);
                if(LockedByPm(c,tx,nv,project,task))throw new InvalidOperationException("PM da chot gio. Tab 3 chi duoc xem.");
                var latest=Latest(c,tx,nv,project,task);
                if(latest==null||!latest.DangBaoXong||latest.Lan!=expectedRound||latest.PhienBan!=expectedVersion)
                    throw new DBConcurrencyException("Ban tu khai da thay doi. Vui long tai lai.");
                CancelCurrent(c,tx,nv,project,task,latest.Lan,latest.PhienBan);
                Insert(c,tx,nv,project,task,requestId,3,latest.Lan,checked(latest.PhienBan+1),
                    new[]{latest.GiayThuong,latest.GiayTangCa},regular,overtime,reason);
                tx.Commit();
            }
            return LayChiTiet(nv,project,task).HienTai;
        }
        private static void NotifyPm(Guid project,Guid task,Guid nv,int kind,Kbg20BaoXongBanGhi saved)
        {
            try
            {
                var pm=DuAnManager.Instance.LayIdNhanVienQuanLy(project);
                if(!pm.HasValue||pm.Value==Guid.Empty || saved==null)return;
                var cv=TaskManager.Instance.FetchById(task);
                var user=SweetSoft.QLDA.DataAccess.AspnetUser.FetchByID(nv);
                string actor=user!=null&&!string.IsNullOrWhiteSpace(user.DisplayName)?user.DisplayName:"Nhân viên";
                string label=cv==null?"công việc":("["+cv.MaCongViec+"] "+cv.TenCongViec);
                string action=kind==2?"rút báo hoàn thành phần việc":kind==5?"báo hoàn thành phần việc":"báo hoàn thành phần việc";
                string reason=string.IsNullOrWhiteSpace(saved.LyDo)?"Không có":"Có ghi chú/lý do: "+saved.LyDo.Trim();
                string fmtR=(saved.PhutTuKhaiThuong.HasValue?(saved.PhutTuKhaiThuong.Value/60)+"g "+(saved.PhutTuKhaiThuong.Value%60)+"p":"Chưa báo");
                string fmtO=(saved.PhutTuKhaiTangCa.HasValue?(saved.PhutTuKhaiTangCa.Value/60)+"g "+(saved.PhutTuKhaiTangCa.Value%60)+"p":"Chưa báo");
                string message=actor+" đã "+action+" đối với "+label+". " +
                    "Giờ hành chính nhân viên báo: "+fmtR+"; giờ tăng ca nhân viên báo: "+fmtO+". " +reason+". " +
                    "Đây là báo hoàn thành cá nhân, không phải duyệt hoàn thành Task chung. Nhấn để xem đầy đủ chi tiết và lịch sử.";
                string prefix=(System.Web.HttpRuntime.AppDomainAppVirtualPath??"/").TrimEnd('/');
                string link=prefix+"/fKhaiBaoGio/KhaiBaoGio.aspx?ProjectId="+project.ToString("D")+
                   "&pmViewTaskId="+task.ToString("D")+"&pmViewEmployeeId="+nv.ToString("D")+
                   "&pmViewRound="+saved.Lan+"&pmViewVersion="+saved.PhienBan;
                var notice=ThongBaoManager.Instance.Create(userId:pm.Value,
                    tieuDe:actor+" "+action+" · "+label,noiDung:message,
                    loaiThongBao:ThongBaoTypes.CongViec,idCongViec:task,idDuAn:project,
                    duongDanLienKet:link,guiEmail:false);
                if(notice==null)throw new InvalidOperationException("Khong tao duoc thong bao PM sau commit.");
            }
            catch(Exception ex)
            {
                try { SweetSoft.QLDA.Core.SysManager.SysLogger.LogError(ex,
                    "KBG20 thong bao ca nhan task=" + task + ", nv=" + nv + ", kind=" + kind); }
                catch { System.Diagnostics.Trace.TraceError("KBG20 thong bao ca nhan: " + ex); }
            }
        }

    }
}

// ============================================================================
// DOT 8 - PM CHOT GIO CONG TAP THE. Giu nguyen moi class/phuong thuc da co.
// Ghep vao file manager da co => khong can them Core .cs vao .csproj.
// ============================================================================
namespace SweetSoft.QLDA.Core.Managers
{
    public sealed class Kbg20PmNhanVien
    {
        public Guid IdNhanVien { get; set; }
        public string TenNhanVien { get; set; }
        public string Email { get; set; }
        public bool DaRutBaoGanNhat { get; set; }
        public bool DaXacNhanGio { get; set; }
        public long GiayHanhChinhHeThong { get; set; }
        public long GiayTangCaHeThong { get; set; }
        public int? PhutHanhChinhNhanVienBao { get; set; }
        public int? PhutTangCaNhanVienBao { get; set; }
        public int PhutHanhChinhDeXuat { get; set; }
        public int PhutTangCaDeXuat { get; set; }
        public int? PhutHanhChinhPmDaChot { get; set; }
        public int? PhutTangCaPmDaChot { get; set; }
        public int LanDoiSoatLonNhat { get; set; }
        public bool DaCoSuKienPmChot { get; set; }
        public bool PmLaSuKienMoiNhat { get; set; }
        public string GhiChuPmChot { get; set; }
    }
    public sealed class Kbg20PmBanDuyet
    {
        public Guid IdDuAn { get; set; }
        public Guid IdCongViec { get; set; }
        public string MaCongViec { get; set; }
        public string TenCongViec { get; set; }
        public int TrangThaiTask { get; set; }
        public int LanThucHien { get; set; }
        public bool CoPhienDangMo { get; set; }
        public bool DaChotCong { get; set; }
        public int SoNhanVienChuaXacNhan { get { return NhanVien.Count(x=>!x.DaXacNhanGio); } }
        public string DauDuLieu { get; set; }
        public List<Kbg20PmNhanVien> NhanVien { get; set; } = new List<Kbg20PmNhanVien>();
        public long TongPhutHanhChinh { get { return NhanVien.Sum(x => (long)(DaChotCong ? x.PhutHanhChinhPmDaChot.GetValueOrDefault() : x.PhutHanhChinhDeXuat)); } }
        public long TongPhutTangCa { get { return NhanVien.Sum(x => (long)(DaChotCong ? x.PhutTangCaPmDaChot.GetValueOrDefault() : x.PhutTangCaDeXuat)); } }
    }
    public sealed class Kbg20PmDongDuyet
    {
        public Guid IdNhanVien { get; set; }
        public int PhutHanhChinh { get; set; }
        public int PhutTangCa { get; set; }
        public string LyDoDieuChinh { get; set; }
    }
    /// <summary>
    /// Chi phuc vu server BackOffice, KHONG cung cap handler cong khai.
    /// Quyen PM/admin phai duoc BackOffice kiem tra; Core kiem tra lai PM du an.
    /// LaBanHienTai trong SQL la hieu luc theo LAN doi soat, khong xoa lich su.
    /// </summary>
    public sealed class Kbg20PmChotCongTaskManager
    {
        private const int GioiHanPhut = 599999;
        private static System.Data.SqlClient.SqlCommand Lenh(System.Data.SqlClient.SqlConnection con, string sql)
        { return new System.Data.SqlClient.SqlCommand(sql, con) { CommandTimeout = 60 }; }
        private static void ThamSo(System.Data.SqlClient.SqlCommand c, string ten, System.Data.SqlDbType loai, object val)
        { c.Parameters.Add(ten, loai).Value = val ?? DBNull.Value; }
        private static void KhoaDuAn(System.Data.SqlClient.SqlConnection c, Guid idDuAn)
        {
            using (var q = Lenh(c, @"DECLARE @KetQua int;
                EXEC @KetQua=sys.sp_getapplock @Resource=@K,@LockMode='Exclusive',
                    @LockOwner='Transaction',@LockTimeout=15000;
                IF @KetQua<0 THROW 51020,N'Khong khoa duoc du an de chot gio cong.',1;"))
            {
                ThamSo(q,"@K",System.Data.SqlDbType.NVarChar,"KBG20:DUAN:"+idDuAn.ToString("N"));
                q.ExecuteNonQuery();
            }
        }
        private static void KhoaTask(System.Data.SqlClient.SqlConnection c,Guid idDuAn,Guid idTask)
        {
            using(var q=Lenh(c,@"SELECT TrangThai FROM dbo.TblCongViec WITH(UPDLOCK,HOLDLOCK)
                WHERE IdDuAn=@D AND IdCongViec=@T AND ISNULL(DaXoa,0)=0"))
            {
                ThamSo(q,"@D",System.Data.SqlDbType.UniqueIdentifier,idDuAn);
                ThamSo(q,"@T",System.Data.SqlDbType.UniqueIdentifier,idTask);
                var value=q.ExecuteScalar();
                if(value==null || (Convert.ToInt32(value)!=2 && Convert.ToInt32(value)!=3))
                    throw new InvalidOperationException("Cong viec da thay doi hoac chua duoc PM xac nhan hoan thanh.");
            }
        }
        private const string SqlDanhSach = @"
;WITH NhanVien AS (
    SELECT DISTINCT cn.IdNhanVien FROM dbo.TblCongViec_NhanVien cn WITH(HOLDLOCK) WHERE cn.IdCongViec=@T
    UNION SELECT DISTINCT p.IdNhanVien FROM dbo.TblKhaiBaoGioPhien p WITH(HOLDLOCK)
          WHERE p.IdDuAn=@D AND p.IdCongViec=@T
    UNION SELECT DISTINCT s.IdNhanVien FROM dbo.TblKhaiBaoGioDoiSoat s WITH(HOLDLOCK)
          WHERE s.IdDuAn=@D AND s.IdCongViec=@T AND s.LoaiSuKien IN(0,1,2,3,5)
), GioHeThong AS (
    SELECT p.IdNhanVien,
        SUM(CASE WHEN p.LoaiGio=1 THEN CAST(ISNULL(k.SoGiay,0) AS bigint) ELSE CAST(0 AS bigint) END) AS HanhChinh,
        SUM(CASE WHEN p.LoaiGio=2 THEN CAST(ISNULL(k.SoGiay,0) AS bigint) ELSE CAST(0 AS bigint) END) AS TangCa
    FROM dbo.TblKhaiBaoGioPhien p
    LEFT JOIN dbo.TblKhaiBaoGioKhoangTinh k ON k.IdPhien=p.IdPhien
    WHERE p.IdDuAn=@D AND p.IdCongViec=@T
    GROUP BY p.IdNhanVien
)
SELECT nv.IdNhanVien, COALESCE(NULLIF(u.DisplayName,N''),u.UserName,N'Nhân viên') Ten,
       COALESCE(m.Email,N'') Email,
       ISNULL(g.HanhChinh,0) GiayHanhChinh,ISNULL(g.TangCa,0) GiayTangCa,
       CASE WHEN cb.LoaiSuKien IN(1,3,5) THEN cb.SoPhutThuongTuKhai
            ELSE NULL END BaoHanhChinh,
       CASE WHEN cb.LoaiSuKien IN(1,3,5) THEN cb.SoPhutTangCaTuKhai
            ELSE NULL END BaoTangCa,
       pm.SoPhutThuongPmChot,pm.SoPhutTangCaPmChot,
       ISNULL(v.Lan,0) LanLonNhat,
       pm.IdDoiSoat,pm.LyDoDieuChinh,
       COALESCE(CONVERT(int,cb.LoaiSuKien),-1) LoaiBaoGanNhat,
       ISNULL(v.PhienBan,0) PhienBanMoiNhat,
       COALESCE(CONVERT(int,v.LoaiSuKien),-1) SuKienMoiNhat, v.IdPmChot,
       cb.SoGiayThuongHeThong AS BaoSnapshotHanhChinh,
       cb.SoGiayTangCaHeThong AS BaoSnapshotTangCa
FROM NhanVien nv
LEFT JOIN dbo.aspnet_Users u ON u.UserId=nv.IdNhanVien
LEFT JOIN dbo.aspnet_Membership m ON m.UserId=nv.IdNhanVien
LEFT JOIN GioHeThong g ON g.IdNhanVien=nv.IdNhanVien
OUTER APPLY (SELECT TOP (1) s.LoaiSuKien,s.SoPhutThuongTuKhai,s.SoPhutTangCaTuKhai,
                   s.SoGiayThuongHeThong,s.SoGiayTangCaHeThong
    FROM dbo.TblKhaiBaoGioDoiSoat s WHERE s.IdDuAn=@D AND s.IdCongViec=@T AND s.IdNhanVien=nv.IdNhanVien
       AND s.LoaiSuKien IN(1,2,3,5)
       AND s.LanThucHien=(SELECT LanThucHien FROM dbo.TblCongViec
                              WHERE IdDuAn=@D AND IdCongViec=@T)
    ORDER BY s.LanDoiSoat DESC,s.PhienBan DESC,s.NgayTao DESC,s.IdDoiSoat DESC) cb
OUTER APPLY (SELECT TOP (1) s.SoPhutThuongTuKhai,s.SoPhutTangCaTuKhai
    FROM dbo.TblKhaiBaoGioDoiSoat s WHERE s.IdDuAn=@D AND s.IdCongViec=@T AND s.IdNhanVien=nv.IdNhanVien
       AND s.LoaiSuKien=0 AND (s.SoPhutThuongTuKhai IS NOT NULL OR s.SoPhutTangCaTuKhai IS NOT NULL)
    ORDER BY s.LanDoiSoat DESC,s.PhienBan DESC,s.NgayTao DESC,s.IdDoiSoat DESC) cu
OUTER APPLY (SELECT TOP (1) s.IdDoiSoat,s.SoPhutThuongPmChot,s.SoPhutTangCaPmChot,s.LyDoDieuChinh
    FROM dbo.TblKhaiBaoGioDoiSoat s WHERE s.IdDuAn=@D AND s.IdCongViec=@T AND s.IdNhanVien=nv.IdNhanVien
       AND s.LoaiSuKien=4
    ORDER BY s.LanDoiSoat DESC,s.PhienBan DESC,s.NgayTao DESC,s.IdDoiSoat DESC) pm
OUTER APPLY (SELECT TOP (1) s.LanDoiSoat Lan,s.PhienBan,s.LoaiSuKien,s.IdPmChot
    FROM dbo.TblKhaiBaoGioDoiSoat s WHERE s.IdDuAn=@D AND s.IdCongViec=@T AND s.IdNhanVien=nv.IdNhanVien
    ORDER BY s.LanDoiSoat DESC,s.PhienBan DESC,s.NgayTao DESC,s.IdDoiSoat DESC) v
ORDER BY COALESCE(NULLIF(u.DisplayName,N''),u.UserName,N'Nhân viên'),nv.IdNhanVien;";

        private static int PhutTuGiay(long giay)
        {
            // Round midpoint away from zero, theo SecondsToMinutes() hien co.
            return checked((int)Math.Round(giay/60.0,MidpointRounding.AwayFromZero));
        }
        private static string TaoDauDuLieu(Kbg20PmBanDuyet b)
        {
            var s=new System.Text.StringBuilder();
            s.Append(b.IdDuAn.ToString("N")).Append('|').Append(b.IdCongViec.ToString("N"))
             .Append('|').Append(b.TrangThaiTask).Append('|').Append(b.LanThucHien)
             .Append('|').Append(b.CoPhienDangMo);
            foreach(var n in b.NhanVien.OrderBy(x=>x.IdNhanVien))
            {
                s.Append('|').Append(n.IdNhanVien.ToString("N"))
                 .Append('|').Append(n.GiayHanhChinhHeThong).Append('|').Append(n.GiayTangCaHeThong)
                 .Append('|').Append(n.PhutHanhChinhNhanVienBao.HasValue?n.PhutHanhChinhNhanVienBao.Value.ToString():"NULL")
                 .Append('|').Append(n.PhutTangCaNhanVienBao.HasValue?n.PhutTangCaNhanVienBao.Value.ToString():"NULL")
                 .Append('|').Append(n.LanDoiSoatLonNhat)
                 .Append('|').Append(n.DaCoSuKienPmChot).Append('|').Append(n.PmLaSuKienMoiNhat);
            }
            using(var hash=System.Security.Cryptography.SHA256.Create())
                return Convert.ToBase64String(hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(s.ToString())));
        }
        private static Kbg20PmBanDuyet Doc(System.Data.SqlClient.SqlConnection c,Guid duAn,Guid task)
        {
            var result=new Kbg20PmBanDuyet{IdDuAn=duAn,IdCongViec=task};
            using(var q=Lenh(c,@"SELECT MaCongViec,TenCongViec,TrangThai,LanThucHien FROM dbo.TblCongViec
                WHERE IdDuAn=@D AND IdCongViec=@T AND ISNULL(DaXoa,0)=0"))
            {
                ThamSo(q,"@D",System.Data.SqlDbType.UniqueIdentifier,duAn);
                ThamSo(q,"@T",System.Data.SqlDbType.UniqueIdentifier,task);
                using(var r=q.ExecuteReader())
                {
                    if(!r.Read())throw new KeyNotFoundException("Khong tim thay cong viec trong du an.");
                    result.MaCongViec=r.IsDBNull(0)?"":r.GetString(0);
                    result.TenCongViec=r.IsDBNull(1)?"":r.GetString(1);
                    result.TrangThaiTask=Convert.ToInt32(r.GetValue(2));
                    result.LanThucHien=Convert.ToInt32(r.GetValue(3));
                }
            }
            using(var q=Lenh(c,@"SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.TblKhaiBaoGioPhien
                   WHERE IdDuAn=@D AND IdCongViec=@T AND KetThucLuc IS NULL) THEN 1 ELSE 0 END"))
            {
                ThamSo(q,"@D",System.Data.SqlDbType.UniqueIdentifier,duAn);
                ThamSo(q,"@T",System.Data.SqlDbType.UniqueIdentifier,task);
                result.CoPhienDangMo=Convert.ToInt32(q.ExecuteScalar())==1;
            }
            using(var q=Lenh(c,SqlDanhSach))
            {
                ThamSo(q,"@D",System.Data.SqlDbType.UniqueIdentifier,duAn);
                ThamSo(q,"@T",System.Data.SqlDbType.UniqueIdentifier,task);
                using(var r=q.ExecuteReader())while(r.Read())
                {
                    var n=new Kbg20PmNhanVien{
                        IdNhanVien=r.GetGuid(0),TenNhanVien=Convert.ToString(r.GetValue(1)),
                        Email=r.IsDBNull(2)?"":Convert.ToString(r.GetValue(2)),
                        DaRutBaoGanNhat=Convert.ToInt32(r.GetValue(12))==2,
                        DaXacNhanGio=(Convert.ToInt32(r.GetValue(12))==1 || Convert.ToInt32(r.GetValue(12))==3 || Convert.ToInt32(r.GetValue(12))==5)
                            && !r.IsDBNull(5) && !r.IsDBNull(6)
                            // Ban bao/khai cu phai bao gom TOAN BO gio he thong hien tai.
                            && !r.IsDBNull(16) && !r.IsDBNull(17)
                            && Convert.ToInt64(r.GetValue(16))==Convert.ToInt64(r.GetValue(3))
                            && Convert.ToInt64(r.GetValue(17))==Convert.ToInt64(r.GetValue(4)),
                        GiayHanhChinhHeThong=Convert.ToInt64(r.GetValue(3)),
                        GiayTangCaHeThong=Convert.ToInt64(r.GetValue(4)),
                        PhutHanhChinhNhanVienBao=r.IsDBNull(5)?(int?)null:Convert.ToInt32(r.GetValue(5)),
                        PhutTangCaNhanVienBao=r.IsDBNull(6)?(int?)null:Convert.ToInt32(r.GetValue(6)),
                        PhutHanhChinhPmDaChot=r.IsDBNull(7)?(int?)null:Convert.ToInt32(r.GetValue(7)),
                        PhutTangCaPmDaChot=r.IsDBNull(8)?(int?)null:Convert.ToInt32(r.GetValue(8)),
                        LanDoiSoatLonNhat=Convert.ToInt32(r.GetValue(9)),
                        DaCoSuKienPmChot=!r.IsDBNull(10),
                        PmLaSuKienMoiNhat=Convert.ToInt32(r.GetValue(14))==4 && !r.IsDBNull(15) && !r.IsDBNull(7) && !r.IsDBNull(8),
                        GhiChuPmChot=r.IsDBNull(11)?"":Convert.ToString(r.GetValue(11))
                    };
                    n.PhutHanhChinhDeXuat=n.PhutHanhChinhNhanVienBao??PhutTuGiay(n.GiayHanhChinhHeThong);
                    n.PhutTangCaDeXuat=n.PhutTangCaNhanVienBao??PhutTuGiay(n.GiayTangCaHeThong);
                    result.NhanVien.Add(n);
                }
            }
            result.DaChotCong=result.TrangThaiTask>=2 && result.TrangThaiTask<=3
                && result.NhanVien.Count>0 && !result.CoPhienDangMo
                && result.NhanVien.All(x=>x.PmLaSuKienMoiNhat && x.PhutHanhChinhPmDaChot.HasValue && x.PhutTangCaPmDaChot.HasValue
                    && x.PhutHanhChinhPmDaChot.Value>=0 && x.PhutTangCaPmDaChot.Value>=0);
            result.DauDuLieu=TaoDauDuLieu(result);
            return result;
        }
        public Kbg20PmBanDuyet LayBanDuyet(Guid duAn,Guid task)
        {
            if(duAn==Guid.Empty || task==Guid.Empty)throw new ArgumentException("Du an/task khong hop le.");
            using(var c=new SweetSoft.QLDA.Core.Respositories.KhaiBaoGio20Repository().MoKetNoi())
                return Doc(c,duAn,task);
        }
        private static Guid MaThaoTacTheoNhanVien(Guid idYeuCau,Guid idNhanVien)
        {
            // UNIQUE IdThaoTac toan bang => moi nhan vien phai co GUID rieng.
            byte[] buffer=new byte[32];
            Buffer.BlockCopy(idYeuCau.ToByteArray(),0,buffer,0,16);
            Buffer.BlockCopy(idNhanVien.ToByteArray(),0,buffer,16,16);
            using(var sha=System.Security.Cryptography.SHA256.Create())
            {
                var digest=sha.ComputeHash(buffer);
                byte[] guidBytes=new byte[16];Array.Copy(digest,guidBytes,16);
                return new Guid(guidBytes);
            }
        }
        public Kbg20PmBanDuyet ChotCongTapThe(Guid duAn,Guid task,Guid idPm,bool laQuanTri,
            Guid idYeuCau,string dauDuLieu,IList<Kbg20PmDongDuyet> danhSach)
        {
            if(duAn==Guid.Empty || task==Guid.Empty || idPm==Guid.Empty || idYeuCau==Guid.Empty)
                throw new ArgumentException("Thieu du an, cong viec, PM hoac ma yeu cau.");
            if(danhSach==null || danhSach.Count==0 || danhSach.Count>10000)
                throw new InvalidOperationException("Danh sach nhan vien can chot khong hop le.");
            if(string.IsNullOrWhiteSpace(dauDuLieu))throw new InvalidOperationException("Thieu phien ban du lieu popup.");
            if(!KhaiBaoGioManager.KhaiBaoGio20DaKichHoat())
                throw new InvalidOperationException("KhaiBaoGio20 chua duoc bat; khong duoc phep chot cong.");
            // Khong tin IsPM do browser, Core xac minh PM du an va caller la server-side.
            var pm=DuAnManager.Instance.LayIdNhanVienQuanLy(duAn);
            if(!laQuanTri && (!pm.HasValue || pm.Value!=idPm))
                throw new UnauthorizedAccessException("Chi PM cua du an hoac quan tri vien duoc chot gio cong.");
            if(danhSach.Select(x=>x.IdNhanVien).Distinct().Count()!=danhSach.Count || danhSach.Any(x=>x.IdNhanVien==Guid.Empty))
                throw new InvalidOperationException("Danh sach co nhan vien trung hoac Id khong hop le.");
            foreach(var row in danhSach)
            {
                if(row.PhutHanhChinh<0 || row.PhutTangCa<0 || row.PhutHanhChinh>GioiHanPhut || row.PhutTangCa>GioiHanPhut)
                    throw new ArgumentOutOfRangeException("danhSach","So phut PM duyet phai khong am va nam trong gioi han.");
                if(row.LyDoDieuChinh!=null && row.LyDoDieuChinh.Length>2000)
                    throw new ArgumentException("Ghi chu thay doi qua 2000 ky tu.");
            }
            // TransactionScope chi atomic neu SqlClient tu dong enlist ambient transaction.
            var connectionInfo=new System.Data.SqlClient.SqlConnectionStringBuilder(
                new SweetSoft.QLDA.Core.Respositories.KhaiBaoGio20Repository().ChuoiKetNoi);
            if(!connectionInfo.Enlist)
                throw new InvalidOperationException("KhaiBaoGio20.TenKetNoi phai dat Enlist=true de dam bao rollback tat ca nhan vien.");
            using(var scope=new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.Required,
                new System.Transactions.TransactionOptions{ IsolationLevel=System.Transactions.IsolationLevel.ReadCommitted,
                    Timeout=TimeSpan.FromMinutes(2) }))
            {
                var repo=new SweetSoft.QLDA.Core.Respositories.KhaiBaoGio20Repository();
                using(var con=repo.MoKetNoi())
                {
                    KhoaDuAn(con,duAn);
                    KhoaTask(con,duAn,task);
                    var current=Doc(con,duAn,task);
                    // Bat buoc kiem tra o Core; caller khong duoc tu gan quyet dinh trang thai.
                    if(current.TrangThaiTask!=2 && current.TrangThaiTask!=3)
                        throw new InvalidOperationException("Chi Task da hoan thanh (2/3) moi duoc chot gio cong.");
                    if(current.CoPhienDangMo || current.NhanVien.Count==0)
                        throw new InvalidOperationException("Task con dong ho mo hoac khong co nhan vien can chot.");
                    if(current.NhanVien.Any(x=>!x.DaXacNhanGio))
                        throw new InvalidOperationException("Con nhan vien chua bao hoan thanh phan viec: " +
                            string.Join(", ",current.NhanVien.Where(x=>!x.DaXacNhanGio).Select(x=>x.TenNhanVien)) +
                            ". Can bao hoan thanh phan viec truoc khi PM chot cong tap the.");
                    if(current.NhanVien.Any(x=>x.DaCoSuKienPmChot))
                        throw new InvalidOperationException("Task da co du lieu PM chot cong. Khong cho ghi de/ghi mot phan.");
                    if(!string.Equals(current.DauDuLieu,dauDuLieu,StringComparison.Ordinal))
                        throw new DBConcurrencyException("Du lieu gio, bao xong hoac phan cong da thay doi. Vui long mo lai popup.");
                    var actual=current.NhanVien.Select(x=>x.IdNhanVien).OrderBy(x=>x).ToArray();
                    var requested=danhSach.Select(x=>x.IdNhanVien).OrderBy(x=>x).ToArray();
                    if(!actual.SequenceEqual(requested))
                        throw new DBConcurrencyException("Danh sach nhan vien da thay doi, khong the chot mot phan.");
                    DateTime nowUtc=DateTime.UtcNow;
                    foreach(var info in current.NhanVien)
                    {
                        var row=danhSach.Single(x=>x.IdNhanVien==info.IdNhanVien);
                        if((row.PhutHanhChinh!=info.PhutHanhChinhDeXuat || row.PhutTangCa!=info.PhutTangCaDeXuat)
                           && string.IsNullOrWhiteSpace(row.LyDoDieuChinh))
                            throw new InvalidOperationException("Can nhap ly do khi dieu chinh gio cua "+info.TenNhanVien+".");
                        using(var cmd=Lenh(con,@"INSERT dbo.TblKhaiBaoGioDoiSoat
                          (IdDoiSoat,IdNhanVien,IdCongViec,IdDuAn,LanDoiSoat,PhienBan,LaBanHienTai,
                           SoGiayThuongHeThong,SoGiayTangCaHeThong,SoPhutThuongTuKhai,SoPhutTangCaTuKhai,
                           SoPhutThuongPmChot,SoPhutTangCaPmChot,IdPmChot,
                           LyDoDieuChinh,ChotGioHeThongLuc,NguoiTao,NgayTao,LoaiSuKien,IdThaoTac,LanThucHien)
                          VALUES(NEWID(),@U,@T,@D,@L,1,1,@GS,@GT,@BS,@BT,@PS,@PT,@PM,
                           @Reason,@At,@Actor,@At,4,@Request,
                           (SELECT LanThucHien FROM dbo.TblCongViec WHERE IdDuAn=@D AND IdCongViec=@T))"))
                        {
                            ThamSo(cmd,"@U",System.Data.SqlDbType.UniqueIdentifier,info.IdNhanVien);
                            ThamSo(cmd,"@T",System.Data.SqlDbType.UniqueIdentifier,task);
                            ThamSo(cmd,"@D",System.Data.SqlDbType.UniqueIdentifier,duAn);
                            ThamSo(cmd,"@L",System.Data.SqlDbType.Int,checked(info.LanDoiSoatLonNhat+1));
                            ThamSo(cmd,"@GS",System.Data.SqlDbType.BigInt,info.GiayHanhChinhHeThong);
                            ThamSo(cmd,"@GT",System.Data.SqlDbType.BigInt,info.GiayTangCaHeThong);
                            ThamSo(cmd,"@BS",System.Data.SqlDbType.Int,info.PhutHanhChinhNhanVienBao);
                            ThamSo(cmd,"@BT",System.Data.SqlDbType.Int,info.PhutTangCaNhanVienBao);
                            ThamSo(cmd,"@PS",System.Data.SqlDbType.Int,row.PhutHanhChinh);
                            ThamSo(cmd,"@PT",System.Data.SqlDbType.Int,row.PhutTangCa);
                            ThamSo(cmd,"@PM",System.Data.SqlDbType.UniqueIdentifier,idPm);
                            ThamSo(cmd,"@Reason",System.Data.SqlDbType.NVarChar,string.IsNullOrWhiteSpace(row.LyDoDieuChinh)?(object)DBNull.Value:row.LyDoDieuChinh.Trim());
                            ThamSo(cmd,"@At",System.Data.SqlDbType.DateTime2,nowUtc);
                            ThamSo(cmd,"@Actor",System.Data.SqlDbType.NVarChar,idPm.ToString("D"));
                            ThamSo(cmd,"@Request",System.Data.SqlDbType.UniqueIdentifier,MaThaoTacTheoNhanVien(idYeuCau,info.IdNhanVien));
                            if(cmd.ExecuteNonQuery()!=1)throw new DBConcurrencyException("Khong ghi duoc du so nhan vien can chot.");
                        }
                    }
                    // Doc lai ngay trong transaction va xac thuc day du moi nguoi duoc chot.
                    var finished=Doc(con,duAn,task);
                    if(!finished.DaChotCong || finished.NhanVien.Count!=current.NhanVien.Count)
                        throw new InvalidOperationException("Khong hoan tat du cong tat ca nhan vien; da huy giao dich.");
                    scope.Complete();
                    return finished;
                }
            }
        }
        /// <summary>Mot truy van cho ca GridView du an, khong N+1 SQL theo tung row.</summary>
        // Co nhan vien truc tiep HOAC lich su gio/doi soat: khong bo sot nguoi da go phan cong.
        public HashSet<Guid> LayTaskCoNhanVienHoacGio(Guid duAn)
        {
            var ids=new HashSet<Guid>();
            using(var c=new SweetSoft.QLDA.Core.Respositories.KhaiBaoGio20Repository().MoKetNoi())
            using(var q=Lenh(c,@"SELECT DISTINCT a.IdCongViec FROM dbo.TblCongViec_NhanVien a
                   JOIN dbo.TblCongViec t ON t.IdCongViec=a.IdCongViec WHERE t.IdDuAn=@D
                   UNION SELECT DISTINCT IdCongViec FROM dbo.TblKhaiBaoGioPhien WHERE IdDuAn=@D
                   UNION SELECT DISTINCT IdCongViec FROM dbo.TblKhaiBaoGioDoiSoat WHERE IdDuAn=@D"))
            {
                ThamSo(q,"@D",SqlDbType.UniqueIdentifier,duAn);
                using(var rd=q.ExecuteReader())while(rd.Read())ids.Add(rd.GetGuid(0));
            }
            return ids;
        }
        public Dictionary<Guid,bool> LayTrangThaiNhieuTask(Guid duAn)
        {
            var result=new Dictionary<Guid,bool>();
            if(duAn==Guid.Empty)return result;
            const string sql=@"
;WITH Tasks AS (SELECT IdCongViec FROM dbo.TblCongViec WHERE IdDuAn=@D AND TrangThai IN(2,3) AND ISNULL(DaXoa,0)=0),
CanChot AS (
 SELECT a.IdCongViec,a.IdNhanVien FROM dbo.TblCongViec_NhanVien a INNER JOIN Tasks t ON t.IdCongViec=a.IdCongViec
 UNION SELECT p.IdCongViec,p.IdNhanVien FROM dbo.TblKhaiBaoGioPhien p INNER JOIN Tasks t ON t.IdCongViec=p.IdCongViec WHERE p.IdDuAn=@D
 UNION SELECT s.IdCongViec,s.IdNhanVien FROM dbo.TblKhaiBaoGioDoiSoat s INNER JOIN Tasks t ON t.IdCongViec=s.IdCongViec WHERE s.IdDuAn=@D AND s.LoaiSuKien IN(0,1,2,3,5)
), Moi AS (
 SELECT s.IdCongViec,s.IdNhanVien,s.LoaiSuKien,s.IdPmChot,s.SoPhutThuongPmChot,s.SoPhutTangCaPmChot,
        ROW_NUMBER() OVER(PARTITION BY s.IdCongViec,s.IdNhanVien ORDER BY s.LanDoiSoat DESC,s.PhienBan DESC,s.NgayTao DESC,s.IdDoiSoat DESC) AS Rn
 FROM dbo.TblKhaiBaoGioDoiSoat s INNER JOIN Tasks t ON t.IdCongViec=s.IdCongViec WHERE s.IdDuAn=@D
), Tong AS (
 SELECT c.IdCongViec,COUNT(*) Can,
 SUM(CASE WHEN s.LoaiSuKien=4 AND s.IdPmChot IS NOT NULL AND s.SoPhutThuongPmChot>=0 AND s.SoPhutTangCaPmChot>=0 THEN 1 ELSE 0 END) Da
 FROM CanChot c LEFT JOIN Moi s ON s.IdCongViec=c.IdCongViec AND s.IdNhanVien=c.IdNhanVien AND s.Rn=1 GROUP BY c.IdCongViec
)
SELECT t.IdCongViec,CASE WHEN a.Can>0 AND a.Da=a.Can
 AND NOT EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioPhien p WHERE p.IdDuAn=@D AND p.IdCongViec=t.IdCongViec AND p.KetThucLuc IS NULL)
 AND NOT EXISTS(SELECT 1 FROM Moi x WHERE x.IdCongViec=t.IdCongViec AND x.Rn=1 AND x.LoaiSuKien=4
       AND NOT EXISTS(SELECT 1 FROM CanChot c WHERE c.IdCongViec=x.IdCongViec AND c.IdNhanVien=x.IdNhanVien))
 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END DaChot FROM Tasks t LEFT JOIN Tong a ON a.IdCongViec=t.IdCongViec;";
            using(var c=new SweetSoft.QLDA.Core.Respositories.KhaiBaoGio20Repository().MoKetNoi())
            using(var q=Lenh(c,sql))
            {
                ThamSo(q,"@D",System.Data.SqlDbType.UniqueIdentifier,duAn);
                using(var rd=q.ExecuteReader())while(rd.Read())result[rd.GetGuid(0)]=rd.GetBoolean(1);
            }
            return result;
        }
    }
}
