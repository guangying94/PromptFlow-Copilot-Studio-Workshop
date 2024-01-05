namespace Chatbot.Function
{
    public class ChatHistory
    {
        public string conversationId { get; set; }
        public string id { get; set; }
        public Chat_History[] chat_history { get; set; }
        public string chat_input { get; set; }
    }

    public class Chat_History
    {
        public Inputs inputs { get; set; }
        public Outputs outputs { get; set; }
    }

    public class Inputs
    {
        public string question { get; set; }
    }

    public class Outputs
    {
        public string chat_output { get; set; }
    }

    public class PromptInput
    {
        public Chat_History[] chat_history { get; set; }
        public string chat_input { get; set; }
    }

    public class ChatOutput
    {
        public string answer { get; set; }
    }
}