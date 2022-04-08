using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Data.Solution.Models
{
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long CategoryId { get; set; }
        public string Name { get; set; }
        public ICollection<CompanyCategoryMapping> CompanyCategoryMapping { get; set; }
        public long CreatedBy { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
    }
}
