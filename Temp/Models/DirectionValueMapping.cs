using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class DirectionValueMapping
    {
        [Key]
        public string MapDirection { get; set; }
        public string MapValue { get; set; }
        public string Description { get; set; }
    }
}
