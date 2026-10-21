// KhaiBaoGio 2.0 - doi soat gio TU KHAI sau khi PM duyet.
// File moi. KHONG thay doi cac bang/timer V1, khong cap nhat gio he thong.
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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
                WHERE s.IdNhanVien=@U AND s.IdDuAn=@D AND s.LaBanHienTai=1
                  AND s.LanDoiSoat=(SELECT MAX(x.LanDoiSoat) FROM dbo.TblKhaiBaoGioDoiSoat x
                    WHERE x.IdNhanVien=s.IdNhanVien AND x.IdCongViec=s.IdCongViec)
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
                    WHERE s.IdNhanVien=@U AND s.IdDuAn=@D AND s.IdCongViec=@T
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
                      WHERE IdNhanVien=@U AND IdDuAn=@D AND IdCongViec=@T AND LaBanHienTai=1
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
        // Dot 2 Realtime: callback dang ky tai OWIN Startup cua BackOffice.
        // Core KHONG tham chieu BackOffice/SignalR. Khong phat khi rollback.
        public static Action<Guid, Guid> SauCommitDongHoTask { get; set; }

        private static void DangKyTinHieuSauCommit(Guid duAn, Guid task)
        {
            // Snapshot delegate de thay doi setting trong luong khac khong gay race.
            Action<Guid, Guid> callback = SauCommitDongHoTask;
            var transaction = Transaction.Current;
            if (callback == null || transaction == null) return;

            // TransactionScope.Required co the tham gia ambient transaction o ngoai.
            // Chi gui sau khi ROOT transaction THUC SU commit, khong gui luc scope con.
            transaction.TransactionCompleted += (sender, args) =>
            {
                if (args == null || args.Transaction == null ||
                    args.Transaction.TransactionInformation.Status != TransactionStatus.Committed)
                    return;

                try
                {
                    // Publisher khong duoc ke thua ambient transaction da hoan tat.
                    using (var noTransaction = new TransactionScope(TransactionScopeOption.Suppress))
                    {
                        callback(duAn, task);
                        noTransaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    // SQL da commit. Realtime loi khong duoc doi ket qua nghiep vu.
                    System.Diagnostics.Trace.TraceError("KBG20 SignalR post-commit task " + task + ": " + ex);
                }
            };
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
                // Gan callback truoc Complete; chi kich hoat neu transaction commit.
                DangKyTinHieuSauCommit(duAn,task);
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
