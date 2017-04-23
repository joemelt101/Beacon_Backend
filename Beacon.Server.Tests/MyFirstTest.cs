using System;
using Xunit;
using Beacon.Server.Models;
using Beacon.Server.Controllers;

namespace Beacon.Server.Tests
{
    public class EventsControllerTests
    {
        private bool IsWithinPercentError(double actualValue, double expectedValue, double acceptablePercentError)
        {
            acceptablePercentError /= 100d; // Turn this into a percent...
            double maxVal = Math.Abs(expectedValue + expectedValue * acceptablePercentError);
            double minVal = Math.Abs(expectedValue - expectedValue * acceptablePercentError);
            return Math.Abs(actualValue) < maxVal && Math.Abs(actualValue) > minVal;
        }

        [Theory]
        [InlineData(0, 0, 110574, -0.5, -0.5, 0.5, 0.5, 2)]
        public void LatitudeAndLongitudeCalculationsWork(double lat, double lon, double boxEdgeLength, double eMinLat, double eMinLon, double eMaxLat, double eMaxLon, double percentErr)
        {
            EventsController ec = new EventsController();
            var geoBox = ec.CalculateBoundingGeoBox((decimal)lat, (decimal)lon, boxEdgeLength);
            Assert.True(IsWithinPercentError((double)geoBox.MinLatitude, eMinLat, percentErr));
            Assert.True(IsWithinPercentError((double)geoBox.MinLongitude, eMinLon, percentErr));
            Assert.True(IsWithinPercentError((double)geoBox.MaxLatitude, eMaxLat, percentErr));
            Assert.True(IsWithinPercentError((double)geoBox.MaxLongitude, eMaxLon, percentErr));
        }
    }
}
