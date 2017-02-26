using System;
using System.Collections.Generic;

namespace Beacon.Server.Models
{
    public partial class Users
    {
        public Users()
        {
            Event = new HashSet<Event>();
        }

        public int Uid { get; set; }
        public string Fname { get; set; }
        public string Lname { get; set; }
        public DateTime? Birthdate { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }

        public virtual ICollection<Event> Event { get; set; }
    }
}
