<%@ WebHandler Language="C#" Class="KhaiBaoGio20Handler" %>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Web.SessionState;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.DataAccess;

// Chay theo cac API V2 da duoc tach rieng. Khong thay endpoint V1.
public sealed class KhaiBaoGio20Handler : IHttpHandler, IReadOnlySessionState
{
    public bool IsReusable { get { return false; } }
    static readonly DateTime Epoch=new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc);
    static long Ms(DateTime utc) { return (long)(DateTime.SpecifyKind(utc,DateTimeKind.Utc)-Epoch).TotalMilliseconds; }
    static long? MsNullable(DateTime? utc) { return utc.HasValue ? (long?)Ms(utc.Value) : null; }
    // Giá trị PHÚT chuẩn cho tab Khai báo giờ thủ công và đối chiếu tính công.
    // Cộng TỔNG giây từng loại giờ trước, làm tròn 1 lần: >=30 giây lên 1 phút.
    // Không thay đổi số giây nguyên bản, không làm tròn theo ngày hay theo phiên.
    static long PhutHeThongDaLamTron(long tongGiay)
    {
        if (tongGiay < 0) throw new ArgumentOutOfRangeException("tongGiay");
        return tongGiay / 60L + (tongGiay % 60L >= 30L ? 1L : 0L);
    }
    static void Write(HttpContext c, object data) { c.Response.Write(new JavaScriptSerializer().Serialize(data)); }
    static void Error(HttpContext c, int status, string message)
    {
        c.Response.StatusCode=status;
        Write(c,new {ok=false,error=message});
    }
    static bool CheckToken(HttpContext c, Guid uid)
    {
        string str=c.Request.Headers["X-KBG20-Token"];
        if(string.IsNullOrEmpty(str) || str.Length>1024) return false;
        try {
            byte[] data=MachineKey.Unprotect(Convert.FromBase64String(str),"KBG20","WebActionV1");
            if(data==null) return false;
            string decoded=Encoding.UTF8.GetString(data);
            for(int k=0;k<2;k++) {
                string expected=uid.ToString("N")+"|"+DateTime.UtcNow.AddDays(-k).ToString("yyyyMMdd",CultureInfo.InvariantCulture);
                if(string.Equals(decoded,expected,StringComparison.Ordinal)) return true;
            }
        } catch(FormatException){} catch(System.Security.Cryptography.CryptographicException){}
        return false;
    }
    // Lich V2 su dung gio Viet Nam, khong phu thuoc mui gio may khach.
    static TimeZoneInfo VietNamZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"); }
    }
    static long LocalMs(DateTime local, TimeZoneInfo zone)
    {
        return Ms(TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local,DateTimeKind.Unspecified),zone));
    }
    public void ProcessRequest(HttpContext context)
    {
        var resp=context.Response;
        resp.ContentType="application/json; charset=utf-8";
        resp.Cache.SetCacheability(HttpCacheability.NoCache);
        resp.Cache.SetNoStore();
        resp.TrySkipIisCustomErrors=true;
        resp.SuppressFormsAuthenticationRedirect=true;
        try {
            if(!KhaiBaoGioManager.KhaiBaoGio20DaKichHoat()) {Error(context,404,"Khai bao gio 2.0 chua duoc kich hoat.");return;}
            AspnetUser user=SweetContext.Current!=null?SweetContext.Current.User:null;
            if(user==null || user.UserId==Guid.Empty){Error(context,401,"not_authenticated");return;}
            if(!user.LaNhanVien){Error(context,403,"not_employee");return;}
            string method=context.Request.HttpMethod;
            if(method=="GET") {
                Guid project=Guid.Empty;
                string raw=context.Request.QueryString["projectId"];
                if(!string.IsNullOrEmpty(raw) && !Guid.TryParse(raw,out project)) {Error(context,400,"Ma du an khong hop le.");return;}
                var man=new KhaiBaoGio20Manager();
                if(string.Equals(context.Request.QueryString["view"],"reconcile-list",StringComparison.OrdinalIgnoreCase)) {
                    if(project==Guid.Empty) {Error(context,400,"Thieu du an.");return;}
                    var items=new Kbg20DoiSoatManager().LayDanhSachDoiSoat(user.UserId,project);
                    Write(context,new {ok=true,items=items.ConvertAll(x=>new {
                        taskId=x.IdCongViec.ToString("D"),code=x.MaCongViec,name=x.TenCongViec,
                        round=x.LanDoiSoat,version=x.PhienBan,
                        regular=x.GiayThuongHeThong,overtime=x.GiayTangCaHeThong,
                        systemRegularMinutes=PhutHeThongDaLamTron(x.GiayThuongHeThong),
                        systemOvertimeMinutes=PhutHeThongDaLamTron(x.GiayTangCaHeThong),
                        declaredRegular=x.PhutThuongTuKhai,declaredOvertime=x.PhutTangCaTuKhai,
                        submitted=x.PhutThuongTuKhai.HasValue && x.PhutTangCaTuKhai.HasValue})});
                    return;
                }
                if(string.Equals(context.Request.QueryString["view"],"reconcile",StringComparison.OrdinalIgnoreCase)) {
                    Guid task;
                    if(project==Guid.Empty || !Guid.TryParse(context.Request.QueryString["taskId"],out task))
                        {Error(context,400,"Task hoac du an khong hop le.");return;}
                    var detail=new Kbg20DoiSoatManager().LayChiTiet(user.UserId,project,task);
                    var v=detail.BanHienTai;
                    if(v==null) {Error(context,409,"Task chua co ban doi soat sau PM duyet.");return;}
                    Write(context,new {ok=true,taskId=task.ToString("D"),code=v.MaCongViec,name=v.TenCongViec,
                        round=v.LanDoiSoat,version=v.PhienBan,
                        regular=v.GiayThuongHeThong,overtime=v.GiayTangCaHeThong,
                        systemRegularMinutes=PhutHeThongDaLamTron(v.GiayThuongHeThong),
                        systemOvertimeMinutes=PhutHeThongDaLamTron(v.GiayTangCaHeThong),
                        declaredRegular=v.PhutThuongTuKhai,declaredOvertime=v.PhutTangCaTuKhai,
                        reason=v.LyDoDieuChinh,
                        history=detail.LichSu.ConvertAll(x=>new {round=x.LanDoiSoat,version=x.PhienBan,
                            regular=x.PhutThuongTuKhai,overtime=x.PhutTangCaTuKhai,
                            systemRegular=x.GiayThuongHeThong,systemOvertime=x.GiayTangCaHeThong,
                            systemRegularMinutes=PhutHeThongDaLamTron(x.GiayThuongHeThong),
                            systemOvertimeMinutes=PhutHeThongDaLamTron(x.GiayTangCaHeThong),
                            reason=x.LyDoDieuChinh,createdUtc=Ms(x.NgayTaoUtc)})});
                    return;
                }
                if(string.Equals(context.Request.QueryString["view"],"history",StringComparison.OrdinalIgnoreCase)) {
                    // Lich su chi duoc doc cho CHINH NHAN VIEN va DU AN hien tai.
                    DateTime day;
                    string rawDay=context.Request.QueryString["day"];
                    if(project==Guid.Empty || !DateTime.TryParseExact(rawDay,"yyyy-MM-dd",
                        CultureInfo.InvariantCulture,DateTimeStyles.None,out day)) {
                        Error(context,400,"Ngay hoac du an khong hop le.");return;
                    }
                    var lichSu=man.LayLichSuTheoNgay(user.UserId,project,day);
                    Write(context,new {ok=true,projectId=project.ToString("D"),day=day.ToString("yyyy-MM-dd"),
                        serverNow=Ms(lichSu.ServerNowUtc),
                        regular=lichSu.SoGiayThuong,overtime=lichSu.SoGiayTangCa,total=lichSu.TongSoGiay,
                        tasks=lichSu.CongViec.ConvertAll(x=>new {id=x.IdCongViec.ToString("D"),
                            code=x.MaCongViec,name=x.TenCongViec,
                            regular=x.SoGiayThuong,overtime=x.SoGiayTangCa,total=x.TongSoGiay})});
                    return;
                }
                var view=man.LayTrangDuAn(user.UserId,project);
                var now=view.ServerNowUtc;
                var zone=VietNamZone();
                DateTime localDay=TimeZoneInfo.ConvertTimeFromUtc(now,zone).Date;
                var shifts=man.LayLichCaTrongNgay(localDay);
                var exception=shifts.Count==0 ? LichBieuChungManager.Instance.GetExceptionByDate(localDay) : null;
                Guid? pmId=DuAnManager.Instance.LayIdNhanVienQuanLy(view.IdDuAn);
                Write(context,new {ok=true,serverNow=Ms(now),workflowEnabled=Kbg20TichHopTask.DuocKichHoat,
                    isPm=pmId.HasValue && pmId.Value==user.UserId,projectId=view.IdDuAn.ToString("D"),
                    workCalendar=new {
                        day=localDay.ToString("yyyy-MM-dd"),
                        isWorkingDay=shifts.Count>0,
                        dayOffReason=exception!=null ? exception.TenNgoaiLe : "Ngày nghỉ theo lịch làm việc",
                        shifts=shifts.ConvertAll(x=>new {type=x.LoaiCa,
                            start=x.GioBatDau,end=x.GioKetThuc,
                            startMs=LocalMs(x.BatDau,zone),endMs=LocalMs(x.KetThuc,zone)})
                    },
                    projects=view.DuAn.ConvertAll(x=>new {id=x.IdDuAn.ToString("D"),code=x.MaDuAn,name=x.TenDuAn}),
                    tasks=view.CongViec.ConvertAll(x=>new {
                        id=x.IdCongViec.ToString("D"),projectId=x.IdDuAn.ToString("D"),
                        parentId=x.IdCongViecCha.HasValue?x.IdCongViecCha.Value.ToString("D"):null,
                        code=x.MaCongViec,name=x.TenCongViec,
                        start=x.NgayBatDau.HasValue?x.NgayBatDau.Value.ToString("dd/MM/yyyy"):null,
                        end=x.NgayKetThuc.HasValue?x.NgayKetThuc.Value.ToString("dd/MM/yyyy"):null,
                        startDay=x.NgayBatDau.HasValue?x.NgayBatDau.Value.ToString("yyyy-MM-dd"):null,
                        endDay=x.NgayKetThuc.HasValue?x.NgayKetThuc.Value.ToString("yyyy-MM-dd"):null,
                        durationDays=x.ThoiHanNgay,
                        status=x.TrangThai,pending=x.ChoDuyet,assigned=x.LaPhanCongTrucTiep,
                        regular=x.SoGiayThuong,overtime=x.SoGiayTangCa,
                        startedRegular=x.DaTungBatDauThuong,startedOvertime=x.DaTungBatDauTangCa}),
                    timers=view.DongHo.ConvertAll(x=>new {
                        id=x.Phien.IdPhien.ToString("D"),taskId=x.Phien.IdCongViec.ToString("D"),
                        projectId=x.Phien.IdDuAn.ToString("D"),code=x.Phien.MaCongViec,name=x.Phien.TenCongViec,
                        projectCode=x.Phien.MaDuAn,projectName=x.Phien.TenDuAn,
                        type=(byte)x.Phien.LoaiGio,source=x.Phien.NguonBatDau,
                        overdue=x.Phien.LaTiepTucQuaHan,
                        seconds=x.SoGiayHienTai,counting=x.DangCongGio,boundary=MsNullable(x.MocDungCongLucUtc)}),
                    history=view.LichSu.ConvertAll(x=>new {
                        id=x.IdPhien.ToString("D"),code=x.MaCongViec,name=x.TenCongViec,
                        type=(byte)x.LoaiGio,source=x.NguonBatDau,
                        started=Ms(x.BatDauLucUtc),ended=MsNullable(x.KetThucLucUtc),seconds=x.SoGiayDaKetToan})
                });
                return;
            }
            if(method!="POST") { resp.AddHeader("Allow","GET, POST");Error(context,405,"method_not_allowed");return; }
            if(!CheckToken(context,user.UserId)){Error(context,403,"Token thao tac khong hop le. Hay tai lai trang.");return;}
            string action=context.Request.Form["action"];
            Guid projectId,taskId,requestId,sessionId;
            Guid.TryParse(context.Request.Form["projectId"],out projectId);
            Guid.TryParse(context.Request.Form["taskId"],out taskId);
            Guid.TryParse(context.Request.Form["requestId"],out requestId);
            Guid.TryParse(context.Request.Form["sessionId"],out sessionId);
            var manager=new KhaiBaoGio20Manager();
            if(action=="saveReconcile") {
                if(!Kbg20TichHopTask.DuocKichHoat){Error(context,403,"Doi soat chua duoc bat.");return;}
                int r,o,v;
                if(projectId==Guid.Empty || taskId==Guid.Empty ||
                   !int.TryParse(context.Request.Form["regularMinutes"],NumberStyles.None,CultureInfo.InvariantCulture,out r) ||
                   !int.TryParse(context.Request.Form["overtimeMinutes"],NumberStyles.None,CultureInfo.InvariantCulture,out o) ||
                   !int.TryParse(context.Request.Form["expectedVersion"],NumberStyles.None,CultureInfo.InvariantCulture,out v))
                   {Error(context,400,"Gia tri doi soat khong hop le.");return;}
                int newVersion=new Kbg20DoiSoatManager().LuuTuKhai(user.UserId,projectId,taskId,r,o,
                    context.Request.Form["reason"],v);
                Write(context,new {ok=true,version=newVersion,message="Da luu phien ban doi soat so "+newVersion+"."});
                return;
            }
            Kbg20KetQua result;
            if(action=="start") {
                result=manager.BatDauGioThuong(user.UserId,projectId,taskId,requestId,
                    context.Request.Form["confirmOverdue"]=="true", context.Request.Form["confirmDependency"]=="true");
            } else if(action=="overtime") {
                result=manager.BatDauTangCa(user.UserId,projectId,taskId,requestId);
            } else if(action=="stop") {
                result=manager.DungPhien(user.UserId,sessionId);
            } else {Error(context,400,"Thao tac khong hop le.");return;}
            Write(context,new {ok=result.ThanhCong,message=result.ThongBao,
                sessionId=result.IdPhien.HasValue?result.IdPhien.Value.ToString("D"):null,
                confirmOverdue=result.CanXacNhanQuaHan,confirmDependency=result.CanXacNhanPhuThuoc,
                warnings=result.CanhBao,autoStarted=result.SoPhienTuDong});
        }
        catch(UnauthorizedAccessException) {Error(context,403,"Khong co quyen xem du an nay.");}
        catch(ArgumentOutOfRangeException) {Error(context,400,"Ngay xem nam ngoai pham vi hop le.");}
        catch(Exception ex) {
            System.Diagnostics.Trace.TraceError("KBG20 handler: {0}",ex);
            Error(context,500,"Loi he thong. Vui long kiem tra log may chu.");
        }
    }
}
