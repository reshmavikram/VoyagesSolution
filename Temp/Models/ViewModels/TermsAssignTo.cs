using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models.ViewModels
{
    public class TermsAssignTo
    {
        public long vesselId { get; set; }
        public string vesselName { get; set; }
        public long vesselGroupId { get; set; }
        public string vesselGroupName { get; set; }
        public long vesselClassId { get; set; }
        public string vesselClassName { get; set; }
       

    }
}
