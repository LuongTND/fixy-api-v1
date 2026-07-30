using System.Text.Json;
using Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class GoongService : IGoongService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<GoongService> _logger;
        private const string BASE_URL = "https://rsapi.goong.io";

        public GoongService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GoongService> logger
        )
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["GOONG_API_KEY"] ?? Environment.GetEnvironmentVariable("GOONG_API_KEY") ?? string.Empty;
        }

        /// <inheritdoc />
        public async Task<(double? Lat, double? Lng)> GeocodeAddressAsync(string fullAddress)
        {
            if (string.IsNullOrWhiteSpace(fullAddress) || string.IsNullOrWhiteSpace(_apiKey))
            {
                return (null, null);
            }

            try
            {
                var encodedAddress = Uri.EscapeDataString(fullAddress);
                var url = $"{BASE_URL}/Geocode?address={encodedAddress}&api_key={_apiKey}";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "[GoongService] Geocode failed for address: {Address}, StatusCode: {StatusCode}",
                        fullAddress,
                        response.StatusCode
                    );
                    return (null, null);
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (
                    root.TryGetProperty("results", out var results)
                    && results.GetArrayLength() > 0
                )
                {
                    var firstResult = results[0];
                    if (firstResult.TryGetProperty("geometry", out var geometry))
                    {
                        if (geometry.TryGetProperty("location", out var location))
                        {
                            var lat = location.GetProperty("lat").GetDouble();
                            var lng = location.GetProperty("lng").GetDouble();

                            _logger.LogInformation(
                                "[GoongService] Geocoded '{Address}' -> Lat={Lat}, Lng={Lng}",
                                fullAddress,
                                lat,
                                lng
                            );
                            return (lat, lng);
                        }
                    }
                }

                _logger.LogWarning(
                    "[GoongService] No geocoding results for address: {Address}",
                    fullAddress
                );
                return (null, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[GoongService] Exception during geocoding for address: {Address}",
                    fullAddress
                );
                return (null, null);
            }
        }

        /// <inheritdoc />
        public async Task<(double? DistanceKm, int? DurationMinutes)> GetDistanceAndDurationAsync(
            double originLat,
            double originLng,
            double destinationLat,
            double destinationLng
        )
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                return (null, null);
            }

            try
            {
                var origins = $"{originLat},{originLng}";
                var destinations = $"{destinationLat},{destinationLng}";
                var url =
                    $"{BASE_URL}/DistanceMatrix?origins={origins}&destinations={destinations}&vehicle=bike&api_key={_apiKey}";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "[GoongService] DistanceMatrix failed, StatusCode: {StatusCode}",
                        response.StatusCode
                    );
                    return (null, null);
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("rows", out var rows) && rows.GetArrayLength() > 0)
                {
                    var firstRow = rows[0];
                    if (
                        firstRow.TryGetProperty("elements", out var elements)
                        && elements.GetArrayLength() > 0
                    )
                    {
                        var element = elements[0];

                        if (
                            element.TryGetProperty("status", out var status)
                            && status.GetString() == "OK"
                        )
                        {
                            double? distanceKm = null;
                            int? durationMinutes = null;

                            if (element.TryGetProperty("distance", out var distanceObj))
                            {
                                // Goong returns distance.value in meters
                                var meters = distanceObj.GetProperty("value").GetInt32();
                                distanceKm = Math.Round(meters / 1000.0, 2);
                            }

                            if (element.TryGetProperty("duration", out var durationObj))
                            {
                                // Goong returns duration.value in seconds
                                var seconds = durationObj.GetProperty("value").GetInt32();
                                durationMinutes = (int)Math.Ceiling(seconds / 60.0);
                            }

                            _logger.LogInformation(
                                "[GoongService] DistanceMatrix: {DistanceKm} km, {DurationMinutes} min",
                                distanceKm,
                                durationMinutes
                            );
                            return (distanceKm, durationMinutes);
                        }
                    }
                }

                return (null, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[GoongService] Exception during DistanceMatrix calculation"
                );
                return (null, null);
            }
        }
    }
}
