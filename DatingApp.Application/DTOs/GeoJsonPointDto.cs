namespace DatingApp.Application.DTOs
{
    public class GeoJsonPointDto
    {
        public string Type { get; set; } = "Point";
        public double[] Coordinates { get; set; } = [];
    }
}
