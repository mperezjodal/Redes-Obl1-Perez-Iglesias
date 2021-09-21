using System.Collections.Generic;
using System.Text.Json;

namespace Domain
{
    public class User
    {
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
            return JsonSerializer.Serialize(this);
        }

        public static User Decode(string jsonString)
        {
            return JsonSerializer.Deserialize<User>(jsonString);
        }

        public string EncodeGames(){
            return JsonSerializer.Serialize(this.Games);
        }
    }
}