using System;
using System.Collections.Generic;

namespace Beacon.Server.Models
{
    public partial class Vote
    {
        public int EventId { get; set; }
        public int UserId { get; set; }
        public int NumVotes { get; set; }

        public virtual Event Event { get; set; }
        public virtual User User { get; set; }
    }
}
