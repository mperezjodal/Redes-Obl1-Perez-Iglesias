using System.Collections.Generic;
using System.Text.Json;

namespace Domain
{
    public class UserGamePair
    {
        public static string UserGameSeparator = "@";
        public User User { get; set; }
        public Game Game { get; set; }

        public UserGamePair(User user, Game game)
        {
            this.User = user;
            this.Game = game;
        }
        public string Encode()
        {
            List<string> data = new List<string>() { User.Encode(), Game.Encode() };
            return CustomEncoder.Encode(data, UserGameSeparator);
        }

        public static UserGamePair Decode(string dataString)
        {
            List<string> data = CustomEncoder.Decode(dataString, UserGameSeparator);
            return new UserGamePair(User.Decode(data[0]), Game.Decode(data[1]));
        }
    }
}