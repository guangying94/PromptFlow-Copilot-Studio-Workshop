namespace Chatbot.Function
{
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
}