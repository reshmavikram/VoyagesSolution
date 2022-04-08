using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Solution.Models
{
    public class FuelCategory   : CommonField
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long FuelCategoryId { get; set; }
        public string Name { get; set; }
        public decimal Co2mt { get; set; }
        public decimal So2Percentage { get; set; }
        public bool IsFuelOil { get; set; }
    }
}
