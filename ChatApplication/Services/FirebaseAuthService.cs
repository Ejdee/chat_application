using System;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Google.Api;
using Google.Cloud.Firestore;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApplication.Services;

public class FirebaseAuthService
{
    private readonly FirestoreDb _db;
    private readonly FirebaseAuthClient _authClient;
    private readonly FirebaseService _service;

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
        
        _service = App.ServiceProvider?.GetRequiredService<FirebaseService>() ?? new FirebaseService();
        _db = _service.GetFirestoreDb();
    }

    public async Task<(bool Success, string Message)> RegisterUserAsync(string email, string password, string username)
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

            _service.CurrentUser = userCredential.User.Uid;

            return (true, userCredential.User.Uid);
        }
        catch (Firebase.Auth.FirebaseAuthException ae)
        {
            switch (ae.Reason)
            {
                case AuthErrorReason.WeakPassword:
                    return (false, "Weak password. Must be at least 6 characters long.");
                case AuthErrorReason.EmailExists:
                    return (false, "Email already exists.");
                case AuthErrorReason.MissingEmail:
                    return (false, "Email address is invalid.");
                case AuthErrorReason.MissingPassword:
                    return (false, "Password is invalid.");
                case AuthErrorReason.InvalidEmailAddress:
                    return (false, "Invalid email address.");
                default:
                    Console.WriteLine($"Firebase Auth Error: {ae}");
                    return (false, $"Registration failed: {ae.Reason}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return (false, $"{e}");
        }
    }

    public async Task<(bool Success, string Message)> LoginUserAsync(string email, string password)
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
                
                // store the current user
                 _service.CurrentUser = userCredential.User.Uid; 
                 
                // get the username
                var username = userData["Username"].ToString();
                if (string.IsNullOrEmpty(username)) { throw new Exception("Username is empty."); }
                return (true, username);
            }
            else
            {
                Console.WriteLine("Login Failed: User not found");
                return (false, "User not found.");
            }
        }
        catch (Firebase.Auth.FirebaseAuthException ae)
        {
            switch (ae.Reason)
            {
                case AuthErrorReason.MissingEmail:
                    return (false, "Email is invalid.");
                case AuthErrorReason.MissingPassword:
                    return (false, "Password is invalid.");
                case AuthErrorReason.InvalidEmailAddress:
                    return (false, "Invalid email address.");
                case AuthErrorReason.WrongPassword:
                    return (false, "Wrong password.");
                case AuthErrorReason.UnknownEmailAddress:
                    return (false, "Unknown email address.");
                case AuthErrorReason.WeakPassword:
                    return (false, "Weak password. Must be at least 6 characters long.");
                case AuthErrorReason.UserNotFound:
                    return (false, "User not found.");
                default:
                    
                    if (ae.Message.Contains("INVALID_LOGIN_CREDENTIALS"))
                    {
                        return (false, "Invalid email or password.");
                    }
                    
                    Console.WriteLine($"Firebase Auth Error: {ae}");
                    return (false, $"Invalid login, possible reason: {ae.Reason}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return (false, $"Invalid login: {e.Message}");
        }
    }
    
}