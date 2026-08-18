using Newtonsoft.Json;
using SweetSoft.QLDA.Core.Caches;
using SweetSoft.QLDA.Core.ExceptionHelpers;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Interfaces;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.Core.ValueObjects;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace SweetSoft.QLDA.Core.Managers
{
    public class RoleManager : BaseManager
    {
        private static readonly Lazy<RoleManager> _instance = new Lazy<RoleManager>(() => new RoleManager());
        public static RoleManager Instance => _instance.Value;
        private readonly RoleRepository _repository;
        private readonly AuditManager _auditManager;
        public RoleManager(IAppContext applicationContext = null) : base(applicationContext) 
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new RoleRepository(_auditManager);
        }
        public DataTable SearchRoles(string searchTerm, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            return _repository.SearchPaging(searchTerm, orderBy, pageNumber, pageSize, out totalRecord);
        }
        public DataTable SearchRoles(Dictionary<string, object> searchParameters, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            return _repository.SearchPaging(searchParameters, orderBy, pageNumber, pageSize, out totalRecord);
        }
        public AspnetRole CreateOrUpdate(AspnetRole dto)
        {
            // Validate input data
            BusinessValidator.ThrowIfNull(dto, BackEndResourceKeys.INVALID_DATA);
            BusinessValidator.ThrowIfNullOrEmpty(dto.RoleName, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.RoleName));
            BusinessValidator.ThrowIfNullOrEmpty(dto.LoweredRoleName, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.LoweredRoleName));

            AspnetRole aspnetRole;
            // Validate unique constraints
            if (dto.RoleId == Guid.Empty)
            {
                BusinessValidator.ThrowIf(_repository.GetByRoleName(dto.RoleName) != null,
                    BackEndResourceKeys.USERNAME_ALREADY_EXISTS, nameof(dto.RoleName), ErrorCodes.Conflict);

            }

            if (dto.RoleId != Guid.Empty)
            {
                aspnetRole = _repository.GetById(dto.RoleId);
                BusinessValidator.ThrowIfNull(aspnetRole, BackEndResourceKeys.NOT_FOUND, nameof(dto.RoleId), ErrorCodes.NotFound);

                ObjectHelper.CopyBusinessProperties(dto, aspnetRole,
                    x => x.ApplicationId,
                    x => x.CreatedBy,
                    x => x.CreatedDate);
                aspnetRole = _repository.Update(aspnetRole);
                BusinessValidator.ThrowIfNull(aspnetRole, BackEndResourceKeys.SERVICE_UNAVAILABLE, nameof(dto), ErrorCodes.ServiceUnavailable);
                return aspnetRole;
            }
            else
            {
                aspnetRole = dto.Clone() as AspnetRole;
                BusinessValidator.ThrowIfNull(aspnetRole, BackEndResourceKeys.INVALID_DATA);
                aspnetRole.RoleId = UUIDv7.NewGuid();
                aspnetRole = _repository.Insert(aspnetRole);
                BusinessValidator.ThrowIfNull(aspnetRole, BackEndResourceKeys.SERVICE_UNAVAILABLE, nameof(dto), ErrorCodes.ServiceUnavailable);
                return aspnetRole;
            }
        }
        public bool Delete(AspnetRole item)
        {
            BusinessValidator.ThrowIfNull(item, BackEndResourceKeys.INVALID_DATA);
            //--------------------------------------------
            return _repository.Delete(item);
        }
        public AspnetRole GetRoleById(Guid id)
        {
            return _repository.GetById(id);
        }
        public AspnetRole GetRoleByRoleName(string userName)
        {
            return _repository.GetByRoleName(userName);
        }
        public AspnetRole GetRoleByUserId(Guid userId)
        {
            return _repository.GetRoleByUserId(userId);
        }
        public List<AspnetRole> GetAllRoles()
        {
            return _repository.GetAllAspnetRoles();
        }
        public bool IsAssignPermission(Guid userId)
        {
            return _repository.IsAssignPermission(userId);
        }
        public bool IsUserInRole(Guid userId, Guid roleId)
        {
            return _repository.IsUserInRole(userId, roleId);
        }
        public void RemoveAllRoleOfUser(Guid userId)
        {
            _repository.RemoveAllRoleOfUser(userId);
        }
        #region Helpers
        public AutocompleteObj AllRoleAutocomplete(string keyword, int maxResult, string lang)
        {
            int total;
            DataTable dt = SearchRoles(keyword, "RoleName ASC", 1, maxResult, out total);

            List<AutocompleteItem> listAutocompleteItem = new List<AutocompleteItem>();
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                total = dt.Rows.Count;
                foreach (DataRow row in dt.Rows)
                {
                    AutocompleteItem autocompleteItem = new AutocompleteItem();
                    string title = row["RoleName"].ToString();
                    if (string.IsNullOrEmpty(title))
                        title = row["Email"].ToString();

                    autocompleteItem.Label = string.Format("<span class=\"tag activated\">{0}</span>" +
                        "<span class=\"sub-info\">" +
                        "<i>Name: {1}</i>" +
                        "</span>"
                            , title
                            , row["RoleName"]);

                    autocompleteItem.Value = title;
                    autocompleteItem.Data = row["Id"].ToString();
                    autocompleteItem.OtherData = JsonConvert.SerializeObject(new
                    {
                        RoleName = row["RoleName"].ToString(),
                        Email = row["Email"].ToString(),
                    });
                    listAutocompleteItem.Add(autocompleteItem);
                }
            }

            AutocompleteObj autocompleteObj = new AutocompleteObj();
            autocompleteObj.Total = total;
            autocompleteObj.ListAutocompleteItem = listAutocompleteItem;
            return autocompleteObj;
        }
        #endregion
    }
}
