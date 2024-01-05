namespace Chatbot.Function
{
    public class PromptFlowInput
    {
        public Chat_History[] chat_history { get; set; }
        public string question { get; set; }
    }
}