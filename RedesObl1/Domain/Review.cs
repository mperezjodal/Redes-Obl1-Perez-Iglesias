using System.Text.Json;

namespace Domain
{
    public class Review
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }

        public string Encode(){
            return JsonSerializer.Serialize(this);
        }

        public static Review Decode(string jsonString){
            return JsonSerializer.Deserialize<Review>(jsonString);
        }
    }
}