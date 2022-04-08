using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class BunkerType
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SFPM_BunkerTypeId { get; set; }
        public string BunkerTypeName { get; set; }
        public long CreatedBy { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
    }
}
