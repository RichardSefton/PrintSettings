using PrintSettings.Models;
using MongoDB.Driver;

namespace PrintSettings.Tests.Helpers;

public class Scaffold {
    public static async Task<List<User>> Up(IMongoCollection<User> _userCollection) {
        await _userCollection.DeleteManyAsync(Builders<User>.Filter.Empty);
        List<User> users = new() {
            new("test@test.com", User.HashPassword("SomePass4NAV!")),
            new("test@test2.com", User.HashPassword("SomePass4NAV!")),
            new("test@test3.com", User.HashPassword("SomePass4NAV!")),
            new("test@test4.com", User.HashPassword("SomePass4NAV!")),
            new("test@test5.com", User.HashPassword("SomePass4NAV!")),
        };

        _userCollection.InsertMany(users);

        return users;
    } 
}