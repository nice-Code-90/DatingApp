using System.Net.Http.Json;
using DatingApp.Application.Interfaces;
using DatingApp.Application.Helpers;
using NetTopologySuite.Geometries;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DatingApp.Infrastructure.Services;

public class GeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly GeometryFactory _geometryFactory;
    private readonly ILogger<GeocodingService> _logger;
    private readonly ICacheService _cacheService;
    private readonly OpenCageSettings _openCageSettings;

    public GeocodingService(HttpClient httpClient, IOptions<OpenCageSettings> config, ILogger<GeocodingService> logger, ICacheService cacheService)
    {
        _httpClient = httpClient;
        _logger = logger;
        _cacheService = cacheService;
        _openCageSettings = config.Value;
        _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
    }

    public async Task<Point?> GetCoordinatesForAddressAsync(string city, string country)
    {
        if (string.IsNullOrEmpty(_openCageSettings.ApiKey) || string.IsNullOrEmpty(_openCageSettings.BaseUrl))
        {
            _logger.LogError("OpenCage API settings (ApiKey or BaseUrl) are not configured.");
            return null;
        }

        var cacheKey = $"geocode-{city}-{country}".ToLower();

        var cachedPoint = await _cacheService.GetAsync<Point>(cacheKey);
        if (cachedPoint != null) return cachedPoint;

        var address = Uri.EscapeDataString($"{city}, {country}"); 
        var url = $"{_openCageSettings.BaseUrl}?q={address}&key={_openCageSettings.ApiKey}";

        Point? point = null;

        try
        {
            var response = await _httpClient.GetFromJsonAsync<OpenCageResponse>(url);

            if (response?.Results?.Any() == true)
            {
                var geometry = response.Results.First().Geometry;
                if (geometry != null)
                {
                    point = _geometryFactory.CreatePoint(new Coordinate(geometry.Lng, geometry.Lat));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while calling OpenCage Geocoding API.");
            return null; 
        }

        if (point != null)
        {
            await _cacheService.SetAsync(cacheKey, point, TimeSpan.FromDays(30));
        }

        return point;
    }
}


public class OpenCageResponse
{
    public List<OpenCageResult>? Results { get; set; }
}

public class OpenCageResult
{
    public OpenCageGeometry? Geometry { get; set; }
}

public class OpenCageGeometry
{
    public double Lat { get; set; }
    public double Lng { get; set; }
}