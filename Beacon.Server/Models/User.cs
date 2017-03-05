using System;
using System.Collections.Generic;

namespace Beacon.Server.Models
{
    public partial class User
    {
        public User()
        {
            Event = new HashSet<Event>();
            Token = new HashSet<Token>();
            Vote = new HashSet<Vote>();
        }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string HashedPassword { get; set; }
        public string Salt { get; set; }
        public string UserName { get; set; }
        public int? CurrentAttendedEventId { get; set; }

        public virtual ICollection<Event> Event { get; set; }
        public virtual ICollection<Token> Token { get; set; }
        public virtual ICollection<Vote> Vote { get; set; }
        public virtual Event CurrentAttendedEvent { get; set; }
    }
}
