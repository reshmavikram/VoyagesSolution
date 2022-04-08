using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
   public  class AdditionalColumn
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_AdditionalColumnId { get; set; }
        [ForeignKey("Forms")]
        public long FormId { get; set; }
        public virtual Forms Forms { get; set; }

        public string STS_Operation { get; set; }
        public string Storage_Tanker { get; set; }
        public string CountryCode { get; set; }
        public string X11 { get; set; }
        public string X12 { get; set; }
        public string X13 { get; set; }
        public string X14 { get; set; }
        public string X15 { get; set; }
        public string X16 { get; set; }
        public string X17 { get; set; }
        public string X18 { get; set; }
        public string X19 { get; set; }
        public string X20 { get; set; }
        public string X21 { get; set; }
        public string X22 { get; set; }
        public string X23 { get; set; }
        public string X24 { get; set; }
        public string X25 { get; set; }

        public DateTime? CreatedDateTime { get; set; }
        public DateTime? ModifiedDateTime { get; set; }

       
    }
}
