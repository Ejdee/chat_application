using System.Collections.Generic;
using System.Linq;
using ReactiveUI;

namespace AvaloniaApplication1.Models;

public class ChatModel
{
    public List<MessageModel> Messages { get; set; }

    public void AddMessage(MessageModel message)
    {
        Messages.Add(message);
    }

    public void UpdateMessage(MessageModel updatedMessage)
    {
        var existingMessage = Messages.FirstOrDefault(m => m.Timestamp == updatedMessage.Timestamp);
        if (existingMessage != null)
        {
            existingMessage.Content = updatedMessage.Content;
        }
    }
    
    public void RemoveMessage(MessageModel messageToRemove)
    {
        var message = Messages.FirstOrDefault(m => m.Timestamp == messageToRemove.Timestamp);
        if (message != null)
        {
            Messages.Remove(message);
        }
    }
}