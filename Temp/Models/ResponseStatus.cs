using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Solution.Models
{
    public enum ResponseStatus
    {
        ALREADYEXIST,
        SAVED,
        NOTFOUND,
        CURRENTLYINUSE,
        MASTERISINACTIVE,
        INACTIVE,
        INVALIDUSER,
        INITIALAPPROVALREQUIRED,
        APPROVALREQUIRED,
        ALREADYAPPROVED
    }
}
