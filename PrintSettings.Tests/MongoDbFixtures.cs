using PrintSettings.Models;
using Mongo2Go;
using MongoDB.Driver;
using Xunit.Abstractions;

namespace PrintSettings.Tests.Helpers;

public class MongoDbFixture : IDisposable {
    public ITestOutputHelper? Output { get; set; }
    private readonly MongoDbRunner _mongoRunner;
    public IMongoDatabase Database { get; private set; }

    public MongoDbFixture() {
        Output = null;
        _mongoRunner = MongoDbRunner.Start();
        var client = new MongoClient(_mongoRunner.ConnectionString);
        Database = client.GetDatabase("TestDb");

        CreateCollectionIfNotExists("Users");
    }

    private void CreateCollectionIfNotExists(string collectionName) {
        var collectionList = Database.ListCollectionNames().ToList();
        if (!collectionList.Contains(collectionName)) {
            Database.CreateCollection(collectionName);
        }
    }

    public IMongoCollection<T> GetCollection<T>(string name) {
        return Database.GetCollection<T>(name);
    }

    public void Dispose() {
        _mongoRunner.Dispose();
    }
}