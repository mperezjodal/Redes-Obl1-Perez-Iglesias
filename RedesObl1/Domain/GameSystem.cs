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
            if(Games.Find(g => g.Title.Equals(game.Title)) == null){
                Games.Add(game);
            }
            else{
                throw new Exception("Un juego con este título ya existe.");
            }

            
        }

        public void DeleteGame(Game game){
            if(Games.Find(g => g.Title.Equals(game.Title)) != null){
                Games.RemoveAll(g => g.Title.Equals(game.Title));
            }
            else{
                throw new Exception("El juego ya no se encuentra en el sistema.");
            }
        }

        public User AddUser(string userName){
            User newUser = new User() { Name = userName };
            Users.Add(newUser);
            return newUser;
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

         public string EncodeUsers(){
            return JsonSerializer.Serialize(this.Users);
        }

        public static string EncodeUsers(List<User> users){
            return JsonSerializer.Serialize(users);
        }

        public static List<User> DecodeUsers(string jsonString){
            return JsonSerializer.Deserialize<List<User>>(jsonString);
        }
    }
}