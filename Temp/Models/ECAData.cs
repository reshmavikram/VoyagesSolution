using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class ECAData : CommonField
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_ECADataId { get; set; }
        public string Region { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }
}

