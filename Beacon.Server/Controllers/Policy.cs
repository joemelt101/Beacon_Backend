using Beacon.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beacon.Server.Controllers.EventsController;

namespace Beacon.Server.Controllers
{
    public static class Policy
    {
        public const double SURROUNDING_GEOBOX_SIDE_LENGTH = 1609 * 30; //30 miles per side

        public static bool ShouldMarkEventForDeletion(Event e)
        {
            if (e.Deleted)
            {
                return false;
            }

            if (e.VoteCount <= -3)
            {
                return true;
            }

            // Current policy is an exponential decline algorithm
            double prelogScore = e.VoteCount;
            double baseScore = (float)Math.Log(Math.Max(prelogScore, 1f));
            double dt = (DateTime.UtcNow - e.TimeCreated).TotalHours;

            if (dt > 0.5)
            {
                double x = dt - 1;
                baseScore *= Math.Exp(-8 * x * x);

                if (baseScore < 0.3)
                {
                    // Should delete the event
                    return true;
                }

            }

            return false;
        }

        public static bool ShouldRemoveEvent(Event e)
        {
            return e.Deleted && DateTime.UtcNow > e.TimeCreated.AddDays(1);
        }
    }
}
