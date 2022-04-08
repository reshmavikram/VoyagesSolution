using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Solution.Models.ViewModels
{
   public class ECADataViewModel
    {
        public string Region { get; set; }
        public List<ECALatlongViewModel> LatLogList { get; set; }
    }
    public class ECALatlongViewModel
    {
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }
}
