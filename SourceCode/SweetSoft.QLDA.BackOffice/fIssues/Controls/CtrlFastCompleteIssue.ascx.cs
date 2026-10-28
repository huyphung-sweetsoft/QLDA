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
    public partial class CtrlFastCompleteIssue : BaseAdminUserControl
    {
        public event EventHandler ActionCompleted;
        private Guid IssueId { get { Guid x; return Guid.TryParse(Convert.ToString(ViewState["IssueId"]),out x)?x:Guid.Empty; } set { ViewState["IssueId"]=value.ToString("D"); } }
        private Guid DuAn { get { var p=Page as BaseAdminPage; return p==null?Guid.Empty:p.CurrentProjectId; } }
        private Guid Nguoi { get { return SweetContext.Current==null?Guid.Empty:SweetContext.Current.UserId; } }
        public void OpenModal(Guid issueId) { OpenCore(issueId,false); }
        public void OpenModalWhenDialogReady(Guid issueId) { OpenCore(issueId,true); }
        private void OpenCore(Guid issueId,bool whenReady)
        {
            try
            {
                var mgr=new GioXuLyVanDeManager();
                var info=mgr.LayThongTin(DuAn,issueId,Nguoi,CURRENT_PAGE.IsAdministrator);
                if(info.TrangThai!=0 || (info.TrangThaiDuyetGio!=0 && info.TrangThaiDuyetGio!=3))
                    throw new InvalidOperationException("Vấn đề không ở trạng thái có thể gửi yêu cầu hoàn thành.");
                if(!info.DaDuocPhanCong || !info.DaTungXuLy || !info.DaBaoPhanViec)
                    throw new InvalidOperationException("Bạn phải được phân công, có phiên xử lý và đã báo phần việc trước khi gửi duyệt.");
                IssueId=issueId;
                hdfCompleteIssueId.Value=issueId.ToString("D");
                ltrIssueName.Text=HttpUtility.HtmlEncode("["+info.MaVanDe+"] "+info.TenVanDe);
                txtGhiChu.Text=string.Empty;
                var rows=mgr.LayBaoCao(DuAn,issueId,Nguoi,CURRENT_PAGE.IsAdministrator);
                var missing=rows.Where(x=>(x.GiayHeThong>0 || x.DaBao) && !x.DaBao).ToList();
                pnlWarning.Visible=missing.Count>0;
                if(pnlWarning.Visible)
                    ltrWarning.Text="<strong>Chưa báo phần việc ("+missing.Count+"):</strong> "+HttpUtility.HtmlEncode(string.Join(", ",missing.Select(x=>x.TenNhanVien)))+". Đồng hồ nhóm sẽ dừng và những người chưa báo sẽ được nhắc.";
                upnlFastComplete.Update();upFooterComplete.Update();
                if(whenReady)RegisterOpenWhenReady();else mdlFastComplete.OpenModal(true);
            }
            catch(Exception ex){ShowNotify(ex.Message,MSGType.Warning);}
        }
        private void RegisterOpenWhenReady()
        {
            string selector=HttpUtility.JavaScriptStringEncode("#"+mdlFastComplete.ClientID);
            string script=@"(function(){var q='__ID__',tries=0,timer=null;
              function open(){
                tries++;
                var element=document.querySelector(q);
                if(element&&element.classList.contains('show')){if(timer!==null)clearInterval(timer);return;}
                var jq=window.jQuery||window.$;
                if(element&&window.CMSMasterJs&&typeof window.CMSMasterJs.OpenDialog==='function'&&
                   jq&&jq.fn&&typeof jq.fn.modal==='function'){
                  if(timer!==null)clearInterval(timer);
                  window.CMSMasterJs.OpenDialog(q,'Gửi yêu cầu hoàn thành vấn đề');return;
                }
                if(tries>=300&&timer!==null)clearInterval(timer);
              }
              timer=setInterval(open,50);open();})();";
            ScriptManager.RegisterStartupScript(Page,Page.GetType(),"IssueSendOnLoad"+Guid.NewGuid().ToString("N"),script.Replace("__ID__",selector),true);
        }
        protected void btnConfirmComplete_Click(object sender,EventArgs e)
        {
            try
            {
                Guid id=IssueId;
                if(id==Guid.Empty || hdfCompleteIssueId.Value!=id.ToString("D"))throw new InvalidOperationException("Phiên gửi duyệt không hợp lệ.");
                int count=new GioXuLyVanDeManager().GuiHoanThanh(DuAn,id,Nguoi,txtGhiChu.Text);
                mdlFastComplete.CloseModal();
                ShowNotify("Đã gửi PM duyệt hoàn thành vấn đề. Đã dừng "+count+" đồng hồ đang chạy; thời gian đã ghi vẫn được giữ.",MSGType.Success);
                if(ActionCompleted!=null)ActionCompleted(this,EventArgs.Empty);
            }
            catch(Exception ex){ShowNotify(ex.Message,MSGType.Error);upnlFastComplete.Update();}
        }
    }
}
