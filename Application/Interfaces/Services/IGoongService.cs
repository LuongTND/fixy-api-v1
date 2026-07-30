namespace Application.Interfaces.Services
{
    public interface IGoongService
    {
        /// <summary>
        /// Geocode a full address string to Lat/Lng coordinates using Goong Maps API.
        /// </summary>
        Task<(double? Lat, double? Lng)> GeocodeAddressAsync(string fullAddress);

        /// <summary>
        /// Calculate driving distance (km) and duration (minutes) between two GPS coordinates using Goong DistanceMatrix API (vehicle=bike).
        /// </summary>
        Task<(double? DistanceKm, int? DurationMinutes)> GetDistanceAndDurationAsync(
            double originLat,
            double originLng,
            double destinationLat,
            double destinationLng
        );
    }
}
