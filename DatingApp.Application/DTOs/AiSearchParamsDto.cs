using DatingApp.Application.Helpers;
using System.ComponentModel;

namespace DatingApp.Application.DTOs
{
    
    public class AiSearchParamsDto : MemberParams
    {
        [Description("The natural language description of the person the user is looking for (e.g., 'someone who loves art and long walks').")]
        public string? Query { get; set; }
    }
}