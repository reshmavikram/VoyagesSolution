using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Solution.Models.ViewModels
{
    public class FluidBunkerViewModel
    {
        public long FormId { get; set; }
        public long RobId { get; set; }
        public long? EventROBsRowId { get; set; }
        public string FluidType { get; set; }
        public string Unit { get; set; }
        public string BunkerType { get; set; }
        public decimal Consumption { get; set; }
    }
}
