# Azure Function
This Azure Function bootstrap the API from PromptFlow, and added capability to upsert conversation history into Cosmos DB.

##  Usage
```http
POST http://<azure-function-endpoint>/api/HttpChatTrigger?id=<conversation-id>
Content-Type: application/json

{
    "question" : "user input"
}
```

## Parameters (Application Configuration)
|Parameter Name | Remarks | Sample |
|---|---|---|
|CosmosDBConnection|Azure Cosmos DB Connection String|AccountEndpoint=https://xxx.documents.azure.com:443/;AccountKey=xxxxxxxxxx;|
|COSMOS_DB_NAME| Database name created in Cosmos DB. This database is used to store chat history. | ChatDB|
|COSMOS_CONTAINER_NAME| Container within the database created above. This container is used to store chat history. | ChatContainer|
|CHAT_ENDPOINT|The managed endpoint after deployment in Promptflow.|https://xxxxxxxx.southeastasia.inference.ml.azure.com/score|
|CHAT_KEY|Authentication key obtained from managed endpoint above. Can be found under **Consume** tab. | xxxxxx|
|CHAT_DEPLOYMENT| Deployment name of managed endpoint above. |chatflow-endpoint-1|

## Logic Flow
This Azure Function uses HTTP Trigger, and Cosmos DB input binding to retrieve existing conversation history based on conversation ID, which is used as partition key.

If Cosmos DB return empty object based on the id, it will then proceed to create a new object in Azure Cosmos DB, using the conversation ID as partition key. This object will capture the input from user and output from promptflow.

If Cosmos DB returns valid object, it will then update the object with latest input and output.