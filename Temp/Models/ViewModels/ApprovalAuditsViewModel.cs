using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Solution.Models.ViewModels
{
    public class ApprovalAuditsViewModel
    {
        public string Action { get; set; }
        public string User { get; set; }
        public string Role { get; set; }
        public string Approval { get; set; }
        public string  DateTime { get; set; } 
    }
}
