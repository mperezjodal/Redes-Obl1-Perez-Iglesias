using System.Text.Json;

namespace Domain
{
    public class UserGamePair
    {
        public User User { get; set; }
        public Game Game { get; set; }

        public UserGamePair(User user, Game game)
        {
            this.User = user;
            this.Game = game;
        }
        public string Encode()
        {
            return JsonSerializer.Serialize(this);
        }

        public static UserGamePair Decode(string jsonString)
        {
            return JsonSerializer.Deserialize<UserGamePair>(jsonString);
        }
    }
}