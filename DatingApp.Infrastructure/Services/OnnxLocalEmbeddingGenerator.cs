//For local embedding generation using ONNX Runtime and Microsoft.ML.Tokenizers, we need to add the following NuGet packages to our project:
//<PackageReference Include="Microsoft.ML.OnnxRuntime" Version="1.23.2" />
//< PackageReference Include = "Microsoft.ML.Tokenizers" Version = "2.0.0" />


//using Microsoft.Extensions.AI;
//using Microsoft.ML.OnnxRuntime;
//using Microsoft.ML.OnnxRuntime.Tensors;
//using Microsoft.ML.Tokenizers;
//using Microsoft.Extensions.Logging;

//namespace DatingApp.Infrastructure.AI;

//public class OnnxLocalEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
//{
//    private readonly InferenceSession _session;
//    private readonly Tokenizer _tokenizer;
//    private readonly ILogger<OnnxLocalEmbeddingGenerator> _logger;
//    private const int ModelDimension = 768;

//    public OnnxLocalEmbeddingGenerator(string modelPath, string vocabPath, ILogger<OnnxLocalEmbeddingGenerator> logger)
//    {
//        _session = new InferenceSession(modelPath);
//        _tokenizer = BertTokenizer.Create(vocabPath);
//        _logger = logger;
//    }

//    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
//        IEnumerable<string> values,
//        EmbeddingGenerationOptions? options = null,
//        CancellationToken cancellationToken = default)
//    {
//        var embeddings = new List<Embedding<float>>();

//        foreach (var text in values)
//        {
//            var cleanText = text.ToLowerInvariant();
//            var originalIds = _tokenizer.EncodeToIds(cleanText);

//            // MPNet specifikus keretezés: <s> (0) és </s> (2)
//            var inputIdsList = new List<long> { 0 };
//            inputIdsList.AddRange(originalIds.Select(t => (long)t));
//            inputIdsList.Add(2);

//            var inputIds = inputIdsList.ToArray();
//            var attentionMask = Enumerable.Repeat(1L, inputIds.Length).ToArray();

//            ReadOnlySpan<int> dimensions = new int[] { 1, inputIds.Length };
//            var inputTensor = new DenseTensor<long>(inputIds, dimensions);
//            var maskTensor = new DenseTensor<long>(attentionMask, dimensions);

//            // CSAK AZT A KÉT BEMENETET KÜLDJÜK, AMIT A MODELL ELVÁR
//            var inputs = new List<NamedOnnxValue>
//            {
//                NamedOnnxValue.CreateFromTensor("input_ids", inputTensor),
//                NamedOnnxValue.CreateFromTensor("attention_mask", maskTensor)
//            };

//            using var results = _session.Run(inputs);

//            // MEAN POOLING
//            var outputTensor = results.First().AsTensor<float>();
//            int sequenceLength = inputIds.Length;
//            float[] meanVector = new float[ModelDimension];

//            for (int d = 0; d < ModelDimension; d++)
//            {
//                float sum = 0;
//                for (int s = 0; s < sequenceLength; s++)
//                {
//                    sum += outputTensor[0, s, d];
//                }
//                meanVector[d] = sum / sequenceLength;
//            }

//            var normalizedVector = Normalize(meanVector);
//            embeddings.Add(new Embedding<float>(normalizedVector));
//        }

//        return await Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(embeddings));
//    }

//    private float[] Normalize(float[] vector)
//    {
//        float sumSquare = vector.Select(x => x * x).Sum();
//        float norm = (float)Math.Sqrt(sumSquare);

//        if (norm < 1e-12) return vector;
//        return vector.Select(x => x / norm).ToArray();
//    }

//    public void Dispose()
//    {
//        _session.Dispose();
//        GC.SuppressFinalize(this);
//    }

//    public object? GetService(Type serviceType, object? serviceKey = null) => null;
//}