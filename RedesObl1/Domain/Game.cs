using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Domain
{
    public class Game
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public List<Review> Reviews { get; set; }
        public int Rating { get; set; }
        public string Synopsis { get; set; }
        public string Cover { get; set; }

        public void AddReview(Review newReview){
            this.Reviews.Add(newReview);
        }

        public void UpdateReviews(List<Review> newReviewList){
            this.Reviews = newReviewList;
        }

        public void Update(Game newGame){
            this.Title = newGame.Title;
            this.Genre = newGame.Genre;
            this.Synopsis = newGame.Synopsis;
            this.Cover = newGame.Cover;
        }

        public string Encode(){
            return JsonSerializer.Serialize(this);
        }

        public static Game Decode(string jsonString){
            return JsonSerializer.Deserialize<Game>(jsonString);
        }
    }
}