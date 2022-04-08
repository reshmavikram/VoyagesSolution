using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class AlertSettingEmailMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_AlertSettingEmailMappingId { get; set; }
        public long UserId { get; set; }
        public string Emails { get; set; }
        [ForeignKey("AlertSettings")]
        public long AlertSettingId { get; set; }
        public virtual AlertSettings AlertSettings { get; set; }
    }
}
