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
                     ChotGioHeThongLuc,NguoiTao)
                  SELECT NEWID(),u.IdNhanVien,@T,@D,
                     ISNULL((SELECT MAX(x.LanDoiSoat) FROM dbo.TblKhaiBaoGioDoiSoat x WITH(UPDLOCK,HOLDLOCK)
                              WHERE x.IdNhanVien=u.IdNhanVien AND x.IdCongViec=@T),0)+1,
                     1,1,ISNULL(s.Thuong,0),ISNULL(s.TangCa,0),NULL,NULL,@At,N'KBG20_PM_DUYET'
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
            if(phutThuong<0 || phutTangCa<0 || phutThuong>52560000 || phutTangCa>52560000)
                throw new ArgumentOutOfRangeException("phutThuong", "So phut phai hop le va khong am.");
            if(lyDo!=null && lyDo.Length>2000) throw new ArgumentException("Ly do dieu chinh qua 2000 ky tu.");
            using(var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel=System.Transactions.IsolationLevel.ReadCommitted,
                    Timeout=TimeSpan.FromMinutes(2) }))
            using(var c=Open())
            {
                ProjectLock(c,project);
                EnsureEligible(c,nv,project,task);
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
                     LyDoDieuChinh,ChotGioHeThongLuc,NguoiTao)
                    VALUES (NEWID(),@U,@T,@D,@L,@V,1,@R,@O,@SR,@SO,@Reason,@At,@User)"))
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
        public static void LuuHoanThanh(Guid duAn,Guid task,bool pmXacNhan,Action luuTask)
        {
            if(luuTask==null) throw new ArgumentNullException("luuTask");
            if(!DuocKichHoat) { luuTask(); return; }
            using(var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel=System.Transactions.IsolationLevel.ReadCommitted,
                    Timeout=TimeSpan.FromMinutes(2) }))
            {
                var clock=new KhaiBaoGio20Manager();
                clock.ChotTatCaPhienTaskTrongTransactionScope(duAn,task);
                luuTask(); // GIU NGUYEN partner validations + .Save + AutoSet*.
                if(pmXacNhan)
                    new Kbg20DoiSoatManager().TaoDotSauKhiPmDuyetTrongTransactionScope(duAn,task,DateTime.UtcNow);
                scope.Complete();
            }
        }
        public static void DuyetHoanThanh(Guid duAn,Guid task,Action luuTask)
        {
            LuuHoanThanh(duAn,task,true,luuTask);
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
                           "Bạn có thể vào Tính giờ công việc > Đối soát giờ để nhập/sửa giờ tự khai.")
                        : ((nv==nguoiThucHien ? "Bạn" : tenNguoi) + " đã gửi yêu cầu duyệt hoàn thành " + nhanTask +
                           " đến " + tenPm + ". Đồng hồ trực tiếp trên công việc của nhóm đã được chốt; " +
                           "giờ đã ghi được bảo lưu. Nếu PM từ chối, bạn có thể tự Start lại.");
                    // Duong dan noi bo toi trang Tinh gio; PM hoan thanh truc tiep mo doi soat luon.
                    string basePath=(System.Web.HttpRuntime.AppDomainAppVirtualPath ?? "/").TrimEnd('/');
                    string link=basePath+"/fKhaiBaoGio/KhaiBaoGio.aspx?ProjectId="+duAn.ToString("D");
                    if(pmXacNhan) link+="&reconcile=1&taskId="+taskId.ToString("D");
                    ThongBaoManager.Instance.Create(userId:nv,tieuDe:title,noiDung:desc,
                        loaiThongBao:ThongBaoTypes.CongViec,
                        idCongViec:taskId,idDuAn:duAn,duongDanLienKet:link,guiEmail:false);
                } catch(Exception ex) {
                    System.Diagnostics.Trace.TraceError("KBG20 thong bao nhom task " + taskId + ": " + ex);
                }
            }
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
                    if(current.NgayBatDau.HasValue && current.NgayBatDau.Value.Date>DateTime.Today)
                        throw new InvalidOperationException("Task chua den ngay bat dau.");
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
        public long GiayThuong { get; set; }
        public long GiayTangCa { get; set; }
        public int? PhutTuKhaiThuong { get; set; }
        public int? PhutTuKhaiTangCa { get; set; }
        public string LyDo { get; set; }
        public DateTime NgayTaoUtc { get; set; }
        public bool DaPmChot { get; set; }
        public bool DangBaoXong { get { return LoaiSuKien==1 || LoaiSuKien==3; } }
    }
    public sealed class Kbg20BaoXongChiTiet
    {
        public Kbg20BaoXongBanGhi HienTai { get; set; }
        public List<Kbg20BaoXongBanGhi> LichSu { get; set; } = new List<Kbg20BaoXongBanGhi>();
        public long GiayThuongHeThong { get; set; }
        public long GiayTangCaHeThong { get; set; }
        public bool ConPhanCong { get; set; }
        public bool KhoaPm { get; set; }
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
           s.SoPhutThuongTuKhai,s.SoPhutTangCaTuKhai,s.LyDoDieuChinh,s.NgayTao";
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
                // Chi LayTomTat truy van them co khoa PM. Cac lenh doc legacy du 12 cot van hoat dong.
                DaPmChot=r.FieldCount>12 && !r.IsDBNull(12) && Convert.ToBoolean(r.GetValue(12))
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
        { if(reg<0||ot<0||reg>52560000||ot>52560000)throw new ArgumentException("So phut tu khai khong hop le."); }
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
                WHERE s.IdNhanVien=@U AND s.IdDuAn=@D AND s.IdCongViec=@T AND s.LoaiSuKien IN (1,2,3,4)
                ORDER BY s.LanDoiSoat DESC,s.PhienBan DESC,s.NgayTao DESC"))
            { Keys(cmd,nv,project,task);using(var r=cmd.ExecuteReader())return r.Read()?Read(r):null; }
        }
        private static bool LockedByPm(SqlConnection c,SqlTransaction tx,Guid nv,Guid project,Guid task)
        {
            using(var cmd=Cmd(c,tx,@"SELECT TOP(1) 1 FROM dbo.TblKhaiBaoGioDoiSoat WITH(UPDLOCK,HOLDLOCK)
                WHERE IdNhanVien=@U AND IdDuAn=@D AND IdCongViec=@T AND LoaiSuKien=4"))
            { Keys(cmd,nv,project,task);return cmd.ExecuteScalar()!=null; }
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
                  AND LanDoiSoat=@L AND PhienBan=@V AND LaBanHienTai=1 AND LoaiSuKien IN (1,2,3)"))
            { Keys(cmd,nv,project,task);P(cmd,"@L",SqlDbType.Int,lan);P(cmd,"@V",SqlDbType.Int,version);
              if(cmd.ExecuteNonQuery()!=1)throw new DBConcurrencyException("Lich su bao xong da thay doi. Vui long tai lai."); }
        }
        private static void Insert(SqlConnection c,SqlTransaction tx,Guid nv,Guid project,Guid task,
            Guid requestId,int type,int lan,int version,long[] system,int? regular,int? overtime,string note)
        {
            using(var cmd=Cmd(c,tx,@"INSERT dbo.TblKhaiBaoGioDoiSoat
                (IdDoiSoat,IdNhanVien,IdDuAn,IdCongViec,LanDoiSoat,PhienBan,LaBanHienTai,
                 SoGiayThuongHeThong,SoGiayTangCaHeThong,SoPhutThuongTuKhai,SoPhutTangCaTuKhai,
                 LyDoDieuChinh,ChotGioHeThongLuc,NguoiTao,NgayTao,LoaiSuKien,IdThaoTac)
                VALUES(NEWID(),@U,@D,@T,@L,@V,1,@R,@O,@SR,@SO,@Reason,@Now,@Actor,@Now,@Type,@Id)"))
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
              WHERE s.IdNhanVien=@U AND s.IdDuAn=@D AND s.LoaiSuKien IN(1,2,3,4)
                AND NOT EXISTS(SELECT 1 FROM dbo.TblKhaiBaoGioDoiSoat newer
                  WHERE newer.IdCongViec=s.IdCongViec AND newer.IdNhanVien=s.IdNhanVien AND newer.IdDuAn=s.IdDuAn
                    AND newer.LoaiSuKien IN(1,2,3,4)
                    AND (newer.LanDoiSoat>s.LanDoiSoat OR
                      (newer.LanDoiSoat=s.LanDoiSoat AND newer.PhienBan>s.PhienBan)))"))
            { P(cmd,"@U",SqlDbType.UniqueIdentifier,nv);P(cmd,"@D",SqlDbType.UniqueIdentifier,project);
              using(var r=cmd.ExecuteReader())while(r.Read()){var x=Read(r);items[x.IdCongViec]=x;} }
            return items;
        }
        public Kbg20BaoXongChiTiet LayChiTiet(Guid nv,Guid project,Guid task)
        {
            Valid(nv,project,task);var detail=new Kbg20BaoXongChiTiet();
            using(var c=repo.MoKetNoi())
            {
                EnsureView(c,null,nv,project,task);
                detail.ConPhanCong=repo.CoPhanCong(c,null,task,nv);
                detail.KhoaPm=LockedByPm(c,null,nv,project,task);
                using(var cmd=Cmd(c,null,@"SELECT "+Columns+@" FROM dbo.TblKhaiBaoGioDoiSoat s
                    JOIN dbo.TblCongViec t ON t.IdCongViec=s.IdCongViec
                    WHERE s.IdNhanVien=@U AND s.IdDuAn=@D AND s.IdCongViec=@T
                    AND s.LoaiSuKien IN (1,2,3,4)
                    ORDER BY s.LanDoiSoat DESC,s.PhienBan DESC,s.NgayTao DESC"))
                { Keys(cmd,nv,project,task);using(var r=cmd.ExecuteReader())while(r.Read())detail.LichSu.Add(Read(r)); }
                detail.HienTai=detail.LichSu.FirstOrDefault();
                var total=TotalSeconds(c,null,nv,task);
                detail.GiayThuongHeThong=total[0];detail.GiayTangCaHeThong=total[1];
            }
            return detail;
        }
        public Kbg20BaoXongBanGhi BaoXong(Guid nv,Guid project,Guid task,Guid requestId,int regular,int overtime,string note)
        {
            Valid(nv,project,task);ValidMinutes(regular,overtime);ValidReason(note,false);
            if(requestId==Guid.Empty)throw new ArgumentException("Thieu ma thao tac.");
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
                if(last!=null && last.LoaiSuKien!=2)throw new DBConcurrencyException("Ban da bao xong phan viec. Vui long tai lai.");
                // Chu y: transaction CHUNG ket toan + dong phien rieng cua nv + snapshot + insert.
                new KhaiBaoGio20Manager().ChotPhienCaNhanTrongGiaoDich(c,tx,project,task,nv);
                var total=TotalSeconds(c,tx,nv,task);
                int lan=checked(MaxRound(c,tx,nv,project,task)+1);
                Insert(c,tx,nv,project,task,requestId,1,lan,1,total,regular,overtime,note);
                tx.Commit();written=true;
            }
            if(written)NotifyPm(project,task,nv,false);
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
                if(LockedByPm(c,tx,nv,project,task))throw new InvalidOperationException("PM da chot cong. Khong the rut bao.");
                var latest=Latest(c,tx,nv,project,task);
                if(latest==null||!latest.DangBaoXong||latest.Lan!=expectedRound||latest.PhienBan!=expectedVersion)
                    throw new DBConcurrencyException("Trang thai bao xong da thay doi. Hay tai lai.");
                CancelCurrent(c,tx,nv,project,task,latest.Lan,latest.PhienBan);
                Insert(c,tx,nv,project,task,requestId,2,latest.Lan,checked(latest.PhienBan+1),
                    new[]{latest.GiayThuong,latest.GiayTangCa},latest.PhutTuKhaiThuong,latest.PhutTuKhaiTangCa,reason);
                tx.Commit();written=true;
            }
            if(written)NotifyPm(project,task,nv,true);
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
        private static void NotifyPm(Guid project,Guid task,Guid nv,bool withdrawn)
        {
            try
            {
                var pm=DuAnManager.Instance.LayIdNhanVienQuanLy(project);
                if(!pm.HasValue||pm.Value==Guid.Empty)return;
                var cv=TaskManager.Instance.FetchById(task);
                var user=SweetSoft.QLDA.DataAccess.AspnetUser.FetchByID(nv);
                string actor=user!=null&&!string.IsNullOrWhiteSpace(user.DisplayName)?user.DisplayName:"Nhân viên";
                string label=cv==null?"công việc":("["+cv.MaCongViec+"] "+cv.TenCongViec);
                string prefix=(System.Web.HttpRuntime.AppDomainAppVirtualPath??"/").TrimEnd('/');
                ThongBaoManager.Instance.Create(userId:pm.Value,
                    tieuDe:withdrawn?"Nhân viên rút báo xong phần việc":"Nhân viên báo xong phần việc",
                    noiDung:actor+(withdrawn?" đã rút báo hoàn thành phần việc trong ":" đã xác nhận xong phần việc trong ")+label+
                        ". Đây là xác nhận CÁ NHÂN, không phải yêu cầu duyệt hoàn thành Task chung.",
                    loaiThongBao:ThongBaoTypes.CongViec,idCongViec:task,idDuAn:project,
                    duongDanLienKet:prefix+"/fKhaiBaoGio/KhaiBaoGio.aspx?ProjectId="+project.ToString("D"),guiEmail:false);
            }
            catch(Exception ex) { System.Diagnostics.Trace.TraceError("KBG20 thong bao ca nhan: "+ex); }
        }
    }
}
