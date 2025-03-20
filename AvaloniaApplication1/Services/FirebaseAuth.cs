using System;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace AvaloniaApplication1.Services;

public class FirebaseAuth
{
    private readonly FirestoreDb _db;

    public FirebaseAuth()
    {
        var service = new FirebaseService();
        _db = service.GetFirestoreDb();
    }

    public async Task<bool> UserAuth(string username, string password)
    {
        DocumentReference userRef = _db.Collection("users").Document(username);
        DocumentSnapshot snapshot = await userRef.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            string hashedPassword = snapshot.GetValue<string>("password");
            
            bool passwordMatch = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            
            if (passwordMatch)
            {
                Console.WriteLine("User authenticated.");
                return true;
            }
            else
            {
                Console.WriteLine("User not authenticated.");
                return false;
            }
        }
        else
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            
            await userRef.SetAsync(new { username, password = hashedPassword });
            Console.WriteLine("User registered successfuly.");
            return true;
        }
    }
}