using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class FleetViewKPISetting : CommonField
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_FleetViewKPISettingsId { get; set; }
        public string KeyName { get; set; }
        public string Keyvalue { get; set; }
    }
}
