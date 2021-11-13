using System;
using System.Collections.Generic;
using System.IO;

namespace Domain
{
    public class Game : Encodable
    {
        public static string GameSeparator = "~";
        public static string GameListSeparator = "%";
        public int Id { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public List<Review> Reviews { get; set; }
        public int Rating { get; set; }
        public string Synopsis { get; set; }
        public string Cover { get; set; }

        public void AddReview(Review newReview)
        {
            this.Reviews.Add(newReview);
            int totalRating = 0;
            int cont = 0;
            foreach (Review r in this.Reviews)
            {
                cont++;
                totalRating += r.Rating;
            }
            this.Rating = totalRating / cont;
        }

        public void UpdateReviews(List<Review> newReviewList)
        {
            this.Reviews = newReviewList;
            int totalRating = 0;
            int cont = 0;
            foreach (Review r in this.Reviews)
            {
                cont++;
                totalRating += r.Rating;
            }
            this.Rating = totalRating / cont;
        }

        public void Update(Game newGame)
        {
            if (newGame.Title != "")
            {
                this.Title = newGame.Title;
            }
            if(newGame.Cover != "" && File.Exists(newGame.Cover)){
                this.Cover = newGame.Cover;
            }
            if (newGame.Synopsis != "")
            {
                this.Synopsis = newGame.Synopsis;
            }
            if (newGame.Genre != "")
            {
                this.Genre = newGame.Genre;
            }
        }

        public string Encode()
        {
            List<string> data = new List<string>() { Id.ToString(), Rating.ToString(), Title, Genre, Synopsis, Cover, CustomEncoder.EncodeList(Reviews, Review.ReviewListSeparator) };
            return CustomEncoder.Encode(data, GameSeparator);
        }

        public static Game Decode(string dataString)
        {
            List<string> data = CustomEncoder.Decode(dataString, GameSeparator);

            List<Review> reviews = new List<Review>();
            List<string> reviewsData = CustomEncoder.Decode(data[6], Review.ReviewListSeparator);
            foreach(string rev in reviewsData)
            {
                reviews.Add(Review.Decode(rev));
            }

            return new Game()
            {
                Id = Int32.Parse(data[0]),
                Rating = Int32.Parse(data[1]),
                Title = data[2],
                Genre = data[3],
                Synopsis = data[4],
                Cover = data[5],
                Reviews = reviews
            };
        }
    }

    public class GameModify{
        public string Username { get; set; }
        public string GameTitle { get; set; }
    }
}