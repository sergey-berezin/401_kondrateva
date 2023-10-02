using BERTTokenizers;
using Microsoft.ML.Data;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.IO;
using System.Threading;
using System;
using System.Net;
using static System.Net.WebRequestMethods;

namespace NuGetBert
{
    public class Bert
    {
        public string text;
        private InferenceSession session;
        static SemaphoreSlim sessionLock = new SemaphoreSlim(1, 1);

        public Bert(string textPath)
        {
            this.text = System.IO.File.ReadAllText(textPath);
            var modelPath = "bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
            var ex_count = 0;
            while (ex_count < 3)
            {
                try
                {
                    var myWebClient = new WebClient();
                    string addres = "https://storage.yandexcloud.net/dotnet4/bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
                    myWebClient.DownloadFile(addres, modelPath);
                }
                catch
                {
                    ex_count++;
                    continue;
                }
                ex_count = 3;
            }

            session = new InferenceSession(modelPath);

        }

        ~Bert()
        {
            session.Dispose();
        }

        public async Task<string> GetAnswer(string question, CancellationToken token)
        {
            return await Task<string>.Factory.StartNew(_ =>
            {
                var sentence = $"{{\"question\": \"{question}\", \"context\": \"{text}\"}}";

                var tokenizer = new BertUncasedLargeTokenizer();

                var tokens = tokenizer.Tokenize(sentence);

                var encoded = tokenizer.Encode(tokens.Count(), sentence);

                // Break out encoding to InputIds, AttentionMask and TypeIds from list of (input_id, attention_mask, type_id).
                var bertInput = new BertInput()
                {
                    InputIds = encoded.Select(t => t.InputIds).ToArray(),
                    AttentionMask = encoded.Select(t => t.AttentionMask).ToArray(),
                    TypeIds = encoded.Select(t => t.TokenTypeIds).ToArray(),
                };

                // Create input tensor.

                var input_ids = ConvertToTensor(bertInput.InputIds, bertInput.InputIds.Length);
                var attention_mask = ConvertToTensor(bertInput.AttentionMask, bertInput.InputIds.Length);
                var token_type_ids = ConvertToTensor(bertInput.TypeIds, bertInput.InputIds.Length);


                // Create input data for session.
                var input = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("input_ids", input_ids),
                                                        NamedOnnxValue.CreateFromTensor("input_mask", attention_mask),
                                                        NamedOnnxValue.CreateFromTensor("segment_ids", token_type_ids) };
                sessionLock.Wait();

                // Run session and send the input data in to get inference output. 
                var output = session.Run(input);

                sessionLock.Release();

                // Call ToList on the output.
                // Get the First and Last item in the list.
                // Get the Value of the item and cast as IEnumerable<float> to get a list result.
                List<float> startLogits = (output.ToList().First().Value as IEnumerable<float>).ToList();
                List<float> endLogits = (output.ToList().Last().Value as IEnumerable<float>).ToList();

                // Get the Index of the Max value from the output lists.
                var startIndex = startLogits.ToList().IndexOf(startLogits.Max());
                var endIndex = endLogits.ToList().IndexOf(endLogits.Max());

                // From the list of the original tokens in the sentence
                // Get the tokens between the startIndex and endIndex and convert to the vocabulary from the ID of the token.
                var predictedTokens = tokens
                            .Skip(startIndex)
                            .Take(endIndex + 1 - startIndex)
                            .Select(o => tokenizer.IdToToken((int)o.VocabularyIndex))
                            .ToList();

                // Print the result.

                return String.Join(" ", predictedTokens);

            }, token, TaskCreationOptions.None);
        }

        public static Tensor<long> ConvertToTensor(long[] inputArray, int inputDimension) //+
        {
            // Create a tensor with the shape the model is expecting. Here we are sending in 1 batch with the inputDimension as the amount of tokens.
            Tensor<long> input = new DenseTensor<long>(new[] { 1, inputDimension });

            // Loop through the inputArray (InputIds, AttentionMask and TypeIds)
            for (var i = 0; i < inputArray.Length; i++)
            {
                // Add each to the input Tenor result.
                // Set index and array value of each input Tensor.
                input[0, i] = inputArray[i];
            }
            return input;
        }

        public class BertInput
        {
            public long[] InputIds { get; set; }
            public long[] AttentionMask { get; set; }
            public long[] TypeIds { get; set; }
        }
    }
}