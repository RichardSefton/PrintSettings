using Xunit;
using PrintSettings.Models;

namespace PrintSettings.Tests.Models;

public class UserModelTests {

    public UserModelTests() {

    }

    [Fact]
    public void UserModelCreatesFromAllKeyData() {
        User user = new("1234", "test@test.com", "SomePass4NAV!");

        Assert.Equal("1234", user.Id);
        Assert.Equal("test@test.com", user.Email);
        Assert.Equal("SomePass4NAV!", user.Password);
    }

    [Fact]
    public void UserModelCreatesFromEmailAndPassword() {
        User user = new("test@test.com", "SomePass4NAV!");

        Assert.Equal("test@test.com", user.Email);
        Assert.Equal("SomePass4NAV!", user.Password);
    }

    [Fact]
    public void UserModelCreatesFromEmailAddressOnly() {
        User user = new("test@test.com");

        Assert.Equal("test@test.com", user.Email);
    }

    [Fact]
    public void UserModelHashPasswordHashesPasswords() {
        string pswd = User.HashPassword("SomePass4NAV!");
        Assert.NotNull(pswd);
        Assert.True(pswd.Length > 0);
    }

    [Fact]
    public void UserModelVerifyPasswordVerifiesCorrectHash() {
        string pswd = "SomePass4NAV!";
        string hashed = User.HashPassword(pswd);
        Assert.True(User.VerifyPassword(hashed, pswd));
    }

    [Fact]
    public void UserModelSetPasswordHashesPassword() {
        string pswd = "SomePass4NAV!";
        User user = new("test@test.com");
        user.SetPassword(pswd);

        Assert.NotNull(user.Password);
        Assert.True(user.Password.Length > 0);
        Assert.True(User.VerifyPassword(user.Password, pswd));
    }
}