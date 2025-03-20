using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AvaloniaApplication1.ViewModels;
using Google.Cloud.Firestore;

namespace AvaloniaApplication1.Services;

public class FirebaseService
{
    private readonly FirestoreDb _firestoreDb;
    
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
            var user = doc.GetValue<string>("username");
            users.Add(new UserViewModel { Name = user, IsActive = false});
        }

        return users;
    }
}