using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Solution.Models
{
    public class CustomVesselField : CommonField
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_CustomVesselFieldId { get; set; }
        public string FieldName { get; set; }
        public virtual VesselType VesselType { get; set; }
        [ForeignKey("VesselType")]
        public long VesselTypeId { get; set; }
    }
}
