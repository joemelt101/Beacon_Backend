using System;
using System.Collections.Generic;

namespace Beacon.Server.Models
{
    public partial class Token
    {
        public string Value { get; set; }
        public int CorrespondingLoginId { get; set; }

        public virtual Event CorrespondingLogin { get; set; }
    }
}
