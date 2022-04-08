using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Solution.Models.ViewModels
{
    public class ExcludeReportLogs
    {
        public long ExcludeReportLogId { get; set; }
        public bool Excluded { get; set; }
        public string ReportName { get; set; }
        public string Username { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string Remarks { get; set; }
    }
}
