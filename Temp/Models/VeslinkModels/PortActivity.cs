using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Solution.Models
{
    public class PortActivity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_PortActivity_Id { get; set; }
        public long id { get; set; }
        public string CompanyCode { get; set; }
        public string name { get; set; }
        public bool NeedsCargo { get; set; }
        public bool NeedsBerth { get; set; }
        [ForeignKey("PortActivityType")]
        public long PortActivityTypeId { get; set; }
        public virtual PortActivityType activityType { get; set; }
        [NotMapped]
        public virtual String activityTypeCode { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime ModifiedDateTime { get; set; }

    }
}
