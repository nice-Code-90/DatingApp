using DatingApp.Application.Interfaces;
using DatingApp.Application.DTOs;
using DatingApp.Application.Extensions;
using DatingApp.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.AI;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace DatingApp.Infrastructure.Services
{
    public class AiMatchmakingService : IAiMatchmakingService
    {
        private readonly QdrantClient _qdrantClient;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingService;

        private const string CollectionName = "members_index";
        private const ulong VectorSize = 768;

        public AiMatchmakingService(
            IConfiguration config,
            IEmbeddingGenerator<string, Embedding<float>> embeddingService,
            IUnitOfWork unitOfWork)
        {
            _embeddingService = embeddingService;
            _unitOfWork = unitOfWork;

            string qdrantUrl = config["Qdrant:Url"] ?? "http://localhost:6334";
            string? apiKey = config["Qdrant:ApiKey"];

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

            var embedding = await _embeddingService.GenerateVectorAsync(textDescription);

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
            var queryVector = await _embeddingService.GenerateVectorAsync(searchQuery);

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

        public async Task<IEnumerable<MemberDto>> FindMatchingMembersAsync(string searchQuery)
        {
            var matchIds = await FindMatchesIdsAsync(searchQuery);

            if (!matchIds.Any())
            {
                return Enumerable.Empty<MemberDto>();
            }
            var members = await _unitOfWork.MemberRepository.GetMembersByIdsAsync(matchIds);

            return members.Select(member => member.ToDto()).Where(dto => dto != null)!;
        }
    }
}