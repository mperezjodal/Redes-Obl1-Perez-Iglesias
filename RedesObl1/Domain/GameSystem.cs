using System;
using System.Collections.Generic;

namespace Domain
{
    public class GameSystem
    {
        public List<Game> Games { get; set; }
        public List<Game> GamesBeingModified { get; set; }
        public List<User> Users { get; set; }

        public GameSystem()
        {
            Games = new List<Game>();
            GamesBeingModified = new List<Game>();
            Users = new List<User>();
        }

        public void AddGameBeingModified(Game game)
        {
            if (GamesBeingModified.Find(g => g.Title.Equals(game.Title)) == null)
            {
                GamesBeingModified.Add(game);
            }
            else
            {
                throw new Exception("Un juego con este ya está siendo modificado.");
            }
        }

        public void AddGame(Game game)
        {
            if (Games.Find(g => g.Title.Equals(game.Title)) == null)
            {
                if(game.Title != ""){
                    Games.Add(game);
                }
                else
                {
                    throw new Exception("El nombre del juego no puede ser vacío.");
                }
            }
            else
            {
                throw new Exception("Error, no se pudo insertar juego. Un juego con este título ya existe.");
            }
        }

        public void DeleteGameBeingModified(Game game)
        {
            if (GamesBeingModified.Find(g => g.Title.Equals(game.Title)) != null)
            {
                GamesBeingModified.RemoveAll(g => g.Title.Equals(game.Title));
            }
            else
            {
                throw new Exception("El juego ya no se encuentra en el sistema.");
            }
        }

        public void DeleteGame(Game game)
        {
            if (Games.Find(g => g.Title.Equals(game.Title)) != null)
            {
                foreach(User user in Users)
                {
                    user.Games.RemoveAll(g => g.Title == game.Title);
                }
                Games.RemoveAll(g => g.Title.Equals(game.Title));
            }
            else
            {
                throw new Exception("El juego ya no se encuentra en el sistema.");
            }
        }

        public Game UpdateGame(Game oldGame, Game newGame)
        {
            oldGame.Update(newGame);
            foreach(User user in Users)
            {
                if(user.Games.FindIndex(g => g.Title == oldGame.Title) != -1)
                {
                    user.Games.RemoveAll(g => g.Title == oldGame.Title);
                    user.AcquireGame(newGame);
                }
            }
            return newGame;
        }
        public User UpdateUser(User oldUser, User newUser)
        {
            oldUser.Update(newUser);
            return newUser;
        }

        public void UpdateReviews(Game game, List<Review> reviews)
        {
            game.UpdateReviews(reviews);
            foreach(User user in Users)
            {
                if(user.Games.FindIndex(g => g.Title == game.Title) != -1)
                {
                    user.Games.RemoveAll(g => g.Title == game.Title);
                    user.AcquireGame(game);
                }
            }
        }

        public bool GameExists(Game game)
        {
            return Games.FindIndex(g => g.Title == game.Title) != -1;
        }

        public bool IsGameBeingModified(Game game)
        {
            return GamesBeingModified.FindIndex(g => g.Title == game.Title) != -1;
        }

        public User AddUser(string userName)
        {
            if(userName != ""){
                User newUser = new User() { Name = userName };
                Users.Add(newUser);
                return newUser;
            }
            else{
                throw new Exception("El nombre del usuario no puede ser vacío.");
            }
            
        }

        public string EncodeGames()
        {
            return CustomEncoder.EncodeList(Games, Game.GameListSeparator);
        }

        public static string EncodeGames(List<Game> games)
        {
            return CustomEncoder.EncodeList(games, Game.GameListSeparator);
        }

        public static List<Game> DecodeGames(string jsonString)
        {
            List<Game> games = new List<Game>();
            List<string> gamesData = CustomEncoder.Decode(jsonString, Game.GameListSeparator);
            foreach (string game in gamesData)
            {
                games.Add(Game.Decode(game));
            }

            return games;
        }

        public void LoginUser(string user)
        {
            Users.Find(u => u.Name.Equals(user)).Login = true;
        }

        public void LogoutUser(string user)
        {
            Users.Find(u => u.Name.Equals(user)).Login = false;
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
            foreach (string user in usersData)
            {
                users.Add(User.Decode(user));
            }

            return users;
        }
    }
}