using System;

namespace Domain
{
    public class Game
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public int Rating { get; set; }
        public string Synopsis { get; set; }
        public string Cover { get; set; }
    }
}