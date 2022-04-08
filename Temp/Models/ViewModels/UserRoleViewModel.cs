using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Solution.Models.ViewModels
{
    public class UserRoleViewModel
    {
        public User user { get; set; }
        public List<Role> roles { get; set; }
    }
}
