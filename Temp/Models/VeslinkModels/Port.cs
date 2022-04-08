using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    [Serializable]
    public class Port
    {
        [System.ComponentModel.DataAnnotations.Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PortId { get; set; }
        public int PortNo { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string UNCode { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}
