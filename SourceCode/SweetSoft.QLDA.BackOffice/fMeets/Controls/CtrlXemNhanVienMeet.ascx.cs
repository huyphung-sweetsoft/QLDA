using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;

namespace SweetSoft.QLDA.BackOffice.fMeets.Controls
{
    public partial class CtrlXemNhanVienMeet : BaseAdminUserControl
    {
        protected void Page_Load(object sender, EventArgs e) { }

        public void OpenModal(Guid idCuocHop, DateTime startDate, DateTime endDate, string tenCuocHop, Guid? idNguoiChuTri = null)
        {
            TblLichHop meet = TblLichHop.FetchByID(idCuocHop);

            List<Guid> assignedIds = MeetManager.Instance.GetNhanVienCuocHop(idCuocHop);
            DataTable dtUsers = ThanhVienDuAnManager.Instance.GetThanhVienDuAnDetail(this.CURRENT_PAGE.CurrentProjectId);

            var filteredRows = dtUsers.AsEnumerable()
                .Where(row => assignedIds.Contains((Guid)row["UserId"]))
                .OrderByDescending(row => idNguoiChuTri.HasValue && (Guid)row["UserId"] == idNguoiChuTri.Value)
                .ThenBy(row => row["DisplayName"].ToString())
                .ToList();

            if (filteredRows.Count > 0)
            {
                rptAssignedMembers.DataSource = BuildDisplayList(filteredRows, idNguoiChuTri);
                rptAssignedMembers.DataBind();
                rptAssignedMembers.Visible = true;
                divEmpty.Visible = false;
            }
            else
            {
                rptAssignedMembers.Visible = false;
                divEmpty.Visible = true;
            }

            ltrTenCuocHop.Text = meet != null ? meet.TenCuocHop : tenCuocHop;
            ltrMaCuocHop.Text = meet != null ? (meet.MaCuocHop ?? "—") : "—";
            ltrThoiGian.Text = $"{startDate:dd/MM/yyyy HH:mm} - {endDate:dd/MM/yyyy HH:mm}";
            ltrDiaDiem.Text = meet != null && !string.IsNullOrEmpty(meet.DiaDiemHop) ? meet.DiaDiemHop : "—";
            ltrTotalMember.Text = filteredRows.Count.ToString();

            mdlViewMeetMember.Title = "Chi tiết thành viên tham gia";
            mdlViewMeetMember.OpenModal(true);
        }

        private List<object> BuildDisplayList(List<DataRow> rows, Guid? hostId)
        {
            var list = new List<object>();
            for (int i = 0; i < rows.Count; i++)
            {
                DataRow row = rows[i];
                Guid userId = (Guid)row["UserId"];
                string displayName = row["DisplayName"] != DBNull.Value ? row["DisplayName"].ToString() : "";
                string email = row["Email"] != DBNull.Value ? row["Email"].ToString() : "";
                string avatar = row["Avatar"] != DBNull.Value ? row["Avatar"].ToString() : "";

                list.Add(new
                {
                    UserId = userId,
                    DisplayName = displayName,
                    Email = email,
                    IsHost = hostId.HasValue && userId == hostId.Value,
                    AvatarHtml = GetSingleAvatarHtml(displayName, avatar, i)
                });
            }
            return list;
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "";
            string[] parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
            return (parts[parts.Length - 2].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
        }

        private string GetSingleAvatarHtml(string name, string avatar, int index)
        {
            string[] colors = { "#f59e0b", "#3b82f6", "#10b981", "#8b5cf6", "#ec4899" };
            string color = colors[index % colors.Length];
            bool isDefaultAvatar = string.IsNullOrEmpty(avatar) || avatar.EndsWith("/Styles/images/user-icon.png", StringComparison.OrdinalIgnoreCase);

            if (!isDefaultAvatar)
            {
                string avatarUrl = avatar.StartsWith("~") ? Page.ResolveUrl(avatar) : avatar;
                string fallbackHtml = $"<div class=\\'single-avatar-circle\\' style=\\'background-color: {color};\\'>{GetInitials(name)}</div>";
                return $"<img src='{avatarUrl}' class='single-avatar-circle' style='object-fit: cover;' onerror=\"this.onerror=null; this.outerHTML='{fallbackHtml}';\" />";
            }
            else
            {
                return $"<div class='single-avatar-circle' style='background-color: {color};'>{GetInitials(name)}</div>";
            }
        }
    }
}