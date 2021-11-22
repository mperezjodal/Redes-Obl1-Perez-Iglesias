using System;
using System.Collections.Generic;
using System.Text.Json;
using Domain;

namespace GRPCLibrary
{
    public class ProtoBuilder
    {
        public static Game Game(GameModel gm) {
            Game game = new Game() { 
                Id = gm.Id, 
                Title = gm.Title, 
                Genre = gm.Genre, 
                Reviews = new List<Review>(),
                Rating = gm.Rating,
                Synopsis = gm.Synopsis,
                Cover = gm.Cover
            };
            foreach (ReviewModel review in gm.Reviews)
            {
                game.Reviews.Add(new Review() { 
                    Id = review.Id, 
                    Comment = review.Comment, 
                    Rating = review.Rating
                });
            }

            return game;
        }
        public static Game Game(GameModifyModel gm) {
            Game game = new Game() { 
                Id = gm.Id, 
                Title = gm.Title, 
                Genre = gm.Genre, 
                Reviews = new List<Review>(),
                Rating = gm.Rating,
                Synopsis = gm.Synopsis,
                Cover = gm.Cover
            };
            foreach (ReviewModel review in gm.Reviews)
            {
                game.Reviews.Add(new Review() { 
                    Id = review.Id, 
                    Comment = review.Comment, 
                    Rating = review.Rating
                });
            }
            return game;
        }

        public static GameModel GameModel(Game game) {
            GameModel gm = new GameModel() { 
                Id = game.Id, 
                Title = game.Title, 
                Genre = game.Genre,
                Rating = game.Rating,
                Synopsis = game.Synopsis
            };
            if (game.Cover != null)
            {
                gm.Cover = game.Cover;
            }
            foreach (Review review in game.Reviews)
            {
                gm.Reviews.Add(new ReviewModel() { 
                    Id = review.Id, 
                    Comment = review.Comment, 
                    Rating = review.Rating
                });
            }

            return gm;
        }

        public static GameModel GameModel(GameModifyModel game) {
            GameModel gm = new GameModel() { 
                Id = game.Id, 
                Title = game.Title, 
                Genre = game.Genre,
                Rating = game.Rating,
                Synopsis = game.Synopsis
            };
            if (game.Cover != null)
            {
                gm.Cover = game.Cover;
            }
            foreach (ReviewModel review in game.Reviews)
            {
                gm.Reviews.Add(new ReviewModel() { 
                    Id = review.Id, 
                    Comment = review.Comment, 
                    Rating = review.Rating
                });
            }

            return gm;
        }

        public static GameModifyModel GameModifyModel(Game game, string titleGameToModify) {
            GameModifyModel gm = new GameModifyModel() { 
                Id = game.Id, 
                Title = game.Title, 
                Genre = game.Genre,
                Rating = game.Rating,
                Synopsis = game.Synopsis,
                TitleGameToModify = titleGameToModify
            };
            if (game.Cover != null)
            {
                gm.Cover = game.Cover;
            }
            foreach (Review review in game.Reviews)
            {
                gm.Reviews.Add(new ReviewModel() { 
                    Id = review.Id, 
                    Comment = review.Comment, 
                    Rating = review.Rating
                });
            }

            return gm;
        }

        public static GameModel GameModel(Game game, string username)
        {
            GameModel gm = GameModel(game);
            gm.User = username;
            return gm;
        }

        public static List<Game> Games(GamesModel gms) {
            List<Game> games = new List<Game>();
            foreach (GameModel gm in gms.Games)
            {
                games.Add(Game(gm));
            }
            return games;
        }

        public static GamesModel GamesModel(List<Game> games) {
            GamesModel gms = new GamesModel();
            foreach (Game game in games)
            {
                gms.Games.Add(GameModel(game));
            }
            return gms;
        }

        public static GamesModel GamesModel(List<Game> games, string username) {
            GamesModel gms = GamesModel(games);
            gms.Games[0].User = username;
            return gms;
        }

        public static User User(UserModel um) {
            User user = new User() { 
                Id = um.Id, 
                Name = um.Name, 
                Login = um.Login,
                Games = new List<Game>()
            };
            foreach (GameModel gm in um.Games)
            {
                user.Games.Add(Game(gm));
            }

            return user;
        }

        
        public static UserModel UserModel(User user) {
            UserModel um = new UserModel() { 
                Id = user.Id,
                Name = user.Name,
                Login = user.Login
            };
            foreach (Game game in user.Games)
            {
                um.Games.Add(GameModel(game));
            }
            return um;
        }

        public static UserModifyModel UserModifyModel(User user, string NameUserToModify) {
            UserModifyModel um = new UserModifyModel() { 
                Id = user.Id,
                Name = user.Name,
                Login = user.Login,
                NameUserToModify = NameUserToModify
                
            };
            foreach (Game game in user.Games)
            {
                um.Games.Add(GameModel(game));
            }
            return um;
        }

        public static User User(UserModifyModel user) {
            User um = new User() { 
                Id = user.Id,
                Name = user.Name,
                Login = user.Login
            };
            foreach (GameModel game in user.Games)
            {
                um.Games.Add(ProtoBuilder.Game(game));
            }
            return um;
        }

        public static List<User> Users(UsersModel ums) {
            List<User> users = new List<User>();
            foreach (UserModel um in ums.Users)
            {
                users.Add(User(um));
            }
            return users;
        }

        public static UsersModel UsersModel(List<User> users) {
            UsersModel ums = new UsersModel();
            foreach (User user in users)
            {
                ums.Users.Add(UserModel(user));
            }
            return ums;
        }

        public static Review Review(ReviewModel rm) {
            return new Review() { 
                Id = rm.Id,
                Comment = rm.Comment,
                Rating = rm.Rating
            };
        }

        public static ReviewModel ReviewModel(Review review) {
            return new ReviewModel() {
                Id = review.Id,
                Comment = review.Comment,
                Rating = review.Rating
            };
        }

        public static List<Review> GetReviews(GameModel gm) {
            List<Review> reviews = new List<Review>();
            foreach (ReviewModel rm in gm.Reviews)
            {
                reviews.Add(Review(rm));
            }
            return reviews;
        }
    }
}
