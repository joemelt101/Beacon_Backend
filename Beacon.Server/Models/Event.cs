using System;
using System.Collections.Generic;

namespace Beacon.Server.Models
{
    public partial class Event
    {
        public Event()
        {
            User = new HashSet<User>();
            Vote = new HashSet<Vote>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime TimeLastUpdated { get; set; }
        public int? CreatorId { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int VoteCount { get; set; }
        public DateTime TimeCreated { get; set; }
        public bool Deleted { get; set; }

        public virtual ICollection<User> User { get; set; }
        public virtual ICollection<Vote> Vote { get; set; }
        public virtual User Creator { get; set; }
    }
}
