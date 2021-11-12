using System;
using System.Collections.Generic;
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

        public static GameModel GameModel(Game game) {
            GameModel gm = new GameModel() { 
                Id = game.Id, 
                Title = game.Title, 
                Genre = game.Genre,
                Rating = game.Rating,
                Synopsis = game.Synopsis,
                Cover = game.Cover
            };
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
    }
}
