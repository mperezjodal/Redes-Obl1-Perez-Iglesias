using System.Text.Json;

namespace Domain
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Encode(){
            return JsonSerializer.Serialize(this);
        }

        public static User Decode(string jsonString){
            return JsonSerializer.Deserialize<User>(jsonString);
        }
    }
}