﻿using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models
{
   public class CustomPort : CommonField
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_CustomPortId { get; set; }
        public string PortCode { get; set; }
        public string PortName { get; set; }
        public string AlternatePortName { get; set; }
        public string CustomPortTimeZone { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }
}
