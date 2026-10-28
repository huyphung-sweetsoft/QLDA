<%@ WebHandler Language="C#" Class="KhaiBaoGio20Handler" %>

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
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
                // Chi tai khoan dang dang nhap duoc doc gio PM chot cua chinh minh.
                if(string.Equals(context.Request.QueryString["view"],"pm-final",StringComparison.OrdinalIgnoreCase)) {
                    Guid task,eventId=Guid.Empty;
                    if(project==Guid.Empty || !Guid.TryParse(context.Request.QueryString["taskId"],out task) || task==Guid.Empty)
                        {Error(context,400,"Thiếu dự án hoặc công việc cần xem.");return;}
                    string rawEvent=context.Request.QueryString["eventId"];
                    if(!string.IsNullOrWhiteSpace(rawEvent) && !Guid.TryParse(rawEvent,out eventId))
                        {Error(context,400,"Mã kết quả giờ công không hợp lệ.");return;}
                    var pmFinalDetail=new Kbg20PmChotCongTaskManager().LayKetQuaCaNhan(user.UserId,project,task,eventId);
                    Func<Kbg20PmChotCongTaskManager.Kbg20GioDaChot,object> map=x=>new {
                        id=x.IdBan.ToString("D"),pmId=x.IdPm.ToString("D"),regularMinutes=x.PhutHanhChinh,
                        overtimeMinutes=x.PhutTangCa,declaredRegular=x.PhutNhanVienBaoHanhChinh,
                        declaredOvertime=x.PhutNhanVienBaoTangCa,systemRegularMinutes=PhutHeThongDaLamTron(x.GiayHanhChinhHeThong),
                        systemOvertimeMinutes=PhutHeThongDaLamTron(x.GiayTangCaHeThong),
                        reason=x.LyDo,createdUtc=Ms(x.NgayTaoUtc)
                    };
                    Write(context,new {ok=true,code=pmFinalDetail.MaCongViec,name=pmFinalDetail.TenCongViec,
                        selected=map(pmFinalDetail.BanDuocThongBao),latest=map(pmFinalDetail.BanMoiNhat),
                        history=pmFinalDetail.LichSu.Select(map).ToArray()});return;
                }
                if(string.Equals(context.Request.QueryString["view"],"switch-preview",StringComparison.OrdinalIgnoreCase)) {
                    Guid target;
                    if(!Guid.TryParse(context.Request.QueryString["taskId"],out target)||target==Guid.Empty)
                        {Error(context,400,"Thiếu công việc chuẩn bị Start.");return;}
                    var eligible=man.LayPhienThuCongTaskLaCoTheDung(user.UserId,target);
                    Write(context,new {ok=true,eligibleIds=eligible.ConvertAll(x=>x.ToString("D"))});return;
                }
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
                // Ca nhan: Doc trang thai bao/rut, khong dua tren Task.TrangThai=2/3.
                if(string.Equals(context.Request.QueryString["view"],"personal-list",StringComparison.OrdinalIgnoreCase)) {
                    if(project==Guid.Empty){Error(context,400,"Thieu du an.");return;}
                    var personalManager=new Kbg20BaoXongManager();
                    var statuses=personalManager.LayTomTat(user.UserId,project);
                    var pending=personalManager.LayViecCanXacNhan(user.UserId,project);
                    var taskStates=new Dictionary<Guid,Kbg20ViecChoXacNhanGio>();
                    foreach(var candidate in pending)taskStates[candidate.IdCongViec]=candidate;
                    // Task hoan thanh/cho duyet ma chua BAO lan nao van phai co trong Tab 3.
                    var results=new List<object>();
                    foreach(var x in statuses.Values) {
                        Kbg20ViecChoXacNhanGio taskState;
                        bool known=taskStates.TryGetValue(x.IdCongViec,out taskState);
                        results.Add(new {
                            taskId=x.IdCongViec.ToString("D"),code=x.MaCongViec,name=x.TenCongViec,
                            status=x.CoGocXacNhanSauTask?5:x.LoaiSuKien,round=x.Lan,version=x.PhienBan,
                            taskStatus=known?taskState.TrangThai:x.TrangThaiTask,deleted=x.DaXoa,
                            systemRegularMinutes=known?PhutHeThongDaLamTron(taskState.GiayHanhChinh):PhutHeThongDaLamTron(x.GiayThuong),
                            systemOvertimeMinutes=known?PhutHeThongDaLamTron(taskState.GiayTangCa):PhutHeThongDaLamTron(x.GiayTangCa),
                            declaredRegular=(x.DangBaoXong||x.LoaiSuKien==4)?x.PhutTuKhaiThuong:(int?)null,
                            declaredOvertime=(x.DangBaoXong||x.LoaiSuKien==4)?x.PhutTuKhaiTangCa:(int?)null,
                            pmLocked=x.DaPmChot,createdUtc=Ms(x.NgayTaoUtc)
                        });
                    }
                    foreach(var x in pending)if(!statuses.ContainsKey(x.IdCongViec)) {
                        results.Add(new {
                            taskId=x.IdCongViec.ToString("D"),code=x.MaCongViec,name=x.TenCongViec,
                            status=0,round=0,version=0,taskStatus=x.TrangThai,deleted=false,
                            systemRegularMinutes=PhutHeThongDaLamTron(x.GiayHanhChinh),
                            systemOvertimeMinutes=PhutHeThongDaLamTron(x.GiayTangCa),
                            declaredRegular=(int?)null,declaredOvertime=(int?)null,
                            pmLocked=false,createdUtc=(long?)null
                        });
                    }
                    Write(context,new {ok=true,items=results});return;
                }
                if(string.Equals(context.Request.QueryString["view"],"personal",StringComparison.OrdinalIgnoreCase)) {
                    Guid task;
                    if(project==Guid.Empty || !Guid.TryParse(context.Request.QueryString["taskId"],out task))
                        {Error(context,400,"Du an/cong viec khong hop le.");return;}
                    var detail=new Kbg20BaoXongManager().LayChiTiet(user.UserId,project,task);
                    var v=detail.HienTai;
                    Write(context,new {ok=true,taskId=task.ToString("D"),
                        status=v==null?0:(v.CoGocXacNhanSauTask?5:v.LoaiSuKien),round=v==null?0:v.Lan,version=v==null?0:v.PhienBan,
                        canEdit=!detail.KhoaPm && !detail.DaXoa && v!=null && v.DangBaoXong,
                        canWithdraw=!detail.KhoaPm && !detail.DaXoa && detail.TrangThaiTask==1 && v!=null &&
                            (v.LoaiSuKien==1 || v.LoaiSuKien==3),
                        canConfirm=!detail.KhoaPm && !detail.DaXoa && (detail.TrangThaiTask==4 ||
                            detail.TrangThaiTask==2 || detail.TrangThaiTask==3) &&
                            (v==null || v.LoaiSuKien==2),
                        taskStatus=detail.TrangThaiTask,deleted=detail.DaXoa,
                        assigned=detail.ConPhanCong,pmLocked=detail.KhoaPm,
                        regular=detail.GiayThuongHeThong,overtime=detail.GiayTangCaHeThong,
                        systemRegularMinutes=PhutHeThongDaLamTron(detail.GiayThuongHeThong),
                        systemOvertimeMinutes=PhutHeThongDaLamTron(detail.GiayTangCaHeThong),
                        declaredRegular=v==null||v.LoaiSuKien==2?null:v.PhutTuKhaiThuong,
                        declaredOvertime=v==null||v.LoaiSuKien==2?null:v.PhutTuKhaiTangCa,
                        history=detail.LichSu.ConvertAll(x=>new {kind=x.CoGocXacNhanSauTask?5:x.LoaiSuKien,
                            editedAfterConfirm=x.LoaiSuKien==3 && x.CoGocXacNhanSauTask,
                            round=x.Lan,version=x.PhienBan,
                            regular=x.PhutTuKhaiThuong,overtime=x.PhutTuKhaiTangCa,
                            systemRegular=x.GiayThuong,systemOvertime=x.GiayTangCa,
                            reason=x.LyDo,createdUtc=Ms(x.NgayTaoUtc)})});return;
                }
                if(string.Equals(context.Request.QueryString["view"],"pm-personal",StringComparison.OrdinalIgnoreCase)) {
                    Guid task,employee;
                    if(project==Guid.Empty || !Guid.TryParse(context.Request.QueryString["taskId"],out task) ||
                        !Guid.TryParse(context.Request.QueryString["employeeId"],out employee))
                        {Error(context,400,"Thiếu dự án, công việc hoặc nhân viên.");return;}
                    // KHONG cho phep doc nhan vien khac bang query-string neu khong phai PM cua du an.
                    var pmQuanLyId=DuAnManager.Instance.LayIdNhanVienQuanLy(project);
                    if(!pmQuanLyId.HasValue || pmQuanLyId.Value!=user.UserId)
                        {Error(context,403,"Chi PM du an duoc xem noi dung bao/rut cua nhan vien.");return;}
                    var detail=new Kbg20BaoXongManager().LayChiTiet(employee,project,task);
                    var v=detail.HienTai;
                    var staff=AspnetUser.FetchByID(employee);
                    string staffName=staff!=null && !string.IsNullOrWhiteSpace(staff.DisplayName)?staff.DisplayName:"Nhân viên";
                    Write(context,new {ok=true,employeeName=staffName,@readonly=true,taskId=task.ToString("D"),employeeId=employee.ToString("D"),
                        status=v==null?0:(v.CoGocXacNhanSauTask?5:v.LoaiSuKien),round=v==null?0:v.Lan,version=v==null?0:v.PhienBan,
                        taskStatus=detail.TrangThaiTask,deleted=detail.DaXoa,pmLocked=detail.KhoaPm,
                        systemRegularMinutes=PhutHeThongDaLamTron(detail.GiayThuongHeThong),
                        systemOvertimeMinutes=PhutHeThongDaLamTron(detail.GiayTangCaHeThong),
                        declaredRegular=v==null||v.LoaiSuKien==2?null:v.PhutTuKhaiThuong,
                        declaredOvertime=v==null||v.LoaiSuKien==2?null:v.PhutTuKhaiTangCa,
                        history=detail.LichSu.ConvertAll(x=>new {kind=x.CoGocXacNhanSauTask?5:x.LoaiSuKien,
                            editedAfterConfirm=x.LoaiSuKien==3 && x.CoGocXacNhanSauTask,
                            round=x.Lan,version=x.PhienBan,
                            regular=x.PhutTuKhaiThuong,overtime=x.PhutTuKhaiTangCa,
                            systemRegular=x.GiayThuong,systemOvertime=x.GiayTangCa,
                            reason=x.LyDo,createdUtc=Ms(x.NgayTaoUtc)})});return;
                }
                 // Read-only per-task session timeline for Tab 2. Never expose another employee's hours.
                 if(string.Equals(context.Request.QueryString["view"],"history-sessions",StringComparison.OrdinalIgnoreCase)) {
                     Guid chiTietTaskGuid;
                     DateTime chiTietNgay;
                     if(project==Guid.Empty || !Guid.TryParse(context.Request.QueryString["taskId"],out chiTietTaskGuid)
                       || !DateTime.TryParseExact(context.Request.QueryString["day"],"yyyy-MM-dd",
                           CultureInfo.InvariantCulture,DateTimeStyles.None,out chiTietNgay))
                       {Error(context,400,"Ngày hoặc công việc không hợp lệ.");return;}
                     var chiTietPhien=man.LayPhienChiTietTheoNgay(user.UserId,project,chiTietTaskGuid,chiTietNgay);
                     var thongTinNguon=man.LayNguonStartTheoPhien(user.UserId,project,chiTietPhien.Select(p=>p.IdPhien).ToArray(),null);
                     Write(context,new {ok=true,projectId=project.ToString("D"),taskId=chiTietTaskGuid.ToString("D"),
                         day=chiTietNgay.ToString("yyyy-MM-dd"),items=chiTietPhien.ConvertAll(p=>new {
                         id=p.IdPhien.ToString("D"),type=p.LoaiGio,
                         started=Ms(p.BatDauLucUtc),ended=MsNullable(p.KetThucLucUtc),
                         committedSeconds=p.SoGiayDaKetToanTrongNgay,
                         source=thongTinNguon.ContainsKey(p.IdPhien)?thongTinNguon[p.IdPhien].NguonBatDau:0,
                         triggerTaskCode=thongTinNguon.ContainsKey(p.IdPhien)?thongTinNguon[p.IdPhien].MaCongViecKichHoat:null,
                         triggerTaskName=thongTinNguon.ContainsKey(p.IdPhien)?thongTinNguon[p.IdPhien].TenCongViecKichHoat:null,
                         triggerActor=thongTinNguon.ContainsKey(p.IdPhien)?thongTinNguon[p.IdPhien].TenNguoiKichHoat:null})});return;
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
                    var dauMocSomNhatKy=man.LayDauMocStartSom(user.UserId,project);
                    var nguonStartNhatKy=man.LayNguonStartTheoPhien(user.UserId,project,null,day);
                    DateTime batDauNgayUtc=DateTime.SpecifyKind(day.Date.AddHours(-7),DateTimeKind.Utc);
                    DateTime ketThucNgayUtc=batDauNgayUtc.AddDays(1);
                    Write(context,new {ok=true,projectId=project.ToString("D"),day=day.ToString("yyyy-MM-dd"),
                        serverNow=Ms(lichSu.ServerNowUtc),
                        regular=lichSu.SoGiayThuong,overtime=lichSu.SoGiayTangCa,total=lichSu.TongSoGiay,
                        tasks=lichSu.CongViec.ConvertAll(x=>new {id=x.IdCongViec.ToString("D"),
                            code=x.MaCongViec,name=x.TenCongViec,
                            regular=x.SoGiayThuong,overtime=x.SoGiayTangCa,total=x.TongSoGiay,
                            hadActivity=x.CoHoatDong,isOpen=x.DangCoPhienMo,deleted=x.DaXoa,
                            startSources=nguonStartNhatKy.Values.Where(src=>src.IdCongViec==x.IdCongViec
                                && src.BatDauUtc<ketThucNgayUtc && (!src.KetThucUtc.HasValue || src.KetThucUtc.Value>=batDauNgayUtc))
                                .Select(src=>new {source=src.NguonBatDau,triggerTaskCode=src.MaCongViecKichHoat,
                                   triggerTaskName=src.TenCongViecKichHoat,triggerActor=src.TenNguoiKichHoat}).ToList(),
                            isEarly=dauMocSomNhatKy.ContainsKey(x.IdCongViec),
                            earlyInferred=dauMocSomNhatKy.ContainsKey(x.IdCongViec) && dauMocSomNhatKy[x.IdCongViec].LaSuyDoan,
                            earlyStarted=dauMocSomNhatKy.ContainsKey(x.IdCongViec)?
                                Ms(dauMocSomNhatKy[x.IdCongViec].BatDauUtc):(long?)null,
                            earlyPlan=dauMocSomNhatKy.ContainsKey(x.IdCongViec)?
                                dauMocSomNhatKy[x.IdCongViec].NgayBatDauKeHoach.ToString("dd/MM/yyyy"):null,
                            earlyType=dauMocSomNhatKy.ContainsKey(x.IdCongViec)?
                                (byte?)dauMocSomNhatKy[x.IdCongViec].LoaiGio:null,
                            isOverdue=x.NgayKetThucDuKien.HasValue &&
                                lichSu.NgayLam.Date>x.NgayKetThucDuKien.Value.Date,
                            personalStatus=x.LoaiSuKienCaNhan})});
                    return;
                }
                var view=man.LayTrangDuAn(user.UserId,project);
                var now=view.ServerNowUtc;
                var zone=VietNamZone();
                DateTime localDay=TimeZoneInfo.ConvertTimeFromUtc(now,zone).Date;
                var shifts=man.LayLichCaTrongNgay(localDay);
                var previousShifts=man.LayLichCaTrongNgay(localDay.AddDays(-1));
                var exception=shifts.Count==0 ? LichBieuChungManager.Instance.GetExceptionByDate(localDay) : null;
                Guid? pmId=DuAnManager.Instance.LayIdNhanVienQuanLy(view.IdDuAn);
                var personalStates=new Kbg20BaoXongManager().LayTomTat(user.UserId,project);
                var earlyBlockers=man.LayViecCanXuLyTruocStartSom(user.UserId,view.IdDuAn,localDay);
                var dauMocSom=man.LayDauMocStartSom(user.UserId,view.IdDuAn);
                var nguonPhien=man.LayNguonStartTheoPhien(user.UserId,view.IdDuAn,
                    view.DongHo.Select(x=>x.Phien.IdPhien).Concat(view.LichSu.Select(x=>x.IdPhien)).ToArray(),null);
                Write(context,new {ok=true,serverNow=Ms(now),workflowEnabled=Kbg20TichHopTask.DuocKichHoat,
                    earlyStart=new {allowed=earlyBlockers.Count==0,remainingCount=earlyBlockers.Count,
                        blockingTasks=earlyBlockers.GetRange(0,Math.Min(4,earlyBlockers.Count)).ToArray()},
                    isPm=pmId.HasValue && pmId.Value==user.UserId,projectId=view.IdDuAn.ToString("D"),
                    workCalendar=new {
                        day=localDay.ToString("yyyy-MM-dd"),
                        isWorkingDay=shifts.Count>0,
                        dayOffReason=exception!=null ? exception.TenNgoaiLe : "Ngày nghỉ theo lịch làm việc",
                        shifts=shifts.ConvertAll(x=>new {type=x.LoaiCa,
                            start=x.GioBatDau,end=x.GioKetThuc,
                            startMs=LocalMs(x.BatDau,zone),endMs=LocalMs(x.KetThuc,zone)}),
                        previousShifts=previousShifts.ConvertAll(x=>new {
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
                        personalStatus=personalStates.ContainsKey(x.IdCongViec)
                             ?(personalStates[x.IdCongViec].CoGocXacNhanSauTask?5:personalStates[x.IdCongViec].LoaiSuKien):0,
                        personalVersion=personalStates.ContainsKey(x.IdCongViec)?personalStates[x.IdCongViec].PhienBan:0,
                        regular=x.SoGiayThuong,overtime=x.SoGiayTangCa,
                        startedRegular=x.DaTungBatDauThuong,startedOvertime=x.DaTungBatDauTangCa,
                        isEarly=dauMocSom.ContainsKey(x.IdCongViec),
                        earlyInferred=dauMocSom.ContainsKey(x.IdCongViec) && dauMocSom[x.IdCongViec].LaSuyDoan,
                        earlyStarted=dauMocSom.ContainsKey(x.IdCongViec)?
                            Ms(dauMocSom[x.IdCongViec].BatDauUtc):(long?)null,
                        earlyPlan=dauMocSom.ContainsKey(x.IdCongViec)?
                            dauMocSom[x.IdCongViec].NgayBatDauKeHoach.ToString("dd/MM/yyyy"):null,
                        earlyType=dauMocSom.ContainsKey(x.IdCongViec)?
                            (byte?)dauMocSom[x.IdCongViec].LoaiGio:null}),
                    timers=view.DongHo.ConvertAll(x=>new {
                        id=x.Phien.IdPhien.ToString("D"),taskId=x.Phien.IdCongViec.ToString("D"),
                        projectId=x.Phien.IdDuAn.ToString("D"),code=x.Phien.MaCongViec,name=x.Phien.TenCongViec,
                        projectCode=x.Phien.MaDuAn,projectName=x.Phien.TenDuAn,
                        type=(byte)x.Phien.LoaiGio,source=x.Phien.NguonBatDau,
                        triggerTaskCode=nguonPhien.ContainsKey(x.Phien.IdPhien)?nguonPhien[x.Phien.IdPhien].MaCongViecKichHoat:null,
                        triggerTaskName=nguonPhien.ContainsKey(x.Phien.IdPhien)?nguonPhien[x.Phien.IdPhien].TenCongViecKichHoat:null,
                        triggerActor=nguonPhien.ContainsKey(x.Phien.IdPhien)?nguonPhien[x.Phien.IdPhien].TenNguoiKichHoat:null,
                        overdue=x.Phien.LaTiepTucQuaHan,
                        seconds=x.SoGiayHienTai,counting=x.DangCongGio,boundary=MsNullable(x.MocDungCongLucUtc)}),
                    history=view.LichSu.ConvertAll(x=>new {
                        id=x.IdPhien.ToString("D"),taskId=x.IdCongViec.ToString("D"),
                        code=x.MaCongViec,name=x.TenCongViec,
                        type=(byte)x.LoaiGio,source=x.NguonBatDau,
                        triggerTaskCode=nguonPhien.ContainsKey(x.IdPhien)?nguonPhien[x.IdPhien].MaCongViecKichHoat:null,
                        triggerTaskName=nguonPhien.ContainsKey(x.IdPhien)?nguonPhien[x.IdPhien].TenCongViecKichHoat:null,
                        triggerActor=nguonPhien.ContainsKey(x.IdPhien)?nguonPhien[x.IdPhien].TenNguoiKichHoat:null,
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
            // DOT2: Bao/rut/sua CHINH CA NHAN, khong thay doi trang thai Task chung.
            if(action=="reportPersonal" || action=="withdrawPersonal" || action=="editPersonal" || action=="confirmPersonalHours") {
                if(!Kbg20TichHopTask.DuocKichHoat){Error(context,403,"Chuc nang bao xong ca nhan chua duoc kich hoat.");return;}
                if(projectId==Guid.Empty || taskId==Guid.Empty || requestId==Guid.Empty)
                    {Error(context,400,"Thieu du an, cong viec hoac ma thao tac.");return;}
                var personal=new Kbg20BaoXongManager();
                Kbg20BaoXongBanGhi saved;
                int r,o,v,round;
                if(action=="reportPersonal" || action=="editPersonal" || action=="confirmPersonalHours") {
                    if(!int.TryParse(context.Request.Form["regularMinutes"],NumberStyles.None,CultureInfo.InvariantCulture,out r) ||
                       !int.TryParse(context.Request.Form["overtimeMinutes"],NumberStyles.None,CultureInfo.InvariantCulture,out o))
                        {Error(context,400,"So phut tu khai khong hop le.");return;}
                    if(action=="reportPersonal")saved=personal.BaoXong(user.UserId,projectId,taskId,requestId,r,o,context.Request.Form["reason"]);
                    else if(action=="confirmPersonalHours")saved=personal.XacNhanGioSauTask(user.UserId,projectId,taskId,requestId,r,o,context.Request.Form["reason"]);
                    else {
                        if(!int.TryParse(context.Request.Form["expectedVersion"],out v) ||
                           !int.TryParse(context.Request.Form["expectedRound"],out round) || round<=0)
                            {Error(context,400,"Thieu lan bao hoac phien ban. Vui long tai lai trang.");return;}
                        saved=personal.SuaTuKhai(user.UserId,projectId,taskId,requestId,round,v,r,o,context.Request.Form["reason"]);
                    }
                } else {
                    if(!int.TryParse(context.Request.Form["expectedVersion"],out v) ||
                       !int.TryParse(context.Request.Form["expectedRound"],out round) || round<=0)
                        {Error(context,400,"Thieu lan bao hoac phien ban. Vui long tai lai trang.");return;}
                    saved=personal.RutBao(user.UserId,projectId,taskId,requestId,round,v,context.Request.Form["reason"]);
                }
                Write(context,new {ok=true,status=saved==null?0:saved.LoaiSuKien,
                    version=saved==null?0:saved.PhienBan,
                    message=action=="withdrawPersonal"?"Đã rút báo hoàn thành phần việc. Giờ công vẫn được giữ nguyên.":
                        action=="reportPersonal"?"Đã gửi báo hoàn thành phần việc. Số giờ bạn báo đã được ghi nhận.":
                        action=="confirmPersonalHours"?"Đã xác nhận giờ công sau khi Task chung chờ duyệt/hoàn thành. Không thay đổi trạng thái Task.":"Đã cập nhật số giờ đã báo."});
                return;
            }
            // Endpoint cu khong duoc tiep tuc chinh gio ngoai luong bao ca nhan.
            if(action=="saveReconcile") {
                Error(context,409,"Quy trinh tu khai da thay doi. Vui long vao Tab 3 va su dung ban bao xong ca nhan.");return;
            }
            Kbg20KetQua result;
            if(action=="switchStart") {
                if(projectId==Guid.Empty||taskId==Guid.Empty||requestId==Guid.Empty)
                    {Error(context,400,"Thiếu dự án, Task hoặc mã thao tác.");return;}
                var rawList=context.Request.Form["stopSessionIds"]??"";
                var rawIds=rawList.Split(new[]{','},StringSplitOptions.RemoveEmptyEntries);
                if(rawIds.Length==0 || rawIds.Length>100){Error(context,400,"Danh sách phiên cần dừng không hợp lệ.");return;}
                var stops=new List<Guid>();
                foreach(var rawId in rawIds){Guid parsed;
                    if(!Guid.TryParse(rawId,out parsed)){Error(context,400,"ID phiên cần dừng không hợp lệ.");return;}
                    stops.Add(parsed);
                }
                // Older clients may omit the snapshot: keep the previous API contract.
                // New clients always send it to reject stale stop/keep choices.
                var expectedRaw=context.Request.Form["expectedSessionIds"];
                List<Guid> expected=null;
                if(expectedRaw!=null){
                    var expectedParts=expectedRaw.Split(new[]{','},StringSplitOptions.RemoveEmptyEntries);
                    if(expectedParts.Length>100){Error(context,400,"Danh sách đồng hồ quá dài.");return;}
                    expected=new List<Guid>();
                    foreach(var rawId in expectedParts){Guid parsed;
                        if(!Guid.TryParse(rawId,out parsed)||parsed==Guid.Empty||expected.Contains(parsed))
                            {Error(context,400,"Danh sách đồng hồ đã xem không hợp lệ.");return;}
                        expected.Add(parsed);
                    }
                }
                result=manager.DungPhienThuCongVaBatDau(user.UserId,projectId,taskId,requestId,stops,
                    context.Request.Form["hourType"]=="2",context.Request.Form["confirmOverdue"]=="true",
                    context.Request.Form["confirmDependency"]=="true",expected);
            } else if(action=="start") {
                result=manager.BatDauGioThuong(user.UserId,projectId,taskId,requestId,
                    context.Request.Form["confirmOverdue"]=="true", context.Request.Form["confirmDependency"]=="true");
            } else if(action=="overtime") {
                result=manager.BatDauTangCa(user.UserId,projectId,taskId,requestId,
                    context.Request.Form["confirmOverdue"]=="true",context.Request.Form["confirmDependency"]=="true");
            } else if(action=="stop") {
                result=manager.DungPhien(user.UserId,sessionId);
            } else {Error(context,400,"Thao tac khong hop le.");return;}
            Write(context,new {ok=result.ThanhCong||result.CanXacNhanQuaHan||result.CanXacNhanPhuThuoc,
                success=result.ThanhCong,message=result.ThongBao,
                sessionId=result.IdPhien.HasValue?result.IdPhien.Value.ToString("D"):null,
                confirmOverdue=result.CanXacNhanQuaHan,confirmDependency=result.CanXacNhanPhuThuoc,
                warnings=result.CanhBao,autoStarted=result.SoPhienTuDong});
        }
        catch(DBConcurrencyException ex) {Error(context,409,ex.Message);}
        catch(ArgumentOutOfRangeException) {Error(context,400,"Gia tri nam ngoai pham vi hop le.");}
        catch(ArgumentException ex) {Error(context,400,ex.Message);}
        catch(InvalidOperationException ex) {Error(context,409,ex.Message);}
        catch(UnauthorizedAccessException) {Error(context,403,"Khong co quyen thuc hien thao tac nay.");}
        catch(Exception ex) {
            System.Diagnostics.Trace.TraceError("KBG20 handler: {0}",ex);
            Error(context,500,"Loi he thong. Vui long kiem tra log may chu.");
        }
    }
}
