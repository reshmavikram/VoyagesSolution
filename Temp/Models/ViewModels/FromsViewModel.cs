using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Solution.Models.ViewModels
{
    [NotMapped]
    public class FromsViewModel
    {
        public string OriginalEmailText { get; set; }
        public string EmailAttachmentFileName { get; set; }
        public string OriginalFormsXML { get; set; }
    }
}
