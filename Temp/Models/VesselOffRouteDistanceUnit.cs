using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class VesselOffRouteDistanceUnit
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_VesselOffRouteDistanceUnitId { get; set; }
        public string VesselOffRouteDistanceUnitName { get; set; }
        public string VesselOffRouteDistanceUnitCode { get; set; }
    }
}
