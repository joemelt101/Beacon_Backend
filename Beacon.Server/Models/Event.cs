using System;
using System.Collections.Generic;

namespace Beacon.Server.Models
{
    public partial class Event
    {
        public int Eid { get; set; }
        public string EName { get; set; }
        public string EDescription { get; set; }
        public byte[] TimeLastUpdated { get; set; }
        public int? CreatorId { get; set; }

        public virtual Users Creator { get; set; }
    }
}
