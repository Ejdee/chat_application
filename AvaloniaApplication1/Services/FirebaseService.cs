using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Markup.Xaml.Converters;
using Avalonia.Metadata;
using AvaloniaApplication1.ViewModels;
using Google.Cloud.Firestore;
using Google.Protobuf.WellKnownTypes;

namespace AvaloniaApplication1.Services;

public class FirebaseService
{
    private readonly FirestoreDb _firestoreDb;
    private string? CurrentChatUid { get; set; } = null;
    private FirestoreChangeListener? _chatListener;
    private FirestoreChangeListener? _userStatusListener;

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
        //await Task.Delay(5000); 
        
        var usersCollection = _firestoreDb.Collection("users");
        var snapshot = await usersCollection.GetSnapshotAsync();

        List<UserViewModel> users = new List<UserViewModel>();
        if (users == null) throw new ArgumentNullException(nameof(users));

        foreach (var doc in snapshot.Documents)
        {
            // skip the current user
            if (doc.Id == CurrentUser) { continue; }
            
            // extract the username and create a new UserViewModel for it
            var user = doc.GetValue<string>("Username");
            var active = (doc.GetValue<string>("Status") == "Online") ? true : false;
            users.Add(new UserViewModel { Name = user, IsActive = active});
        }

        return users;
    }

    public async Task<List<MessageViewModel>> LoadChatAsync(string? userName)
    {
        var chatMessagesResult = new List<MessageViewModel>();
        
        // get the Uid of the "receiver" user
        var userUid = await GetUserUid(userName);
        if(userUid == null) { throw new DataException("User not found."); }

        // generate an Uid for the chat - combination of the Uids of the two users, sorted alphabetically
        var chatUid = String.CompareOrdinal(CurrentUser, userUid) < 0 ? 
            CurrentUser + "_" + userUid : 
            userUid + "_" + CurrentUser;
        
        // clear the user from the "currentlyReading" list of the old chat, since he is opening a new one
        await ClearTheCurrentlyReadingAsync();
        
        // store the chat Uid for future reference
        CurrentChatUid = chatUid;
        
        var chatCollection = _firestoreDb.Collection("chat");
        var query = chatCollection.WhereEqualTo("ChatUid", chatUid);
        var snapshot = await query.GetSnapshotAsync();

        if (snapshot.Documents.Count > 0)
        {
            var chatDocument = snapshot.Documents[0];
            
            // update the currently reading list with adding the current user (that opened it)
            await chatDocument.Reference.UpdateAsync("CurrentlyReading", FieldValue.ArrayUnion(CurrentUser));
            Console.WriteLine("Currently reading set: " + CurrentUser);
            
            
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
            var userIds = chatUid.Split("_");
            await chatRefDoc.SetAsync(new
            {
                ChatUid = chatUid,
                CreatedAt = DateTime.UtcNow,
                CurrentlyReading = new List<string>(),
                NewMessageIndicator = new Dictionary<string, bool>
                {
                    { userIds[0], false },
                    { userIds[1], false }
                }
            });
        }

        return chatMessagesResult;
    }

    public async Task ClearTheCurrentlyReadingAsync()
    {
        var chatCollection = _firestoreDb.Collection("chat");
        Console.WriteLine("Current ChatUID: " + CurrentChatUid);
        if (CurrentChatUid != null)
        {
            var query = chatCollection.WhereEqualTo("ChatUid", CurrentChatUid);
            var snapshotChat = await query.GetSnapshotAsync();
            if (snapshotChat.Documents.Count > 0)
            {
                var chatDocument = snapshotChat.Documents[0];
                await chatDocument.Reference.UpdateAsync("CurrentlyReading", FieldValue.ArrayRemove(CurrentUser));
                Console.WriteLine("Currently reading removed: " + CurrentUser);
            }
        }
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

    public async Task SendMessageAsync(string content)
    {
        try
        {
            var chatCollection = _firestoreDb.Collection("chat").Document(CurrentChatUid);
            var messagesCollection = chatCollection.Collection("messages");

            await messagesCollection.AddAsync(new
            {
                Content = content,
                Sender = CurrentUser,
                Timestamp = DateTime.UtcNow.ToString("HH:mm")
            });
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error sending message: {e}");
            throw;
        }
    }

    public void ListenForChatUpdates(Action<MessageViewModel, string> onMessagesUpdated)
    {
        _chatListener?.StopAsync();
        
        var chatCollection = _firestoreDb.Collection("chat").Document(CurrentChatUid).Collection("messages").OrderBy("Timestamp");
        _chatListener = chatCollection.Listen(snapshot =>
        {
            foreach (var change in snapshot.Changes)
            {
                // get the message
                var messageDoc = change.Document;
                var message = new MessageViewModel
                {
                    Content = messageDoc.GetValue<string>("Content"),
                    IsOwnMessage = messageDoc.GetValue<string>("Sender") == CurrentUser,
                    Timestamp = messageDoc.GetValue<string>("Timestamp")
                };
                
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    var changeType = change.ChangeType.ToString().ToLower();
                    onMessagesUpdated(message, changeType);
                });
            }
        });
    }

    public async Task LogOutUserAsync()
    {
        try
        {
            // stop the chat listener
            if (_chatListener != null)
            {
                await _chatListener.StopAsync();
                _chatListener = null;
            }
            
            // stop the user status listener
            if (_userStatusListener != null)
            {
                await _userStatusListener.StopAsync();
                _userStatusListener = null;
            }

            if (!string.IsNullOrEmpty(CurrentUser))
            {
                var userDocRef = _firestoreDb.Collection("users").Document(CurrentUser);
                await userDocRef.UpdateAsync("Status", "Offline");
            }

            CurrentUser = "";
            CurrentChatUid = "";
        }
        catch (Exception e)
        {
            Console.WriteLine("Error logging out: " + e);
        }
    }

    public void ListenForUserStatus(Action<UserViewModel, string> onUserUpdated)
    {
        _userStatusListener?.StopAsync();

        var usersDoc = _firestoreDb.Collection("users");
        _userStatusListener = usersDoc.Listen(snapshot =>
        {
            foreach (var change in snapshot.Changes)
            {
                var userDoc = change.Document;

                // skip the current user 
                if (userDoc.Id == CurrentUser) { continue; }
                
                var user = new UserViewModel
                {
                    Name = userDoc.GetValue<string>("Username"),
                    IsActive = userDoc.GetValue<string>("Status") == "Online"
                };

                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    onUserUpdated(user, change.ChangeType.ToString().ToLower());
                });
            }
        });
    }
}