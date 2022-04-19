using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Solution.Models.ViewModels
{
    public class ImoDcsDataViewModel
    {
        public string Description { get; set; }
        public string Period { get; set; }
        public int ImoNumber { get; set; }
        public double AER { get; set; } 
        public double TotalCo2Emission { get; set; }    
        public int NoOfPassages { get; set; }   
        public long Miles { get; set; }  
        public long Cargo { get; set; }
        public long WorkDone { get; set; }
        public int SailingDuration{get; set; }
        public int PortDuration { get; set; }


        public string LoadCondition { get; set; }
      
        public string VesselCode { get; set; }
        public long VoyageNumber { get; set; }
        public long SFPM_VoyagesId { get; set; }
        public DateTime? ActualEndOfSeaPassage { get; set; }
        public DateTime ActualStartOfSeaPassage { get; set; }
        public string ArrivalTimezone { get; set; }
        public string ArrivalPort { get; set; }
        public string DepartureTimezone { get; set; }
        public string DeparturePort { get; set; }

    }
}
