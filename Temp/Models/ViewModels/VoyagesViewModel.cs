using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models.ViewModels
{
    public class VoyagesViewModel
    {
        public bool IsCharterPartyExclude { get; set; }
        public DateTime ModifiedDateTime { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public long CreatedBy { get; set; }
        public DateTime? ActualEndOfSeaPassage { get; set; }
        public DateTime ActualStartOfSeaPassage { get; set; }
        public string ArrivalTimezone { get; set; }
        public string ArrivalPort { get; set; }
        public string DepartureTimezone { get; set; }
        public string DeparturePort { get; set; }
        public string LoadCondition { get; set; }
        public string Description { get; set; }
        public string VesselCode { get; set; }
        public long VoyageNumber { get; set; }
        public long SFPM_VoyagesId { get; set; }
        public bool IsPoolExclude { get; set; }
        public string ReportExclude_Remarks { get; set; }
        public string Approval { get; set; }
        public string Action { get; set; }
        public string IMONumber { get; set; }
        [NotMapped]
        public bool IsConflict { get; set; }
        public bool isPassageWarningExists { get; set; }
    }
}
