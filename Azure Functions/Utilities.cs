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
        public static async Task<PromptFlowOutput> CallChatAsync(PromptFlowInput input, ILogger log)
        {
            // use HTTP Client, to perform HTTP post, to a variable called "url", with the input as the body
            // add the following headers: {"Content-Type":"application/json"},{"Authorization":"bearer"},{"azureml-model-deployment":"blue"}
            // return the response body
            try
            {
                log.LogInformation($"Received input: {JsonConvert.SerializeObject(input)}");
                string url = Environment.GetEnvironmentVariable("CHAT_ENDPOINT");
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Environment.GetEnvironmentVariable("CHAT_KEY"));
                client.DefaultRequestHeaders.Add("azureml-model-deployment", Environment.GetEnvironmentVariable("CHAT_DEPLOYMENT"));
                var response = await client.PostAsync(url, new StringContent(JsonConvert.SerializeObject(input), System.Text.Encoding.UTF8, "application/json"));
                PromptFlowOutput chatOutput = JsonConvert.DeserializeObject<PromptFlowOutput>(await response.Content.ReadAsStringAsync());
                return chatOutput;
            }
            catch (Exception e)
            {
                log.LogError($"Error: {e.Message}");
                return null;
            }
        }

        // Create a function call SaveToCosmosAsync, which takes in PromptInput and a string called conversationId as input
        // Create a CosmosDB output binding, which takes in PromptInput as input, and saves it to the chatHistory container
        // note that the partition key is the conversationId
        // return the PromptInput object
        public static async Task SaveToCosmosAsync(CosmosChatHistory chatHistory, string conversationId, ILogger log)
        {
            try
            {
                CosmosClient cosmosClient = new CosmosClient(connectionString: Environment.GetEnvironmentVariable("CosmosDBConnection"));
                Container container = cosmosClient.GetContainer(Environment.GetEnvironmentVariable("COSMOS_DB_NAME"), Environment.GetEnvironmentVariable("COSMOS_CONTAINER_NAME"));
                await container.CreateItemAsync(chatHistory, new PartitionKey(conversationId));
                log.LogInformation($"Created new object: {JsonConvert.SerializeObject(chatHistory)}");
            }
            catch (Exception e)
            {
                log.LogError($"Error: {e.Message}");
            }
        }

        // Function to update existing item in Cosmos DB, using patch operation
        public static async Task UpdateItem(PromptFlowInput promptInput, string response, string conversationId, ILogger log)
        {
            try
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
                Container container = cosmosClient.GetContainer(Environment.GetEnvironmentVariable("COSMOS_DB_NAME"), Environment.GetEnvironmentVariable("COSMOS_CONTAINER_NAME"));
                CosmosChatHistory updated = await container.PatchItemAsync<CosmosChatHistory>(conversationId, new PartitionKey(conversationId), operations);
                log.LogInformation($"Updated item in database: {JsonConvert.SerializeObject(updated)}");
            }
            catch (Exception e)
            {
                log.LogError($"Error: {e.Message}");
            }
        }
    }

}