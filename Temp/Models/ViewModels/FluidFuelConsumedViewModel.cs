using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Solution.Models.ViewModels
{
    public class FluidFuelConsumedViewModel
    {
        public long FormId { get; set; }
        public long RobId { get; set; }
        public long? AllocationId { get; set; }
        public long? EventRobsRowId { get; set; }
        public string FluidType { get; set; }
        public string Unit { get; set; }
        public string Category { get; set; }
        public decimal Consumption { get; set; }
    }
}
