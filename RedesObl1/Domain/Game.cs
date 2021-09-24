using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        }

        public void Update(Game newGame)
        {
            if (newGame.Title != "")
            {
                this.Title = newGame.Title;
            }
            if (newGame.Cover != "")
            {
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
            List<string> reviewsData = CustomEncoder.Decode(data[7], Review.ReviewListSeparator);
            foreach(string rev in reviewsData)
            {
                reviews.Add(Review.Decode(rev));
            }

            return new Game()
            {
                Id = Int32.Parse(data[1]),
                Rating = Int32.Parse(data[2]),
                Title = data[3],
                Genre = data[4],
                Synopsis = data[5],
                Cover = data[6],
                Reviews = reviews
            };
        }
    }
}