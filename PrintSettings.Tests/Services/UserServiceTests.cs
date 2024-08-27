using Xunit;
using PrintSettings.Models;
using PrintSettings.Data.Services;
using Xunit.Abstractions;
using MongoDB.Driver;
using PrintSettings.Tests.Helpers;

namespace PrintSettings.Tests.Services;

public class UserServiceTests : IClassFixture<MongoDbFixture> {
    private readonly ITestOutputHelper _output;
    private readonly MongoDbFixture _mongoDbFixture;
    private readonly IMongoCollection<User> _userCollection;
    private readonly UserService _userService;

    public UserServiceTests(MongoDbFixture mongoDbFixture, ITestOutputHelper output) {
        _output = output;
        _mongoDbFixture = mongoDbFixture;
        _userCollection = _mongoDbFixture.GetCollection<User>("Users");
        _userService = new UserService(_userCollection);
    }

    [Fact]
    public async Task GetAsync_ReturnsUserById() {
        List<User> users = await Scaffold.Up(_userCollection);
        User user = users[0];
        Assert.NotNull(user);
        Assert.NotNull(user.Id);
        var results = await _userService.GetAsync(user.Id, UserService.UserSearchType.Id);
        Assert.NotNull(results);
        Assert.Equal(user.Email, results.Email);
    }

    [Fact]
    public async Task GetAsync_ReturnsUserByEmail() {
        List<User> users = await Scaffold.Up(_userCollection);
        User user = users[0];
        Assert.NotNull(user);
        Assert.NotNull(user.Email);
        var results = await _userService.GetAsync(user.Email, UserService.UserSearchType.Email);
        Assert.NotNull(results);
        Assert.Equal(user.Email, results.Email);
    }

    [Fact]
    public async Task GetAsync_ReturnsNullWhenGivenInvalidId() {
        await Scaffold.Up(_userCollection);
        var results = await _userService.GetAsync("123", UserService.UserSearchType.Id);
        Assert.Null(results);
    }

    [Fact]
    public async Task GetAsync_ReturnsNullWhenGivenEmptyId() {
        await Scaffold.Up(_userCollection);
        var results = await _userService.GetAsync("", UserService.UserSearchType.Id);
        Assert.Null(results);
    }

    [Fact]
    public async Task GetAsync_ReturnsNullWhenGivenInvalidEmail() {
        await Scaffold.Up(_userCollection);
        var results = await _userService.GetAsync("123", UserService.UserSearchType.Email);
        Assert.Null(results);
    }

    [Fact]
    public async Task GetAsync_ReturnsNullWhenGivenEmptyEmail() {
        await Scaffold.Up(_userCollection);
        var results = await _userService.GetAsync("", UserService.UserSearchType.Email);
        Assert.Null(results);
    }

    [Fact]
    public async Task CreateAsync_ReturnsWithNewUser() {
        await Scaffold.Up(_userCollection);
        User user = new("test6@test.com", User.HashPassword("SomePass4NAV!"));
        var results = await _userService.CreateAsync(user);
        Assert.NotNull(results);
        Assert.Equal(user.Email, results.Email);
        Assert.NotNull(results.Id);
        Assert.NotEmpty(results.Id);
    }

    [Fact]
    public async Task CreateAsync_ThrowsErrorWithDuplicateEmail() {
        await Scaffold.Up(_userCollection);
        User user = new("test@test.com", User.HashPassword("SomePass4NAV!"));
        var error = await Assert.ThrowsAsync<Exception>(() => _userService.CreateAsync(user));
        //Test the error message
        Assert.Equal("User already exists", error.Message);
    }

    [Fact]
    public async Task CreateAsync_OverwritesGivenId() {
        await Scaffold.Up(_userCollection);
        User user = new("test6@test.com", User.HashPassword("SomePass4NAV!")) {
            Id = "123"
        };
        var results = await _userService.CreateAsync(user);
        Assert.NotNull(results);
        Assert.Equal(user.Email, results.Email);
        Assert.NotNull(results.Id);
        Assert.NotEmpty(results.Id);
        Assert.NotEqual("123", results.Id);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsWithTrueForUserWithValidId() {
        List<User> users = await Scaffold.Up(_userCollection);
        User user = users[0];
        Assert.NotNull(user);
        Assert.NotNull(user.Id);

        user.Password = User.HashPassword("NewPassword123!");
        var results = await _userService.UpdateAsync(user);
        Assert.True(results);
        Assert.True(User.VerifyPassword(user.Password, "NewPassword123!"));
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalseForUserWithInvalidId() {
        List<User> users = await Scaffold.Up(_userCollection);
        User user = users[0];
        Assert.NotNull(user);
        Assert.NotNull(user.Id);

        user.Id = "123";
        user.Password = User.HashPassword("NewPassword123!");
        var results = await _userService.UpdateAsync(user);
        Assert.False(results);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalseForUserWithEmptyId() {
        List<User> users = await Scaffold.Up(_userCollection);
        User user = users[0];
        Assert.NotNull(user);
        Assert.NotNull(user.Id);

        user.Id = "";
        user.Password = User.HashPassword("NewPassword123!");
        var results = await _userService.UpdateAsync(user);
        Assert.False(results);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalseForUserWithNullId() {
        List<User> users = await Scaffold.Up(_userCollection);
        User user = users[0];
        Assert.NotNull(user);
        Assert.NotNull(user.Id);

        user.Id = null;
        user.Password = User.HashPassword("NewPassword123!");
        var results = await _userService.UpdateAsync(user);
        Assert.False(results);
    }

    [Fact]
    public async Task RemoveAsync_ReturnsTrueForUserWithValidId() {
        List<User> users = await Scaffold.Up(_userCollection);
        User user = users[0];
        Assert.NotNull(user);
        Assert.NotNull(user.Id);

        var results = await _userService.RemoveAsync(user.Id);
        Assert.True(results);
    }

    [Fact]
    public async Task RemoveAsync_ReturnsFalseForUserWithInvalidId() {
        List<User> users = await Scaffold.Up(_userCollection);
        User user = users[0];
        Assert.NotNull(user);
        Assert.NotNull(user.Id);

        var results = await _userService.RemoveAsync("123");
        Assert.False(results);
    }

    [Fact]
    public async Task RemoveAsync_ReturnsFalseForUserWithEmptyId() {
        List<User> users = await Scaffold.Up(_userCollection);
        User user = users[0];
        Assert.NotNull(user);
        Assert.NotNull(user.Id);

        var results = await _userService.RemoveAsync("");
        Assert.False(results);
    }

    [Fact]
    public async Task LoginAsync_ReturnsUserForValidCredentials() {
        List<User> users = await Scaffold.Up(_userCollection);
        User user = users[0];
        Assert.NotNull(user);
        Assert.NotNull(user.Email);
        Assert.NotNull(user.Password);

        var results = await _userService.LoginAsync(user.Email, "SomePass4NAV!");
        Assert.NotNull(results);
        Assert.Equal(user.Email, results.Email);
    }

    [Fact]
    public async Task LoginAsync_ReturnsNullForInvalidCrenentials() {
        List<User> users = await Scaffold.Up(_userCollection);
        User user = users[0];
        Assert.NotNull(user);
        Assert.NotNull(user.Email);
        Assert.NotNull(user.Password);

        var results = await _userService.LoginAsync(user.Email, "SomePass4NAV");
        Assert.Null(results);
    }
}