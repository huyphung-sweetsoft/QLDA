using Newtonsoft.Json;
using SweetSoft.QLDA.BackOffice.Common;
using SweetSoft.QLDA.Core.Caches;
using SweetSoft.QLDA.Core.EnumHelper;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.Core.ValueObjects;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using static SweetSoft.QLDA.Core.Functions.FunctionManager;

namespace SweetSoft.QLDA.BackOffice.fUsers.Controls
{
    public partial class CtrlPermission : BaseAdminUserControl
    {
        private readonly static string[] AllPermissions = new[] { "All" }
          .Concat(Enum.GetNames(typeof(ActionKeys))
              .Where(x => x != nameof(ActionKeys.None) && x != nameof(ActionKeys.All)))
          .ToArray();
        public Guid RoleId
        {
            get
            {
                if (ViewState["RoleId"] == null)
                    return Guid.Empty;
                return (Guid)ViewState["RoleId"];
            }
            set
            {
                ViewState["RoleId"] = value;
            }
        }
        public bool IsDisabled
        {
            get
            {
                if (ViewState["IsDisabled"] == null)
                    return false;
                return (bool)ViewState["IsDisabled"];
            }
            set
            {
                ViewState["IsDisabled"] = value;
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        public void InitPermission()
        {
            bool isDev = AppSettingHelpers.GetSetting<bool>("IsDevelopment");
            btnAddPermission.Visible = isDev;
            RenderAction();
            RenderPermission();
        }
        private void RenderAction()
        {
            string html = string.Empty;
            string template = itemTemplateHeader.InnerHtml;
            foreach(var t in AllPermissions)
            {
                string text = GetResourceText(t.ToUpper());
                switch (t)
                {
                    case "Create":
                        html += string.Format(template, "text-primary", text);
                        break;
                    case "Update":
                        html += string.Format(template, "text-warning", text);
                        break;
                    case "Delete":
                        html += string.Format(template, "text-danger", text);
                        break;
                    case "Export":
                        html += string.Format(template, "text-secondary", text + " Excel");
                        break;
                    case "View":
                    default:
                        html += string.Format(template, "text-secondary", text);
                        break;
                }
            }
            ltrHeader.Text = html;
        }
        private void RenderPermission()
        {
            List<AspnetFunction> aspnetFunctions = FunctionManager.Instance.GetAspnetFunctionWithPermissionKey();
            bool isDev = AppSettingHelpers.GetSetting<bool>("IsDevelopment");
            if (!isDev)
            {
                aspnetFunctions = aspnetFunctions.Where(t => t.IsActivated).ToList();
            }
            //--------------------------------------------
            ddlParentCode.Items.Clear();
            var functionParents = aspnetFunctions.Where(t => string.IsNullOrEmpty(t.ParentCode)).ToList();
            ddlParentCode.DataTextField = AspnetFunction.Columns.FunctionCode;
            ddlParentCode.DataValueField = AspnetFunction.Columns.FunctionCode;
            ddlParentCode.DataSource = functionParents;
            ddlParentCode.DataBind();
            //--------------------------------------------
            Func<string, string> buildFunc = null;

            string itemTemplateHtml = itemTemplate.InnerHtml;
            string itemTemplateParentHtml = itemTemplateParent.InnerHtml;
            List<AspnetPermission> aspnetPermissions = FunctionManager.Instance.GetAspnetPermissions();
            // Group quyền theo FunctionId
            var permissionDict = aspnetPermissions
     .GroupBy(p => p.FunctionId)
     .ToDictionary(g => g.Key, g => g.Select(p => p.PermissionKey).ToHashSet());
            int maxCol = AllPermissions.Length + 1;
            buildFunc = (parentCode) =>
            {
                string html = string.Empty;

                var children = aspnetFunctions
                    .Where(t => t.Id != Guid.Empty && t.ParentCode == parentCode)
                    .GroupBy(p => p.FunctionCode)
                    .Select(g => g.First())
                    .OrderBy(t => t.DisplayOrder)
                    .ToList();
                foreach (var child in children)
                {
                    string childHtml = buildFunc(child.FunctionCode);

                    var hasUrl = !string.IsNullOrEmpty(child.PageUrl);
                    var hasPermissions = permissionDict.TryGetValue(child.Id, out var perms);

                    if (string.IsNullOrEmpty(childHtml))
                    {
                        if (!hasUrl)
                        {
                            html += string.Format(itemTemplateParentHtml, child.Id, maxCol, GetResourceText(child.FunctionName));
                        }
                        else
                        {
                            // Tạo checkbox disabled nếu quyền không tồn tại
                            var checkboxHtml = BuildCheckboxes(child.FunctionCode, perms);
                            html += string.Format(itemTemplateHtml, child.Id, GetResourceText(child.FunctionName), checkboxHtml);
                        }
                    }
                    else
                    {
                        html += string.Format(itemTemplateParentHtml, child.Id, maxCol, GetResourceText(child.FunctionName)) + childHtml;
                    }
                }

                return html;
            };

            string htmlPermission = buildFunc("");

            ltrPermission.Text = htmlPermission.ToString();
            List<AspnetAssignRole> currentPermissions = FunctionManager.Instance.GetAspnetAssignRole(this.RoleId);
            List<PermissionModel> permissionModels = new List<PermissionModel>();
            currentPermissions?.ForEach(item =>
            {
                permissionModels.Add(new PermissionModel()
                {
                    IsAllowed = item.IsAllowed,
                    PermissionKey = item.PermissionKey
                });
            });
            hdfPermission.Value = JsonConvert.SerializeObject(permissionModels);
            pnlPermission.Update();
        }
        private string BuildCheckboxes(string functionCode, HashSet<string> permissions)
        {
            string html = "";

            foreach (var perm in AllPermissions)
            {
                bool isAvailable = (permissions?.Any(p => p.EndsWith(perm, StringComparison.OrdinalIgnoreCase)) ?? false)
                    || perm == "All"; // "All" luôn enable nếu bạn muốn
                string disabled = isAvailable && !this.IsDisabled ? "" : "disabled";
                string checkboxName = perm == "All" ? $"{functionCode}_All" : $"{functionCode}.{perm}";

                html += $"<td class=\"text-center\">" +
                            $"<input type=\"checkbox\" class=\"form-check-input {(!isAvailable ? "ignore-checkbox" : "")}\" name=\"{checkboxName}\" {disabled}>" +
                        $"</td>";
            }

            return html;
        }

        private class PermissionModel
        {
            public bool IsAllowed { get; set; }
            public string PermissionKey { get; set; }
            public bool IsDisable { get; set; }
        }

        protected void btnAddPermission_ServerClick(object sender, EventArgs e)
        {
            InitPermission();
            txtName.Text
               = txtKey.Text
               = txtPageUrl.Text
               = txtIcon.Text
               = string.Empty;
            ddlParentCode.SelectedIndex = -1;
            dlAddPermission.OpenModal(true);
        }
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtName.Text))
                {
                    ShowNotify("Vui lòng nhập tên quyền", MSGType.Warning);
                    return;
                }
                if (string.IsNullOrEmpty(txtKey.Text))
                {
                    ShowNotify("Vui lòng nhập mã quyền", MSGType.Warning);
                    return;
                }
                if (!string.IsNullOrEmpty(ddlParentCode.SelectedValue) && string.IsNullOrEmpty(txtPageUrl.Text))
                {
                    ShowNotify("Vui lòng nhập đường dẫn trang", MSGType.Warning);
                    return;
                }
                if (string.IsNullOrEmpty(txtIcon.Text))
                {
                    ShowNotify("Vui lòng nhập icon", MSGType.Warning);
                    return;
                }
                if (FunctionManager.Instance.IsFunctionCodeExisted(txtKey.Text))
                {
                    ShowNotify("Mã quyền đã tồn tại", MSGType.Warning);
                    return;
                }
                string userName = SweetContext.Current.UserName;
                var aspnetFunction = new AspnetFunction()
                {
                    FunctionCode = txtKey.Text,
                    ParentCode = ddlParentCode.SelectedValue,
                    FunctionName = txtName.Text,
                    PageUrl = txtPageUrl.Text,
                    Icon = txtIcon.Text,
                    IsActivated = true,
                };
                aspnetFunction.Save();
                if (string.IsNullOrEmpty(ddlParentCode.SelectedValue))
                    goto outer;
                // Save permission for module
                //----------------------------------------
                if (chkView.Checked)
                {
                    var permission = new AspnetPermission()
                    {
                        Id = UUIDv7.NewGuid(),
                        FunctionId = aspnetFunction.Id,
                        PermissionKey = $"{txtKey.Text}.View",
                        IsActivated = true,
                        IsDeleted = false,
                        DisplayOrder = 1,
                        CreatedBy = userName,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedBy = userName,
                        UpdatedDate = DateTime.UtcNow
                    };
                    permission.Save();
                }
                //----------------------------------------
                if (chkCreate.Checked)
                {
                    var permission = new AspnetPermission()
                    {
                        Id = UUIDv7.NewGuid(),
                        FunctionId = aspnetFunction.Id,
                        PermissionKey = $"{txtKey.Text}.Create",
                        PermissionName = $"Tạo {txtName.Text.ToLower()}",
                        IsActivated = true,
                        IsDeleted = false,
                        DisplayOrder = 2,
                        CreatedBy = userName,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedBy = userName,
                        UpdatedDate = DateTime.UtcNow
                    };
                    permission.Save();
                }
                //----------------------------------------
                if (chkUpdate.Checked)
                {
                    var permission = new AspnetPermission()
                    {
                        Id = UUIDv7.NewGuid(),
                        FunctionId = aspnetFunction.Id,
                        PermissionKey = $"{txtKey.Text}.Update",
                        PermissionName = $"Cập nhật {txtName.Text.ToLower()}",
                        IsActivated = true,
                        IsDeleted = false,
                        DisplayOrder = 3,
                        CreatedBy = userName,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedBy = userName,
                        UpdatedDate = DateTime.UtcNow
                    };
                    permission.Save();
                }
                //----------------------------------------
                if (chkDelete.Checked)
                {
                    var permission = new AspnetPermission()
                    {
                        Id = UUIDv7.NewGuid(),
                        FunctionId = aspnetFunction.Id,
                        PermissionKey = $"{txtKey.Text}.Delete",
                        PermissionName = $"Xóa {txtName.Text.ToLower()}",
                        IsActivated = true,
                        IsDeleted = false,
                        DisplayOrder = 4,
                        CreatedBy = userName,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedBy = userName,
                        UpdatedDate = DateTime.UtcNow
                    };
                    permission.Save();
                }//----------------------------------------
                if (chkExport.Checked)
                {
                    var permission = new AspnetPermission()
                    {
                        Id = UUIDv7.NewGuid(),
                        FunctionId = aspnetFunction.Id,
                        PermissionKey = $"{txtKey.Text}.Export",
                        PermissionName = $"Export {txtName.Text.ToLower()}",
                        IsActivated = true,
                        IsDeleted = false,
                        DisplayOrder = 5,
                        CreatedBy = userName,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedBy = userName,
                        UpdatedDate = DateTime.UtcNow
                    };
                    permission.Save();
                }
            outer:
                AppCache.Clear();
                InitPermission();
                ShowNotify("Thêm thành công", MSGType.Success);
                txtName.Text
           = txtKey.Text
           = txtPageUrl.Text
           = txtIcon.Text
           = string.Empty;
                ddlParentCode.SelectedIndex = -1;
                dlAddPermission.UpdateContentModal();
            }
            catch (Exception exc)
            {
                ShowNotify(exc.Message, MSGType.Error);
            }
        }

        public bool SavePermission()
        {
            if (this.RoleId == Guid.Empty)
                return false;
            if (string.IsNullOrEmpty(hdfPermission.Value))
                return false;
            List<PermissionModel> permissionModels = new List<PermissionModel>();
            try
            {
                permissionModels = JsonConvert.DeserializeObject<List<PermissionModel>>(hdfPermission.Value);
            }
            catch
            {
                permissionModels = null;
            }
            if (permissionModels == null)
                return false;
            var roleId = this.RoleId;
            string userName = SweetContext.Current.UserName;
            DateTime dt = DateTime.UtcNow;
            FunctionManager functionManager = FunctionManager.Instance;
            permissionModels.ForEach(p =>
            {
                AspnetAssignRole aspnetAssignRole = null;
                aspnetAssignRole = functionManager.GetAssignRole(roleId, p.PermissionKey);
                if (aspnetAssignRole == null)
                {
                    aspnetAssignRole = new AspnetAssignRole();
                    aspnetAssignRole.RoleId = roleId;
                    aspnetAssignRole.PermissionKey = p.PermissionKey;
                    aspnetAssignRole.IsAllowed = p.IsAllowed;
                    aspnetAssignRole.CreatedBy = userName;
                    aspnetAssignRole.CreatedDate = dt;
                    aspnetAssignRole.UpdatedBy = userName;
                    aspnetAssignRole.UpdatedDate = dt;
                    aspnetAssignRole.Save();
                }
                else
                {
                    aspnetAssignRole.IsAllowed = p.IsAllowed;
                    aspnetAssignRole.UpdatedBy = userName;
                    aspnetAssignRole.UpdatedDate = dt;
                    aspnetAssignRole.Save();
                }
            });
            return true;
        }
    }
}