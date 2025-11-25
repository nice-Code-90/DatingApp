using System.Net.Http.Json;
using DatingApp.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using NetTopologySuite.Geometries;

namespace DatingApp.Infrastructure.Services;

public class GeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly GeometryFactory _geometryFactory;

    public GeocodingService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
        
        _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
    }

    public async Task<Point?> GetCoordinatesForAddressAsync(string city, string country)
    {
        var apiKey = _config["OpenCageApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            
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
                return _geometryFactory.CreatePoint(new Coordinate(geometry.Lng, geometry.Lat));
            }
        }
        catch (Exception ex)
        {
            
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