using System.Text.RegularExpressions;
using PrintSettings.Models;
using PrintSettings.Data.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Amazon.Runtime.Internal;

namespace PrintSettings.Data.Services;

public class UserService {
    private readonly IMongoCollection<User> _userCollection;

    public UserService(IMongoCollection<User> userCollection) {
        _userCollection = userCollection;
    }

    public UserService(IOptions<PrintSettingsDatabaseSettings> printSettingsDatabaseSettings) {
        var mongoClient = new MongoClient(printSettingsDatabaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(printSettingsDatabaseSettings.Value.DatabaseName);
        _userCollection = mongoDatabase.GetCollection<User>(
            printSettingsDatabaseSettings.Value.UserCollectionName
        );
    }

    public async Task<User> GetAsync(string userId) {
        return await _userCollection.Find(user => user.Id == userId).FirstOrDefaultAsync();
    }

    public async Task<User> CreateAsync(User newUser) {
        newUser.Id = null;
        
        string email = newUser.Email;
        //verify valid email address with a regex
        Regex emailValidation = new(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?");
        if (!emailValidation.IsMatch(email))
            throw new Exception("Invalid email address");

        if (string.IsNullOrEmpty(newUser.Password))
            throw new Exception("Password is required");

        User user = new(newUser.Email, newUser.Password);

        await _userCollection.InsertOneAsync(user);
        return user;
    }

    public async Task<bool> UpdateAsync(User updatedUser) {
        try {
            User user = new(updatedUser.Email) {
                Id = updatedUser.Id
            };
            var update = await _userCollection.ReplaceOneAsync(user => user.Id == updatedUser.Id, user);
            return update.ModifiedCount > 0;
        } catch {
            return false;
        }
    } 

    public async Task<bool> RemoveAsync(string userId) {
        try {
            var result = await _userCollection.DeleteOneAsync(user => user.Id == userId);
            return result.DeletedCount > 0;
        } catch {
            return false;
        }
    }
}