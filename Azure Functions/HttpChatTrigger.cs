using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Chatbot.Function
{
    public static class HttpChatTrigger
    {
        [FunctionName("HttpChatTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "%COSMOS_DB_NAME%",
                containerName: "%COSMOS_CONTAINER_NAME%",
                Connection ="CosmosDBConnection",
                Id = "{Query.id}",
                PartitionKey = "{Query.id}")] CosmosChatHistory chatHistory,
            ILogger log)
        {
            PromptFlowInput promptInput = new PromptFlowInput();

            // Get content from http request body, then deseriaize it into "Inputs" object
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            Inputs inputs = JsonConvert.DeserializeObject<Inputs>(requestBody);

            // get value from query called "id", and set it to the conversationId
            string conversationId = req.Query["id"];

            if (chatHistory == null)
            {
                // new conversation
                log.LogInformation($"No chat history for {conversationId}");
                promptInput.chat_input = inputs.question;
                Chat_History _chatHistory = new Chat_History();
                promptInput.chat_history = new Chat_History[] { _chatHistory };

                PromptFlowOutput replyPVA = await ChatUtilities.CallChatAsync(promptInput, log);

                // Save item to cosmos DB, constructing the object as follows:
                CosmosChatHistory newChatHistory = new CosmosChatHistory();
                newChatHistory.conversationId = conversationId;
                newChatHistory.id = conversationId;

                Chat_History[] chat_History = new Chat_History[] { new Chat_History() };
                Inputs _inputs = new Inputs();
                _inputs.question = inputs.question;
                Outputs _outputs = new Outputs();
                _outputs.chat_output = replyPVA.answer;
                chat_History[0].inputs = _inputs;
                chat_History[0].outputs = _outputs;
                newChatHistory.chat_history = chat_History;

                await ChatUtilities.SaveToCosmosAsync(newChatHistory, conversationId, log);
                return new OkObjectResult(replyPVA);
            }
            else
            {
                // existing conversation
                log.LogInformation(JsonConvert.SerializeObject(chatHistory));

                promptInput.chat_history = chatHistory.chat_history;
                promptInput.chat_input = inputs.question;

                PromptFlowOutput replyPVA = await ChatUtilities.CallChatAsync(promptInput, log);

                await ChatUtilities.UpdateItem(promptInput, replyPVA.answer, conversationId, log);
                return new OkObjectResult(replyPVA);
            }
        }
    }
}
