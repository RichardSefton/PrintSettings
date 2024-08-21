using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using BCrypt.Net;

namespace PrintSettings.Models;

public class User {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Email { get; set; }

    public string? Password { get; set; }

    public User(string id, string email, string password) {
        Id = id;
        Email = email;
        Password = HashPassword(password);
    }

    public User(string email, string password) {
        Id = "";
        Email = email;
        Password = password;
    }

    public User(string email) {
        Id = "";
        Email = email;
        Password = null;
    }

    public static string HashPassword(string password) {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public static bool VerifyPassword(string hash, string password) {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}