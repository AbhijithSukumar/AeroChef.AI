namespace AeroChef.AI.UI.Models
{
    public class ChatMessage
    {
        public string Content { get; set; }
        public bool IsUser { get; set; } // true = User, false = AI
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
