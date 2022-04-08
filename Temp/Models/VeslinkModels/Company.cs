 using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Solution.Models
{
    public class Company
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long CompanyId { get; set; }
        public string CompanyCode { get; set; }
        public string Name { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }
        public bool IsAgent { get; set; }
        public bool IsThirdParty { get; set; }
        public ICollection<CompanyCategoryMapping> Categories { get; set; }
        public long CreatedBy { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedDateTime { get; set; }

    }
}
