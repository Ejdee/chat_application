using System;
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
}