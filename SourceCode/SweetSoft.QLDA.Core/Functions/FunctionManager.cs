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
using System.Data.SqlClient;
using System.Diagnostics;
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
        public bool CanAccessSigningInbox(Guid userId, bool process = false)
        {
            if (userId == Guid.Empty) return false;
            var user = AspnetUser.FetchByID(userId);
            if (user == null || user.IsActivated != true || user.IsDeleted == true) return false;
            if (!_repository.GetAllAspnetFunctions().Any(f =>
                f.FunctionCode == ModuleKeys.DocumentSigningInbox.ToString())) return false;
            if (UserManager.Instance.IsAdministrator(userId)) return true;
            // Read current grants, including active role/permission flags. Do not
            // reuse session rights after a group administrator revokes access.
            string sql = string.Format(@"
                SELECT COUNT(DISTINCT p.PermissionKey)
                FROM dbo.aspnet_UsersInRoles ur
                JOIN dbo.aspnet_Roles r ON r.RoleId=ur.RoleId AND r.IsActivated=1 AND r.IsDeleted=0
                JOIN dbo.aspnet_AssignRoles a ON a.RoleId=r.RoleId AND a.IsAllowed=1
                JOIN dbo.aspnet_Permission p ON p.PermissionKey=a.PermissionKey
                    AND p.IsActivated=1 AND p.IsDeleted=0
                JOIN dbo.aspnet_Functions f ON f.Id=p.FunctionId
                    AND f.FunctionCode='DocumentSigningInbox' AND f.IsActivated=1
                WHERE ur.UserId='{0}' AND (p.PermissionKey='DocumentSigningInbox.View'
                    OR ({1}=1 AND p.PermissionKey='DocumentSigningInbox.Update'))",
                userId, process ? 1 : 0);
            return new SubSonic.InlineQuery().ExecuteScalar<int>(sql) == (process ? 2 : 1);
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
            bool canViewDocuments = false;
            try
            {
                canViewDocuments = DocumentManager.Instance.CanAccessDocumentArea(ActionKeys.View);
            }
            catch (SqlException ex) when (ex.Message.IndexOf("fn_HoSo_GroupRight", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // Hồ sơ chưa được triển khai trong DB: không để lỗi quyền của
                // một module làm biến mất toàn bộ menu hệ thống.
                Trace.TraceWarning("Document permission function is unavailable: {0}", ex.Message);
            }
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

            // Recheck group access instead of trusting a cached menu entry.
            // Assignment checks still restrict the inbox to the user's files.
            visible.RemoveAll(module => string.Equals(
                module.FunctionCode,
                ModuleKeys.DocumentSigningInbox.ToString(),
                StringComparison.OrdinalIgnoreCase));
            if (CanAccessSigningInbox(userId))
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

                visible.Add(_repository.GetAllAspnetFunctions().First(module =>
                    module.FunctionCode == ModuleKeys.DocumentSigningInbox.ToString()));
            }
            if (!visible.Any(module => module.OfProject != true
                && string.Equals(module.ParentCode, "fDocument", StringComparison.OrdinalIgnoreCase)))
                visible.RemoveAll(module => string.Equals(module.FunctionCode,
                    "fDocument", StringComparison.OrdinalIgnoreCase));
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
