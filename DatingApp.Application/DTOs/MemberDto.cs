
namespace DatingApp.Application.DTOs
{
    public class MemberDto
    {
        public string Id { get; set; }
        public int Age { get; set; }
        public string? ImageUrl { get; set; }
        public string DisplayName { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }
        public string Gender { get; set; }
        public string? Description { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public ICollection<PhotoDto> Photos { get; set; } = [];
    }
}
