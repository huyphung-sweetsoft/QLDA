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
            return modules;
        }

        public List<AspnetFunction> GetProjectFunctionByUserId(Guid userId, Guid projectId)
        {
            List<AspnetFunction> modules = null;
            string cacheKey = $"ProjectModule_{userId}_{projectId}";
            if (!CacheManager.GetCacheData(cacheKey, out modules) || modules == null)
            {
                modules = _repository.GetProjectFunctionByUserId(userId, projectId);
                CacheManager.SetCacheData(cacheKey, modules);
            }
            return modules;
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
            return modules;
        }
        public List<AspnetFunction> GetAspnetFunctionWithPermissionKey()
        {
            return _repository.GetAspnetFunctionWithPermissionKey();
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
