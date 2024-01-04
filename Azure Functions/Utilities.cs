using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Chatbot.Function
{
    public class ChatUtilities
    {
        public static async Task<ChatOutput> CallChatAsync(PromptInput input, ILogger log)
        {
            // use HTTP Client, to perform HTTP post, to a variable called "url", with the input as the body
            // add the following headers: {"Content-Type":"application/json"},{"Authorization":"bearer"},{"azureml-model-deployment":"blue"}
            // return the response body

            log.LogInformation($"Received input: {JsonConvert.SerializeObject(input)}");

            string url = Environment.GetEnvironmentVariable("CHAT_ENDPOINT");
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Environment.GetEnvironmentVariable("CHAT_KEY"));
            client.DefaultRequestHeaders.Add("azureml-model-deployment", Environment.GetEnvironmentVariable("CHAT_DEPLOYMENT"));
            //client.DefaultRequestHeaders.Add("Content-Type", "application/json");
            var response = await client.PostAsync(url, new StringContent(JsonConvert.SerializeObject(input), System.Text.Encoding.UTF8, "application/json"));
            ChatOutput chatOutput = JsonConvert.DeserializeObject<ChatOutput>(await response.Content.ReadAsStringAsync());
            return chatOutput;
        }

        // Create a function call SaveToCosmosAsync, which takes in PromptInput and a string called conversationId as input
        // Create a CosmosDB output binding, which takes in PromptInput as input, and saves it to the chatHistory container
        // note that the partition key is the conversationId
        // return the PromptInput object
        public static async Task SaveToCosmosAsync(ChatHistory chatHistory, string conversationId, ILogger log)
        {
            log.LogInformation($"Creating new object: {chatHistory}");
            CosmosClient cosmosClient = new CosmosClient(connectionString: Environment.GetEnvironmentVariable("CosmosDBConnection"));
            Container container = cosmosClient.GetContainer("AOAIDemo", "chatHistory");
            await container.CreateItemAsync(chatHistory, new PartitionKey(conversationId));
        }

        public static async Task UpdateItem(PromptInput promptInput, string response, string conversationId, ILogger log)
        {
            Chat_History newEntry = new Chat_History();
            newEntry.inputs = new Inputs();
            newEntry.inputs.question = promptInput.chat_input;
            newEntry.outputs = new Outputs();
            newEntry.outputs.chat_output = response;

            // append newEntry to chat_history
            Chat_History[] fullChatHistory = promptInput.chat_history;
            fullChatHistory.Append(newEntry);

            Array.Resize(ref fullChatHistory, fullChatHistory.Length + 1);
            fullChatHistory[fullChatHistory.Length - 1] = newEntry;

            List<PatchOperation> operations = new()
            {
                PatchOperation.Replace("/chat_history", fullChatHistory)
            };

            CosmosClient cosmosClient = new CosmosClient(connectionString: Environment.GetEnvironmentVariable("CosmosDBConnection"));
            Container container = cosmosClient.GetContainer("AOAIDemo", "chatHistory");
            ChatHistory updated = await container.PatchItemAsync<ChatHistory>(conversationId, new PartitionKey(conversationId), operations);
            log.LogInformation($"Updated item in database: {JsonConvert.SerializeObject(updated)}");
        }
    }

}