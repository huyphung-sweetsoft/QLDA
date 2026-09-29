using SubSonic;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.EnumHelper.Defines;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace SweetSoft.QLDA.BackOffice.fRisks.Controls
{
    public partial class CtrlViewRiskDetail : BaseAdminUserControl
    {
        // Hàm mở popup
        public void OpenModal(Guid riskId)
        {
            if (riskId == Guid.Empty)
                return;

            TblRuiRoDuAn risk = TblRuiRoDuAn.FetchByID(riskId);
            if (risk == null || risk.DaXoa == true)
                return;

            BindRisk(risk);
            upRiskView.Update();
            mdlRiskView.OpenModal(true);
        }

        // Đổ dữ liệu rủi ro ra giao diện
        private void BindRisk(TblRuiRoDuAn risk)
        {
            lblRiskName.Text = HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(risk.TenRuiRo) ? "—" : risk.TenRuiRo);

            // Xác suất (thêm dấu % cho đẹp)
            lblProbability.Text = RiskManager.Instance.GetXacSuatRuiRoText(risk.XacSuatXayRa);

            // Mức độ ảnh hưởng
            lblImpact.Text = GetMucDoAnhHuongText(risk.MucDoAnhHuong);

            // Mức độ rủi ro (Tính toán + Phân loại)
            lblRiskLevel.Text = GetRiskLevelText(risk.XacSuatXayRa, risk.MucDoAnhHuong, Convert.ToDecimal(risk.DiemRuiRo.Value));

            // Thông tin khởi tạo
            lblCreatedBy.Text = HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(risk.NguoiTao) ? "—" : risk.NguoiTao);
            lblCreatedDate.Text = risk.NgayTao.ToString("dd/MM/yyyy HH:mm");

            // Kế hoạch (có định dạng xuống dòng)
            ltrKeHoachPhongNgua.Text = ToDisplayHtml(risk.KeHoachPhongNgua);
            ltrKeHoachUngPho.Text = ToDisplayHtml(risk.KeHoachUngPho);

            // Load nhân viên giám sát
            BindAssignee(risk.IdNhanVienXuLy);
        }

        // Hàm bind duy nhất 1 nhân viên xử lý
        private void BindAssignee(Guid? assigneeId)
        {
            List<object> result = new List<object>();

            if (assigneeId.HasValue && assigneeId.Value != Guid.Empty)
            {
                AspnetUser user = UserManager.Instance.GetUserById(assigneeId.Value);
                if (user != null)
                {
                    string displayName = string.IsNullOrWhiteSpace(user.DisplayName) ? user.UserName : user.DisplayName;
                    string email = GetUserEmail(user.UserName);

                    result.Add(new
                    {
                        DisplayName = displayName,
                        Email = string.IsNullOrWhiteSpace(email) ? "Chưa cập nhật email" : email,
                        AvatarHtml = GetAvatarHtml(displayName, user.Avatar, 0)
                    });
                }
            }

            rptAssignees.DataSource = result;
            rptAssignees.DataBind();
            pnlNoAssignees.Visible = result.Count == 0;
        }

        // Logic tính và phân loại Điểm rủi ro (y hệt form Edit)
        private string GetRiskLevelText(decimal? xacSuat, int? mucDoAnhHuong, decimal? diemRuiRo)
        {
            if (!xacSuat.HasValue || !mucDoAnhHuong.HasValue) return "—";

            decimal score = diemRuiRo ?? ((xacSuat.Value / 100m) * mucDoAnhHuong.Value);
            string textMucDoRuiRo = "";

            if (xacSuat.Value >= 75m || mucDoAnhHuong.Value >= 4)
            {
                if (score >= 4.5m)
                    textMucDoRuiRo = GetResourceText(BackEndResourceKeys.VERY_HIGH);
                else
                    textMucDoRuiRo = GetResourceText(BackEndResourceKeys.HIGH);
            }
            else
            {
                if (score < 1.0m)
                    textMucDoRuiRo = GetResourceText(BackEndResourceKeys.VERY_LOW);
                else if (score >= 1.0m && score < 2.0m)
                    textMucDoRuiRo = GetResourceText(BackEndResourceKeys.LOW);
                else if (score >= 2.0m && score < 3.5m)
                    textMucDoRuiRo = GetResourceText(BackEndResourceKeys.MEDIUM);
                else if (score >= 3.5m && score < 4.5m)
                    textMucDoRuiRo = GetResourceText(BackEndResourceKeys.HIGH);
                else
                    textMucDoRuiRo = GetResourceText(BackEndResourceKeys.VERY_HIGH);
            }

            return score > 0 ? $"{textMucDoRuiRo} ({score.ToString("0.##")})" : "—";
        }

        private string GetUserEmail(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return "";
            try
            {
                MembershipUser user = Membership.GetUser(userName);
                return user != null ? user.Email : "";
            }
            catch
            {
                return "";
            }
        }

        private string GetAvatarHtml(string displayName, string avatar, int index)
        {
            string[] colors = { "#7c3aed", "#2563eb", "#059669", "#d97706", "#db2777" };
            string color = colors[index % colors.Length];
            string safeName = HttpUtility.HtmlEncode(displayName ?? "");
            string initials = HttpUtility.HtmlEncode(GetInitials(displayName));
            bool isDefaultAvatar = string.IsNullOrWhiteSpace(avatar) || avatar.EndsWith("/Styles/images/user-icon.png", StringComparison.OrdinalIgnoreCase);

            if (!isDefaultAvatar)
            {
                string avatarUrl = avatar.StartsWith("~", StringComparison.Ordinal) ? Page.ResolveUrl(avatar) : avatar;
                avatarUrl = HttpUtility.HtmlAttributeEncode(avatarUrl);
                string fallbackHtml = $"<div class='risk-person-avatar' style='background:{color};'>{initials}</div>";
                return $"<img src='{avatarUrl}' class='risk-person-avatar' alt='{safeName}' title='{safeName}' onerror=\"this.onerror=null;this.outerHTML='{HttpUtility.JavaScriptStringEncode(fallbackHtml)}';\" />";
            }
            return $"<div class='risk-person-avatar' style='background:{color};' title='{safeName}'>{initials}</div>";
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "?";
            string[] parts = fullName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0].Substring(0, 1).ToUpper();
            return (parts[parts.Length - 2].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
        }

        private string ToDisplayHtml(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "<span class='text-muted'>—</span>";
            return HttpUtility.HtmlEncode(value).Replace("\r\n", "<br />").Replace("\n", "<br />");
        }

        private string GetMucDoAnhHuongText(object value)
        {
            if (value == null || value == DBNull.Value)
                return "—";
            // Sử dụng chung Enum Mức độ ảnh hưởng với Issue
            MucDoAnhHuonEnum mucDo = (MucDoAnhHuonEnum)Convert.ToInt32(value);
            return HttpUtility.HtmlEncode(GetResourceText(IssueManager.Instance.GetValueForMucDoAnhHuong(mucDo)));
        }
    }
}