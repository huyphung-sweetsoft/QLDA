using SubSonic;
using SweetSoft.QLDA.Core.Caches;
using SweetSoft.QLDA.Core.Functions;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Language;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Infrastructure;
using SweetSoft.QLDA.Core.Infrastructure.Cookies;
using SweetSoft.QLDA.Core.Infrastructure.Interfaces;
using SweetSoft.QLDA.Core.Infrastructure.SessionContext;
using SweetSoft.QLDA.Core.Infrastructure.Stores;
using SweetSoft.QLDA.Core.Interfaces;
using SweetSoft.QLDA.Core.Managers;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.SessionState;

namespace SweetSoft.QLDA.Core.Infrastructure
{
    public sealed class SweetContext : IAppContext
    {
        private const string CurrentLanguageIdKey = "CURRENT_LANGUAGE_ID";
        private const string CurrentPageSizeKey = "CURRENT_PAGE_SIZE";
        private const string CurrentUserKey = "CURRENT_USER";
        private const string CurrentUserNameKey = "CURRENT_USER_NAME";
        private const string CurrentUserIdKey = "CURRENT_USER_ID";
        private const string CurrentUserAgentKey = "CURRENT_USER_AGENT";
        private const string CurrentUserFunctionsPrefix = "CURRENT_USER_FUNCTIONS_";
        private const string CurrentFunctionsPrefix = "CURRENT_FUNCTIONS_";
        private const string ApplicationIdCacheKey = SecurityUtilities.ApplicationName + ".ApplicationId";

        private static readonly string SessionPrefixValue = ConfigurationManager.AppSettings["SessionAppContext"] ?? string.Empty;

        private static readonly CompositeAppContextStore ContextStore = new CompositeAppContextStore(
            new HttpContextAppContextStore(),
            new AmbientAppContextStore());

        private readonly IRequestEnvironment _environment;
        private readonly ISessionContext _session;
        private readonly ICookieManager _cookies;

        private readonly byte _defaultLanguageId = LanguageHelpers.Defaultlanguage;
        private byte _currentLanguageId;
        private string _currentPageSize;
        private static string _systemName = string.Empty;

        private SweetContext(IRequestEnvironment environment, ISessionContext session, ICookieManager cookies)
        {
            _environment = environment;
            _session = session;
            _cookies = cookies;
        }

        public static SweetContext Current => ContextStore.GetOrCreate(CreateContext);

        public static string SessionPrefix => SessionPrefixValue;

        public IDictionary Items => _environment.Items;

        public bool IsWebRequest => _environment.HasHttpContext;

        public HttpContext Context => _environment.Context;

        public HttpSessionState Session => _session.Session;

        public string SiteUrl => _environment.SiteUrl;

        public Uri CurrentUri
        {
            get => _environment.CurrentUri;
            set => _environment.CurrentUri = value ?? RequestUtilities.DefaultUri;
        }

        public string HostPath => _environment.HostPath;

        public static void ClearAdminData()
        {
            var context = Current;

            AppCache.Clear();
            context._session.ClearAll();
            context.ClearSessionValue(CurrentUserKey);
            context.ClearSessionValue(CurrentUserNameKey);
            context.ClearSessionValue(CurrentUserIdKey);
            context.ClearSessionValue(CurrentUserAgentKey);
            context.ClearSessionValue("CurrentUserIp");
            context.ClearSessionValue("CurrentUserFunctions");
            //----------------------------------------------
            var userId = context.UserId;
            context.ClearSessionValue(CurrentUserFunctionsPrefix + userId);
            context.ClearSessionValue(CurrentFunctionsPrefix + userId);
            //----------------------------------------------
            context.User = null;
            context.UserId = Guid.Empty;
            context.UserName = "Anonymous";
        }

        public void ClearSession(string sessionName)
        {
            ClearSessionValue(sessionName);
        }

        public string MapPath(string path) => _environment.MapPath(path);

        public string PhysicalPath(string path) => _environment.PhysicalPath(path);

        public string CurrentLanguageCode => GetCurrentLanguageCode(CurrentLanguageId);

        public byte CurrentLanguageId
        {
            get => GetCurrentLanguage();
            set
            {
                _currentLanguageId = value;
                WriteLanguageIdToCookie(_currentLanguageId);
            }
        }

        public string CurrentPageSize
        {
            get
            {
                if (!string.IsNullOrEmpty(_currentPageSize))
                {
                    return _currentPageSize;
                }

                var cookieValue = _cookies.Get(CurrentPageSizeKey);
                if (!string.IsNullOrEmpty(cookieValue))
                {
                    _currentPageSize = cookieValue;
                    return _currentPageSize;
                }

                var pageSize = SettingManager.Instance.GetSettingValueInt(SettingKeys.DataGridItemsPerPage, 20);
                _currentPageSize = pageSize.ToString();
                return _currentPageSize;
            }
            set
            {
                _currentPageSize = value;
                if (!string.IsNullOrEmpty(value))
                {
                    _cookies.Set(CurrentPageSizeKey, value, DateTime.UtcNow.AddDays(7));
                }
                else
                {
                    ClearSessionValue(CurrentPageSizeKey);
                }
            }
        }

        public string SystemName
        {
            get
            {
                if (!string.IsNullOrEmpty(_systemName))
                {
                    return _systemName;
                }

                _systemName = AppSettingHelpers.GetSetting<string>("SystemName");
                return _systemName;
            }
        }

        public Guid ApplicationId
        {
            get
            {
                var cached = AppCache.Get(ApplicationIdCacheKey);
                if (cached is Guid cachedGuid && cachedGuid != Guid.Empty)
                {
                    return cachedGuid;
                }

                if (cached is string cachedString && Guid.TryParse(cachedString, out var parsedGuid) && parsedGuid != Guid.Empty)
                {
                    return parsedGuid;
                }

                var appId = new SubSonic.InlineQuery().ExecuteScalar<Guid>(
                    "SELECT ApplicationId FROM aspnet_Applications WHERE ApplicationName=@appName",
                    SecurityUtilities.ApplicationName);

                AppCache.Remove(ApplicationIdCacheKey);
                AppCache.Max(ApplicationIdCacheKey, appId);
                return appId;
            }
        }

        public List<string> CurrentUserFunctions
        {
            get => GetSessionValue(CurrentUserFunctionsPrefix + UserId) as List<string>;
            set => SetSessionValue(CurrentUserFunctionsPrefix + UserId, value);
        }

        public bool CheckFunctionPermission(Guid userId, ModuleKeys module)
        {
            try
            {
                if (IsAdministrator || module == ModuleKeys.Dashboard)
                {
                    return true;
                }

                var currentUserFunctions = CurrentUserFunctions;
                if (currentUserFunctions == null || currentUserFunctions.Count == 0)
                {
                    var isDevelopment = AppSettingHelpers.GetSetting<bool>("IsDevelopment");
                    currentUserFunctions = FunctionManager.Instance.GetAllModules(userId, isDevelopment);
                    CurrentUserFunctions = currentUserFunctions;
                }

                return currentUserFunctions != null && currentUserFunctions.Contains(module.ToString());
            }
            catch
            {
                return false;
            }
        }

        public List<string> CurrentFunctions
        {
            get => GetSessionValue(CurrentFunctionsPrefix + UserId) as List<string>;
            set => SetSessionValue(CurrentFunctionsPrefix + UserId, value);
        }

        public bool IsAdministrator => User?.IsActivated == true &&
                                       string.Equals(User.UserName, "administrator", StringComparison.OrdinalIgnoreCase);

        public Guid UserId
        {
            get
            {
                var rawValue = GetSessionValue(CurrentUserIdKey);
                if (rawValue is Guid guid && guid != Guid.Empty)
                {
                    return guid;
                }

                if (rawValue != null && Guid.TryParse(rawValue.ToString(), out var parsedGuid) && parsedGuid != Guid.Empty)
                {
                    return parsedGuid;
                }

                var user = User;
                if (user?.UserId == null)
                {
                    return Guid.Empty;
                }

                SetSessionValue(CurrentUserIdKey, user.UserId);
                return user.UserId;
            }
            set => SetSessionValue(CurrentUserIdKey, value);
        }

        public string UserName
        {
            get
            {
                var sessionValue = GetSessionValue(CurrentUserNameKey) as string;
                if (!string.IsNullOrWhiteSpace(sessionValue))
                {
                    return sessionValue;
                }

                var identityName = Context?.User?.Identity?.Name;
                if (!string.IsNullOrWhiteSpace(identityName))
                {
                    SetSessionValue(CurrentUserNameKey, identityName);
                    return identityName;
                }

                return string.Empty;
            }
            set => SetSessionValue(CurrentUserNameKey, value);
        }

        public AspnetUser User
        {
            get
            {
                var currentUser = GetSessionValue(CurrentUserKey) as AspnetUser;
                if (currentUser != null)
                {
                    return currentUser;
                }

                var user = UserManager.Instance.GetUserByUserName(UserName);
                if (user != null)
                {
                    SetSessionValue(CurrentUserKey, user);
                }

                return user;
            }
            set => SetSessionValue(CurrentUserKey, value);
        }

        public string CurrentUserIp => _environment.GetUserIpAddress();

        public string CurrentUserAgent
        {
            get
            {
                var cachedAgent = GetSessionValue(CurrentUserAgentKey) as string;
                if (!string.IsNullOrEmpty(cachedAgent))
                {
                    return cachedAgent;
                }

                var userAgent = _environment.GetUserAgent();
                if (_session.HasSession)
                {
                    SetSessionValue(CurrentUserAgentKey, userAgent);
                }

                return userAgent;
            }
            set => SetSessionValue(CurrentUserAgentKey, value);
        }

        public static SweetContext CreateBackgroundContext(Uri baseUri = null, string siteUrl = null)
        {
            return new SweetContext(
                new BackgroundRequestEnvironment(baseUri, siteUrl),
                new InMemorySessionContext(),
                new InMemoryCookieManager());
        }

        public static void Use(SweetContext context)
        {
            if (context == null)
            {
                return;
            }

            ContextStore.Set(context);
        }

        public static void Unload()
        {
            ContextStore.Clear();
        }

        private static SweetContext CreateContext()
        {
            var httpContext = HttpContext.Current;
            if (httpContext != null)
            {
                return new SweetContext(
                    new WebRequestEnvironment(httpContext),
                    new HttpSessionContext(httpContext.Session),
                    new HttpCookieManager(httpContext));
            }

            return CreateBackgroundContext();
        }

        private byte GetCurrentLanguage()
        {
            if (_currentLanguageId != 0)
            {
                return _currentLanguageId;
            }

            var cookieValue = _cookies.Get(CurrentLanguageIdKey);
            if (!string.IsNullOrEmpty(cookieValue) && byte.TryParse(cookieValue, out var languageId))
            {
                _currentLanguageId = languageId;
            }

            return _currentLanguageId == 0 ? _defaultLanguageId : _currentLanguageId;
        }

        private void WriteLanguageIdToCookie(byte languageId)
        {
            _cookies.Set(CurrentLanguageIdKey, languageId.ToString(), DateTime.UtcNow.AddDays(1));
        }

        private static string GetCurrentLanguageCode(byte languageId)
        {
            if (LanguageHelpers.LanguageCode.TryGetValue(languageId, out var languageCode))
            {
                return languageCode;
            }

            return LanguageHelpers.LanguageCode[LanguageHelpers.Vietnamese];
        }

        private string BuildSessionKey(string key)
        {
            return string.Concat(SessionPrefix, key);
        }

        private object GetSessionValue(string key)
        {
            return _session.Get(BuildSessionKey(key));
        }

        private void SetSessionValue(string key, object value)
        {
            var sessionKey = BuildSessionKey(key);
            if (value == null)
            {
                _session.Remove(sessionKey);
                return;
            }

            _session.Set(sessionKey, value);
        }

        private void ClearSessionValue(string key)
        {
            _session.Remove(BuildSessionKey(key));
        }
    }
}
