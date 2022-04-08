using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Solution.Models
{
    public class UnitofMeasure  : CommonField
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public  long UMId { get; set; }
        public string Code { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
    }
}
