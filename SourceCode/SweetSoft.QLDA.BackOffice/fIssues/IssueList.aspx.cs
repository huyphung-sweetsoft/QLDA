using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fIssues
{
    public partial class IssueList : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE => ModuleKeys.Issue;
        private Guid IssueId
        {
            get
            {
                if (ViewState["IssueId"] != null)
                    return (Guid)ViewState["IssueId"];
                return Guid.Empty;
            }
            set => ViewState["IssueId"] = value;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            CtrlProjectTabs1.ProjectId = CurrentProjectId;
            CtrlIssue1.NewIssueHandlerCallback += NewIssueAction;
            CtrlIssue1.EditIssueHandlerCallback += EditIssueAction;

            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);

                if (CurrentProjectId == Guid.Empty)
                {
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Projects), true);
                    return;
                }

                SetMetaTagsOgTags(GetResourceText(BackEndResourceKeys.ISSUE_LIST) ?? "Danh sách vấn đề");
                Navigation1.MainTitle = GetResourceText(BackEndResourceKeys.ISSUE_LIST) ?? "Danh sách vấn đề";
                Navigation1.keyValuePairUrls = new Dictionary<string, string>
                {
                    { GetRelativeClientPath(RewriteURLHelper.Projects), GetResourceText(BackEndResourceKeys.PROJECT_LIST) },
                    { "javascript:;", GetResourceText(BackEndResourceKeys.ISSUE_LIST) ?? "Danh sách vấn đề" }
                };
                ApplyControlsText();
                CtrlIssue1.InitControls();
            }
        }

        private void ApplyControlsText()
        {
            ddlCongViecBiAnhHuong.PlaceHolder = ddlMucDoAnhHuong.PlaceHolder =
            ddlCongViecPhatSinh.PlaceHolder   = ddlNguonGocVanDe.PlaceHolder =
            GetResourceText(BackEndResourceKeys.SELECT_VALUE);

            dlDetail.CloseText = GetResourceText(BackEndResourceKeys.CLOSE);

            txtTenVanDe.PlaceHolder = txtMoTaChiTiet.PlaceHolder = txtKeHoachXuLy.PlaceHolder = 
            txtNhanVien.PlaceHolder = GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);

        }

        private void NewIssueAction(object sender, EventArgs e)
        {
            RefreshIssueInfo();
            lbtSubmit.Visible = this.IsAdd;
            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.SAVE);
            dlDetail.Title = GetResourceText(BackEndResourceKeys.ADD_NEW);
            dlDetail.OpenModal(true);
        }

        private void EditIssueAction(object sender, EventArgs e)
        {
            if (sender == null)
            {
                ShowInvalidDataError();
                return;
            }

            Guid issueId = (Guid)sender;
            if (issueId == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            RefreshIssueInfo();
            lbtSubmit.Visible = this.IsEdit;

            TblVanDe issue = TblVanDe.FetchByID(issueId);
            if (issue == null || issue.DaXoa == true)
            {
                Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error404), false);
                return;
            }

            this.IssueId = issue.IdVanDe;
            txtTenVanDe.Text = issue.TenVanDe;
            txtMoTaChiTiet.Text = issue.MoTaChiTiet;
            txtKeHoachXuLy.Text = issue.KeHoachXuLy;

            if (issue.IdCongViecBiAnhHuong != null)
                ddlCongViecBiAnhHuong.SelectedValue = issue.IdCongViecBiAnhHuong.ToString();

            if (issue.MucDoAnhHuong != null)
                ddlMucDoAnhHuong.SelectedValue = issue.MucDoAnhHuong.ToString();

            if (issue.NguonGocVanDe != null)
                ddlNguonGocVanDe.SelectedValue = issue.NguonGocVanDe.ToString();

            if (issue.IdCongViecPhatSinh != null)
            {
                ddlCongViecPhatSinh.SelectedValue = issue.IdCongViecPhatSinh.ToString();
                txtNhanVien.Text = TaskManager.Instance.GetNhanVienByCongViec(issue.IdCongViecPhatSinh.Value);
            }
            else
            {
                txtNhanVien.Text = string.Empty;
            }

            lbtSubmit.ToolTip = lbtSubmit.Text = GetResourceText(BackEndResourceKeys.UPDATE);
            dlDetail.Title = GetResourceText(BackEndResourceKeys.EDIT);

            if (!IsPostBack)
                dlDetail.OpenModal(true, 1000);
            else
                dlDetail.OpenModal(true);
        }

        private void RefreshIssueInfo()
        {
            ControlHelpers controlHelpers = new ControlHelpers();

            controlHelpers.BindCongViecDuAn(ddlCongViecBiAnhHuong, CtrlIssue1.ProjectId); 
            controlHelpers.BindCongViecDuAn(ddlCongViecPhatSinh, CtrlIssue1.ProjectId);
            controlHelpers.BindMucDoAnhHuong(ddlMucDoAnhHuong); 
            controlHelpers.BindNguonGocVanDe(ddlNguonGocVanDe);

            lbtSubmit.Visible = false;
            txtTenVanDe.Text = txtMoTaChiTiet.Text = txtKeHoachXuLy.Text = txtNhanVien.Text="";

            if (ddlCongViecBiAnhHuong.Items.Count > 0) ddlCongViecBiAnhHuong.SelectedIndex = 0;
            if (ddlCongViecPhatSinh.Items.Count > 0) ddlCongViecPhatSinh.SelectedIndex = 0;
            if (ddlMucDoAnhHuong.Items.Count > 0) ddlMucDoAnhHuong.SelectedIndex = 0;
            if (ddlNguonGocVanDe.Items.Count > 0) ddlNguonGocVanDe.SelectedIndex = 0;

            this.IssueId = Guid.Empty;
        }

        protected void lbtSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                ValidationEngine validationEngine = ValidationEngine.Instance(this.Page);
                validationEngine.CheckValidControls(dlDetail.Controls);
                if (!validationEngine.IsValid)
                {
                    validationEngine.ShowErrorPrompt();
                    return;
                }

                TblVanDe issueDto = new TblVanDe();
                bool isNew = (this.IssueId == Guid.Empty);

                if (!isNew)
                {
                    issueDto.IdVanDe = this.IssueId;
                }

                issueDto.IdDuAn = CtrlIssue1.ProjectId;
                issueDto.TenVanDe = txtTenVanDe.Text.Trim();
                issueDto.MoTaChiTiet = !string.IsNullOrEmpty(txtMoTaChiTiet.Text.Trim()) ? txtMoTaChiTiet.Text.Trim() : null;
                issueDto.KeHoachXuLy = !string.IsNullOrEmpty(txtKeHoachXuLy.Text.Trim()) ? txtKeHoachXuLy.Text.Trim() : null;

                Guid idCongViecBiAnhHuong = Guid.Empty;
                if (this.GetValue(ddlCongViecBiAnhHuong, out idCongViecBiAnhHuong) && idCongViecBiAnhHuong != Guid.Empty)
                    issueDto.IdCongViecBiAnhHuong = idCongViecBiAnhHuong;

                Guid idCongViecPhatSinh = Guid.Empty;
                if (this.GetValue(ddlCongViecPhatSinh, out idCongViecPhatSinh) && idCongViecPhatSinh != Guid.Empty)
                    issueDto.IdCongViecPhatSinh = idCongViecPhatSinh;

                int mucDoAnhHuong = 0;
                if (this.GetValue(ddlMucDoAnhHuong, out mucDoAnhHuong) && mucDoAnhHuong > 0)
                    issueDto.MucDoAnhHuong = mucDoAnhHuong;

                int nguonGoc = 0;
                if (this.GetValue(ddlNguonGocVanDe, out nguonGoc))
                    issueDto.NguonGocVanDe = nguonGoc;

                TblVanDe savedIssue = IssueManager.Instance.CreateOrUpdate(issueDto);

                if (savedIssue == null)
                {
                    ShowInvalidDataError();
                    return;
                }

                ShowSuccessSaveData();
                dlDetail.CloseModal();
                CtrlIssue1.Rebind();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        public override void ConfirmRequest(ConfirmResult e)
        {
            CtrlIssue1.ConfirmRequest(e);
        }
        protected void ddlCongViecPhatSinh_SelectedIndexChanged(object sender, EventArgs e)
        {
            string idCongViec = ddlCongViecPhatSinh.SelectedValue;
            if (!string.IsNullOrEmpty(idCongViec) && idCongViec != "null")
            {
                Guid taskId = Guid.Parse(idCongViec);
                string danhSachNhanVien = TaskManager.Instance.GetNhanVienByCongViec(taskId);
                txtNhanVien.Text = danhSachNhanVien;
            }
            else
            {
                txtNhanVien.Text = "Công việc này chưa có nhân viên phụ trách!";
            }
        }
    }
}
