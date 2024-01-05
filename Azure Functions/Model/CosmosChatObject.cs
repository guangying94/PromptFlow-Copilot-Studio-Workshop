namespace Chatbot.Function
{
        public class CosmosChatHistory
    {
        public string conversationId { get; set; }
        public string id { get; set; }
        public Chat_History[] chat_history { get; set; }
        public string chat_input { get; set; }
    }
}