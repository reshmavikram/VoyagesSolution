using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Data.Solution.Models
{
    public class CommonField
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public Status Status { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
    }
}
