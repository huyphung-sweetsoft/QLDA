using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using SubSonic;
using SweetSoft.QLDA.Core.Caches;
using SweetSoft.QLDA.Core.EnumHelper;
using SweetSoft.QLDA.Core.ExceptionHelpers;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Interfaces;
using SweetSoft.QLDA.Core.ResourceTexts;
using SweetSoft.QLDA.Core.Respositories;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using System.Windows;

namespace SweetSoft.QLDA.Core.Managers
{
    public class UserManager : BaseManager
    {
        private static readonly Lazy<UserManager> _instance = new Lazy<UserManager>(() => new UserManager());
        public static UserManager Instance => _instance.Value;
        private readonly UserRepository _repository;
        private readonly AuditManager _auditManager;
        public UserManager(IAppContext applicationContext = null) : base(applicationContext) 
        {
            _auditManager = new AuditManager(GetClientInfo());
            _repository = new UserRepository(_auditManager);
        }
        public DataTable SearchUsers(string searchTerm, Guid roleId, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            return _repository.SearchPaging(searchTerm, roleId, orderBy, pageNumber, pageSize, out totalRecord);
        }
        public DataTable SearchUsers(string searchTerm, Dictionary<string, object> searchParameters, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            return _repository.SearchPaging(searchTerm, searchParameters, orderBy, pageNumber, pageSize, out totalRecord);
        }
        public DataTable SearchUsers(Dictionary<string, object> searchParameters, string orderBy, int pageNumber, int pageSize, out int totalRecord)
        {
            return _repository.SearchPaging(searchParameters, orderBy, pageNumber, pageSize, out totalRecord);
        }
        public AspnetUser CreateOrUpdate(AspnetUser dto)
        {
            // Validate input data
            BusinessValidator.ThrowIfNull(dto, BackEndResourceKeys.INVALID_DATA);
            BusinessValidator.ThrowIfNullOrEmpty(dto.UserName, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.UserName));
            BusinessValidator.ThrowIfNullOrEmpty(dto.Email, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.Email));
            //BusinessValidator.ThrowIfNullOrEmpty(dto.Password, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.Password));
            //BusinessValidator.ThrowIfNullOrEmpty(dto.LoweredUserName, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.LoweredUserName));
            BusinessValidator.ThrowIfNullOrEmpty(dto.DisplayName, BackEndResourceKeys.PLEASE_ENTER_THE_VALUE, nameof(dto.DisplayName));

            // Validate unique constraints
            if (dto.UserId == Guid.Empty)
            {
                BusinessValidator.ThrowIf(_repository.GetByUserName(dto.UserName) != null, 
                    BackEndResourceKeys.USERNAME_ALREADY_EXISTS, nameof(dto.UserName), ErrorCodes.Conflict);
            }

            AspnetUser user;
            if (dto.UserId != Guid.Empty)
            {
                user = _repository.GetById(dto.UserId);
                BusinessValidator.ThrowIfNull(user, BackEndResourceKeys.NOT_FOUND, nameof(dto.UserId), ErrorCodes.NotFound);
                //-------------------------------------------
                BusinessValidator.ThrowIf(IsEmailExist(user.UserId, user.Email), BackEndResourceKeys.EMAIL_ALREADY_EXISTS, nameof(user.Email), ErrorCodes.Conflict);
                //-------------------------------------------
                BusinessValidator.ThrowIf(_repository.IsUserNameExist(user.UserId, user.UserName), 
                    BackEndResourceKeys.USERNAME_ALREADY_EXISTS, nameof(user.UserName), ErrorCodes.Conflict);
                //-------------------------------------------
                MembershipUser membershipUser = Membership.GetUser(user.UserName);
                BusinessValidator.ThrowIfNull(membershipUser, BackEndResourceKeys.NOT_FOUND, nameof(user.UserName), ErrorCodes.NotFound);

                if (user.IsActivated && membershipUser.IsLockedOut)
                    membershipUser.UnlockUser();

                if (membershipUser.Email.CompareTo(dto.Email) != 0)
                    membershipUser.Email = dto.Email;

                if (!user.IsActivated)
                    membershipUser.IsApproved = false;
                else if (!membershipUser.IsApproved)
                    membershipUser.IsApproved = true;
                if (!string.IsNullOrEmpty(dto.Password))
                {
                    string oldPass = membershipUser.ResetPassword();
                    BusinessValidator.ThrowIf(!membershipUser.ChangePassword(oldPass, dto.Password), 
                        BackEndResourceKeys.UNABLE_TO_UPDATE_PASSWORD_FOR_ACCOUNT, nameof(dto.Password));
                }
                Membership.UpdateUser(membershipUser);
                //-------------------------------------------
                if(dto.RoleId != user.RoleId)
                {
                    AspnetRole role = RoleManager.Instance.GetRoleById(dto.RoleId);
                    BusinessValidator.ThrowIfNull(role, BackEndResourceKeys.SERVICE_UNAVAILABLE, nameof(dto.RoleId), ErrorCodes.ServiceUnavailable);

                    RoleManager.Instance.RemoveAllRoleOfUser(user.UserId);
                    System.Web.Security.Roles.AddUserToRole(user.UserName, role.LoweredRoleName);
                }
                //-------------------------------------------
                ObjectHelper.CopyBusinessProperties(dto, user, 
                    x => x.ApplicationId,
                    x => x.UserId,
                    x => x.Email,
                    x => x.RoleId,
                    x => x.Password,
                    x => x.Email);
                //-------------------------------------------
                user = _repository.Update(user);
                BusinessValidator.ThrowIfNull(user, BackEndResourceKeys.SERVICE_UNAVAILABLE, nameof(dto), ErrorCodes.ServiceUnavailable);
                //-------------------------------------------
                user.Email = membershipUser.Email;
                user.Password = dto.Password;
                user.RoleId = dto.RoleId;   
                return user;
            }
            else
            {
                user = dto.Clone() as AspnetUser;
                BusinessValidator.ThrowIfNull(user, BackEndResourceKeys.INVALID_DATA);
                //-------------------------------------------
                if(string.IsNullOrEmpty(user.Password))
                    user.Password = SecurityUtilities.CreateAlphaNumericString(8);
                #region Membership
                MembershipUser membershipUser = Membership.CreateUser(dto.UserName, dto.Password, dto.Email);
                //MembershipCreateStatus membershipCreateStatus = MembershipCreateStatus.Success;
                //MembershipUser membershipUser = Membership.CreateUser(dto.UserName, dto.Password, dto.Email, "Name of the system?",
                //    SecurityUtilities.ApplicationName, dto.IsActivated, out membershipCreateStatus);
                //if(membershipCreateStatus != MembershipCreateStatus.Success)
                //{
                //    switch (membershipCreateStatus)
                //    {
                //        case MembershipCreateStatus.UserRejected:
                //            BusinessExceptionHelper.ThrowAndNotify("USER_REJECTED", nameof(user.UserName));
                //            break;
                //        case MembershipCreateStatus.InvalidUserName:
                //            BusinessExceptionHelper.ThrowAndNotify("INVALID_USERNAME", nameof(user.UserName));
                //            break;
                //        case MembershipCreateStatus.DuplicateUserName:
                //            BusinessExceptionHelper.ThrowAndNotify("DUPLICATE_USERNAME", nameof(user.UserName));
                //            break;
                //        case MembershipCreateStatus.InvalidPassword:
                //            BusinessExceptionHelper.ThrowAndNotify("INVALID_PASSWORD", nameof(user.Password));
                //            break;
                //        case MembershipCreateStatus.DuplicateEmail:
                //            BusinessExceptionHelper.ThrowAndNotify("DUPLICATE_EMAIL", nameof(user.Email));
                //            break;
                //        default:
                //            BusinessExceptionHelper.ThrowAndNotify("INVALID_USER", nameof(user.UserName));
                //            break;
                //    }
                    
                //}
               
                #endregion
                //-------------------------------------------
                if (dto.RoleId != Guid.Empty && !RoleManager.Instance.IsUserInRole(user.UserId, user.RoleId))
                {
                    AspnetRole role = RoleManager.Instance.GetRoleById(dto.RoleId);
                    BusinessValidator.ThrowIfNull(role, BackEndResourceKeys.SERVICE_UNAVAILABLE, nameof(dto.RoleId), ErrorCodes.ServiceUnavailable);

                    System.Web.Security.Roles.AddUserToRole(user.UserName, role.LoweredRoleName);
                }
                //-------------------------------------------
                user = _repository.GetByUserName(dto.UserName);
                ObjectHelper.CopyBusinessProperties(dto, user,
                     x => x.ApplicationId,
                    x => x.UserId,
                    x => x.Email,
                    x => x.RoleId,
                    x => x.Password);
                //-------------------------------------------
                user.Email = membershipUser.Email;
                user.Password = dto.Password;
                user.RoleId = dto.RoleId;
                BusinessValidator.ThrowIfNull(user, BackEndResourceKeys.SERVICE_UNAVAILABLE, nameof(dto), ErrorCodes.ServiceUnavailable);
                return _repository.Update(user);
            }
        }
        public bool Delete(AspnetUser item)
        {
            BusinessValidator.ThrowIfNull(item, BackEndResourceKeys.INVALID_DATA);
            //--------------------------------------------
            return _repository.Delete(item);
        }
        public AspnetUser GetUserById(Guid id)
        {
            return _repository.GetById(id);
        }
        public AspnetUser GetUserByUserName(string userName)
        {
            return _repository.GetByUserName(userName);
        }
        public AspnetUser GetUserByEmail(string email)
        {
            return _repository.GetByEmail(email);
        }
        public string GetDisplayNameByUserName(string username)
        {
            return _repository.GetDisplayNameByUserName(username);
        }
        public bool ValidateUser(string userName, string password)
        {
            return _repository.ValidateUser(userName, password);
        }
        public bool IsEmailExist(Guid ID, string email)
        {
            return _repository.IsEmailExist(ID, email);
        }
        public List<AspnetUser> GetAllAspnetUsers()
        {
            return _repository.GetAllAspnetUsers();
        }
        #region Helpers
        public AutocompleteObj AllUserAutocomplete(string keyword, int maxResult, string lang)
        {
            int total;
            DataTable dt = SearchUsers(keyword, Guid.Empty, "DisplayName ASC", 1, maxResult, out total);

            List<AutocompleteItem> listAutocompleteItem = new List<AutocompleteItem>();
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                total = dt.Rows.Count;
                foreach (DataRow row in dt.Rows)
                {
                    AutocompleteItem autocompleteItem = new AutocompleteItem();
                    string title = row["UserName"].ToString();
                    if (string.IsNullOrEmpty(title))
                        title = row["Email"].ToString();

                    autocompleteItem.Label = string.Format("<span class=\"tag activated\">{0}</span>" +
                        "<span class=\"sub-info\">" +
                        "<i>Full Name: {1}</i>" +
                        "</span>"
                            , title
                            , row["DisplayName"]);

                    autocompleteItem.Value = title;
                    autocompleteItem.Data = row["Id"].ToString();
                    autocompleteItem.OtherData = JsonConvert.SerializeObject(new
                    {
                        DisplayName = row["DisplayName"].ToString(),
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
        public bool IsAdministrator(Guid ID)
        {
            try
            {
                string value = AppCache.Get(string.Format("IS_ADMINISTRATION_{0}", SweetContext.Current.UserId)) as string;
                if (string.IsNullOrEmpty(value))
                {
                    AspnetUser user = _repository.GetById(ID);
                    bool isAdmin = user != null && user.UserName.ToLower().Equals("administrator");
                    AppCache.Insert(string.Format("IS_ADMINISTRATION_{0}", SweetContext.Current.UserId), isAdmin);
                    return isAdmin;
                }
                return bool.Parse(value);
            }
            catch
            {
                return false;
            }

        }
        #endregion
    }
}
