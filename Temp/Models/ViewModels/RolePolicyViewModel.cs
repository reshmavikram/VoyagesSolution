using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Solution.Models.ViewModels
{
    public class UserRoleRequest
    {
        public List<UserRoleMapping> userRoleList { get; set; }
    }
    public class RolePolicyRequest
    {
        public List<RolePolicyMapping> rolePolicyList { get; set; }
    }
}
