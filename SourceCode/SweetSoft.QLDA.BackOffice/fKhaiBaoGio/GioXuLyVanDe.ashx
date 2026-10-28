<%@ WebHandler Language="C#" Class="GioXuLyVanDeHandler" %>
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Web.SessionState;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.DataAccess;

/// <summary>
/// Endpoint cho khung "Vấn đề" trong KhaiBaoGio. Không sử dụng handler Task
/// và không thay đổi phiên giờ Task. XSRF token dùng cùng purpose với KBG hiện tại.
/// </summary>
public sealed class GioXuLyVanDeHandler : IHttpHandler, IReadOnlySessionState
{
    public bool IsReusable { get { return false; } }
    private static readonly DateTime Epoch=new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc);
    private static long? Milliseconds(DateTime? value)
    {
        return value.HasValue ? (long?)(DateTime.SpecifyKind(value.Value,DateTimeKind.Utc)-Epoch).TotalMilliseconds : null;
    }
    private static bool CheckToken(HttpContext context, Guid userId)
    {
        string raw=context.Request.Headers["X-KBG20-Token"];
        if(string.IsNullOrEmpty(raw) || raw.Length>1024)return false;
        try
        {
            byte[] data=MachineKey.Unprotect(Convert.FromBase64String(raw),"KBG20","WebActionV1");
            if(data==null)return false;
            string decoded=Encoding.UTF8.GetString(data);
            for(int i=0;i<2;i++)
            {
                string expected=userId.ToString("N")+"|"+DateTime.UtcNow.AddDays(-i).ToString("yyyyMMdd",CultureInfo.InvariantCulture);
                if(string.Equals(decoded,expected,StringComparison.Ordinal))return true;
            }
        }
        catch(FormatException) { }
        catch(CryptographicException) { }
        return false;
    }
    private static void Write(HttpContext context,int code,object data)
    {
        context.Response.StatusCode=code;
        context.Response.TrySkipIisCustomErrors=true;
        context.Response.Write(new JavaScriptSerializer{MaxJsonLength=2097152}.Serialize(data));
    }
    private static Guid ParseGuid(string raw)
    {
        Guid x;return Guid.TryParse(raw,out x)?x:Guid.Empty;
    }
    private static object MapSummary(ThongTinGioXuLyVanDe a)
    {
        return new {id=a.IdVanDe.ToString("D"),code=a.MaVanDe,name=a.TenVanDe,
            status=(int)a.TrangThai,approval=(int)a.TrangThaiDuyetGio,
            assigned=a.DaDuocPhanCong,manager=a.LaQuanLy,
            running=a.DangChay,worked=a.DaTungXuLy,reported=a.DaBaoPhanViec,
            withdrawn=a.DaRutBaoGanNhat,
            mySeconds=a.TongGiayCuaToi,groupSeconds=a.TongGiayDaLuu,
            regularSeconds=a.GiayThuongCuaToi,overtimeSeconds=a.GiayTangCaCuaToi,
            groupRegularSeconds=a.GiayThuongNhom,groupOvertimeSeconds=a.GiayTangCaNhom,
            runningOvertime=a.DangChayTangCa,started=Milliseconds(a.BatDauPhienCuaToiUtc)};
    }
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType="application/json; charset=utf-8";
        context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        context.Response.Cache.SetNoStore();
        context.Response.SuppressFormsAuthenticationRedirect=true;
        try
        {
            if(!KhaiBaoGioManager.KhaiBaoGio20DaKichHoat())
                {Write(context,404,new{ok=false,error="Khai báo giờ chưa được kích hoạt."});return;}
            AspnetUser user=SweetContext.Current==null?null:SweetContext.Current.User;
            if(user==null || user.UserId==Guid.Empty)
                {Write(context,401,new{ok=false,error="Phiên đăng nhập đã hết hạn."});return;}
            if(!user.LaNhanVien && !UserManager.Instance.IsAdministrator(user.UserId))
                {Write(context,403,new{ok=false,error="Tài khoản không có quyền sử dụng Khai báo giờ."});return;}
            bool isAdmin=UserManager.Instance.IsAdministrator(user.UserId);
            bool isPost=string.Equals(context.Request.HttpMethod,"POST",StringComparison.OrdinalIgnoreCase);
            bool isGet=string.Equals(context.Request.HttpMethod,"GET",StringComparison.OrdinalIgnoreCase);
            if(!isPost && !isGet)
                {Write(context,405,new{ok=false,error="Phương thức không được hỗ trợ."});return;}
            Guid projectId=ParseGuid(isPost?context.Request.Form["projectId"]:context.Request.QueryString["projectId"]);
            Guid issueId=ParseGuid(isPost?context.Request.Form["vanDeId"]:context.Request.QueryString["vanDeId"]);
            if(projectId==Guid.Empty){Write(context,400,new{ok=false,error="Thiếu mã dự án."});return;}
            var workflow=new GioXuLyVanDeManager();
            if(isGet)
            {
                string view=context.Request.QueryString["view"]??"list";
                if(string.Equals(view,"history",StringComparison.OrdinalIgnoreCase))
                {
                    DateTime day;
                    if(!DateTime.TryParseExact(context.Request.QueryString["day"],"yyyy-MM-dd",
                        CultureInfo.InvariantCulture,DateTimeStyles.None,out day)
                        || day.Date>DateTime.UtcNow.AddHours(7).Date)
                        {Write(context,400,new{ok=false,error="Ngày nhật ký không hợp lệ."});return;}
                    var log=workflow.LayNhatKy(projectId,user.UserId,day);
                    Write(context,200,new{ok=true,items=log.ConvertAll(x=>new{
                        id=x.IdVanDe.ToString("D"),code=x.MaVanDe,name=x.TenVanDe,
                        seconds=x.Giay,regular=x.GiayThuong,overtime=x.GiayTangCa,
                        running=x.DangChay,type="issue"})});return;
                }
                if(string.Equals(view,"history-sessions",StringComparison.OrdinalIgnoreCase))
                {
                    DateTime day;
                    if(issueId==Guid.Empty || !DateTime.TryParseExact(context.Request.QueryString["day"],"yyyy-MM-dd",
                        CultureInfo.InvariantCulture,DateTimeStyles.None,out day)
                        || day.Date>DateTime.UtcNow.AddHours(7).Date)
                        {Write(context,400,new{ok=false,error="Ngày hoặc mã vấn đề không hợp lệ."});return;}
                    var rows=workflow.LayPhienTrongNgay(projectId,issueId,user.UserId,isAdmin,day);
                    Write(context,200,new{ok=true,items=rows.ConvertAll(x=>new{
                        started=Milliseconds(x.BatDauUtc),ended=Milliseconds(x.KetThucUtc),
                        overtime=x.LaTangCa,source="manual",committedSeconds=x.GiayDaKetToanTrongNgay
                    })});return;
                }
                if(string.Equals(view,"ledger",StringComparison.OrdinalIgnoreCase))
                {
                    var rows=workflow.LayDanhSach(projectId,user.UserId,isAdmin);
                    var byIssue=workflow.LayBangKeCuaToi(projectId,user.UserId,isAdmin);
                    var ledger=new List<object>();
                    foreach(var row in rows)
                    {
                        if(!row.DaDuocPhanCong && !row.LaQuanLy)continue;
                        GioXuLyVanDeManager.BaoCaoGioVanDe mine;
                        byIssue.TryGetValue(row.IdVanDe,out mine);
                        if(mine==null && !row.LaQuanLy)continue;
                        ledger.Add(new{id=row.IdVanDe.ToString("D"),code=row.MaVanDe,name=row.TenVanDe,
                            type="issue",seconds=mine==null?0L:mine.GiayHeThong,
                            reported=mine!=null&&mine.DaBao,withdrawn=mine!=null&&mine.SuKien==2,chot=mine!=null&&mine.DaChot,
                            systemRegularSeconds=mine==null?0:mine.GiayThuongHeThong,
                            systemOvertimeSeconds=mine==null?0:mine.GiayTangCaHeThong,
                            round=mine==null?0:mine.Lan,version=mine==null?0:mine.PhienBan,
                            regular=mine==null?null:mine.PhutThuongTuKhai,
                            overtime=mine==null?null:mine.PhutTangCaTuKhai,
                            pmRegular=mine==null?null:mine.PhutThuongPmChot,
                            pmOvertime=mine==null?null:mine.PhutTangCaPmChot,
                            approval=(int)row.TrangThaiDuyetGio});
                    }
                    Write(context,200,new{ok=true,items=ledger});return;
                }
                if(string.Equals(view,"list",StringComparison.OrdinalIgnoreCase))
                {
                    var all=workflow.LayDanhSach(projectId,user.UserId,isAdmin);
                    DateTime nowUtc=DateTime.UtcNow;
                    var lich=new KhaiBaoGio20Manager();
                    DateTime day=nowUtc.AddHours(7).Date;
                    var shifts=lich.LayLichCaTrongNgay(day);
                    var prevShifts=lich.LayLichCaTrongNgay(day.AddDays(-1));
                    long serverMs=(long)(nowUtc-Epoch).TotalMilliseconds;
                    Write(context,200,new {ok=true,serverNow=serverMs,
                        workCalendar=new {day=day.ToString("yyyy-MM-dd"),
                            shifts=shifts.ConvertAll(c=>new {startMs=(long)(c.BatDau.AddHours(-7)-Epoch).TotalMilliseconds,
                                endMs=(long)(c.KetThuc.AddHours(-7)-Epoch).TotalMilliseconds}),
                            previousShifts=prevShifts.ConvertAll(c=>new {startMs=(long)(c.BatDau.AddHours(-7)-Epoch).TotalMilliseconds,
                                endMs=(long)(c.KetThuc.AddHours(-7)-Epoch).TotalMilliseconds})},
                        items=all.ConvertAll(x=>{
                            bool running=x.DangChay && x.BatDauPhienCuaToiUtc.HasValue;
                            bool overtime=x.DangChayTangCa;
                            DateTime start=running?x.BatDauPhienCuaToiUtc.Value:nowUtc;
                            DateTime todayStart=day.AddHours(-7);
                            return new {id=x.IdVanDe.ToString("D"),code=x.MaVanDe,name=x.TenVanDe,
                                status=(int)x.TrangThai,approval=(int)x.TrangThaiDuyetGio,
                                assigned=x.DaDuocPhanCong,manager=x.LaQuanLy,
                                running=x.DangChay,worked=x.DaTungXuLy,reported=x.DaBaoPhanViec,
                                withdrawn=x.DaRutBaoGanNhat,
                                mySeconds=x.TongGiayCuaToi,groupSeconds=x.TongGiayDaLuu,
                                regularSeconds=x.GiayThuongCuaToi,overtimeSeconds=x.GiayTangCaCuaToi,
                                groupRegularSeconds=x.GiayThuongNhom,groupOvertimeSeconds=x.GiayTangCaNhom,
                                runningOvertime=x.DangChayTangCa,started=Milliseconds(x.BatDauPhienCuaToiUtc),
                                runningLiveSeconds=running?GioXuLyVanDeManager.GiayPhienChuaKetToan(start,nowUtc,overtime):0L,
                                runningTodaySeconds=running?GioXuLyVanDeManager.GiayPhienChuaKetToan(start>todayStart?start:todayStart,nowUtc,overtime):0L};
                        })});return;
                }
                // PM result belongs to the signed-in employee. Never accept employeeId from the browser.
                // For a notification, validate both ownership and the exact Issue/action before reading history.
                if(string.Equals(view,"pm-final",StringComparison.OrdinalIgnoreCase) && issueId!=Guid.Empty)
                {
                    Guid notificationId=ParseGuid(context.Request.QueryString["notificationId"]);
                    string requestedNotification=context.Request.QueryString["notificationId"];
                    TblThongBao notice=null;
                    if(!string.IsNullOrEmpty(requestedNotification))
                    {
                        if(notificationId==Guid.Empty)
                            {Write(context,400,new{ok=false,error="Mã thông báo không hợp lệ."});return;}
                        notice=ThongBaoManager.Instance.GetById(notificationId);
                        string linkOld="GioXuLyVanDe:"+issueId.ToString("D")+":detail";
                        string linkNew="GioXuLyVanDe:"+issueId.ToString("D")+":pm-final";
                        string title=notice==null?"":notice.TieuDe??"";
                        bool pmNotice=title.IndexOf("chốt giờ",StringComparison.OrdinalIgnoreCase)>=0
                            || title.IndexOf("điều cBhỉnh giờ",StringComparison.OrdinalIgnoreCase)>=0;
                        if(notice==null||notice.DaXoa==true||notice.UserId!=user.UserId
                           ||!notice.IdDuAn.HasValue||notice.IdDuAn.Value!=projectId
                           ||(!string.Equals(notice.DuongDanLienKet,linkOld,StringComparison.OrdinalIgnoreCase)
                               && !string.Equals(notice.DuongDanLienKet,linkNew,StringComparison.OrdinalIgnoreCase))
                           ||!pmNotice)
                            {Write(context,403,new{ok=false,error="Thông báo chốt giờ không thuộc về bạn hoặc không hợp lệ."});return;}
                    }
                    // LayLichSuPmCaNhan verifies the Issue/project and reads only this session's user.
                    var records=workflow.LayLichSuPmCaNhan(projectId,issueId,user.UserId,isAdmin);
                    var matches=new List<GioXuLyVanDeManager.KetQuaPmVanDeTheoLan>();
                    if(notice!=null)
                    {
                        bool adjusted=(notice.TieuDe??"").IndexOf("điều chỉnh",StringComparison.OrdinalIgnoreCase)>=0;
                        foreach(var rec in records)
                        {
                            double difference=Math.Min(Math.Abs((notice.NgayTao-rec.NgayTaoUtc).TotalSeconds),
                                Math.Abs((notice.NgayTao-rec.NgayTaoUtc.AddHours(7)).TotalSeconds));
                            if(rec.LoaiSuKien==(adjusted?6:4)&&difference<=30)matches.Add(rec);
                        }
                    }
                    GioXuLyVanDeManager.KetQuaPmVanDeTheoLan selected=notice==null
                        ? records.Count>0?records[0]:null
                        : matches.Count==1?matches[0]:null;
                    Func<GioXuLyVanDeManager.KetQuaPmVanDeTheoLan,object> map=r=>r==null?null:(object)new{
                        id=r.NgayTaoUtc.Ticks.ToString(CultureInfo.InvariantCulture)+"-"+r.LoaiSuKien.ToString(CultureInfo.InvariantCulture),
                        createdUtc=Milliseconds(r.NgayTaoUtc),kind=(int)r.LoaiSuKien,
                        systemRegularSeconds=r.GiayThuongHeThong,systemOvertimeSeconds=r.GiayTangCaHeThong,
                        declaredRegular=r.PhutThuongTuKhai,declaredOvertime=r.PhutTangCaTuKhai,
                        regularMinutes=r.PhutThuongPmChot,overtimeMinutes=r.PhutTangCaPmChot,reason=r.LyDo};
                    Write(context,200,new {ok=true,notificationMatched=notice==null||matches.Count==1,
                        selected=map(selected),latest=records.Count==0?null:map(records[0]),
                        history=records.ConvertAll(r=>map(r))});return;
                }
                if(string.Equals(view,"detail",StringComparison.OrdinalIgnoreCase) && issueId!=Guid.Empty)
                {
                    ThongTinGioXuLyVanDe a=workflow.LayThongTin(projectId,issueId,user.UserId,isAdmin);
                    var data=new {
                        id=a.IdVanDe.ToString("D"),code=a.MaVanDe,name=a.TenVanDe,
                        status=(int)a.TrangThai,approval=(int)a.TrangThaiDuyetGio,
                        assigned=a.DaDuocPhanCong,manager=a.LaQuanLy,
                        running=a.DangChay,worked=a.DaTungXuLy,reported=a.DaBaoPhanViec,
            withdrawn=a.DaRutBaoGanNhat,
                        mySeconds=a.TongGiayCuaToi,groupSeconds=a.TongGiayDaLuu,
                        regularSeconds=a.GiayThuongCuaToi,overtimeSeconds=a.GiayTangCaCuaToi,
                        groupRegularSeconds=a.GiayThuongNhom,groupOvertimeSeconds=a.GiayTangCaNhom,
                        runningOvertime=a.DangChayTangCa,started=Milliseconds(a.BatDauPhienCuaToiUtc),rejectReason=a.LyDoTuChoi,
                        reports=workflow.LayBaoCao(projectId,issueId,user.UserId,isAdmin).ConvertAll(x=>new {
                            userId=x.IdNhanVien.ToString("D"),name=x.TenNhanVien,
                            seconds=x.GiayHeThong,regular=x.PhutThuongTuKhai,overtime=x.PhutTangCaTuKhai,
                            systemRegularSeconds=x.GiayThuongHeThong,systemOvertimeSeconds=x.GiayTangCaHeThong,
                            pmRegular=x.PhutThuongPmChot,pmOvertime=x.PhutTangCaPmChot,
                            reported=x.DaBao,running=x.DangChay,chot=x.DaChot,
                            note=x.GhiChu,round=x.Lan,version=x.PhienBan}),
                        currentUserId=user.UserId.ToString("D"),
                        reportHistory=workflow.LayLichSuBaoCao(projectId,issueId,user.UserId,isAdmin).ConvertAll(x=>new {
                            employee=x.NhanVien,actor=x.NguoiThaoTac,round=x.Lan,version=x.PhienBan,
                            kind=(int)x.LoaiSuKien,regular=x.PhutThuong,overtime=x.PhutTangCa,
                            pmRegular=x.PhutThuongPm,pmOvertime=x.PhutTangCaPm,note=x.LyDo,
                            created=Milliseconds(x.NgayTaoUtc)}),
                        history=a.LichSu.ConvertAll(x=>new {id=x.IdPhien.ToString("D"),employee=x.NhanVien,
                            start=Milliseconds(x.BatDauUtc),end=Milliseconds(x.KetThucUtc),seconds=x.SoGiay,
                            overtime=x.LaTangCa})
                    };
                    Write(context,200,new{ok=true,item=data});return;
                }
                Write(context,400,new{ok=false,error="Yêu cầu đọc Issue không hợp lệ."});return;
            }
            if(!CheckToken(context,user.UserId))
                {Write(context,403,new{ok=false,error="Mã bảo vệ thao tác đã hết hạn. Hãy tải lại trang."});return;}
            if(issueId==Guid.Empty){Write(context,400,new{ok=false,error="Thiếu mã vấn đề."});return;}
            string action=context.Request.Form["action"]??"";
            string note=context.Request.Form["note"]??"";
            if(note.Length>2500){Write(context,400,new{ok=false,error="Nội dung quá dài."});return;}
            if(action=="start" || action=="startOvertime")
            {
                Guid operationId=ParseGuid(context.Request.Form["operationId"]);
                if(operationId==Guid.Empty){Write(context,400,new{ok=false,error="Thiếu mã chống bấm trùng."});return;}
                var result=workflow.BatDau(user.UserId,projectId,issueId,operationId,action=="startOvertime");
                Write(context,200,new{ok=true,message=result.ThongBao});return;
            }
            if(action=="stop")
            {
                var result=workflow.Dung(user.UserId,projectId,issueId);
                Write(context,200,new{ok=true,message=result.ThongBao});return;
            }
            if(action=="reportPersonal" || action=="withdrawPersonal")
            {
                int regular,overtime;
                if(!int.TryParse(context.Request.Form["regularMinutes"],NumberStyles.None,CultureInfo.InvariantCulture,out regular)
                  || !int.TryParse(context.Request.Form["overtimeMinutes"],NumberStyles.None,CultureInfo.InvariantCulture,out overtime))
                  {Write(context,400,new{ok=false,error="Số phút tự khai không hợp lệ."});return;}
                int expectedRound,expectedVersion;
                if(!int.TryParse(context.Request.Form["expectedRound"],NumberStyles.None,CultureInfo.InvariantCulture,out expectedRound)
                  || !int.TryParse(context.Request.Form["expectedVersion"],NumberStyles.None,CultureInfo.InvariantCulture,out expectedVersion))
                  {Write(context,400,new{ok=false,error="Thiếu phiên bản báo giờ. Vui lòng mở lại popup giờ cá nhân."});return;}
                workflow.BaoPhanViec(projectId,issueId,user.UserId,regular,overtime,note,
                    action=="withdrawPersonal",expectedRound,expectedVersion);
                Write(context,200,new{ok=true,message=action=="withdrawPersonal"
                    ?"Đã rút báo hoàn thành phần việc. Giờ hệ thống vẫn được giữ nguyên."
                    :"Đã lưu báo hoàn thành phần việc cá nhân. Không ảnh hưởng đồng hồ người khác."});return;
            }
            if(action=="pmChot")
            {
                string raw=context.Request.Form["entries"]??"";
                if(raw.Length>120000){Write(context,400,new{ok=false,error="Danh sách chốt giờ quá dài."});return;}
                var entries=new JavaScriptSerializer().Deserialize<List<GioXuLyVanDeManager.GioPmChotDong>>(raw);
                workflow.ChotCong(projectId,issueId,user.UserId,isAdmin,entries);
                Write(context,200,new{ok=true,message="Đã chốt giờ công của các nhân viên thực sự tham gia."});return;
            }
            if(action=="pmAdjust")
            {
                Guid member=ParseGuid(context.Request.Form["employeeId"]);
                int regular,overtime;
                if(member==Guid.Empty || !int.TryParse(context.Request.Form["regularMinutes"],out regular)
                  || !int.TryParse(context.Request.Form["overtimeMinutes"],out overtime))
                  {Write(context,400,new{ok=false,error="Thiếu nhân viên hoặc số giờ điều chỉnh."});return;}
                workflow.DieuChinhSauChot(projectId,issueId,user.UserId,isAdmin,member,regular,overtime,note);
                Write(context,200,new{ok=true,message="Đã lưu phiên bản điều chỉnh sau chốt và thông báo cho nhân viên."});return;
            }
            if(action=="submit")
            {
                int closed=workflow.GuiHoanThanh(projectId,issueId,user.UserId,note);
                Write(context,200,new{ok=true,message="Đã gửi hoàn thành và lưu/dừng " + closed.ToString(CultureInfo.InvariantCulture) + " đồng hồ của nhóm."});return;
            }
            if(action=="approve" || action=="reject")
            {
                workflow.XuLyYeuCau(projectId,issueId,user.UserId,isAdmin,action=="approve",note);
                Write(context,200,new{ok=true,message=action=="approve"?"Đã duyệt hoàn thành vấn đề.":"Đã từ chối. Nhân viên có thể Start lại khi cần."});return;
            }
            Write(context,400,new{ok=false,error="Thao tác không được hỗ trợ."});
        }
        catch(UnauthorizedAccessException ex){Write(context,403,new{ok=false,error=ex.Message});}
        catch(ArgumentException ex){Write(context,400,new{ok=false,error=ex.Message});}
        catch(System.Data.DBConcurrencyException ex){Write(context,409,new{ok=false,error=ex.Message});}
        catch(InvalidOperationException ex){Write(context,409,new{ok=false,error=ex.Message});}
        catch(Exception ex)
        {
            context.Trace.Warn("GioXuLyVanDe",ex.ToString());
            Write(context,500,new{ok=false,error="Không thể xử lý giờ vấn đề. Vui lòng kiểm tra log máy chủ."});
        }
    }
}
