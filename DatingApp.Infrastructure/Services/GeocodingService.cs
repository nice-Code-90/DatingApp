using System.Net.Http.Json;
using DatingApp.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using NetTopologySuite.Geometries;
using Microsoft.Extensions.Logging;

namespace DatingApp.Infrastructure.Services;

public class GeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly GeometryFactory _geometryFactory;
    private readonly ILogger<GeocodingService> _logger;

    public GeocodingService(HttpClient httpClient, IConfiguration config, ILogger<GeocodingService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
        _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
    }

    public async Task<Point?> GetCoordinatesForAddressAsync(string city, string country)
    {
        var apiKey = _config["OpenCageApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogError("OpenCage API key is not configured.");
            return null;
        }

        var address = Uri.EscapeDataString($"{city}, {country}"); 
        var url = $"https://api.opencagedata.com/geocode/v1/json?q={address}&key={apiKey}";

        try
        {
            var response = await _httpClient.GetFromJsonAsync<OpenCageResponse>(url);

            if (response?.Results?.Any() == true)
            {
                var geometry = response.Results.First().Geometry;
                if (geometry != null)
                {
                    return _geometryFactory.CreatePoint(new Coordinate(geometry.Lng, geometry.Lat));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while calling OpenCage Geocoding API.");
        }
        return null;
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