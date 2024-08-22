namespace PrintSettings.Models;

public class Auth {
    public bool Authenticated { get; set; }
    public string AccessToken { get; set; }
    public User User { get; set; }

    public Auth() {
        Authenticated = false;
        AccessToken = "";
        User = new User("", "");
    }
    public Auth(bool authenticated, string accessToken, User? incoming_user) {
        if (incoming_user == null) {
            incoming_user = new User("", "");
            Authenticated = false;
            AccessToken = "";
            User = incoming_user;
        } else {
            User user = new User(incoming_user.Id, incoming_user.Email, incoming_user.Password);
            Authenticated = authenticated;
            AccessToken = accessToken;
            User = user;
        }
    }
}