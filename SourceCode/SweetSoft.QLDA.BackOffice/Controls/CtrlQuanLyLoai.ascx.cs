using SweetSoft.QLDA.BackOffice.MasterPages;
using SweetSoft.QLDA.Controls;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
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
            lblError.Visible = false;
            lblSuccess.Visible = false;
            grvData.EditIndex = e.NewEditIndex;
            BindData();
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
                Guid idLoai = Guid.Parse(grvData.DataKeys[e.RowIndex].Value.ToString());
                var txtTenLoaiEdit = (ExtraTextBox)grvData.Rows[e.RowIndex].FindControl("txtTenLoaiEdit");
                
                if (txtTenLoaiEdit != null)
                {
                    string newTen = txtTenLoaiEdit.Text;
                    
                    var loai = LoaiManager.Instance.GetLoaiById(idLoai);
                    if (loai != null)
                    {
                        loai.TenLoai = newTen;
                        LoaiManager.Instance.UpdateLoai(loai);
                        
                        grvData.EditIndex = -1;
                        ShowSuccess("Cập nhật thành công!");
                        BindData();
                        
                        // Notify parent
                        OnDataChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
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
