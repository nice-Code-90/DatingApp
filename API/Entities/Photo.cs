namespace API.Entities;

public class Photo
{
    public int Id { get; set; }
    public required string Url { get; set; }
    public string? PublicId { get; set; }

    //Navigation property
    public Member Member { get; set; } = null!;
}