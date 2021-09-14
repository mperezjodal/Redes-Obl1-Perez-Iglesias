using System.Globalization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;

namespace Domain
{
    public class GameSystem
    {
        public List<Game> Games { get; set;}
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

        public string EncodeGames(){
            return JsonSerializer.Serialize(this.Games);
        }

        public static string EncodeGames(List<Game> games){
            return JsonSerializer.Serialize(games);
        }

        public static List<Game> DecodeGames(string jsonString){
            return JsonSerializer.Deserialize<List<Game>>(jsonString);
        }
    }
}