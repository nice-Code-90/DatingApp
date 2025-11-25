using NetTopologySuite.Geometries;

namespace DatingApp.Application.Interfaces;

public interface IGeocodingService
{
    
    Task<Point?> GetCoordinatesForAddressAsync(string city, string country);
}