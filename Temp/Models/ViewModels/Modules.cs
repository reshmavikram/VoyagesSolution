using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Solution.Models.ViewModels
{
    public class Modules
    {
        public long ModuleId { get; set; }
        public string ModuleName { get; set; }       
        public string Policy { get; set; }
    }

    public class UserModulesViewModel
    {
        public UserModel user { get; set; }
        public List<Modules> modules { get; set; }
    }
}
