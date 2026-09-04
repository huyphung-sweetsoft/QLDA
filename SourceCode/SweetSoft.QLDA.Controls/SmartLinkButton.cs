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

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!string.IsNullOrEmpty(ButtonIcon))
            {
                string iconHtml = $"<i class=\"{ButtonIcon}\"></i>";
                string resourceText = GetResourceText(ResourceKey);
                switch (ResourceKey)
                {
                    case "EMPLOYEE_DETAIL":
                        this.CssClass = "btn btn-outline-success btn-sm text-center btn-smart-link";
                        break;
                    case "DELETE":
                        this.CssClass = "btn btn-outline-danger btn-sm text-center btn-smart-link";
                        break;
                    case "EDIT":
                    case "ADDNEW":
                    case "ADD_NEW":
                    case "ADD":
                        this.CssClass = "btn btn-outline-info btn-sm text-center btn-smart-link";
                        break;
                    case "SAVE":
                        this.CssClass = "btn btn-outline-warning btn-sm text-center btn-smart-link";
                        break;
                    case "RESET_PASSWORD":
                    default:
                        this.CssClass = "btn btn-outline-primary btn-sm text-center btn-smart-link";
                        break;
                }
                this.Text = iconHtml; //+ resourceText;
                this.ToolTip = resourceText;
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
                default:
                    return resourceKey;

            }
        }
    }
}
