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

    public enum UserSearchType {
        Id,
        Email
    }

    public async Task<User?> GetAsync(string userId, UserSearchType searchType) {
        try {
            if (searchType == UserSearchType.Email)
                return await _userCollection.Find(user => user.Email == userId).FirstOrDefaultAsync();
            
            if (searchType == UserSearchType.Id)
                return await _userCollection.Find(user => user.Id == userId).FirstOrDefaultAsync();
        
            return null;
        } catch {
            return null;
        }
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

        var matchedUser = await _userCollection.Find(user => user.Email == newUser.Email).FirstOrDefaultAsync();
        if (matchedUser != null)
            throw new Exception("User already exists");

        User user = new(newUser.Email, newUser.Password);

        await _userCollection.InsertOneAsync(user);
        return user;
    }

    public async Task<bool> UpdateAsync(User updatedUser) {
        try {
            if (string.IsNullOrEmpty(updatedUser.Id))
                return false;
            var update = await _userCollection.ReplaceOneAsync(user => user.Id == updatedUser.Id, updatedUser);
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

    public Task<User?> LoginAsync(string email, string password) {
        try {
            User matchedUser = _userCollection.Find(user => user.Email == email).FirstOrDefault();
            if (matchedUser == null)
                return Task.FromResult<User?>(null);

            if (User.VerifyPassword(matchedUser?.Password ?? "", password))
                return Task.FromResult<User?>(matchedUser);
            
            return Task.FromResult<User?>(null);
        } catch(Exception ex) {
            throw new Exception(ex.Message);
        }
    }
}