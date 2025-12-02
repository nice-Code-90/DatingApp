using DatingApp.Application.Interfaces;
using DatingApp.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace DatingApp.Infrastructure.Services
{
    public class AiMatchmakingService : IAiMatchmakingService
    {
        private readonly QdrantClient _qdrantClient;

#pragma warning disable CS0618
        private readonly ITextEmbeddingGenerationService _embeddingService;
#pragma warning restore CS0618

        private const string CollectionName = "members_index";
        private const ulong VectorSize = 768;

        public AiMatchmakingService(IConfiguration config, ITextEmbeddingGenerationService embeddingService)
        {
            _embeddingService = embeddingService;
            string qdrantUrl = config["Qdrant:Url"] ?? "http://localhost:6334";
            string apiKey = config["Qdrant:ApiKey"];

            if (!string.IsNullOrEmpty(apiKey))
                _qdrantClient = new QdrantClient(new Uri(qdrantUrl), apiKey);
            else
                _qdrantClient = new QdrantClient(new Uri(qdrantUrl));
        }

        public async Task InitCollectionAsync()
        {
            var collections = await _qdrantClient.ListCollectionsAsync();
            if (!collections.Contains(CollectionName))
            {
                await _qdrantClient.CreateCollectionAsync(CollectionName,
                    new VectorParams { Size = VectorSize, Distance = Distance.Cosine });
            }
        }

        public async Task UpdateMemberProfileAsync(Member member)
        {
            var textDescription = $"Name: {member.DisplayName}. " +
                                  $"Gender: {member.Gender}. " +
                                  $"City: {member.City}, {member.Country}. " +
                                  $"Description: {member.Description ?? "No description provided."}";

            var embedding = await _embeddingService.GenerateEmbeddingAsync(textDescription);

            var pointId = Guid.NewGuid();

            var point = new PointStruct
            {
                Id = pointId,
                Vectors = embedding.ToArray(),
                Payload = {
                    ["SqlId"] = member.Id,
                    ["City"] = member.City,
                    ["Gender"] = member.Gender,
                    ["DisplayName"] = member.DisplayName
                }
            };

            await _qdrantClient.UpsertAsync(CollectionName, new[] { point });
        }
        public async Task<IEnumerable<string>> FindMatchesIdsAsync(string searchQuery)
        {
            var queryVector = await _embeddingService.GenerateEmbeddingAsync(searchQuery);
            var searchResult = await _qdrantClient.SearchAsync(
                CollectionName,
                queryVector.ToArray(),
                limit: 10,
                scoreThreshold: 0.5f
            );

            var ids = new List<string>();
            foreach (var point in searchResult)
            {
                if (point.Payload.TryGetValue("SqlId", out var sqlIdValue))
                {
                    ids.Add(sqlIdValue.StringValue);
                }
            }
            return ids;
        }
    }
}