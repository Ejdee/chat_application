using Firebase.Database;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

namespace ChatApplication
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Connect to the databse
            FirestoreDb db = await FirestoreDb.CreateAsync("Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID")");
            
            Console.WriteLine("Connected to Firestore.");

            Console.WriteLine("Enter your username: ");
            var username = Console.ReadLine();
            if (username == null) return;
            
            CollectionReference messages = db.Collection("messages");

            _ = Task.Run(() => ListenToMessages(db, username, messages));
            
            await StartSendingMessages(db, username, messages);
            
        }

        static void ListenToMessages(FirestoreDb db, string username, CollectionReference messages)
        {
            // Real-time listener for messages
            messages.Listen(snapshot =>
            {
                foreach (var change in snapshot.Changes)
                {
                    if (change.ChangeType == DocumentChange.Type.Added)
                    {
                        var data = change.Document.ToDictionary();
                        
                        // Receive only messages from other users
                        if ((string)data["sender"] == username) continue;
                        
                        Console.WriteLine($"{data["sender"]}: {data["text"]}");
                    }
                }
            });
        }

        static async Task StartSendingMessages(FirestoreDb db, string user, CollectionReference messages)
        {
            // Send messages
            Console.WriteLine("Start typing 'exit' to quit.");
            while (true)
            {
                var message = Console.ReadLine();
                if (message == "exit")
                {
                    break;
                }

                var newMessage = new
                {
                    sender = user,
                    text = message,
                    timestamp = DateTime.UtcNow,
                };
                await messages.AddAsync(newMessage);
            }
        }
    }
}