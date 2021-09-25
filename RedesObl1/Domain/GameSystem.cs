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

        public void AddGame(Game game)
        {
            if(Games.Find(g => g.Title.Equals(game.Title)) == null)
            {
                Games.Add(game);
            }
            else
            {
                throw new Exception("Un juego con este título ya existe.");
            }
        }

        public void DeleteGame(Game game)
        {
            if(Games.Find(g => g.Title.Equals(game.Title)) != null)
            {
                Games.RemoveAll(g => g.Title.Equals(game.Title));
            }
            else{
                throw new Exception("El juego ya no se encuentra en el sistema.");
            }
        }

        public User AddUser(string userName)
        {
            User newUser = new User() { Name = userName };
            Users.Add(newUser);
            return newUser;
        }

        public string EncodeGames()
        {
            return CustomEncoder.EncodeList(Games, Game.GameListSeparator);
        }

        public static string EncodeGames(List<Game> games)
        {
            return CustomEncoder.EncodeList(games, Game.GameListSeparator);
        }

        public static List<Game> DecodeGames(string jsonString){
            List<Game> games = new List<Game>();
            List<string> gamesData = CustomEncoder.Decode(jsonString, Game.GameListSeparator);
            foreach(string game in gamesData)
            {
                games.Add(Game.Decode(game));
            }

            return games;
        }

        public string EncodeUsers()
        {
            return CustomEncoder.EncodeList(Users, User.UserListSeparator);
        }

        public static string EncodeUsers(List<User> users)
        {
            return CustomEncoder.EncodeList(users, User.UserListSeparator);
        }

        public static List<User> DecodeUsers(string jsonString)
        {
            List<User> users = new List<User>();
            List<string> usersData = CustomEncoder.Decode(jsonString, User.UserListSeparator);
            foreach(string user in usersData)
            {
                users.Add(User.Decode(user));
            }

            return users;
        }
    }
}