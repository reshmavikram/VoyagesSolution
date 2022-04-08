using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Solution.Models
{
    public class EmailSettings
    {

        [JsonProperty("sendGridAPIKey")]
        public string SendGridAPIKey { get; set; }

        [JsonProperty("fromEmail")]
        public string FromEmail { get; set; }

        [JsonProperty("fromName")]
        public string FromName { get; set; }

        //[JsonProperty("fromEmail")]
        //public string FromEmail { get; set; }

        //[JsonProperty("isBodyHtml")]
        //public bool IsBodyHtml { get; set; }

        //[JsonProperty("fromEmailPassword")]
        //public string FromEmailPassword { get; set; }

        //[JsonProperty("host")]
        //public string Host { get; set; }

        //[JsonProperty("port")]
        //public int Port { get; set; }

        //[JsonProperty("enableSsl")]
        //public bool EnableSsl { get; set; }

        //[JsonProperty("useDefaultCredentials")]
        //public bool UseDefaultCredentials { get; set; }
    }
}
