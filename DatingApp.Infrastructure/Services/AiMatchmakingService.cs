using DatingApp.Application.Interfaces;
using DatingApp.Application.DTOs;
using DatingApp.Application.Extensions;
using DatingApp.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.AI;
using Qdrant.Client;
using Microsoft.Extensions.Logging;
using Qdrant.Client.Grpc;
using DatingApp.Application.Helpers;

namespace DatingApp.Infrastructure.Services
{
    public class AiMatchmakingService : IAiMatchmakingService
    {
        private readonly QdrantClient _qdrantClient;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingService;
        private readonly ILogger<AiMatchmakingService> _logger;

        private const string CollectionName = "members_index";
        private const ulong VectorSize = 768;

        public AiMatchmakingService(
            IConfiguration config,
            IEmbeddingGenerator<string, Embedding<float>> embeddingService,
            IUnitOfWork unitOfWork,
            ILogger<AiMatchmakingService> logger)
        {
            _embeddingService = embeddingService;
            _unitOfWork = unitOfWork;
            _logger = logger;

            string qdrantUrl = config["Qdrant:Url"] ?? "http://localhost:6334";
            string? apiKey = config["Qdrant:ApiKey"];

            _logger.LogInformation("[QDRANT_CLIENT_INIT] Initializing QdrantClient for URL: {QdrantUrl}. Using REST API mode.", qdrantUrl);

            _qdrantClient = new QdrantClient(address: new Uri(qdrantUrl), apiKey: apiKey);
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

            var point = new PointStruct
            {
                Id = Guid.Parse(member.Id),
                Vectors = embedding.ToArray(),
                Payload = {
                    ["SqlId"] = member.Id,
                    ["City"] = member.City,
                    ["Gender"] = member.Gender,
                    ["DisplayName"] = member.DisplayName,
                    ["Age"] = member.DateOfBirth.CalculateAge()
                }
            };

            await _qdrantClient.UpsertAsync(CollectionName, new[] { point });
        }

        public async Task<IEnumerable<string>> FindMatchesIdsAsync(AiSearchParams searchParams)
        {
            if (string.IsNullOrEmpty(searchParams.Query))
            {
                return Enumerable.Empty<string>();
            }

            var queryVector = await _embeddingService.GenerateVectorAsync(searchParams.Query);

            var filterConditions = new List<Condition>();

            if (!string.IsNullOrEmpty(searchParams.Gender))
            {
                filterConditions.Add(new Condition
                {
                    Field = new FieldCondition { Key = "Gender", Match = new Match { Keyword = searchParams.Gender } }
                });
            }

            var ageRange = new Qdrant.Client.Grpc.Range();
            bool isAgeRangeSet = false;
            if (searchParams.MinAge > 18)
            {
                ageRange.Gte = searchParams.MinAge;
                isAgeRangeSet = true;
            }
            if (searchParams.MaxAge < 100)
            {
                ageRange.Lte = searchParams.MaxAge;
                isAgeRangeSet = true;
            }

            if (isAgeRangeSet)
            {
                filterConditions.Add(new Condition { Field = new FieldCondition { Key = "Age", Range = ageRange } });
            }

            var searchResult = await _qdrantClient.SearchAsync(
                CollectionName,
                queryVector.ToArray(),
                filter: new Filter { Must = { filterConditions } },
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

        public async Task<Result<IEnumerable<MemberDto>>> FindMatchingMembersAsync(AiSearchParams searchParams)
        {
            if (string.IsNullOrEmpty(searchParams.Query)) return Result<IEnumerable<MemberDto>>.Failure("Search query cannot be empty");

            var matchIds = await FindMatchesIdsAsync(searchParams);

            if (!matchIds.Any())
            {
                return Result<IEnumerable<MemberDto>>.Failure("No matches found based on your description.");
            }
            var members = await _unitOfWork.MemberRepository.GetMembersByIdsAsync(matchIds);

            return Result<IEnumerable<MemberDto>>.Success(members.Select(member => member.ToDto()).Where(dto => dto != null)!);
        }
    }
}