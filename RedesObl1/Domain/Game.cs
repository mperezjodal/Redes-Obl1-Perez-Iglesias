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
            int totalRating = 0;
            int cont = 0;
            foreach (Review r in this.Reviews)
            {
                cont++;
                totalRating += r.Rating;
            }
            this.Rating = totalRating / cont;
        }

        public void UpdateReviews(List<Review> newReviewList){
            this.Reviews = newReviewList;
        }

        public void Update(Game newGame){
            if(newGame.Title != ""){
                this.Title = newGame.Title;
            }
            if(newGame.Cover != ""){
                this.Cover = newGame.Cover;
            }
            if(newGame.Synopsis != ""){
                this.Synopsis = newGame.Synopsis;
            }
            if(newGame.Genre != ""){
                this.Genre = newGame.Genre;
            }
        }

        public string Encode(){
            return JsonSerializer.Serialize(this);
        }

        public static Game Decode(string jsonString){
            return JsonSerializer.Deserialize<Game>(jsonString);
        }
    }
}