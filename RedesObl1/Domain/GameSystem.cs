using System;
using System.Collections.Generic;

namespace Domain
{
    public interface IGameSystem
    {
        public List<Game> Games { get; set; }
        public List<GameModify> GamesBeingModified { get; set; }
        public List<User> Users { get; set; }
        public void AddGameBeingModified(Game game, string username);
        public void AddGame(Game game);
        public void AddGameCover(Game game, string cover);
        public void DeleteGameBeingModified(Game game);
        public void DeleteGame(Game game);
        public Game UpdateGame(Game oldGame, Game newGame);
        public User UpdateUser(User oldUser, User newUser);
        public void UpdateReviews(Game game, List<Review> reviews);
        public bool GameExists(Game game);
        public bool IsGameBeingModified(Game game);
        public bool IsGameBeingModifiedByAnother(Game game, string username);
        public User AddUser(string userName);
        public void LoginUser(string user);
        public User LogoutUser(string user);
    }

    public class GameSystem : IGameSystem
    {
        public List<Game> Games { get; set; }
        public List<GameModify> GamesBeingModified { get; set; }
        public List<User> Users { get; set; }

        public GameSystem()
        {
            Games = new List<Game>();
            GamesBeingModified = new List<GameModify>();
            Users = new List<User>();
        }

        public void AddGameBeingModified(Game game, string username)
        {
            if (GamesBeingModified.Find(g => g.GameTitle.Equals(game.Title)) == null)
            {
                GamesBeingModified.Add(new GameModify { GameTitle = game.Title, Username = username });
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
                if (game.Title != "")
                {
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
            if (GamesBeingModified.Find(g => g.GameTitle.Equals(game.Title)) != null)
            {
                GamesBeingModified.RemoveAll(g => g.GameTitle.Equals(game.Title));
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
                foreach (User user in Users)
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
            foreach (User user in Users)
            {
                if (user.Games.FindIndex(g => g.Title == oldGame.Title) != -1)
                {
                    Game g = user.Games.Find(g => g.Title == oldGame.Title);
                    g.Update(newGame);
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
            foreach (User user in Users)
            {
                if (user.Games.FindIndex(g => g.Title == game.Title) != -1)
                {
                    Game g = user.Games.Find(g => g.Title == game.Title);
                    g.Update(game);
                }
            }
        }

        public bool GameExists(Game game)
        {
            return Games.FindIndex(g => g.Title == game.Title) != -1;
        }

        public bool IsGameBeingModified(Game game)
        {
            return GamesBeingModified.FindIndex(g => g.GameTitle == game.Title) != -1;
        }

        public bool IsGameBeingModifiedByAnother(Game game, string username)
        {
            return GamesBeingModified.FindIndex(g => g.GameTitle == game.Title && g.Username != username) != -1;
        }

        public User AddUser(string userName)
        {
            if (userName != "")
            {
                User newUser = new User(userName);
                Users.Add(newUser);
                return newUser;
            }
            else
            {
                throw new Exception("Error, no se pudo insertar usuario. El nombre del usuario no puede ser vacío.");
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
        public User GetLoggedUser()
        {
            return Users.Find(u => u.Login == true);
        }


        public void LoginUser(string user)
        {
            Users.Find(u => u.Name.Equals(user)).Login = true;
        }

        public User LogoutUser(string user)
        {
            Users.Find(u => u.Name.Equals(user)).Login = false;
            return Users.Find(u => u.Name.Equals(user));
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

        public void AddGameCover(Game game, string cover)
        {
            game.Cover = cover;
            foreach (User user in Users)
            {
                if (user.Games.FindIndex(g => g.Title == game.Title) != -1)
                {
                    Game g = user.Games.Find(g => g.Title == game.Title);
                    g.Cover = cover;
                }
            }
        }
    }
}