using System;

//---------------------- PROGRAMMER LOG ----------------------//
//Created by: Truong, 09 Apr 2025

namespace SweetSoft.QLDA.Core.ExcelManager
{
    public class ReportJobModel
    {
        public Guid Id { get; set; }
        public string SqlQuery { get; set; }
        public string OptionsJson { get; set; }  // Serialize ExcelExportOptions
        public string EmailTo { get; set; }
        public string ReportFileName { get; set; }

        public string Status { get; set; }
        public int Progress { get; set; }
        public int Total { get; set; }
        public string Error { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
    }

}
