using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Solution.Models
{
    public class AlertSettingViewModel
    {

        public long SFPM_AlertSettingId { get; set; }
        public string AlertName { get; set; }
        public bool IsSubscribe { get; set; }
        public long? MappingTypeId { get; set; }
        public int Rule { get; set; }
        public string Unit { get; set; }
        public string Description { get; set; }
        public string Message { get; set; }
        public List<AlertSettingsVesselMapping> AlertSettingsVesselMappings { get; set; }
        public AlertSettingEmailMapping AlertSettingEmailMappings { get; set; }
    }
}
