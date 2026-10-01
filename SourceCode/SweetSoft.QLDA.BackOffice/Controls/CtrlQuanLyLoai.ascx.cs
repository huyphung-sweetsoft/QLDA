using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.BackOffice.Controls
{
    public partial class CtrlQuanLyLoai : System.Web.UI.UserControl
    {
        public event EventHandler OnDataChanged;

        public string DoiTuong
        {
            get => ViewState["DoiTuong"]?.ToString();
            set => ViewState["DoiTuong"] = value;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            txtTenLoaiNew.EnterSubmitClientID = btnSaveNew.ClientID;
            if (!IsPostBack)
            {
                lblError.Visible = false;
                lblSuccess.Visible = false;
            }
        }

        public void ShowModal(string doiTuong)
        {
            DoiTuong = doiTuong;
            string title = "Quản lý loại danh mục";
            if (doiTuong == LoaiManager.LoaiDoiTuong.DuAn) title = "Quản lý Loại Dự Án";
            else if (doiTuong == LoaiManager.LoaiDoiTuong.KhachHang) title = "Quản lý Loại Khách Hàng";
            else if (doiTuong == LoaiManager.LoaiDoiTuong.ChucDanh) title = "Quản lý Chúc danh";
            else if (doiTuong == LoaiManager.LoaiDoiTuong.PhongBan) title = "Quản lý Phòng ban";


                mdlQuanLyLoai.Title = title;
            lblError.Visible = false;
            lblSuccess.Visible = false;
            txtTenLoaiNew.Text = string.Empty;
            
            BindData();
            mdlQuanLyLoai.OpenModal();
            upnlMain.Update();
        }

        private void BindData()
        {
            if (string.IsNullOrEmpty(DoiTuong)) return;
            
            var lst = LoaiManager.Instance.GetByDoiTuong(DoiTuong);
            grvData.DataSource = lst;
            grvData.DataBind();
        }

        protected void btnSaveNew_Click(object sender, EventArgs e)
        {
            try
            {
                string tenLoai = txtTenLoaiNew.Text;
                
                TblLoai newLoai = new TblLoai
                {
                    TenLoai = tenLoai,
                    DoiTuong = DoiTuong,
                    KichHoat = true
                };

                LoaiManager.Instance.InsertLoai(newLoai);
                
                txtTenLoaiNew.Text = string.Empty;
                ShowSuccess("Thêm mới thành công!");
                BindData();
                
                // Notify parent
                OnDataChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        protected void grvData_RowEditing(object sender, GridViewEditEventArgs e)
        {
            grvData.EditIndex = e.NewEditIndex;
            BindData();

            GridViewRow row = grvData.Rows[e.NewEditIndex];

            var txtTenLoaiEdit =
                row.FindControl("txtTenLoaiEdit") as ExtraTextBox;

            var btnUpdate =
                row.FindControl("btnUpdate");

            if (txtTenLoaiEdit != null && btnUpdate != null)
            {
                txtTenLoaiEdit.EnterSubmitClientID = btnUpdate.ClientID;
            }
        }

        protected void grvData_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            lblError.Visible = false;
            lblSuccess.Visible = false;
            grvData.EditIndex = -1;
            BindData();
        }

        protected void grvData_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                Guid idLoai = Guid.Parse(
                    grvData.DataKeys[e.RowIndex].Value.ToString());

                var txtTenLoaiEdit =
                    (ExtraTextBox)grvData.Rows[e.RowIndex]
                        .FindControl("txtTenLoaiEdit");

                if (txtTenLoaiEdit == null)
                {
                    ShowError("Không tìm thấy ô nhập tên loại.");
                    return;
                }

                var loai = LoaiManager.Instance.GetLoaiById(idLoai);

                if (loai == null)
                {
                    ShowError("Không tìm thấy dữ liệu cần cập nhật.");
                    return;
                }

                string oldTen = (loai.TenLoai ?? string.Empty).Trim();

                string[] postedValues =
                    Request.Form.GetValues(txtTenLoaiEdit.UniqueID);

                if (postedValues == null || postedValues.Length == 0)
                {
                    ShowError("Không nhận được dữ liệu tên loại.");
                    return;
                }

                var distinctValues = postedValues
                    .Where(x => x != null)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.Ordinal)
                    .ToList();

                string newTen;

                if (distinctValues.Count == 1)
                {
                    newTen = distinctValues[0];
                }
                else
                {
                    // Trường hợp có nhiều value do cùng một control
                    // xuất hiện nhiều lần trong POST.
                    // Loại bỏ giá trị cũ để tìm giá trị người dùng vừa nhập.
                    var changedValues = distinctValues
                        .Where(x => !string.Equals(
                            x,
                            oldTen,
                            StringComparison.Ordinal))
                        .ToList();

                    if (changedValues.Count == 1)
                    {
                        newTen = changedValues[0];
                    }
                    else
                    {
                        ShowError("Dữ liệu tên loại không hợp lệ hoặc bị gửi trùng.");
                        return;
                    }
                }

                loai.TenLoai = newTen;

                LoaiManager.Instance.UpdateLoai(loai);

                grvData.EditIndex = -1;

                ShowSuccess("Cập nhật thành công!");
                BindData();

                OnDataChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        protected void grvData_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                Guid idLoai = Guid.Parse(grvData.DataKeys[e.RowIndex].Value.ToString());
                LoaiManager.Instance.DeleteLoai(idLoai);
                
                ShowSuccess("Xóa thành công!");
                BindData();
                
                // Notify parent
                OnDataChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            lblError.Visible = true;
            lblSuccess.Visible = false;
        }

        private void ShowSuccess(string message)
        {
            lblSuccess.Text = message;
            lblSuccess.Visible = true;
            lblError.Visible = false;
        }
    }
}
