using System;
using System.Data;
using System.Web.UI;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.DataAccess;
using SubSonic;
using SweetSoft.QLDA.Core.Managers;
namespace SweetSoft.QLDA.BackOffice.fNhanVien
{
    public partial class NhanVienDetail : BaseAdminPage
    {
        // 1. Kế thừa quyền phân quyền Module Nhân Viên
        public override ModuleKeys PAGE_FUNCTION_CODE
        {
            get { return ModuleKeys.NhanVien; }
        }

        private Guid CurrentIdNhanVien
        {
            get
            {
                if (ViewState["IdNhanVien"] != null)
                    return (Guid)ViewState["IdNhanVien"];
                return Guid.Empty;
            }
            set { ViewState["IdNhanVien"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Kiểm tra quyền XEM
                if (!this.IsView)
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error403), true);

                SetMetaTagsOgTags("Chi tiết nhân viên");

                // Lấy ID từ URL (đã mã hóa)
                string idQuery = CommonHelpers.QueryString("id");
                if (!string.IsNullOrEmpty(idQuery))
                {
                    Guid tempId = Guid.Empty;
                    if (Guid.TryParse(SecurityUtilities.UnprotectUrlParameter(idQuery), out tempId))
                    {
                        CurrentIdNhanVien = tempId;
                        LoadDataDetail(tempId);
                    }
                    else
                    {
                        Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error404), true);
                    }
                }
                else
                {
                    Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error404), true);
                }
            }
        }

        private void LoadDataDetail(Guid idNhanVien)
        {
            DataTable dt = NhanVienManager.Instance.GetNhanVienForDetail(idNhanVien);
            // Kiểm tra xem có dữ liệu không
            if (dt.Rows.Count == 0)
            {
                Response.Redirect(GetRelativeClientPath(RewriteURLHelper.Error404), true);
                return;
            }

            // Lấy dòng dữ liệu đầu tiên (và duy nhất)
            DataRow row = dt.Rows[0];

            // 1. Đổ dữ liệu cơ bản
            ltrTenNhanVien.Text = row["TenNhanVien"].ToString();
            ltrCCCD.Text = !string.IsNullOrEmpty(row["IdCCCD"].ToString()) ? row["IdCCCD"].ToString() : "Chưa cập nhật";
            ltrGioiTinh.Text = !string.IsNullOrEmpty(row["GioiTinh"].ToString()) ? row["GioiTinh"].ToString() : "Chưa cập nhật";
            ltrDiaChi.Text = !string.IsNullOrEmpty(row["DiaChi"].ToString()) ? row["DiaChi"].ToString() : "Chưa cập nhật địa chỉ";
            
            if (row["NgaySinh"] != DBNull.Value)
                ltrNgaySinh.Text = Convert.ToDateTime(row["NgaySinh"]).ToString("dd/MM/yyyy");
            else
                ltrNgaySinh.Text = "Chưa cập nhật";

            // 2. Tận dụng dữ liệu đã JOIN sẵn (Không cần gọi Manager)
            ltrChucDanh.Text = !string.IsNullOrEmpty(row["TenChucDanh"].ToString()) ? row["TenChucDanh"].ToString() : "Chưa cập nhật";
            ltrPhongBan.Text = !string.IsNullOrEmpty(row["TenPhongBan"].ToString()) ? row["TenPhongBan"].ToString() : "Chưa cập nhật";
            ltrEmail.Text = !string.IsNullOrEmpty(row["Email"].ToString()) && !row["Email"].ToString().Contains("no-email.com") ? row["Email"].ToString() : "Chưa cập nhật";
            ltrPhone.Text = !string.IsNullOrEmpty(row["PhoneNumber"].ToString()) ? row["PhoneNumber"].ToString() : "Chưa cập nhật";

            // 3. Xử lý logic Thâm niên & Ngày gia nhập
            if (row["NgayGiaNhap"] != DBNull.Value)
            {
                DateTime joinDate = Convert.ToDateTime(row["NgayGiaNhap"]);
                ltrNgayGiaNhap.Text = joinDate.ToString("dd/MM/yyyy");
                ltrThamNien.Text = CalculateSeniority(joinDate);
            }
            else
            {
                ltrNgayGiaNhap.Text = "Chưa cập nhật";
                ltrThamNien.Text = "Chưa xác định";
            }

            // 4. Hình ảnh
            string avatar = row["AnhDaiDien"].ToString();
            if (!string.IsNullOrEmpty(avatar))
                imgAvatar.Src = avatar;

            // 5. Kiểm tra quyền hiển thị Nút Sửa (Sửa lỗi CURRENT_PAGE)
            btnEditProfile.Visible = this.IsEdit;
        }

        // Hàm tiện ích: Tính thâm niên 
        private string CalculateSeniority(DateTime joinDate)
        {
            DateTime now = DateTime.Now;
            if (joinDate > now) return "Chưa bắt đầu làm việc";
            
            int years = now.Year - joinDate.Year;
            int months = now.Month - joinDate.Month;
            
            if (now.Day < joinDate.Day) months--;
            if (months < 0) { years--; months += 12; }
            
            if (years > 0 && months > 0) return $"{years} năm, {months} tháng";
            if (years > 0) return $"{years} năm";
            if (months > 0) return $"{months} tháng";
            
            int days = (now - joinDate).Days;
            return $"{days} ngày";
        }

        // 6. Xử lý khi nhấn nút "Sửa thông tin"
        protected void btnEditProfile_Click(object sender, EventArgs e)
        {
            if (CurrentIdNhanVien != Guid.Empty)
            {
                string editUrl = $"{RewriteURLHelper.NhanVien}?id={SecurityUtilities.ProtectUrlParameter(CurrentIdNhanVien.ToString())}";
                Response.Redirect(editUrl);
            }
        }
    }
}