//-----------------------PROGRAMER LOGS---------------------------
//'**Created by:
//'**Change 01: Truong, 31 Oct 2024 - Convert to MYSQL

using SubSonic;
using SweetSoft.QLDA.Core.Caches;
using SweetSoft.QLDA.Core.Helpers;
using SweetSoft.QLDA.Core.Helpers.Security;
using SweetSoft.QLDA.Core.Interfaces;
using SweetSoft.QLDA.Core.SysManager;
using SweetSoft.QLDA.Core.Utils;
using SweetSoft.QLDA.Core.ValueObjects;
using SweetSoft.QLDA.Core.WebRequest;
using SweetSoft.QLDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Documents;

namespace SweetSoft.QLDA.Core.Managers
{
    public class SettingKeys
    {
        private static string SettingKeyPrefix = "Settings.{0}";
        #region SEO Helper
        public static string SaveLog = string.Format(SettingKeyPrefix, "SaveLog");
        public static string UseSSLWebsite = string.Format(SettingKeyPrefix, "UseSSLWebsite");
        public static string PreventSelection = string.Format(SettingKeyPrefix, "PreventSelection");
        public static string PreventRightClick = string.Format(SettingKeyPrefix, "PreventRightClick");
        #endregion

        #region Company Information and Website
        public static string TitleOfWebsite = string.Format(SettingKeyPrefix, "TitleOfWebsite");
        public static string InternalAnnouncement = string.Format(SettingKeyPrefix, "InternalAnnouncement");
        public static string EmailSignature = string.Format(SettingKeyPrefix, "EmailSignature");
        public static string CompanyName = string.Format(SettingKeyPrefix, "CompanyName");
        public static string CompanyEmail = string.Format(SettingKeyPrefix, "CompanyEmail");
        public static string CompanyAddress = string.Format(SettingKeyPrefix, "CompanyAddress");
        public static string CompanyPhone = string.Format(SettingKeyPrefix, "CompanyPhone");
        public static string CompanyFax = string.Format(SettingKeyPrefix, "CompanyFax");
        public static string CompanyHotline = string.Format(SettingKeyPrefix, "CompanyHotline");
        public static string DataGridItemsPerPage = string.Format(SettingKeyPrefix, "DataGridItemsPerPage");

        public static string NumberAuditTrails = string.Format(SettingKeyPrefix, "NumberAuditTrails");
        public static string MyTimeZone = string.Format(SettingKeyPrefix, "MyTimeZone");

        public static string DefaultDescriptionForOrder = string.Format(SettingKeyPrefix, "DefaultDescriptionForOrder");
        public static string DefaultNoteForProduct = string.Format(SettingKeyPrefix, "DefaultNoteForProduct");
        public static string DefaultProcessor = string.Format(SettingKeyPrefix, "DefaultProcessor");
        public static string TaxCode = string.Format(SettingKeyPrefix, "TaxCode");
        public static string LinkAddress = string.Format(SettingKeyPrefix, "LinkAddress");
        public static string EmbedCodeGoogleMap = string.Format(SettingKeyPrefix, "EmbedCodeGoogleMap");
        public static string ContentFooter = string.Format(SettingKeyPrefix, "ContentFooter");

        public static string ZaloUrl = string.Format(SettingKeyPrefix, "ZaloUrl");
        public static string MessengerUrl = string.Format(SettingKeyPrefix, "MessengerUrl");
        #endregion

        #region SMTP
        //SMTP Settings
        public static string SmtpMailServerAddress = string.Format(SettingKeyPrefix, "SmtpMailServerAddress");
        public static string SmtpPort = string.Format(SettingKeyPrefix, "SmtpPort");
        public static string SmtpUsingSSL = string.Format(SettingKeyPrefix, "SmtpUsingSSL");
        public static string SmtpSenderEmail = string.Format(SettingKeyPrefix, "SmtpSenderEmail");
        public static string SmtpSenderAccount = string.Format(SettingKeyPrefix, "SmtpSenderAccount");
        public static string SmtpSenderPassword = string.Format(SettingKeyPrefix, "SmtpSenderPassword");
        public static string AdministratorEmail = string.Format(SettingKeyPrefix, "AdministratorEmail");
        public static string ErrorReceiverEmail = string.Format(SettingKeyPrefix, "ErrorReceiverEmail");
        public static string SmtpTimeoutMilliseconds = string.Format(SettingKeyPrefix, "SmtpTimeoutMilliseconds");
        public static string SmtpMaxRetryAttempts = string.Format(SettingKeyPrefix, "SmtpMaxRetryAttempts");
        public static string SmtpMaxRetryDelaySeconds = string.Format(SettingKeyPrefix, "SmtpMaxRetryDelaySeconds");
        #endregion

    }
    public class SettingManager : ISettingManager
    {
        private const string APP_SETTINGS_CACHEKEY = "APP_SETTINGS_CACHEKEY";
        public static SettingManager Instance => new SettingManager();
        public SettingManager()
        {
        }
        public int GetSettingValueInt(string name, int defaultValue)
        {
            int intValue = 0;
            int.TryParse(GetSettingValue(name), out intValue);
            return intValue == 0 ? defaultValue : intValue;
        }

        public decimal GetSettingValueDecimal(string name, decimal defaultValue)
        {
            decimal decimalValue = 0;
            decimal.TryParse(GetSettingValue(name), out decimalValue);
            return decimalValue == 0 ? defaultValue : decimalValue;
        }

        public bool GetSettingValueBoolean(string name)
        {
            return GetSettingValueBoolean(name, true);
        }

        public bool GetSettingValueBoolean(string name, bool defaultValue)
        {
            string value = GetSettingValue(name);
            bool ret = defaultValue;
            if (!string.IsNullOrEmpty(value))
                bool.TryParse(value, out ret);
            return ret;
        }

        public Guid GetSettingValueGuid(string name)
        {
            return GetSettingValueGuid(name, new Guid());
        }

        public Guid GetSettingValueGuid(string name, Guid defaultValue)
        {
            string value = GetSettingValue(name);
            Guid ret = defaultValue;
            if (!string.IsNullOrEmpty(value))
                Guid.TryParse(value, out ret);
            return ret;
        }

        public DateTime GetSettingValueDateTime(string name)
        {
            return GetSettingValueDateTime(name, DateTime.MinValue);
        }

        public DateTime GetSettingValueDateTime(string name, DateTime defaultValue)
        {
            string value = GetSettingValue(name);
            DateTime ret = defaultValue;
            if (!string.IsNullOrEmpty(value))
                DateTime.TryParse(value, out ret);
            return ret;
        }
        public List<TblSetting> ApplicationSettings
        {
            get
            {
                List<TblSetting> m_CurrentSettings = AppCache.Get(APP_SETTINGS_CACHEKEY) as List<TblSetting>;
                if (m_CurrentSettings == null || m_CurrentSettings.Count < 1)
                {
                    m_CurrentSettings = GetAllSettings();
                    AppCache.Remove(APP_SETTINGS_CACHEKEY);
                    AppCache.Max(APP_SETTINGS_CACHEKEY, m_CurrentSettings);
                }
                return m_CurrentSettings;
            }
        }

        private List<TblSetting> GetAllSettings()
        {
            Select select = new Select();
            select.From(TblSetting.Schema);
            return select.ExecuteTypedList<TblSetting>();
        }
        public string GetSettingValueDecryptAES(string settingName)
        {
            try
            {
                TblSetting objSetting = GetSettingByName(settingName);
                if (objSetting != null)
                    return SecurityUtilities.DecryptContent(objSetting.SettingValue);
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
        public string GetSettingValue(string settingName)
        {
            TblSetting objSetting = GetSettingByName(settingName);
            return (objSetting != null) ? objSetting.SettingValue : string.Empty;
        }
        public TblSetting GetSettingByName(string settingName)
        {
            List<TblSetting> allSetting = ApplicationSettings;
            if (allSetting != null && allSetting.Count > 0)
                return allSetting.Where(t => t.SettingName == settingName).FirstOrDefault();
            return null;
        }
        public TblSetting GetTblSetting(string settingName)
        {
            Select select = new Select();
            select.From(TblSetting.Schema);
            select.Where(TblSetting.SettingNameColumn).IsEqualTo(settingName);
            return select.ExecuteSingle<TblSetting>();
        }
        public TblSetting UpdateSetting(TblSetting itemNew, string settingValue)
        {
            if (itemNew == null)
                return null;
            TblSetting itemOld = itemNew.Clone();
            itemNew.SettingValue = settingValue;
            itemNew.Save();
            return itemNew;
        }
        public TblSetting InsertSetting(string settingName, string settingValue)
        {
            TblSetting tblSetting = new TblSetting();
            tblSetting.Id = UUIDv7.NewGuid();
            tblSetting.SettingName = settingName;
            tblSetting.SettingValue = settingValue;
            tblSetting.Save();
            return tblSetting;
        }
        public void ResetSettingsInCache()
        {
            AppCache.Remove(APP_SETTINGS_CACHEKEY);
        }
        public void SaveSetting(string settingName, string value)
        {
            TblSetting setting = GetSettingByName(settingName);
            if (setting != null)
            {
                UpdateSetting(setting, value);
            }
            else
                InsertSetting(settingName, value);
        }
        public void ClearCacheForAPI()
        {
            _ = WebRequestHelpers.GetRequestJson($"{CommonHelpers.GetAPIHostPath()}api/v1/clearcache", null);
        }
        public DataSet GetReportsDashboard(bool isForceUpdate, out DateTime lastUpdateDate)
        {
            lastUpdateDate = DateTime.MinValue;
            try
            {
                DataSet dataSet = null;
                var parameters = new object[] { DateTime.UtcNow.ToString("yyyy-MM-dd") };
                if (isForceUpdate)
                    CacheManager.ClearCache(CacheManager.CacheKeys.DASHBOARD_REPORT, parameters);
                if (!CacheManager.GetCacheData(CacheManager.CacheKeys.DASHBOARD_REPORT,
                   parameters,
                    out dataSet,
                    out lastUpdateDate)
                    || dataSet == null
                    || dataSet.Tables.Count == 0)
                {
                    StoredProcedure sp = SPs.SpDashboardStatistics();
                    if (sp == null)
                        return null;
                    dataSet = sp.GetDataSet();
                    if (dataSet == null || dataSet.Tables.Count <= 0)
                        return null;
                    CacheManager.SetCacheData(CacheManager.CacheKeys.DASHBOARD_REPORT, parameters, dataSet);
                    return dataSet;
                }
                return dataSet;
            }
            catch (Exception exc)
            {
                return null;
            }
        }
        public DataTable GetTopAuditLogs()
        {
            string sql = $"select top 10 * from TblAuditLog_{DateTime.UtcNow.Year} order by ChangedAt DESC;";
            IDataReader dataReader = new InlineQuery(SubsonicHelpers.SysProvider).ExecuteReader(sql);
            if (dataReader == null)
                return null;
            DataTable dt = new DataTable();
            dt.Load(dataReader);
            return dt;
        }

    }
}
