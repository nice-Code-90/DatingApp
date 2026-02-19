using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DatingApp.Infrastructure.Services;

public class HuggingFaceEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    private readonly HttpClient _httpClient;
    private readonly string _modelId;
    private readonly string _apiKey;

    public HuggingFaceEmbeddingGenerator(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _modelId = config["HuggingFace:ModelId"] ?? "sentence-transformers/all-mpnet-base-v2";
        _apiKey = config["HuggingFace:ApiKey"] ?? throw new Exception("Hugging Face API Key is missing!");

        _httpClient.BaseAddress = new Uri("https://api-inference.huggingface.co/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken ct = default)
    {
        var embeddings = new List<Embedding<float>>();

        foreach (var text in values)
        {
            var response = await _httpClient.PostAsJsonAsync($"models/{_modelId}", new { inputs = text }, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                throw new Exception($"Hugging Face API error: {response.StatusCode} - {error}");
            }

            var vector = await response.Content.ReadFromJsonAsync<float[]>(ct);

            if (vector != null)
                embeddings.Add(new Embedding<float>(vector));
        }

        return new GeneratedEmbeddings<Embedding<float>>(embeddings);
    }

    public void Dispose() => _httpClient.Dispose();
    public object? GetService(Type serviceType, object? serviceKey = null) => null;
}