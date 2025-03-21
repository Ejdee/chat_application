using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using AvaloniaApplication1.ViewModels;
using Google.Cloud.Firestore;

namespace AvaloniaApplication1.Services;

public class FirebaseService
{
    private readonly FirestoreDb _firestoreDb;

    public string CurrentUser { get; set; } = string.Empty;

    public FirebaseService()
    {
        _firestoreDb = FirestoreDb.Create("Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID")");
        Console.WriteLine("Connected to the database.");
    }

    public FirestoreDb GetFirestoreDb()
    {
        return _firestoreDb;
    }

    public async Task<List<UserViewModel>> GetUsersAsync()
    {
        // testing purposes delay
        await Task.Delay(5000); 
        
        var usersCollection = _firestoreDb.Collection("users");
        var snapshot = await usersCollection.GetSnapshotAsync();

        List<UserViewModel> users = new List<UserViewModel>();
        if (users == null) throw new ArgumentNullException(nameof(users));

        foreach (var doc in snapshot.Documents)
        {
            // extract the username and create a new UserViewModel for it
            var user = doc.GetValue<string>("Username");
            var active = (doc.GetValue<string>("Status") == "Online") ? true : false;
            users.Add(new UserViewModel { Name = user, IsActive = active});
        }

        return users;
    }

    public async Task<List<MessageViewModel>> LoadChat(string? userName)
    {
        var chatMessagesResult = new List<MessageViewModel>();
        
        // get the Uid of the "receiver" user
        var userUid = await GetUserUid(userName);
        if(userUid == null) { throw new DataException("User not found."); }

        // generate an Uid for the chat - combination of the Uids of the two users, sorted alphabetically
        var chatUid = String.CompareOrdinal(CurrentUser, userUid) < 0 ? 
            CurrentUser + "_" + userUid : 
            userUid + "_" + CurrentUser;
        
        var chatCollection = _firestoreDb.Collection("chat");
        var query = chatCollection.WhereEqualTo("ChatUid", chatUid);
        var snapshot = await query.GetSnapshotAsync();

        if (snapshot.Documents.Count > 0)
        {
            var chatDocument = snapshot.Documents[0];
            var messages = chatDocument.Reference.Collection("messages");
            var messagesSnapshot = await messages.OrderBy("Timestamp").GetSnapshotAsync();

            // load the messages from the chat
            foreach (var message in messagesSnapshot.Documents)
            {
                chatMessagesResult.Add(new MessageViewModel
                {
                    Content = message.GetValue<string>("Content"),
                    IsOwnMessage = message.GetValue<string>("Sender") == CurrentUser,
                    Timestamp = message.GetValue<string>("Timestamp")
                }); 
            }
        }
        else
        {
            // if the chat doesn't exit, create one
            var chatRefDoc = chatCollection.Document();
            await chatRefDoc.SetAsync(new
            {
                ChatUid = chatUid,
                CreatedAt = DateTime.UtcNow,
            });
        }

        return chatMessagesResult;
    }

    /// <summary>
    /// Get the Uid of the user based on the username
    /// </summary>
    /// <param name="userName">the username</param>
    /// <returns>Uid or null if unsuccessful</returns>
    private async Task<string?> GetUserUid(string? userName)
    {
        try
        {
            var usersCollection = _firestoreDb.Collection("users");
            var query = usersCollection.WhereEqualTo("Username", userName);
            var snapshot = await query.GetSnapshotAsync();

            if (snapshot.Documents.Count > 0)
            {
                return snapshot.Documents[0].Id;
            }
            else
            {
                Console.WriteLine("No user found.");
                return null;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error getting the Uid: {e}");
            return null;
        }
    }
}