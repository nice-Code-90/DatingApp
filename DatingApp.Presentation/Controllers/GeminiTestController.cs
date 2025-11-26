using Google.GenAI;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GeminiTestController : ControllerBase
    {
        private readonly IConfiguration _config;

        public GeminiTestController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public async Task<ActionResult<string>> GetGeminiResponse()
        {
            var apiKey = _config["GeminiApiKey"] ?? throw new Exception("Gemini API key not found");
            var client = new Client(apiKey: apiKey);
            
            
            var response = await client.Models.GenerateContentAsync(
            model: "gemini-2.5-flash", contents: "Explain how AI works in a few words");
            var text = response.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
            return Ok(text ?? "No valid text response found.");
        }
    }
}