using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
   public class LoginLogoutLog : CommonField
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_LoginLogoutLogId { get; set; }
        [ForeignKey("Logins")]
        public long UserId { get; set; }
        public DateTime LoginLogoutDateTime { get; set; }
        public string IPAddress { get; set; }
        public string Country { get; set; }
        public string TimeZoneOffset { get; set; }
        public bool IsLogin { get; set; }

    }
}
