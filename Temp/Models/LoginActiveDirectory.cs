using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Solution.Models
{
   public  class LoginActiveDirectory
    {
        public string family_name { get; set; }
        public string given_name { get; set; }
        public string name { get; set; }
        public string unique_name { get; set; }
        public string userName { get; set; }
    }
}
