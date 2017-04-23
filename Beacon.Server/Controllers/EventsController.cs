using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Beacon.Server.Models;
using Beacon.Server.Filters;
using System.Threading;

namespace Beacon.Server.Controllers
{
    [Produces("application/json")]
    [Route("api/Events")]
    [BeaconAuthenticationFilter]
    public class EventsController : Controller
    {
        private readonly BeaconContext _context = new BeaconContext();    
        
        public class GeoBox
        {
            public decimal MinLatitude { get; set; }
            public decimal MaxLatitude { get; set; }

            private decimal StandardizeLongitude(decimal longitude)
            {
                if (longitude < -180)
                {
                    return longitude + 360;
                }

                if (longitude > 180)
                {
                    return longitude - 360;
                }

                return longitude;
            }

            private decimal _minLongitude;
            public decimal MinLongitude
            {
                get
                {
                    return _minLongitude;
                }
                set
                {
                    _minLongitude = StandardizeLongitude(value);
                }
            }

            private decimal _maxLongitude;
            public decimal MaxLongitude
            {
                get
                {
                    return _maxLongitude;
                }
                set
                {
                    _maxLongitude = StandardizeLongitude(value);
                }
            }
        }

        /// <summary>
        /// This should only be used for relatively small covered areas. The middle of the box will center around latitude and longitude.
        /// </summary>
        /// <param name="latitude">The latitude of the box's center</param>
        /// <param name="longitude">The longitude of the box's center</param>
        /// <param name="boxEdgeLength">The length of the box's edge in meters</param>
        /// <returns></returns>
        public GeoBox CalculateBoundingGeoBox(decimal latitude, decimal longitude, double boxEdgeLength)
        {
            // TODO: handle edge cases

            // Validate parameters
            if (latitude > 90 || latitude < -90 || longitude < -180 || longitude > 180)
            {
                throw new ArgumentOutOfRangeException("You must ensure that latitudes and longitudes are within the proper ranges for this method!");
            }

            // Calculate the length of a single degree lat and long...
            double eSquared = .00669438f;
            double a = 6378137f;
            double phi = Math.Abs((double)latitude / 180 * 2 * Math.PI); // Get the phi in terms of radians
            double lengthOfSingleDegreeLongitude = Math.PI * a * Math.Cos(phi) / (180 * Math.Sqrt(1 - eSquared * (Math.Pow(Math.Sin(phi), 2))));
            double lengthOfSingleDegreeLatitude = Math.PI * a * (1 - eSquared) / (180 * Math.Pow((1 - eSquared * Math.Pow(Math.Sin(phi), 2)), 3 / 2));

            // Calculate the diff in longitude and latitude
            double latDelta = (boxEdgeLength / 2) / lengthOfSingleDegreeLatitude;
            double longDelta = (boxEdgeLength / 2) / lengthOfSingleDegreeLongitude;

            // Generate a result
            return new GeoBox()
            {
                MinLatitude = latitude - (decimal)latDelta,
                MinLongitude = longitude - (decimal)longDelta,
                MaxLatitude = latitude + (decimal)latDelta,
                MaxLongitude = longitude + (decimal)longDelta
            };
            
        }

        // GET: api/Events/da?lat=...&lng=...
        [HttpGet("da")]
        public async Task<IActionResult> DownloadAllEventsInArea([FromQuery] decimal lat, [FromQuery] decimal lng)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { WasSuccessful = false, Message = "Model state was invalid for downloading all events..." });
            }

            // Within 30 miles
            GeoBox gb = CalculateBoundingGeoBox(lat, lng, Policy.SURROUNDING_GEOBOX_SIDE_LENGTH);
            
            var @events = await _context.Event.Where(e => (
                e.Latitude < gb.MaxLatitude &&
                e.Latitude > gb.MinLatitude &&
                e.Longitude < gb.MaxLongitude &&
                e.Longitude > gb.MinLongitude 
                )).ToListAsync();
            
            return Json(new { WasSuccessful = true, Events = @events });
        }

        // GET: api/Events/da?lat=49.452&lng=-83.194&lastUpdatedTime=2017-03-04T22:45:39.593
        [HttpGet("du")]
        public async Task<IActionResult> DownloadUpdates([FromQuery] decimal lat, [FromQuery] decimal lng, [FromQuery] DateTime lastUpdatedTime)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { WasSuccessful = false, Message = "Invalid model state for DownloadUpdates." });
            }
            
            // Download all events where the event was updated after the last client's updated time
            GeoBox gb = CalculateBoundingGeoBox(lat, lng, Policy.SURROUNDING_GEOBOX_SIDE_LENGTH);

            var @events = await _context.Event.Where(e => (
                e.Latitude < gb.MaxLatitude &&
                e.Latitude > gb.MinLatitude &&
                e.Longitude < gb.MaxLongitude &&
                e.Longitude > gb.MinLongitude &&
                e.TimeLastUpdated > lastUpdatedTime
                )).ToListAsync();

            // Determine if any of these events need to be deleted...
            var eventsToDelete = events.Where(Policy.ShouldMarkEventForDeletion);

            foreach (Event e in eventsToDelete)
            {
                e.Deleted = true;
                e.TimeLastUpdated = DateTime.UtcNow;
            }

            _context.UpdateRange(eventsToDelete);
            await _context.SaveChangesAsync();

            return Json(new { WasSuccessful = true, Events = @events});
        }

        //// GET: api/Events/5
        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetEvent([FromRoute] int id)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var @event = await _context.Event.SingleOrDefaultAsync(m => m.Id == id);

        //    if (@event == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(@event);
        //}

        // POST: api/Events/

        [HttpPost("Event")]
        public async Task<IActionResult> UpdateEvent([FromBody] Event eventToUpdate)
        {
            if (! ModelState.IsValid)
            {
                return Json(new { WasSuccessful = false, Message = "Invalid model state for event update" });
            }

            // Update the event
            if (EventExists(eventToUpdate.Id) == false)
            {
                // Need the event to exist before we can update it!
                return Json(new { WasSuccessful = false, Message = "The event must exist before you can update it!" });
            }

            // Update the event
            _context.Event.Update(eventToUpdate);

            await _context.SaveChangesAsync();

            return Json(new { WasSuccessful = true, UpdatedEventIndex = eventToUpdate.Id });
        }

        [HttpPost("uv/{eventId}")]
        public async Task<IActionResult> UpvoteEvent([FromQuery] string token, [FromRoute] int eventId)
        {
            return await VoteOnEvent(token, eventId, true);
        }

        [HttpPost("dv/{eventId}")]
        public async Task<IActionResult> DownvoteEvent([FromQuery] string token, [FromRoute] int eventId)
        {
            return await VoteOnEvent(token, eventId, false);
        }

        [HttpPost("unvote/{eventId}")]
        public async Task<IActionResult> UnvoteOnEvent([FromQuery] string token, [FromRoute] int eventId)
        {
            if (! ModelState.IsValid)
            {
                return Json(new { WasSuccessful = false, Message = "Invalid model state..." });
            }

            Token userToken = await _context.Token.FirstOrDefaultAsync(t => t.Value.Equals(token));
            int userId = userToken.CorrespondingLoginId;
            Vote userVote = await _context.Vote.FirstOrDefaultAsync(v => v.UserId == userId);

            if (userVote != null)
            {
                // The user has voted for this, so their vote must be removed...
                _context.Vote.Remove(userVote);
                Event eventVotedOn = await _context.Event.FirstAsync(e => e.Id == userVote.EventId);
                userVote.Event.VoteCount -= userVote.NumVotes;
                _context.Event.Update(eventVotedOn);

                await _context.SaveChangesAsync();

                return Json(new { WasSuccessful = true });
            }

            // Else, the user never voted for this...
            return Json(new { WasSuccessful = false, Message = "User never voted for this event." });
        }

        private async Task<IActionResult> VoteOnEvent(string token, int eventId, bool voteUp)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { WasSuccessful = false, Message = "Invalid model state detected for event vote." });
            }

            // Determine if already upvoted for event
            Token sessionToken = await _context.Token.FirstOrDefaultAsync(t => t.Value.Equals(token));
            // No need to check sessionToken because the user was already validated....
            int currentUser = sessionToken.CorrespondingLoginId;
            Vote vote = await _context.Vote.SingleOrDefaultAsync(v => v.UserId == currentUser && v.EventId == eventId);

            // If the user has not voted for this particular event
            if (vote == null)
            {
                // Vote for it now
                // Update the vote database
                _context.Vote.Add(new Vote() { EventId = eventId, UserId = currentUser, NumVotes = 1 });

                Event eventUpvoted = await _context.Event.FirstOrDefaultAsync(e => e.Id == eventId);

                if (eventUpvoted == null)
                {
                    return BadRequest(ModelState);
                }

                if (voteUp)
                {
                    eventUpvoted.VoteCount++;
                }
                else
                {
                    eventUpvoted.VoteCount--;
                }

                _context.Event.Update(eventUpvoted);

                await _context.SaveChangesAsync();

                return Json(new { WasSuccessful = true });
            }

            // If they have, then notify that the event has already been upvoted...
            return Json(new { WasSuccessful = true, Message = "User has already voted on the event..." });
        }

        [HttpGet("votedFor")]
        private async Task<IActionResult> EventsVotedFor(string token)
        {
            // Get user id
            int userid;
            Token tokenEntry = await this._context.Token.FirstOrDefaultAsync(t => t.Value.Equals(token));

            if (tokenEntry != null)
            {
                userid = tokenEntry.CorrespondingLoginId;
                // Get things voted for
                var votes = await this._context.Vote.Where(v => v.UserId == userid).ToListAsync();
                return Json(new { WasSuccessful = true, Votes = votes});
            }
            else
            {
                return Json(new { wasSuccessful = false });
            }
        }

        // POST: api/Events
        [HttpPost("Create")]
        public async Task<IActionResult> CreateEvent([FromBody] Event @event, [FromQuery] string token)
        {
            if (!ModelState.IsValid)
            {
                return Json( new { WasSuccessful = false, Message = "Invalid model state for CreateEvent()" } );
            }

            var cToken = _context.Token.FirstOrDefault(t => t.Value.Equals(token));

            // Token is validated by the authenticator...

            DateTime now = DateTime.UtcNow;

            Event newEvent = new Event()
            {
                CreatorId = cToken.CorrespondingLoginId,
                Description = @event.Description,
                Latitude = @event.Latitude,
                Longitude = @event.Longitude,
                Name = @event.Name,
                TimeLastUpdated = now,
                TimeCreated = now
            };

            _context.Event.Add(newEvent);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (EventExists(@event.Id))
                {
                    return Json(new { WasSuccessful = false, Message = "Event already created with that particular id!" });
                }
                else
                {
                    throw;
                }
            }

            return Json( new { WasSuccessful = true, EventData = @event });
        }

        // DELETE: api/Events/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent([FromRoute] int id, [FromRoute] string token)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { WasSuccessful = false, Message = "Invalid model state detected." });
            }

            // Get the token
            var matchingToken = await _context.Token.Where(t => t.Value == token).FirstAsync();

            if (matchingToken == null)
            {
                return Json(new { WasSuccessful = false, Message = "Invalid token detected." });
            }

            int loginId = matchingToken.CorrespondingLoginId;

            var @event = await _context.Event.SingleOrDefaultAsync(m => m.Id == id);
            if (@event == null)
            {
                return Json(new { WasSuccessful = false, Message = "Invalid event id" });
            }

            if (loginId == @event.CreatorId)
            {
                _context.Event.Remove(@event);
                await _context.SaveChangesAsync();
                return Json(new { WasSuccessful = true, EventId = @event.Id });
            }

            return Json(new { WasSuccessful = false, Message = "Permission denied for event deletion. You don't own this event!" });
        }

        private bool EventExists(int id)
        {
            return _context.Event.Any(e => e.Id == id);
        }
    }
}