namespace AvaloniaApplication1.Models;

public class MessageModel
{
    public string Content { get; set; }
    public string Timestamp { get; set; }
    public string SenderId { get; set; }
    public bool IsOwnMessage { get; set; }
}