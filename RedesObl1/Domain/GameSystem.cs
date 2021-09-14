using System.Globalization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;

namespace Domain
{
    public class GameSystem
    {
        public ICollection<Game> Games { get; set;}
        public List<User> Users { get; set;}

        public GameSystem(){
            Games = new List<Game>();
            Users = new List<User>();
        }

        public void AddGame(Game game){
            Games.Add(game);
        }

        public void AddUser(User user){
            Users.Add(user);
        }

        public string EncodeGameList(){
            return JsonSerializer.Serialize(this.Games);
        }

        public static List<Game> DecodeGameList(string jsonString){
            return JsonSerializer.Deserialize<List<Game>>(jsonString);
        }
    }
}