using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;
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
    // Giao diện sao chép từ CtrlDuyetGioCongTask, nhưng nguồn dữ liệu và lệnh lưu hoàn toàn thuộc Issue.
    public partial class CtrlDuyetGioCongIssue : BaseAdminUserControl
    {
        public event EventHandler Saved;
        private sealed class NhanVienPmIssue
        {
            public Guid IdNhanVien { get; set; }
            public string TenNhanVien { get; set; }
            public string Email { get; set; }
            public long GiayThuongHeThong { get; set; }
            public long GiayTangCaHeThong { get; set; }
            public int? PhutThuongNhanVienBao { get; set; }
            public int? PhutTangCaNhanVienBao { get; set; }
            public int PhutThuongDeXuat { get; set; }
            public int PhutTangCaDeXuat { get; set; }
            public int? PhutThuongPmDaChot { get; set; }
            public int? PhutTangCaPmDaChot { get; set; }
            public string GhiChuPmChot { get; set; }
            public bool DaXacNhanGio { get; set; }
            public bool DaTungXuLy { get; set; }
            public bool DaRutBaoGanNhat { get; set; }
            public bool DaChot { get; set; }
        }
        private List<GioXuLyVanDeManager.BaoCaoGioVanDe> _reports;
        private Guid _projectPm;
        private readonly Dictionary<Guid,string> _avatars=new Dictionary<Guid,string>();
        private readonly Dictionary<Guid,string> _roles=new Dictionary<Guid,string>();
        private Guid DuAn {get {var p=Page as BaseAdminPage;return p==null?Guid.Empty:p.CurrentProjectId;}}
        private Guid Nguoi {get{return SweetContext.Current==null?Guid.Empty:SweetContext.Current.UserId;}}
        private bool IsAdmin {get{return CURRENT_PAGE.IsAdministrator;}}
        private Guid IdIssue {get {Guid x;return Guid.TryParse(Convert.ToString(ViewState["PmIssueId"]),out x)?x:Guid.Empty;}set{ViewState["PmIssueId"]=value.ToString("D");}}
        private bool DangSua {get{return ViewState["PmIssueEditing"] is bool && (bool)ViewState["PmIssueEditing"]; }set{ViewState["PmIssueEditing"]=value;}}
        private string Fingerprint {get{return Convert.ToString(ViewState["PmIssueFingerprint"])??"";}set{ViewState["PmIssueFingerprint"]=value;}}
        private string LastSaved {get{return Convert.ToString(ViewState["PmIssueRequestDone"])??"";}set{ViewState["PmIssueRequestDone"]=value;}}
        private GioXuLyVanDeManager Mgr {get{return new GioXuLyVanDeManager();}}
        private static string Display(int mins){return (mins/60).ToString(CultureInfo.InvariantCulture)+":"+(mins%60).ToString("00",CultureInfo.InvariantCulture);}
        protected string HienGiay(object value){long s=Convert.ToInt64(value,CultureInfo.InvariantCulture);return Display((int)Math.Min(599999,(s+30)/60));}
        protected string HienPhut(object value){return value==null || value==DBNull.Value?"Chưa báo":Display(Convert.ToInt32(value,CultureInfo.InvariantCulture));}
        protected string HienPhutBao(object value,object rut,object daTungXuLy){
            if(!Convert.ToBoolean(daTungXuLy) && (value==null||value==DBNull.Value))return "Không xử lý vấn đề này";
            return Convert.ToBoolean(rut)?"Đã rút báo":HienPhut(value);
        }
        protected int PhutMacDinh(object deXuat,object daChot){return daChot==null||daChot==DBNull.Value?Convert.ToInt32(deXuat):Convert.ToInt32(daChot);}
        protected string GiaTriSua(object deXuat,object daChot){return Display(daChot==null||daChot==DBNull.Value?Convert.ToInt32(deXuat):Convert.ToInt32(daChot));}
        protected string GiaTriSuaGio(object deXuat,object daChot){return ((daChot==null||daChot==DBNull.Value?Convert.ToInt32(deXuat):Convert.ToInt32(daChot))/60).ToString(CultureInfo.InvariantCulture);}
        protected string GiaTriSuaPhut(object deXuat,object daChot){return ((daChot==null||daChot==DBNull.Value?Convert.ToInt32(deXuat):Convert.ToInt32(daChot))%60).ToString("00",CultureInfo.InvariantCulture);}
        protected string HienTinhTrangBao(object daBao,object daTungXuLy){
            if(Convert.ToBoolean(daBao))return "<span class='qlpm-person-state is-reported'>Đã báo hoàn thành</span>";
            if(!Convert.ToBoolean(daTungXuLy))return "<span class='qlpm-person-state is-not-worked'>Không xử lý vấn đề này</span>";
            return "<span class='qlpm-person-state'>Chưa báo hoàn thành</span>";
        }
        protected string HienChucDanh(object id){Guid g;if(!Guid.TryParse(Convert.ToString(id),out g))return "";string x;return _roles.TryGetValue(g,out x)?HttpUtility.HtmlEncode(x):"";}
        protected string HienCoPm(object id){return Convert.ToString(id)==_projectPm.ToString("D")?"<span class='qlpm-person-pm' title='PM dự án'><i class='fas fa-crown'></i> PM</span>":"";}
        private static string Initials(string name){var p=(name??"").Split(new[]{' '},StringSplitOptions.RemoveEmptyEntries);if(p.Length==0)return "?";if(p.Length==1)return p[0].Substring(0,1).ToUpperInvariant();return (p[p.Length-2].Substring(0,1)+p[p.Length-1].Substring(0,1)).ToUpperInvariant();}
        protected string HienAvatar(object id,object name)
        {
            Guid g;Guid.TryParse(Convert.ToString(id),out g);string txt=Convert.ToString(name)??"";
            string[] colors={"#f59e0b","#3b82f6","#10b981","#8b5cf6","#ec4899"};
            int seed=0;foreach(byte b in g.ToByteArray())seed=(seed+b)%colors.Length;
            string title=HttpUtility.HtmlAttributeEncode(txt);
            string fallback="<span class='qlpm-avatar-fallback' style='background-color:"+colors[seed]+"' title='"+title+"'>"+HttpUtility.HtmlEncode(Initials(txt))+"</span>";
            string path;if(!_avatars.TryGetValue(g,out path)||string.IsNullOrWhiteSpace(path)||path.IndexOf("no-file.png",StringComparison.OrdinalIgnoreCase)>=0||path.EndsWith("/Styles/images/user-icon.png",StringComparison.OrdinalIgnoreCase))return fallback;
            if(path.StartsWith("~",StringComparison.Ordinal))path=Page.ResolveUrl(path);
            return fallback+"<img class='qlpm-avatar-image' src='"+HttpUtility.HtmlAttributeEncode(path)+"' alt='"+title+"' loading='lazy' onerror=\"this.style.display='none'\" />";
        }
        private void LoadIdentity(Guid project)
        {
            _avatars.Clear();_roles.Clear();_projectPm=Guid.Empty;
            try {var pm=DuAnManager.Instance.LayIdNhanVienQuanLy(project);if(pm.HasValue)_projectPm=pm.Value;}catch(Exception ex){System.Diagnostics.Trace.TraceWarning("Issue PM project identity: "+ex.Message);}
            try {foreach(var u in UserManager.Instance.GetAllActiveNhanVien())if(u!=null&&u.UserId!=Guid.Empty)_avatars[u.UserId]=u.Avatar??"";}catch(Exception ex){System.Diagnostics.Trace.TraceWarning("Issue PM avatar: "+ex.Message);}
            try {
                int count;var filters=new Dictionary<string,object>{{"LaNhanVien",true}};
                var data=UserManager.Instance.SearchUsers("",filters,"DisplayName ASC",0,10000,out count);
                if(data!=null && data.Columns.Contains("UserId") && data.Columns.Contains("TenChucDanh"))
                    foreach(System.Data.DataRow row in data.Rows){Guid id;if(Guid.TryParse(Convert.ToString(row["UserId"]),out id))_roles[id]=row["TenChucDanh"]==DBNull.Value?"":Convert.ToString(row["TenChucDanh"]);}
            }catch(Exception ex){System.Diagnostics.Trace.TraceWarning("Issue PM role: "+ex.Message);}
        }
        private static bool IsParticipant(GioXuLyVanDeManager.BaoCaoGioVanDe r){return r.DaTungXuLy || r.DaBao;}
        private static string Signature(IEnumerable<GioXuLyVanDeManager.BaoCaoGioVanDe> data,byte issueApproval)
        {
            var text=new StringBuilder(issueApproval.ToString(CultureInfo.InvariantCulture));
            foreach(var r in data.OrderBy(x=>x.IdNhanVien))
                text.Append('|').Append(r.IdNhanVien.ToString("N")).Append(':').Append(r.Lan).Append(':').Append(r.PhienBan).Append(':').Append(r.SuKien).Append(':')
                  .Append(r.GiayHeThong).Append(':').Append(r.DaTungXuLy).Append(':').Append(r.PhutThuongTuKhai).Append(':').Append(r.PhutTangCaTuKhai).Append(':')
                  .Append(r.PhutThuongPmChot).Append(':').Append(r.PhutTangCaPmChot).Append(':').Append(r.GhiChu).Append(':').Append(r.DangChay).Append(':').Append(r.DaBao);
            using(var hash=SHA256.Create())return Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(text.ToString())));
        }
        private void LoadData(bool resetFingerprint)
        {
            if(IdIssue==Guid.Empty || DuAn==Guid.Empty)return;
            var m=Mgr;var issue=m.LayThongTin(DuAn,IdIssue,Nguoi,IsAdmin);
            if(!issue.LaQuanLy)throw new UnauthorizedAccessException("Bạn không có quyền quản lý giờ công của vấn đề.");
            hfProjectId.Value=DuAn.ToString("D");hfIssueId.Value=IdIssue.ToString("D");
            _reports=m.LayBaoCao(DuAn,IdIssue,Nguoi,IsAdmin);
            if(resetFingerprint){Fingerprint=Signature(_reports,issue.TrangThaiDuyetGio);hfSnapshot.Value=Fingerprint;hfRequestId.Value=Guid.NewGuid().ToString("D");LastSaved="";}
            LoadIdentity(DuAn);
            bool closed=issue.TrangThai==1&&issue.TrangThaiDuyetGio==2;
            bool allChot=_reports.Where(IsParticipant).Any()&&_reports.Where(IsParticipant).All(x=>x.DaChot);
            var model=_reports.Select(r=>new NhanVienPmIssue{
                IdNhanVien=r.IdNhanVien,TenNhanVien=r.TenNhanVien,
                Email=GetEmail(r.TenNhanVien),GiayThuongHeThong=r.GiayThuongHeThong,GiayTangCaHeThong=r.GiayTangCaHeThong,
                PhutThuongNhanVienBao=r.PhutThuongTuKhai,PhutTangCaNhanVienBao=r.PhutTangCaTuKhai,
                PhutThuongDeXuat=r.PhutThuongTuKhai??(int)Math.Min(525600,(r.GiayThuongHeThong+30)/60),
                PhutTangCaDeXuat=r.PhutTangCaTuKhai??(int)Math.Min(525600,(r.GiayTangCaHeThong+30)/60),
                PhutThuongPmDaChot=r.PhutThuongPmChot,PhutTangCaPmDaChot=r.PhutTangCaPmChot,
                GhiChuPmChot=r.DaChot?(r.GhiChu??""):"",DaXacNhanGio=r.DaBao,DaTungXuLy=r.DaTungXuLy,DaRutBaoGanNhat=r.SuKien==2,DaChot=r.DaChot
            }).ToList();
            repNhanVien.DataSource=model;repNhanVien.DataBind();
            pnlShell.CssClass="qlpm-shell"+(model.Count<=1?" qlpm-one":"");
            foreach(RepeaterItem item in repNhanVien.Items)
                foreach(string id in new[]{"txtHanhChinh","txtHanhChinhPhut","txtTangCa","txtTangCaPhut","txtLyDo"})
                {var ctrl=item.FindControl(id) as WebControl;
                 var hidden=item.FindControl("hfNhanVien") as HiddenField;
                 Guid memberId;var member=hidden!=null&&Guid.TryParse(hidden.Value,out memberId)
                     ?_reports.FirstOrDefault(x=>x.IdNhanVien==memberId):null;
                 if(ctrl!=null)ctrl.Enabled=member!=null&&IsParticipant(member)&&(!allChot||DangSua);}
            ltrIssueName.Text=HttpUtility.HtmlEncode("["+issue.MaVanDe+"] "+issue.TenVanDe);
            // Đồng nhất class và trạng thái thực tế với popup PM Task.
            ltrIssueStatus.Text=issue.TrangThaiDuyetGio==1
                ?"<span class='badge-pill-custom badge-status-review'>Chờ PM duyệt</span>"
                :issue.TrangThai==1
                  ?"<span class='badge-pill-custom badge-status-done'>Đã xử lý</span>"
                  :"<span class='badge-pill-custom badge-status-doing'>Đang xử lý</span>";
            ltrPmStatus.Text=allChot
                ?"<span class='qlpm-status is-final'><i class='fas fa-check-double'></i> Đã duyệt giờ công</span>"
                :"<span class='qlpm-status'><i class='fas fa-exclamation-circle'></i> Chưa duyệt giờ công</span>";
            int thamGia=_reports.Count(IsParticipant);
            lblNhanVien.Text=thamGia.ToString(CultureInfo.InvariantCulture);lblCount.Text=model.Count.ToString(CultureInfo.InvariantCulture);
            // Chỉ tổng hợp nhân viên trong phạm vi chốt, không cộng người chưa xử lý.
            var eligible=_reports.Where(IsParticipant).ToList();
            lblTongHanhChinh.Text=Display(model.Where(x=>eligible.Any(e=>e.IdNhanVien==x.IdNhanVien)).Sum(x=>x.PhutThuongPmDaChot??x.PhutThuongDeXuat));
            lblTongTangCa.Text=Display(model.Where(x=>eligible.Any(e=>e.IdNhanVien==x.IdNhanVien)).Sum(x=>x.PhutTangCaPmDaChot??x.PhutTangCaDeXuat));
            int chuaBao=eligible.Count(x=>!x.DaBao);
            bool coPhienMo=eligible.Any(x=>x.DangChay);
            bool daCoBanPm=eligible.Any(x=>x.DaChot);
            bool duocChot=closed && eligible.Count>0 && chuaBao==0 && !coPhienMo && !daCoBanPm;
            lblFooter.Text=DangSua&&allChot?"Chỉ điều chỉnh một nhân viên mỗi lần, bắt buộc nhập lý do":
                allChot?"Đã lưu kết quả chính thức":
                eligible.Count==0?"Chưa có người xử lý cần chốt":
                chuaBao>0?(eligible.Count-chuaBao)+"/"+eligible.Count+" nhân viên đã báo":
                "Chốt toàn bộ "+eligible.Count+" nhân viên";
            // Như Task: nút luôn ở footer trước khi chốt, khóa và giải thích lý do nếu chưa đủ điều kiện.
            btnChotGioCong.Visible=!allChot;
            btnChotGioCong.Enabled=duocChot;
            btnChotGioCong.ToolTip=duocChot?"Đủ điều kiện duyệt giờ công":
                allChot||daCoBanPm?"Giờ công đã chốt, không thể chốt lại":
                chuaBao>0?"Còn nhân viên đã xử lý nhưng chưa báo hoàn thành":
                coPhienMo?"Vẫn còn phiên giờ đang mở":
                eligible.Count==0?"Chưa có nhân viên từng xử lý cần chốt":
                "Chỉ chốt sau khi PM đã duyệt hoàn thành vấn đề";
            btnBatDauDieuChinh.Visible=closed&&allChot&&!DangSua;
            btnLuuDieuChinh.Visible=closed&&allChot&&DangSua;
            btnNhacBaoHoanThanh.Visible=true;
            btnNhacBaoHoanThanh.Enabled=chuaBao>0&&!allChot;
            btnNhacBaoHoanThanh.ToolTip=allChot?"Giờ công đã chốt, không cần nhắc nhở":
                chuaBao==0?"Không còn nhân viên đã xử lý cần nhắc báo giờ":
                "Nhắc "+chuaBao+" nhân viên đã xử lý nhưng chưa báo";
            pnlAlert.Visible=!duocChot;
            if(pnlAlert.Visible)
            {
                lblAlert.Text=allChot?"Giờ công đã được PM duyệt. Chế độ xem, không ghi đè kết quả đã lưu.":
                    chuaBao>0?"Còn "+chuaBao+" nhân viên đã xử lý nhưng chưa báo: "+
                        string.Join(", ",eligible.Where(x=>!x.DaBao).Select(x=>HttpUtility.HtmlEncode(x.TenNhanVien)))+
                        ". Hãy gửi nhắc nhở trước khi chốt.":
                    coPhienMo?"Vẫn còn phiên giờ đang mở. Không thể duyệt giờ công.":
                    daCoBanPm?"Đã phát hiện kết quả chốt PM trước đó, không thể chốt lại.":
                    eligible.Count==0?"Không có nhân viên từng xử lý hoặc đã báo giờ trong phạm vi chốt.":
                    "PM chỉ chốt giờ sau khi đã duyệt hoàn thành vấn đề.";
            }
            upDuyetGioCong.Update();upFooterDuyetCong.Update();
        }
        private static string GetEmail(string username)
        {try{var u=Membership.GetUser(username);return u==null?"":u.Email??"";}catch{return "";}}
        public void OpenModal(Guid issueId) { OpenCore(issueId,false); }
        public void OpenModalWhenDialogReady(Guid issueId) { OpenCore(issueId,true); }
        private void OpenCore(Guid issueId,bool whenReady)
        {
            try{IdIssue=issueId;DangSua=false;LoadData(true);
                if(whenReady)RegisterOpenWhenReady();else mdlDuyetGioCong.OpenModal(true);}
            catch(Exception ex){ShowNotify(ex.Message,MSGType.Error);}
        }
        private void RegisterOpenWhenReady()
        {
            string selector=HttpUtility.JavaScriptStringEncode("#"+mdlDuyetGioCong.ClientID);
            string script=@"(function(){var q='__ID__',tries=0,timer=null;
              function open(){
                tries++;
                var element=document.querySelector(q);
                if(element&&element.classList.contains('show')){if(timer!==null)clearInterval(timer);return;}
                var jq=window.jQuery||window.$;
                if(element&&window.CMSMasterJs&&typeof window.CMSMasterJs.OpenDialog==='function'&&
                   jq&&jq.fn&&typeof jq.fn.modal==='function'){
                  if(timer!==null)clearInterval(timer);
                  window.CMSMasterJs.OpenDialog(q,'Tính công vấn đề');return;
                }
                if(tries>=300&&timer!==null)clearInterval(timer);
              }
              timer=setInterval(open,50);open();})();";
            ScriptManager.RegisterStartupScript(Page,Page.GetType(),"IssuePmOnLoad"+Guid.NewGuid().ToString("N"),script.Replace("__ID__",selector),true);
        }
        protected void Page_Load(object sender,EventArgs e)
        {
            // Re-create Repeater items on every postback so their UniqueID matches posted hour inputs.
            string target=Request.Form["__EVENTTARGET"]??string.Empty;
            if(Page.IsPostBack && IdIssue!=Guid.Empty &&
                target.IndexOf("CtrlDuyetGioCongIssue1",StringComparison.OrdinalIgnoreCase)>=0)
                {try{LoadData(false);}catch(Exception ex){System.Diagnostics.Trace.TraceError("Issue PM load: "+ex);}}
        }
        private void EnsureSnapshot()
        {
            if(IdIssue==Guid.Empty || Nguoi==Guid.Empty||!Guid.TryParse(hfIssueId.Value,out Guid posted)||posted!=IdIssue||!Guid.TryParse(hfProjectId.Value,out Guid pd)||pd!=DuAn)
                throw new InvalidOperationException("Phiên duyệt giờ đã hết hiệu lực.");
            if(hfSnapshot.Value!=Fingerprint)throw new InvalidOperationException("Dữ liệu xác nhận đã thay đổi, vui lòng làm mới popup.");
            var issue=Mgr.LayThongTin(DuAn,IdIssue,Nguoi,IsAdmin);
            var rows=Mgr.LayBaoCao(DuAn,IdIssue,Nguoi,IsAdmin);
            if(Signature(rows,issue.TrangThaiDuyetGio)!=Fingerprint)throw new InvalidOperationException("Nhân viên vừa cập nhật giờ hoặc trạng thái đã đổi. Vui lòng làm mới trước khi chốt.");
            Guid token;if(!Guid.TryParse(hfRequestId.Value,out token)||LastSaved==token.ToString("D"))throw new InvalidOperationException("Thao tác đã được thực hiện hoặc không hợp lệ.");
        }
        private static int ParsePair(string h,string m)
        {
            int a,b;if(string.IsNullOrWhiteSpace(h)||string.IsNullOrWhiteSpace(m)||h.Trim().Length>4||m.Trim().Length>2||
                !int.TryParse(h,NumberStyles.None,CultureInfo.InvariantCulture,out a)||!int.TryParse(m,NumberStyles.None,CultureInfo.InvariantCulture,out b)||
                a<0||a>9999||b<0||b>59||((long)a*60+b)>525600)
                throw new InvalidOperationException("Giờ PM phải từ 0 đến 9.999, phút từ 00 đến 59, tổng mỗi loại không quá 525.600 phút.");
            return a*60+b;
        }
        private string Posted(RepeaterItem item,string id)
        {
            var ctrl=item.FindControl(id) as TextBox;
            if(ctrl==null)throw new InvalidOperationException("Không tìm thấy ô giờ công cần xác nhận.");
            var value=Request.Form[ctrl.UniqueID];
            if(value==null)throw new InvalidOperationException("Thiếu dữ liệu giờ công của nhân viên. Vui lòng làm mới popup.");
            return value;
        }
        private List<GioXuLyVanDeManager.GioPmChotDong> ReadRows(bool adjusting)
        {
            var list=new List<GioXuLyVanDeManager.GioPmChotDong>();
            if(_reports==null)throw new InvalidOperationException("Danh sách giờ công chưa được tải.");
            foreach(RepeaterItem item in repNhanVien.Items)
            {
                var idCtrl=item.FindControl("hfNhanVien") as HiddenField;Guid uid;
                if(idCtrl==null||!Guid.TryParse(idCtrl.Value,out uid))throw new InvalidOperationException("Không xác định được nhân viên cần chốt.");
                var row=_reports.SingleOrDefault(x=>x.IdNhanVien==uid);
                if(row==null)throw new InvalidOperationException("Danh sách nhân viên đã thay đổi.");
                if(!IsParticipant(row))continue;
                int regular=ParsePair(Posted(item,"txtHanhChinh"),Posted(item,"txtHanhChinhPhut"));
                int overtime=ParsePair(Posted(item,"txtTangCa"),Posted(item,"txtTangCaPhut"));
                var reason=Posted(item,"txtLyDo").Trim();
                if(reason.Length>2000)throw new InvalidOperationException("Ghi chú tối đa 2.000 ký tự.");
                int baseRegular=adjusting?(row.PhutThuongPmChot??0):(row.PhutThuongTuKhai??0);
                int baseOvertime=adjusting?(row.PhutTangCaPmChot??0):(row.PhutTangCaTuKhai??0);
                if((regular!=baseRegular||overtime!=baseOvertime)&&reason.Length==0)
                    throw new InvalidOperationException("PM phải nhập lý do nếu số giờ điều chỉnh khác bản nhân viên báo hoặc bản đã chốt.");
                if(adjusting && regular==baseRegular && overtime==baseOvertime)continue;
                list.Add(new GioXuLyVanDeManager.GioPmChotDong{IdNhanVien=uid,PhutThuong=regular,PhutTangCa=overtime,LyDo=reason,
                    LanKyVong=row.Lan,PhienBanKyVong=row.PhienBan,GiayHeThongKyVong=row.GiayHeThong});
            }
            return list;
        }
        protected void btnLamMoiDuyet_Click(object sender,EventArgs e)
        {try{DangSua=false;LoadData(true);}catch(Exception ex){ShowNotify(ex.Message,MSGType.Error);}}
        protected void btnNhacBaoHoanThanh_Click(object sender,EventArgs e)
        {
            try{
                var info=Mgr.LayThongTin(DuAn,IdIssue,Nguoi,IsAdmin);
                if(!info.LaQuanLy)throw new UnauthorizedAccessException("Không có quyền gửi nhắc nhở.");
                var todo=Mgr.LayBaoCao(DuAn,IdIssue,Nguoi,IsAdmin).Where(r=>IsParticipant(r)&&!r.DaBao).ToList();
                if(!todo.Any())throw new InvalidOperationException("Không còn nhân viên cần nhắc báo giờ.");
                int sent=0;
                foreach(var x in todo)
                    if(ThongBaoManager.Instance.Create(x.IdNhanVien,"Nhắc báo hoàn thành phần việc vấn đề",
                        "PM nhắc bạn kiểm tra và xác nhận giờ đã thực hiện trước khi chốt giờ công.",ThongBaoTypes.VanDe,null,DuAn,
                        "GioXuLyVanDe:"+IdIssue.ToString("D")+":report",false)!=null)sent++;
                if(sent!=todo.Count)throw new InvalidOperationException("Chỉ gửi được "+sent+"/"+todo.Count+" nhắc nhở. Vui lòng kiểm tra chuông thông báo trước khi thử lại.");
                ShowNotify("Đã gửi nhắc nhở tới "+sent+" nhân viên.",MSGType.Success);
                LoadData(true);
            }catch(Exception ex){ShowNotify(ex.Message,MSGType.Error);}
        }
        protected void btnBatDauDieuChinh_Click(object sender,EventArgs e)
        {try{DangSua=true;LoadData(false);}catch(Exception ex){ShowNotify(ex.Message,MSGType.Error);}}
        protected void btnChotGioCong_Click(object sender,EventArgs e)
        {
            try{
                EnsureSnapshot();var rows=ReadRows(false);
                if(!rows.Any())throw new InvalidOperationException("Chưa có nhân viên đủ điều kiện chốt giờ.");
                if(_reports.Where(IsParticipant).Any(x=>!x.DaBao))throw new InvalidOperationException("Còn nhân viên chưa báo giờ. Hãy gửi nhắc nhở trước khi chốt.");
                Mgr.ChotCong(DuAn,IdIssue,Nguoi,IsAdmin,rows);
                LastSaved=hfRequestId.Value;
                ShowNotify("PM đã chốt giờ công vấn đề; bản đối soát được lưu vào lịch sử.",MSGType.Success);
                DangSua=false;LoadData(true);
                if(Saved!=null)Saved(this,EventArgs.Empty);
            }catch(Exception ex){ShowNotify(ex.Message,MSGType.Error);}
        }
        protected void btnLuuDieuChinh_Click(object sender,EventArgs e)
        {
            try{
                if(!DangSua)throw new InvalidOperationException("Chưa bật chế độ điều chỉnh giờ.");
                EnsureSnapshot();var changed=ReadRows(true);
                if(changed.Count!=1)throw new InvalidOperationException("Mỗi lần chỉ điều chỉnh giờ của một nhân viên để bảo toàn lịch sử. Vui lòng chỉ thay đổi một dòng.");
                var x=changed[0];Mgr.DieuChinhSauChot(DuAn,IdIssue,Nguoi,IsAdmin,x.IdNhanVien,x.PhutThuong,x.PhutTangCa,x.LyDo,
                    x.LanKyVong,x.PhienBanKyVong);
                LastSaved=hfRequestId.Value;
                ShowNotify("Đã điều chỉnh giờ công của nhân viên và giữ lịch sử bản chốt trước.",MSGType.Success);
                DangSua=false;LoadData(true);
                if(Saved!=null)Saved(this,EventArgs.Empty);
            }catch(Exception ex){ShowNotify(ex.Message,MSGType.Error);}
        }
    }
}
