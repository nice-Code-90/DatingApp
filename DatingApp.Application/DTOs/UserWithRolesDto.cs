namespace DatingApp.Application.DTOs;

public class UserWithRolesDto
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required IList<string> Roles { get; set; }
}