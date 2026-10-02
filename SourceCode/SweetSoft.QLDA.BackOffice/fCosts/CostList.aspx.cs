using SubSonic;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.FileManager;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.fCosts
{
    public partial class CostList : BaseAdminPage
    {
        public override ModuleKeys PAGE_FUNCTION_CODE => ModuleKeys.Cost;

        private ControlHelpers _control = new ControlHelpers();

        private Guid CostId
        {
            get => ViewState["CostId"] != null ? (Guid)ViewState["CostId"] : Guid.Empty;
            set => ViewState["CostId"] = value;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            CtrlProjectTabs1.ProjectId = CurrentProjectId;
            CtrlCost1.NewCostHandlerCallback += NewCostAction;
            CtrlCost1.EditCostHandlerCallback += EditCostAction;
            CtrlCost1.OpenCostFilesHandlerCallback += OpenCostFilesAction;

            fbCostFiles.FileMutationValidator = (recordId, refType, fileId) =>
                ProjectRecordFileAccess.CanAccess(
                    SweetContext.Current.UserId,
                    recordId,
                    refType.ToString(),
                    true) &&
                ProjectRecordFileAccess.BelongsToRecord(
                    recordId,
                    refType.ToString(),
                    fileId);

            if (!IsPostBack)
            {
                if (!this.IsView)
                    Response.Redirect(
                        GetRelativeClientPath(RewriteURLHelper.Error403),
                        true);

                if (CurrentProjectId == Guid.Empty)
                {
                    Response.Redirect(
                        GetRelativeClientPath(RewriteURLHelper.Projects),
                        true);
                    return;
                }

                string title =
                    GetResourceText(BackEndResourceKeys.COST_LIST)
                    ?? "Danh sách chi phí";

                SetMetaTagsOgTags(title);
                Navigation1.MainTitle = title;

                Navigation1.keyValuePairUrls =
                    new Dictionary<string, string>
                    {
                        {
                            GetRelativeClientPath(RewriteURLHelper.Projects),
                            GetResourceText(BackEndResourceKeys.PROJECT_LIST)
                        },
                        {
                            "javascript:;",
                            title
                        }
                    };

                ApplyControlsText();
                CtrlCost1.InitControls();
            }
        }

        private void ApplyControlsText()
        {
            dlDetail.Title = "Thông tin chi phí";
            dlDetail.CloseText =
                GetResourceText(BackEndResourceKeys.CLOSE);

            txtTenKhoanChi.PlaceHolder =
                GetResourceText(BackEndResourceKeys.ENTER_THE_VALUE);

            ddlNhanVienYeuCau.PlaceHolder =
                "Chọn người yêu cầu";

            _control.BindTrangThaiChiPhi(ddlTrangThai);
        }

        private bool IsCreatorPm(Guid? creatorId)
        {
            if (!creatorId.HasValue ||
                creatorId.Value == Guid.Empty)
            {
                return false;
            }

            Guid? pmId =
                DuAnManager.Instance.LayIdNhanVienQuanLy(CurrentProjectId);

            return pmId.HasValue &&
                   pmId.Value != Guid.Empty &&
                   pmId.Value == creatorId.Value;
        }

        private bool CanSelectRequester(Guid? creatorId)
        {
            return this.IsPM ||
                   IsCreatorPm(creatorId);
        }

        private void SetupUIByRole(
            bool isNew,
            Guid? idNhanVienDeNghi = null,
            Guid? creatorId = null)
        {
            bool canSelectRequester =
                CanSelectRequester(creatorId);

            pnlRequesterDropdown.Visible =
                canSelectRequester;

            pnlRequesterFixed.Visible =
                !canSelectRequester;

            ddlNhanVienYeuCau.Visible =
                canSelectRequester;

            Guid requesterId =
                idNhanVienDeNghi.HasValue &&
                idNhanVienDeNghi.Value != Guid.Empty
                    ? idNhanVienDeNghi.Value
                    : SweetContext.Current.UserId;

            if (canSelectRequester)
            {
                BindThanhVienDuAn(requesterId);

                txtNhanVienYeuCau.Text =
                    GetSelectedRequesterName(requesterId);
            }
            else
            {
                BindRequesterInfo(requesterId);

                txtNhanVienYeuCau.Text =
                    litRequesterName.Text;
            }

            ddlTrangThai.Enabled =
                this.IsPM;

            if (!this.IsPM &&
                isNew &&
                ddlTrangThai.Items.FindByValue("0") != null)
            {
                ddlTrangThai.SelectedValue =
                    "0";
            }
        }

        private void BindThanhVienDuAn(Guid selectedUserId)
        {
            ddlNhanVienYeuCau.Items.Clear();

            _control.BindNhanVienDuAnKemAvatar(
                ddlNhanVienYeuCau,
                CurrentProjectId);

            if (selectedUserId != Guid.Empty &&
                ddlNhanVienYeuCau.Items.FindByValue(
                    selectedUserId.ToString()) != null)
            {
                ddlNhanVienYeuCau.SelectedValue =
                    selectedUserId.ToString();
            }
        }

        private string GetSelectedRequesterName(Guid requesterId)
        {
            ListItem item =
                ddlNhanVienYeuCau.Items.FindByValue(
                    requesterId.ToString());

            return item != null
                ? item.Text
                : "—";
        }

        private bool IsProjectMember(Guid userId)
        {
            if (userId == Guid.Empty)
                return false;

            DataTable dtUsers =
                ThanhVienDuAnManager.Instance.GetThanhVienDuAnDetail(
                    CurrentProjectId);

            if (dtUsers == null ||
                !dtUsers.Columns.Contains("UserId"))
            {
                return false;
            }

            foreach (DataRow row in dtUsers.Rows)
            {
                if (row["UserId"] == DBNull.Value)
                    continue;

                if (Guid.TryParse(
                        row["UserId"].ToString(),
                        out Guid memberId) &&
                    memberId == userId)
                {
                    return true;
                }
            }

            return false;
        }

        private string GetAvatarUrl(string avatar)
        {
            if (string.IsNullOrWhiteSpace(avatar))
                return "";

            if (avatar.EndsWith(
                    "/Styles/images/user-icon.png",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "";
            }

            return avatar.StartsWith(
                    "~",
                    StringComparison.Ordinal)
                ? Page.ResolveUrl(avatar)
                : avatar;
        }

        private void BindRequesterInfo(Guid requesterId)
        {
            string displayName = "—";
            string email = "Chưa cập nhật email";
            string avatar = "";

            try
            {
                if (requesterId != Guid.Empty)
                {
                    AspnetUser user =
                        UserManager.Instance.GetUserById(
                            requesterId);

                    if (user != null)
                    {
                        displayName =
                            string.IsNullOrWhiteSpace(user.DisplayName)
                                ? user.UserName
                                : user.DisplayName;

                        email =
                            GetUserEmail(user.UserName);

                        if (string.IsNullOrWhiteSpace(email))
                            email = "Chưa cập nhật email";

                        avatar =
                            user.Avatar;
                    }
                }
            }
            catch
            {
            }

            litRequesterName.Text =
                HttpUtility.HtmlEncode(displayName);

            litRequesterEmail.Text =
                HttpUtility.HtmlEncode(email);

            litRequesterAvatar.Text =
                GetAvatarHtml(
                    displayName,
                    avatar,
                    0);
        }

        private void BindCreatorInfo(Guid creatorId)
        {
            string displayName = "—";
            string email = "Chưa cập nhật email";
            string avatar = "";

            try
            {
                if (creatorId != Guid.Empty)
                {
                    AspnetUser user =
                        UserManager.Instance.GetUserById(
                            creatorId);

                    if (user != null)
                    {
                        displayName =
                            string.IsNullOrWhiteSpace(user.DisplayName)
                                ? user.UserName
                                : user.DisplayName;

                        email =
                            GetUserEmail(user.UserName);

                        if (string.IsNullOrWhiteSpace(email))
                            email = "Chưa cập nhật email";

                        avatar =
                            user.Avatar;
                    }
                }
            }
            catch
            {
            }

            litCreatorName.Text =
                HttpUtility.HtmlEncode(displayName);

            litCreatorEmail.Text =
                HttpUtility.HtmlEncode(email);

            litCreatorAvatar.Text =
                GetAvatarHtml(
                    displayName,
                    avatar,
                    1);

            txtNguoiTao.Text =
                displayName;
        }

        private string GetUserEmail(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return "";

            try
            {
                MembershipUser user =
                    Membership.GetUser(userName);

                return user != null
                    ? user.Email
                    : "";
            }
            catch
            {
                return "";
            }
        }

        private string GetAvatarHtml(
            string displayName,
            string avatar,
            int index)
        {
            string[] colors =
            {
                "#7c3aed",
                "#2563eb",
                "#059669",
                "#d97706",
                "#db2777"
            };

            string color =
                colors[index % colors.Length];

            string safeName =
                HttpUtility.HtmlAttributeEncode(
                    displayName ?? "");

            string initials =
                HttpUtility.HtmlEncode(
                    GetInitials(displayName));

            bool isDefaultAvatar =
                string.IsNullOrWhiteSpace(avatar) ||
                avatar.EndsWith(
                    "/Styles/images/user-icon.png",
                    StringComparison.OrdinalIgnoreCase);

            if (!isDefaultAvatar)
            {
                string avatarUrl =
                    avatar.StartsWith(
                        "~",
                        StringComparison.Ordinal)
                        ? Page.ResolveUrl(avatar)
                        : avatar;

                avatarUrl =
                    HttpUtility.HtmlAttributeEncode(
                        avatarUrl);

                string fallbackHtml =
                    $"<div class='cost-person-avatar' style='background:{color};'>{initials}</div>";

                return
                    $"<img src='{avatarUrl}' " +
                    $"class='cost-person-avatar' " +
                    $"alt='{safeName}' " +
                    $"title='{safeName}' " +
                    $"onerror=\"this.onerror=null;this.outerHTML='{HttpUtility.JavaScriptStringEncode(fallbackHtml)}';\" />";
            }

            return
                $"<div class='cost-person-avatar' style='background:{color};' title='{safeName}'>{initials}</div>";
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "?";

            string[] parts =
                fullName
                    .Trim()
                    .Split(
                        new[] { ' ' },
                        StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
                return parts[0]
                    .Substring(0, 1)
                    .ToUpper();

            return (
                parts[parts.Length - 2]
                    .Substring(0, 1) +
                parts[parts.Length - 1]
                    .Substring(0, 1)
            ).ToUpper();
        }

        private void OpenCostFilesAction(
            object sender,
            EventArgs e)
        {
            Guid idChiPhi =
                sender is Guid
                    ? (Guid)sender
                    : Guid.Empty;

            if (idChiPhi == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            TblChiPhi cost =
                TblChiPhi.FetchByID(idChiPhi);

            if (cost == null ||
                cost.DaXoa == true ||
                cost.IdDuAn != CurrentProjectId)
            {
                ShowInvalidDataError();
                return;
            }

            if (!ProjectRecordFileAccess.CanAccess(
                SweetContext.Current.UserId,
                idChiPhi,
                FileUploadTypes.CostAttachment.ToString(),
                false))
            {
                ShowAccessDeniedNotify();
                return;
            }

            fbCostFiles.IsMultiple = true;

            fbCostFiles.IsEnabled =
                ProjectRecordFileAccess.CanAccess(
                    SweetContext.Current.UserId,
                    idChiPhi,
                    FileUploadTypes.CostAttachment.ToString(),
                    true);

            fbCostFiles.LoadFile(
                idChiPhi,
                FileUploadTypes.CostAttachment);

            dlCostFiles.OpenModal(true);
        }

        private void NewCostAction(
            object sender,
            EventArgs e)
        {
            RefreshCostInfo();
            txtSoLuong.Text = "1";
            lbtSubmit.Visible =
                this.IsAdd;

            lbtSubmit.ToolTip =
                lbtSubmit.Text =
                    GetResourceText(
                        BackEndResourceKeys.SAVE);

            dlDetail.Title =
                GetResourceText(
                    BackEndResourceKeys.ADD_NEW);

            txtNgayTao.Text =
                DateTime.Now.ToString("dd/MM/yyyy");

            txtNguoiTao.Text =
                SweetContext.Current.UserName;

            Guid creatorId =
                SweetContext.Current.UserId;

            SetupUIByRole(
                true,
                creatorId,
                creatorId);

            if (ddlTrangThai.Items.FindByValue("0") != null)
                ddlTrangThai.SelectedValue =
                    "0";

            BindCreatorInfo(
                creatorId);

            pnlStatusNew.Visible = true;
            pnlStatusEdit.Visible = false;

            dlDetail.OpenModal(true);
        }

        private void EditCostAction(
            object sender,
            EventArgs e)
        {
            if (sender == null ||
                (Guid)sender == Guid.Empty)
            {
                ShowInvalidDataError();
                return;
            }

            Guid costId =
                (Guid)sender;

            RefreshCostInfo();

            lbtSubmit.Visible =
                this.IsEdit;

            TblChiPhi cost =
                TblChiPhi.FetchByID(costId);

            if (cost == null ||
                cost.DaXoa == true)
            {
                Response.Redirect(
                    GetRelativeClientPath(
                        RewriteURLHelper.Error404),
                    false);

                return;
            }

            if (!cost.IdNguoiTao.HasValue ||
                cost.IdNguoiTao.Value == Guid.Empty ||
                cost.IdNguoiTao.Value != SweetContext.Current.UserId)
            {
                ShowNotify(
                    "Bạn không có quyền edit",
                    MSGType.Warning);

                return;
            }

            CostId =
                cost.IdChiPhi;

            txtTenKhoanChi.Text =
                cost.TenKhoanChi;

            txtMoTaChiTiet.Text =
                cost.MoTaChiTiet;

            txtDonGia.Text =
                cost.DonGia?.ToString("#,##0", new System.Globalization.CultureInfo("vi-VN"));

            txtSoLuong.Text =
                cost.SoLuong?.ToString();

            txtTongTien.Text =
                cost.SoTien.ToString("#,##0", new System.Globalization.CultureInfo("vi-VN"));

            if (cost.NgayTao != null)
            {
                txtNgayTao.Text =
                    cost.NgayTao.Date.ToString(
                        "dd/MM/yyyy");
            }

            if (cost.TrangThai != null)
            {
                ddlTrangThai.SelectedValue =
                    cost.TrangThai.ToString();

                if (cost.TrangThai == 2)
                {
                    txtLyDoTuChoi.Text =
                        cost.LyDoTuChoi;

                    divLyDoTuChoi.Style["display"] =
                        "block";
                }
                else
                {
                    divLyDoTuChoi.Style["display"] =
                        "none";
                }
            }

            SetupUIByRole(
                false,
                cost.IdNhanVienDeNghi,
                cost.IdNguoiTao);

            if (cost.IdNguoiTao.HasValue &&
                cost.IdNguoiTao.Value != Guid.Empty)
            {
                BindCreatorInfo(
                    cost.IdNguoiTao.Value);
            }
            else
            {
                BindCreatorInfo(Guid.Empty);
            }

            pnlStatusNew.Visible = false;
            pnlStatusEdit.Visible = true;

            lbtSubmit.ToolTip =
                lbtSubmit.Text =
                    GetResourceText(
                        BackEndResourceKeys.UPDATE);

            dlDetail.Title =
                GetResourceText(
                    BackEndResourceKeys.EDIT)
                ?? "Thông tin chi phí";

            dlDetail.OpenModal(
                true,
                IsPostBack ? 0 : 1000);
        }

        private void RefreshCostInfo()
        {
            txtTenKhoanChi.Text =
                txtDonGia.Text =
                txtSoLuong.Text =
                txtTongTien.Text =
                txtMoTaChiTiet.Text =
                txtNgayTao.Text =
                txtNguoiTao.Text =
                    "";

            txtLyDoTuChoi.Text =
                "";

            divLyDoTuChoi.Style["display"] =
                "none";

            litRequesterName.Text =
                "";

            litRequesterEmail.Text =
                "";

            litRequesterAvatar.Text =
                "";

            litCreatorName.Text =
                "";

            litCreatorEmail.Text =
                "";

            litCreatorAvatar.Text =
                "";

            pnlRequesterDropdown.Visible =
                false;

            pnlRequesterFixed.Visible =
                false;

            pnlStatusNew.Visible =
                false;

            pnlStatusEdit.Visible =
                true;

            ddlNhanVienYeuCau.Items.Clear();

            if (ddlTrangThai.Items.Count > 0)
                ddlTrangThai.SelectedIndex =
                    0;

            lbtSubmit.Visible =
                false;

            CostId =
                Guid.Empty;
        }

        protected void lbtSubmit_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                ValidationEngine validationEngine =
                    ValidationEngine.Instance(
                        this.Page);

                validationEngine.CheckValidControls(
                    dlDetail.Controls);

                if (!validationEngine.IsValid)
                {
                    validationEngine.ShowErrorPrompt();
                    return;
                }

                bool isNew =
                    CostId == Guid.Empty;

                TblChiPhi costDto =
                    isNew
                        ? new TblChiPhi()
                        : TblChiPhi.FetchByID(CostId);

                if (costDto == null)
                {
                    ShowInvalidDataError();
                    return;
                }

                Guid? originalCreatorId =
                    costDto.IdNguoiTao;

                Guid? originalRequesterId =
                    costDto.IdNhanVienDeNghi;

                costDto.IdDuAn =
                    CtrlCost1.ProjectId;

                costDto.TenKhoanChi =
                    txtTenKhoanChi.Text.Trim();

                costDto.MoTaChiTiet =
                    !string.IsNullOrEmpty(
                        txtMoTaChiTiet.Text.Trim())
                        ? txtMoTaChiTiet.Text.Trim()
                        : null;

                // ==========================================
                // FIX LỖI PARSE ĐƠN GIÁ VÀ SỐ LƯỢNG: CẮT DẤU CHẤM
                // ==========================================
                string donGiaText = txtDonGia.Text.Trim().Replace(".", "");
                if (decimal.TryParse(donGiaText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal donGia))
                {
                    costDto.DonGia = donGia;
                }

                string soLuongText = txtSoLuong.Text.Trim().Replace(".", "");
                if (int.TryParse(soLuongText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out int soLuong))
                {
                    costDto.SoLuong = soLuong;
                }
                // ==========================================

                costDto.SoTien =
                    (costDto.DonGia ?? 0) *
                    (costDto.SoLuong ?? 0);

                if (int.TryParse(
                        ddlTrangThai.SelectedValue,
                        out int trangThai))
                {
                    costDto.TrangThai =
                        (byte)trangThai;

                    if (trangThai == 2)
                    {
                        if (string.IsNullOrWhiteSpace(
                                txtLyDoTuChoi.Text))
                        {
                            ShowNotify(
                                "Vui lòng nhập lý do từ chối!",
                                MSGType.Warning);

                            divLyDoTuChoi.Style["display"] =
                                "block";

                            dlDetail.OpenModal(true);
                            return;
                        }

                        costDto.LyDoTuChoi =
                            txtLyDoTuChoi.Text.Trim();
                    }
                    else
                    {
                        costDto.LyDoTuChoi =
                            null;
                    }
                }

                Guid creatorId;

                if (isNew)
                {
                    creatorId =
                        SweetContext.Current.UserId;

                    costDto.IdNguoiTao =
                        creatorId;

                    costDto.NgayTao =
                        DateTime.Now;

                    costDto.DaXoa =
                        false;
                }
                else
                {
                    if (!originalCreatorId.HasValue ||
                        originalCreatorId.Value == Guid.Empty)
                    {
                        ShowInvalidDataError();
                        return;
                    }

                    creatorId =
                        originalCreatorId.Value;

                    costDto.IdNguoiTao =
                        creatorId;
                }

                bool canSelectRequester =
                    CanSelectRequester(creatorId);

                Guid requesterId;

                if (canSelectRequester)
                {
                    if (!Guid.TryParse(
                            ddlNhanVienYeuCau.SelectedValue,
                            out requesterId) ||
                        requesterId == Guid.Empty)
                    {
                        ShowNotify(
                            "Vui lòng chọn người yêu cầu!",
                            MSGType.Warning);

                        dlDetail.OpenModal(true);
                        return;
                    }

                    if (!IsProjectMember(
                            requesterId))
                    {
                        ShowNotify(
                            "Người yêu cầu phải là thành viên của dự án.",
                            MSGType.Warning);

                        dlDetail.OpenModal(true);
                        return;
                    }
                }
                else if (isNew)
                {
                    requesterId =
                        SweetContext.Current.UserId;
                }
                else
                {
                    requesterId =
                        originalRequesterId.HasValue &&
                        originalRequesterId.Value != Guid.Empty
                            ? originalRequesterId.Value
                            : creatorId;
                }

                costDto.IdNguoiTao =
                    creatorId;

                costDto.IdNhanVienDeNghi =
                    requesterId;

                TblChiPhi savedCost =
                    CostManager.Instance.CreateOrUpdate(
                        costDto);

                if (savedCost == null)
                {
                    ShowInvalidDataError();
                    return;
                }

                savedCost.IdNguoiTao =
                    creatorId;

                savedCost.IdNhanVienDeNghi =
                    requesterId;

                savedCost.Save();

                ShowSuccessSaveData();

                dlDetail.CloseModal();

                CtrlCost1.Rebind();
            }
            catch (Exception exc)
            {
                ShowNotify(
                    exc.Message,
                    MSGType.Error);
            }
        }
    }
}