using SweetSoft.QLDA.BackOffice.Common;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Web.UI;

namespace SweetSoft.QLDA.BackOffice.fGanttCharts.Controls
{
    // Lớp xử lý chung: định dạng JSON cũ, mã hóa OTP, đồng bộ thao tác file.
    // Cả hai ASCX kế thừa để không phải duy trì hai bản mã hóa/dữ liệu lịch sử.
    public abstract class ProgressReportControlBase : BaseAdminUserControl
    {
        public Guid ProjectId
        {
            get { return ViewState["ProgressReportProjectId"] is Guid ? (Guid)ViewState["ProgressReportProjectId"] : Guid.Empty; }
            set { ViewState["ProgressReportProjectId"] = value; }
        }

        protected static readonly object StorageLock = new object();
        protected static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue, RecursionLimit = 100 };
        protected static string ProtectReportOtp(string otp)
        {
            byte[] encrypted = MachineKey.Protect(Encoding.UTF8.GetBytes(otp), "SweetSoft.QLDA", "GanttReport", "OtpHistoryV1");
            if (encrypted == null) throw new CryptographicException("Không thể mã hóa OTP.");
            return Convert.ToBase64String(encrypted);
        }
        protected static string UnprotectReportOtp(string protectedValue)
        {
            if (string.IsNullOrWhiteSpace(protectedValue)) return null;
            try
            {
                byte[] plain = MachineKey.Unprotect(Convert.FromBase64String(protectedValue), "SweetSoft.QLDA", "GanttReport", "OtpHistoryV1");
                string otp = plain == null ? null : Encoding.UTF8.GetString(plain);
                return otp != null && System.Text.RegularExpressions.Regex.IsMatch(otp, "^[0-9]{6}$") ? otp : null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("Không giải mã được OTP báo cáo: " + ex);
                return null;
            }
        }
        // History UI controls are resolved dynamically to avoid handwritten designer.cs changes.
        protected static T FindHistoryControl<T>(Control parent, string id) where T : Control
        {
            if (parent == null) return null;
            if (parent.ID == id) return parent as T;
            foreach (Control child in parent.Controls)
            {
                T found = FindHistoryControl<T>(child, id);
                if (found != null) return found;
            }
            return null;
        }

        protected string HistoryFolder
        {
            get { return Server.MapPath("~/App_Data/ProgressReports/History/" + ProjectId.ToString("N") + "/"); }
        }

        public sealed class ProgressReportHistoryItem
        {
            public string ReportId { get; set; }
            public string ProjectId { get; set; }
            public string ProjectName { get; set; }
            public string ReportName { get; set; }
            public string CreatedById { get; set; }
            public string CreatedByName { get; set; }
            public DateTime CreatedUtc { get; set; }
            public int TaskCount { get; set; }
            public string[] SelectedCodes { get; set; }
            public string FileName { get; set; }
        }

        protected static string GenerateOtp()
        {
            byte[] bytes = new byte[4];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create()) rng.GetBytes(bytes);
            uint value = BitConverter.ToUInt32(bytes, 0) % 1000000U;
            return value.ToString("D6", CultureInfo.InvariantCulture);
        }
        protected static string GenerateSalt()
        {
            byte[] bytes = new byte[32];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create()) rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
        protected internal static string HashOtp(string salt, string otp)
        {
            byte[] saltBytes = Convert.FromBase64String(salt ?? string.Empty);
            using (Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(otp ?? string.Empty, saltBytes, 100000))
                return Convert.ToBase64String(deriveBytes.GetBytes(32));
        }
    }
}
