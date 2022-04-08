using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
    public class About : CommonField
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_AboutId { get; set; }
        public string CopyrightDetails { get; set; }
        public string ApplicationVersion { get; set; }
        public string BrowsersSupported { get; set; }
        public string BrowserVersionSupported { get; set; }
        public string BrowserPlatformSupported { get; set; }
        public string UserAgents { get; set; }
        public string ApplicationUpdateDetails { get; set; }
        public string Notes { get; set; }
    }
}
