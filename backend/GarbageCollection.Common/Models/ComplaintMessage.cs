namespace GarbageCollection.Common.Models
{
    public class ComplaintMessage
    {
        public string Sender { get; set; } = string.Empty;   // "user" | "admin"
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Time { get; set; }
    }
}
