using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.DataAccess;
using SweetSoft.QLDA.Core.EnumHelper;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Controls;

namespace SweetSoft.QLDA.BackOffice.fIssues.Controls
{
    public partial class CtrlFastReviewIssue : BaseAdminUserControl
    {
        public event EventHandler ActionCompleted;
        private Guid IssueId { get { Guid x;return Guid.TryParse(Convert.ToString(ViewState["ReviewIssueId"]),out x)?x:Guid.Empty; } set {ViewState["ReviewIssueId"]=value.ToString("D");} }
        private Guid DuAn { get { var p=Page as BaseAdminPage;return p==null?Guid.Empty:p.CurrentProjectId; } }
        private Guid Nguoi { get {return SweetContext.Current==null?Guid.Empty:SweetContext.Current.UserId;} }
        private bool LaAdmin {get {return CURRENT_PAGE.IsAdministrator;} }
        public void OpenModal(Guid issueId)
        {
            OpenCore(issueId, false);
        }
        public void OpenModalWhenDialogReady(Guid issueId)
        {
            OpenCore(issueId, true);
        }
        private void OpenCore(Guid issueId,bool waitForReady)
        {
            try
            {
                var m=new GioXuLyVanDeManager();
                var x=m.LayThongTin(DuAn,issueId,Nguoi,LaAdmin);
                if(!x.LaQuanLy)throw new UnauthorizedAccessException("Chỉ PM hoặc quản trị viên được duyệt yêu cầu.");
                if(x.TrangThai!=0 || x.TrangThaiDuyetGio!=1)
                    throw new InvalidOperationException("Vấn đề không còn ở trạng thái chờ duyệt.");
                mdlFastReview.Title="Duyệt nhanh hoàn thành vấn đề";
                pnlReviewForm.Visible=true;pnlNotificationResult.Visible=false;
                btnApproveCompletion.Visible=true;btnRejectCompletion.Visible=true;
                IssueId=issueId;hdfReviewIssueId.Value=issueId.ToString("D");
                ltrReviewIssueName.Text=HttpUtility.HtmlEncode("["+x.MaVanDe+"] "+x.TenVanDe);
                string senderName="Nhân viên";
                if(x.NguoiGuiDuyet.HasValue){var u=UserManager.Instance.GetUserById(x.NguoiGuiDuyet.Value);if(u!=null)senderName=string.IsNullOrWhiteSpace(u.DisplayName)?u.UserName:u.DisplayName;}
                ltrSender.Text=HttpUtility.HtmlEncode(senderName);
                ltrSentAt.Text=x.NgayGuiDuyetUtc.HasValue?HttpUtility.HtmlEncode(x.NgayGuiDuyetUtc.Value.AddHours(7).ToString("dd/MM/yyyy HH:mm")):"—";
                var workers=m.LayBaoCao(DuAn,issueId,Nguoi,LaAdmin);
                int need=workers.Count(a=>(a.DaTungXuLy||a.DaBao));int done=workers.Count(a=>(a.DaTungXuLy||a.DaBao)&&a.DaBao);
                ltrWorkers.Text=done+"/"+need+" người đã báo phần việc";
                ltrTotalTime.Text=HienPhut((int)Math.Min(int.MaxValue,(x.TongGiayDaLuu+30)/60));
                txtRejectReason.Text=string.Empty;
                upnlReviewComplete.Update();upnlReviewFooter.Update();
                if(waitForReady)MoSauKhiTaiXong();else mdlFastReview.OpenModal(true);
            }
            catch(Exception ex){ShowNotify(ex.Message,MSGType.Warning);}
        }
        // Show an existing notification verbatim; do not recompute the historical PM decision.
        public void OpenNotificationResult(Guid issueId,Guid notificationId,bool waitForReady=false)
        {
            try
            {
                var note=ThongBaoManager.Instance.GetById(notificationId);
                string expected="GioXuLyVanDe:"+issueId.ToString("D")+":detail";
                if(note==null||note.DaXoa==true||note.UserId!=Nguoi||!note.IdDuAn.HasValue||note.IdDuAn.Value!=DuAn||
                   !string.Equals(note.DuongDanLienKet,expected,StringComparison.OrdinalIgnoreCase))
                    throw new UnauthorizedAccessException("Thông báo không thuộc về bạn hoặc không còn hợp lệ.");
                var info=new GioXuLyVanDeManager().LayThongTin(DuAn,issueId,Nguoi,LaAdmin);
                bool isPmTimeNotice=LaThongBaoPmGio(note);
                mdlFastReview.Title=isPmTimeNotice?"Chi tiết giờ công đã chốt":"Kết quả xử lý vấn đề";
                pnlReviewForm.Visible=false;pnlNotificationResult.Visible=true;
                pnlNotificationResult.CssClass=isPmTimeNotice?"fr-pm-final-layout":string.Empty;
                btnApproveCompletion.Visible=false;btnRejectCompletion.Visible=false;
                ltrResultIssue.Text=HttpUtility.HtmlEncode("["+info.MaVanDe+"] "+info.TenVanDe);
                ltrResultTitle.Text=HttpUtility.HtmlEncode(note.TieuDe??"Kết quả xử lý vấn đề");
                ltrResultWhen.Text=HttpUtility.HtmlEncode(note.NgayTao.ToString("dd/MM/yyyy HH:mm"));
                ltrResultBody.Text=isPmTimeNotice?HienThiChiTietPm(note,issueId):HienThiNoiDungKetQuaCu(note,issueId);
                upnlReviewComplete.Update();upnlReviewFooter.Update();
                if(waitForReady)MoSauKhiTaiXong();else mdlFastReview.OpenModal(true);
            }
            catch(Exception ex){ShowNotify(ex.Message,MSGType.Warning);}
        }
        private static bool LaThongBaoPmGio(TblThongBao note)
        {
            string title=note==null?string.Empty:note.TieuDe??string.Empty;
            return title.IndexOf("chốt giờ",StringComparison.OrdinalIgnoreCase)>=0
                || title.IndexOf("điều chỉnh giờ",StringComparison.OrdinalIgnoreCase)>=0;
        }
        private static string Vn(DateTime utc)
        {
            return utc.AddHours(7).ToString("HH:mm dd/MM/yyyy",CultureInfo.InvariantCulture);
        }
        private static void ThemGio(System.Text.StringBuilder sb,string regular,string overtime)
        {
            sb.Append("<div class='fr-pm-hours'><div><span>Giờ làm theo lịch</span><strong>")
              .Append(HttpUtility.HtmlEncode(regular)).Append("</strong></div><div><span>Tăng ca</span><strong>")
              .Append(HttpUtility.HtmlEncode(overtime)).Append("</strong></div></div>");
        }
        private static void ThemTheGio(System.Text.StringBuilder sb,string title,string desc,string regular,string overtime)
        {
            sb.Append("<section class='fr-pm-card'><h4>").Append(HttpUtility.HtmlEncode(title)).Append("</h4>")
              .Append("<p class='fr-pm-caption'>").Append(HttpUtility.HtmlEncode(desc)).Append("</p>");
            ThemGio(sb,regular,overtime);
            sb.Append("</section>");
        }
        private static double SaiSoThongBao(DateTime noteTime,DateTime eventUtc)
        {
            return Math.Min(Math.Abs((noteTime-eventUtc).TotalSeconds),
                    Math.Abs((noteTime-eventUtc.AddHours(7)).TotalSeconds));
        }
        private string HienThiChiTietPm(TblThongBao note,Guid issueId)
        {
            var history=new GioXuLyVanDeManager().LayLichSuPmCaNhan(DuAn,issueId,Nguoi,LaAdmin);
            bool adjusted=(note.TieuDe??string.Empty).IndexOf("điều chỉnh",StringComparison.OrdinalIgnoreCase)>=0;
            var candidates=history.Where(x=>x.LoaiSuKien==(adjusted?6:4)&&SaiSoThongBao(note.NgayTao,x.NgayTaoUtc)<=30).ToList();
            var selected=candidates.Count==1?candidates[0]:null;
            var sb=new System.Text.StringBuilder();
            sb.Append("<div class='fr-pm-result-body'><div class='fr-pm-left'>");
            if(selected==null)
            {
                sb.Append("<div class='fr-pm-note is-warning'><strong>Không xác định được bản chốt lịch sử duy nhất</strong>")
                  .Append("<p>Thông báo này chưa đủ dữ liệu đối chiếu an toàn với một bản chốt. Không lấy số giờ hiện tại thay thế số giờ trong quá khứ.</p></div>");
                sb.Append(HienThiNoiDungKetQuaCu(note,issueId));
            }
            else
            {
                sb.Append("<div class='fr-pm-cards'>");
                ThemTheGio(sb,"Giờ hệ thống ghi nhận","Giờ được lưu tại thời điểm PM chốt, không bị thay đổi bởi việc điều chỉnh sau này",
                    HienPhut((int)Math.Min(int.MaxValue,(selected.GiayThuongHeThong+30)/60)),
                    HienPhut((int)Math.Min(int.MaxValue,(selected.GiayTangCaHeThong+30)/60)));
                ThemTheGio(sb,"Giờ bạn đã báo","Số giờ tự khai lưu trong phiên bản đối soát PM đã sử dụng",
                    selected.PhutThuongTuKhai.HasValue?HienPhut(selected.PhutThuongTuKhai.Value):"—",
                    selected.PhutTangCaTuKhai.HasValue?HienPhut(selected.PhutTangCaTuKhai.Value):"—");
                sb.Append("</div>");
                ThemTheGio(sb,"Giờ quản lý đã chốt","Lưu lúc "+Vn(selected.NgayTaoUtc),
                    HienPhut(selected.PhutThuongPmChot),HienPhut(selected.PhutTangCaPmChot));
                sb.Append("<div class='fr-pm-note'><strong>Lý do / ghi chú của quản lý</strong><p>")
                  .Append(HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(selected.LyDo)?
                    "Quản lý chốt theo dữ liệu đối soát, không có ghi chú điều chỉnh.":selected.LyDo))
                  .Append("</p></div>");
                if(history.Count>0&&history[0]!=selected)
                {
                    var latest=history[0];
                    sb.Append("<div class='fr-pm-note is-warning'><strong>Kết quả thông báo này đã được điều chỉnh sau đó</strong>")
                      .Append("<p>Đây là bản chốt tại thời điểm bạn nhận thông báo. Kết quả mới nhất: ")
                      .Append(HttpUtility.HtmlEncode(HienPhut(latest.PhutThuongPmChot)))
                      .Append(" làm theo lịch, ").Append(HttpUtility.HtmlEncode(HienPhut(latest.PhutTangCaPmChot)))
                      .Append(" tăng ca.</p></div>");
                }
            }
            sb.Append("</div><aside class='fr-pm-history'><div class='fr-pm-history-head'><strong>Lịch sử PM chốt và điều chỉnh</strong><span>")
              .Append(history.Count).Append(" lần</span></div><div class='fr-pm-history-list'>");
            for(int i=0;i<history.Count;i++)
            {
                var rec=history[i];
                sb.Append("<div class='fr-pm-history-item'><div class='fr-pm-history-item-head'><strong>")
                  .Append(i==0?"Kết quả mới nhất":"Bản chốt trước").Append("</strong><time>")
                  .Append(HttpUtility.HtmlEncode(Vn(rec.NgayTaoUtc))).Append("</time></div><p>")
                  .Append(HttpUtility.HtmlEncode(HienPhut(rec.PhutThuongPmChot))).Append(" làm theo lịch · ")
                  .Append(HttpUtility.HtmlEncode(HienPhut(rec.PhutTangCaPmChot))).Append(" tăng ca</p>");
                if(!string.IsNullOrWhiteSpace(rec.LyDo))sb.Append("<p>").Append(HttpUtility.HtmlEncode(rec.LyDo)).Append("</p>");
                sb.Append("</div>");
            }
            sb.Append("</div></aside></div>");
            return sb.ToString();
        }

        // Thong bao PM phai the hien ket qua GIO PM CHOT trong su kien do, khong suy ra tu DB hien tai.
        // Chuoi NoiDung luu cung transaction voi chot, nen cac thong bao sau dieu chinh khong sua lich su.
        private string HienThiNoiDungKetQuaCu(TblThongBao note,Guid issueId)
        {
            string body=note.NoiDung??string.Empty;
            bool pmNotice=(note.TieuDe??string.Empty).IndexOf("chốt giờ",StringComparison.OrdinalIgnoreCase)>=0
                || (note.TieuDe??string.Empty).IndexOf("điều chỉnh giờ",StringComparison.OrdinalIgnoreCase)>=0;
            if(!pmNotice) return "<div class='fr-result-note'>"+HttpUtility.HtmlEncode(body).Replace("\r\n","\n").Replace("\n","<br />")+"</div>";
            string regular=null,overtime=null,total=null;
            var other=new System.Collections.Generic.List<string>();
            foreach(string raw in body.Replace("\r\n","\n").Split('\n'))
            {
                string line=raw.Trim();if(line.Length==0)continue;
                int split=line.IndexOf(':');
                if(split>0)
                {
                    string key=line.Substring(0,split).Trim(),value=line.Substring(split+1).Trim();
                    if(key=="Giờ làm theo lịch PM chốt") {regular=value;continue;}
                    if(key=="Giờ tăng ca PM chốt") {overtime=value;continue;}
                    if(key=="Tổng giờ PM chốt") {total=value;continue;}
                }
                other.Add(line);
            }
            var sb=new System.Text.StringBuilder();
            if(regular!=null && overtime!=null && total!=null)
            {
                sb.Append("<div class='fr-result-hours'>");
                AppendAmount(sb,"Giờ làm theo lịch PM chốt",regular);
                AppendAmount(sb,"Giờ tăng ca PM chốt",overtime);
                AppendAmount(sb,"Tổng giờ PM chốt",total);
                sb.Append("</div>");
            }
            else
            {
                Tuple<int,int> historic=null;
                try
                {
                    bool adjusted=(note.TieuDe??string.Empty).IndexOf("điều chỉnh",StringComparison.OrdinalIgnoreCase)>=0;
                    historic=new GioXuLyVanDeManager().LayGioPmTuThongBaoCu(DuAn,issueId,Nguoi,LaAdmin,note.NgayTao,adjusted);
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Trace.TraceWarning("Issue historical PM notice: "+ex.Message);
                }
                if(historic!=null)
                {
                    int minutes=historic.Item1+historic.Item2;
                    sb.Append("<div class='fr-result-hours'>");
                    AppendAmount(sb,"Giờ làm theo lịch PM chốt",HienPhut(historic.Item1));
                    AppendAmount(sb,"Giờ tăng ca PM chốt",HienPhut(historic.Item2));
                    AppendAmount(sb,"Tổng giờ PM chốt",HienPhut(minutes));
                    sb.Append("</div><div class='fr-result-legacy'>Số giờ được đối chiếu từ bản ghi PM chốt trong lịch sử cùng thời điểm thông báo.</div>");
                }
                else
                    sb.Append("<div class='fr-result-legacy'>Thông báo cũ không lưu số giờ PM chốt. Không tìm thấy một bản chốt lịch sử duy nhất khớp thời điểm; vui lòng xem bản đối soát.</div>");
            }
            if(other.Count>0)sb.Append("<div class='fr-result-note'>")
                .Append(string.Join("<br />",other.Select(x=>HttpUtility.HtmlEncode(x)))).Append("</div>");
            return sb.ToString();
        }
        private static string HienPhut(int value)
        {
            return (value/60).ToString(System.Globalization.CultureInfo.InvariantCulture)+":"
                 +(value%60).ToString("00",System.Globalization.CultureInfo.InvariantCulture);
        }
        private static void AppendAmount(System.Text.StringBuilder sb,string label,string value)
        {
            sb.Append("<div class='fr-result-amount'><span>").Append(HttpUtility.HtmlEncode(label))
              .Append("</span><strong>").Append(HttpUtility.HtmlEncode(value)).Append("</strong></div>");
        }
        private void MoSauKhiTaiXong()
        {
            string selector=HttpUtility.JavaScriptStringEncode("#"+mdlFastReview.ClientID);
            string popupTitle=HttpUtility.JavaScriptStringEncode(mdlFastReview.Title??"Vấn đề");
            string script=@"(function(){var q='__ID__',count=0,handle=null;
                function open(){count++;if(window.CMSMasterJs&&typeof window.CMSMasterJs.OpenDialog==='function'&&
                     window.jQuery&&typeof window.jQuery.fn.modal==='function'&&document.querySelector(q)){
                     clearInterval(handle);window.CMSMasterJs.OpenDialog(q,'__TITLE__');return;}
                     if(count>=300)clearInterval(handle);}
                handle=setInterval(open,50);open();})();";
            ScriptManager.RegisterStartupScript(Page,Page.GetType(),"IssueReview"+Guid.NewGuid().ToString("N"),script.Replace("__ID__",selector).Replace("__TITLE__",popupTitle),true);
        }
        private void SaveDecision(bool approved)
        {
            try
            {
                Guid id=IssueId;
                if(id==Guid.Empty || hdfReviewIssueId.Value!=id.ToString("D"))throw new InvalidOperationException("Phiên duyệt không hợp lệ.");
                string reason=(txtRejectReason.Text??string.Empty).Trim();
                if(!approved && reason.Length==0)throw new InvalidOperationException("Bạn phải nhập lý do từ chối.");
                new GioXuLyVanDeManager().XuLyYeuCau(DuAn,id,Nguoi,LaAdmin,approved,reason);
                mdlFastReview.CloseModal();
                ShowNotify(approved?"Đã duyệt hoàn thành vấn đề. Giờ công chưa tự động chốt.":"Đã từ chối yêu cầu hoàn thành và gửi thông báo kết quả.",MSGType.Success);
                if(ActionCompleted!=null)ActionCompleted(this,EventArgs.Empty);
            }
            catch(Exception ex){ShowNotify(ex.Message,MSGType.Error);upnlReviewComplete.Update();}
        }
        protected void btnApproveCompletion_Click(object sender,EventArgs e){SaveDecision(true);}
        protected void btnRejectCompletion_Click(object sender,EventArgs e){SaveDecision(false);}
    }
}
