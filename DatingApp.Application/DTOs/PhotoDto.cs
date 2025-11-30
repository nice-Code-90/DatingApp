namespace DatingApp.Application.DTOs;

public class PhotoDto
{
    public int Id { get; set; }
    public required string Url { get; set; }
    public bool IsMain { get; set; }
    public bool IsApproved { get; set; }
}
