using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Domain
{
    public class User : Encodable
    {
        public static string UserSeparator = "^";
        public static string UserListSeparator = "?";
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Game> Games { get; set; }

        public User(){
            Games = new List<Game>();
        }

        public void AquireGame(Game newGame)
        {
            Games.Add(newGame);
        }

        public void UpdateGames(List<Game> newGameList){
            this.Games = newGameList;
        }

        public string Encode()
        {
            List<string> data = new List<string>() { Id.ToString(), Name, CustomEncoder.EncodeList(Games, Game.GameListSeparator) };
            return CustomEncoder.Encode(data, UserSeparator);
        }

        public static User Decode(string dataString)
        {
            List<string> data = CustomEncoder.Decode(dataString, UserSeparator);

            List<Game> games = new List<Game>();
            List<string> gamesData = CustomEncoder.Decode(data[3], Game.GameListSeparator);
            foreach(string game in gamesData)
            {
                games.Add(Game.Decode(game));
            }

            return new User()
            {
                Id = Int32.Parse(data[1]),
                Name = data[2],
                Games = games
            };
        }

        public string EncodeGames(){
            return GameSystem.EncodeGames(this.Games);
        }
    }
}