using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Data.Solution.Models
{
    [Serializable]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long UserId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public Status Status { get; set; }
        public bool AccountVerified { get; set; }
        public DateTime? VerificationDateTime { get; set; }
        public IList<UserRoleMapping> UserRoles { get; set; }
        public long CreatedBy { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
        public string ProfileImage { get; set; }
        public IList<UserVesselGroupMapping> UserVesselGroupMapping { get; set; }
        public int UserType { get; set; }
        public bool IsSSOEnabled { get; set; }
    }
}
