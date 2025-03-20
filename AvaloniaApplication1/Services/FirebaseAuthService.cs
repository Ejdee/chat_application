using System;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Auth.Providers;
using FirebaseAdmin.Auth;
using Google.Api;
using Google.Cloud.Firestore;

namespace AvaloniaApplication1.Services;

public class FirebaseAuthService
{
    private readonly FirestoreDb _db;
    private readonly FirebaseAuthClient _authClient;

    public FirebaseAuthService()
    {
        var config = new FirebaseAuthConfig
        {
            ApiKey = Environment.GetEnvironmentVariable("FIREBASE_API_KEY"),
            AuthDomain = Environment.GetEnvironmentVariable("FIREBASE_AUTH_DOMAIN"), 
            Providers = new FirebaseAuthProvider[]
            {
                new EmailProvider()
            }
        };

        _authClient = new FirebaseAuthClient(config);
        
        var service = new FirebaseService();
        _db = service.GetFirestoreDb();
    }

    public async Task<string?> RegisterUserAsync(string email, string password, string username)
    {
        try
        {
            var userCredential = await _authClient.CreateUserWithEmailAndPasswordAsync(email, password);

            var userDocRef = _db.Collection("users").Document(userCredential.User.Uid);
            await userDocRef.SetAsync(new
            {
                Email = email,
                Username = username,
                CreatedAt = DateTime.UtcNow,
                Status = "Offline",
            });

            return userCredential.User.Uid;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    public async Task<string?> LoginUserAsync(string email, string password)
    {
        try
        {
            var userCredential = await _authClient.SignInWithEmailAndPasswordAsync(email, password);

            var userDocRef = _db.Collection("users").Document(userCredential.User.Uid);
            var userDoc = await userDocRef.GetSnapshotAsync();

            if (userDoc.Exists)
            {
                // update status to online 
                await userDocRef.UpdateAsync("Status", "Online");
                
                var userData = userDoc.ToDictionary();
                
                // get the username
                var username = userData["username"].ToString();
                return username;
            }
            else
            {
                Console.WriteLine("Login Failed: User not found");
                return null;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
    
}