using System.ComponentModel;

namespace DatingApp.Application.Helpers
{
    
    public class AiSearchParams : MemberParams
    {
        [Description("The natural language description of the person the user is looking for (e.g., 'someone who loves art and long walks').")]
        public string? Query { get; set; }
    }
}