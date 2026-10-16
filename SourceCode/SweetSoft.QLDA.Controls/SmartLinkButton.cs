using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SweetSoft.QLDA.Controls
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:SmartLinkButton runat=server></{0}:SmartLinkButton>")]
    public class SmartLinkButton : LinkButton
    {
        public string ButtonIcon
        {
            get
            {
                if (ViewState["ButtonIcon"] == null)
                    return string.Empty;
                return (string)ViewState["ButtonIcon"];
            }
            set
            {
                ViewState["ButtonIcon"] = value;
            }
        }
        public string ResourceKey
        {
            get
            {
                if (ViewState["ResourceKey"] == null)
                    return string.Empty;
                return (string)ViewState["ResourceKey"];
            }
            set
            {
                ViewState["ResourceKey"] = value;
            }
        }
        public bool VisibleConditionKey
        {
            get
            {
                if (ViewState["VisibleConditionKey"] == null)
                    return false;
                return (bool)ViewState["VisibleConditionKey"];
            }
            set
            {
                ViewState["VisibleConditionKey"] = value;
            }
        }

        // [THÊM MỚI]: Thuộc tính để ép control tự render ra dạng mờ nhạt (disable)
        public bool IsDisabledStyle
        {
            get
            {
                if (ViewState["IsDisabledStyle"] == null)
                    return false;
                return (bool)ViewState["IsDisabledStyle"];
            }
            set
            {
                ViewState["IsDisabledStyle"] = value;
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!string.IsNullOrEmpty(ButtonIcon))
            {
                string iconHtml = $"<i class=\"{ButtonIcon}\"></i>";
                string resourceText = GetResourceText(ResourceKey);
                
                string baseClass = "";
                switch (ResourceKey)
                {
                    case "EMPLOYEE_DETAIL":
                    case "DETAIL":
                        baseClass = "btn btn-outline-success btn-sm text-center btn-smart-link";
                        break;
                    case "DELETE":
                        baseClass = "btn btn-outline-danger btn-sm text-center btn-smart-link";
                        break;
                    case "EDIT":
                    case "ADDNEW":
                    case "ADD_NEW":
                    case "ADD":
                        baseClass = "btn btn-outline-info btn-sm text-center btn-smart-link";
                        break;
                    case "SAVE":
                        baseClass = "btn btn-outline-warning btn-sm text-center btn-smart-link";
                        break;
                    case "FAST_APPROVAL":
                        baseClass = "btn btn-outline-success btn-sm text-center btn-smart-link";
                        break;
                    case "TOGGLE_LOCK": // [THÊM MỚI]: Style dành riêng cho nút khóa
                        baseClass = "btn btn-outline-primary btn-sm text-center btn-smart-link";
                        break;
                    case "RESET_PASSWORD":
                    default:
                        baseClass = "btn btn-outline-primary btn-sm text-center btn-smart-link";
                        break;
                }

                // [SỬA LẠI]: Nếu IsDisabledStyle = true thì add thêm class làm mờ + chặn bấm
                if (this.IsDisabledStyle)
                {
                    this.CssClass = baseClass + " opacity-25 pe-none";
                }
                else
                {
                    this.CssClass = baseClass;
                }

                this.Text = iconHtml; //+ resourceText;
                
                // Tránh ghi đè ToolTip nếu bên ngoài file .ascx đã truyền ToolTip động (như Khóa/Mở khóa)
                if (string.IsNullOrEmpty(this.ToolTip))
                {
                    this.ToolTip = resourceText;
                }
            }
            this.Visible = VisibleConditionKey;
        }

        private string GetResourceText(string resourceKey)
        {
            switch (resourceKey)
            {
                case "RESET_PASSWORD":
                    return "Đặt lại mật khẩu";
                case "EMPLOYEE_DETAIL":
                    return "Chi tiết nhân viên";
                case "DELETE":
                    return "Xóa";
                case "EDIT":
                    return "Sửa";
                case "ADDNEW":
                case "ADD_NEW":
                    return "Thêm mới";
                case "ADD":
                    return "Thêm";
                case "SAVE":
                    return "Lưu";
                case "FAST_APPROVAL":
                    return "Duyệt nhanh";
                case "TOGGLE_LOCK": // [THÊM MỚI]: Text tooltip mặc định cho nút Khóa
                    return "Khóa/Mở khóa tài khoản";
                default:
                    return resourceKey;

            }
        }
    }
}