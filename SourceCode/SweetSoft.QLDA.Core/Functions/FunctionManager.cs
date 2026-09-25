using SweetSoft.QLDA.Core.Caches;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Interfaces;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.Functions
{
    public class FunctionManager : BaseManager
    {
        //-------------------------------------
        // Các hàm này dùng để lấy danh sách các quyền của từng module
        // Chú ý: Các hàm này chỉ trả về danh sách quyền của module đó, không kiểm tra quyền của user

        private static readonly Lazy<FunctionManager> _instance = new Lazy<FunctionManager>(() => new FunctionManager());
        public static FunctionManager Instance => _instance.Value;
        private readonly RoleRepository _repository;
        private readonly AuditManager _auditManager;
        public FunctionManager(IAppContext applicationContext = null) : base(applicationContext)
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new RoleRepository(_auditManager);
        }
        public bool IsActionKeyExisted(Guid userId, ModuleKeys moduleKey, ActionKeys actionKey)
        {
            return _repository.IsAllowAction(userId, moduleKey, actionKey);
        }
        public bool IsFunctionCodeExisted(string functionCode)
        {
            return _repository.IsFunctionCodeExisted(functionCode);
        }
        public List<string> GetPermissionByUserId(Guid userId)
        {
            List<string> permissions = null;
            if (!CacheManager.GetCacheData($"PermissionByUserId_{userId}", out permissions) || permissions == null)
            {
                permissions = _repository.GetFunctionCodesByUserId(userId);
                CacheManager.SetCacheData($"PermissionByUserId_{userId}", permissions);
            }
            return permissions;
        }
        public List<AspnetFunction> GetAspnetFunctionByUserId(Guid userId, bool isDev)
        {
            List<AspnetFunction> modules = null;
            if (!CacheManager.GetCacheData($"ModuleByUserId_{userId}", out modules) || modules == null)
            {
                if (UserManager.Instance.IsAdministrator(userId))
                    modules = _repository.GetAllAspnetFunctions();
                else if (isDev)
                    modules = _repository.GetAspnetFunctionByUserId(userId);
                else
                    modules = _repository.GetAspnetFunctionActiveByUserId(userId);

                CacheManager.SetCacheData($"ModuleByUserId_{userId}", modules);
            }
            List<AspnetFunction> visible = WithoutRetiredDocumentCatalogues(modules);
            bool canViewDocuments = DocumentManager.Instance.CanAccessDocumentArea(ActionKeys.View);
            visible.RemoveAll(module => string.Equals(module.FunctionCode,
                ModuleKeys.Document.ToString(), StringComparison.OrdinalIgnoreCase));
            if (canViewDocuments)
            {
                List<AspnetFunction> configured = _repository.GetAllAspnetFunctions();
                AspnetFunction document = configured.FirstOrDefault(module =>
                    string.Equals(module.FunctionCode, ModuleKeys.Document.ToString(),
                        StringComparison.OrdinalIgnoreCase));
                if (document != null)
                {
                    visible.Add(document);
                    if (!string.IsNullOrEmpty(document.ParentCode)
                        && !visible.Any(module => string.Equals(module.FunctionCode,
                            document.ParentCode, StringComparison.OrdinalIgnoreCase)))
                    {
                        AspnetFunction parent = configured.FirstOrDefault(module =>
                            string.Equals(module.FunctionCode, document.ParentCode,
                                StringComparison.OrdinalIgnoreCase));
                        if (parent != null) visible.Add(parent);
                    }
                }
            }
            // A project-only permission may bring back the fDocument parent
            // through the legacy menu query. Do not render an empty menu.
            if (!visible.Any(module => module.OfProject != true
                && string.Equals(module.ParentCode, "fDocument",
                    StringComparison.OrdinalIgnoreCase)))
            {
                visible.RemoveAll(module => string.Equals(module.FunctionCode,
                    "fDocument", StringComparison.OrdinalIgnoreCase));
            }

            // Signing assignments are per-account and independent of the
            // dossier permissions. Put the inbox under the dossier menu for
            // every signed-in account; the page only returns/processes files
            // assigned to that user.
            visible.RemoveAll(module => string.Equals(
                module.FunctionCode,
                ModuleKeys.DocumentSigningInbox.ToString(),
                StringComparison.OrdinalIgnoreCase));
            if (userId != Guid.Empty)
            {
                AspnetFunction documentMenu = visible.FirstOrDefault(module =>
                    string.Equals(module.FunctionCode, "fDocument",
                        StringComparison.OrdinalIgnoreCase));
                if (documentMenu == null)
                {
                    documentMenu = _repository.GetAllAspnetFunctions()
                        .FirstOrDefault(module => string.Equals(
                            module.FunctionCode,
                            "fDocument",
                            StringComparison.OrdinalIgnoreCase));
                }

                if (documentMenu == null)
                {
                    documentMenu = new AspnetFunction
                    {
                        Id = Guid.NewGuid(),
                        FunctionCode = "fDocument",
                        FunctionName = "DOCUMENT_MANAGEMENT",
                        PageUrl = "/Documents",
                        DisplayOrder = 14,
                        Icon = "fas fa-folder-open",
                        IsActivated = true,
                        OfProject = false
                    };
                }
                documentMenu.ParentCode = string.Empty;
                visible.RemoveAll(module => string.Equals(
                    module.FunctionCode,
                    "fDocument",
                    StringComparison.OrdinalIgnoreCase));
                visible.Add(documentMenu);

                visible.Add(new AspnetFunction
                {
                    Id = Guid.NewGuid(),
                    FunctionCode = ModuleKeys.DocumentSigningInbox.ToString(),
                    ParentCode = "fDocument",
                    FunctionName = "SIGNING_INBOX",
                    PageUrl = "/fDocuments/SigningInbox.aspx",
                    DisplayOrder = 16,
                    Icon = "fas fa-file-signature",
                    IsActivated = true,
                    OfProject = false
                });
            }
            return visible;
        }

        /// <summary>
        /// Danh sách cấu hình tab dùng chung cho ngữ cảnh dự án.
        /// Danh sách này chỉ mô tả tab nào tồn tại; quyền hiển thị của từng
        /// user vẫn được BaseAdminPage.IsUserRight kiểm tra ở UI.
        /// </summary>
        public List<AspnetFunction> GetProjectTabFunctions()
        {
            // Đây là một danh mục cấu hình rất nhỏ và chỉ được đọc một lần khi
            // render thanh tab. Không cache để thay đổi OfProject/PageUrl/Thứ tự
            // hiển thị trong database có hiệu lực ngay, không phải chờ cache hết hạn.
            return WithoutRetiredDocumentCatalogues(_repository.GetProjectTabFunctions());
        }
        public List<string> GetAllModules(Guid userId, bool isDev)
        {
            List<AspnetFunction> permissions = GetAspnetFunctionByUserId(userId, isDev);
            if (permissions == null || permissions.Count == 0)
                return new List<string>();
            return permissions.Select(p => p.FunctionCode).Distinct().ToList();
        }
        public List<AspnetFunction> GetAspnetFunction()
        {
            List<AspnetFunction> modules = null;
            if (!CacheManager.GetCacheData("AllModules", out modules) || modules == null)
            {
                modules = _repository.GetAllAspnetFunctions();
                CacheManager.SetCacheData("AllModules", modules);
            }
            return WithoutRetiredDocumentCatalogues(modules);
        }
        public List<AspnetFunction> GetAspnetFunctionWithPermissionKey()
        {
            return _repository.GetAspnetFunctionWithPermissionKey();
        }

        private static List<AspnetFunction> WithoutRetiredDocumentCatalogues(List<AspnetFunction> modules)
        {
            return (modules ?? new List<AspnetFunction>()).Where(module =>
                !string.Equals(module.FunctionCode, ModuleKeys.DocumentGroup.ToString(), StringComparison.OrdinalIgnoreCase)
                && !string.Equals(module.FunctionCode, ModuleKeys.DocumentTemplate.ToString(), StringComparison.OrdinalIgnoreCase)
                && !string.Equals(module.FunctionCode, ModuleKeys.DocumentAdministration.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
        }
        public List<AspnetPermission> GetAspnetPermissions()
        {
            return _repository.GetAspnetPermissions();
        }
        public List<AspnetAssignRole> GetAspnetAssignRole(Guid roleId)
        {
            return _repository.GetAspnetAssignRoles(roleId);
        }
        public AspnetAssignRole GetAssignRole(Guid roleId, string permissionKey)
        {
            return _repository.GetAssignRole(roleId, permissionKey);
        }
    }
}
