using System;
using System.Collections.Generic;

namespace Domain
{
    public class Review : Encodable
    {
        public static string ReviewSeparator = "+";
        public static string ReviewListSeparator = "=";
        public int Id { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }

        public string Encode()
        {
            List<string> data = new List<string>() { Id.ToString(), Rating.ToString(), Comment };
            return CustomEncoder.Encode(data, ReviewSeparator);
        }

        public static Review Decode(string dataString)
        {
            List<string> data = CustomEncoder.Decode(dataString, ReviewSeparator);
            return new Review()
            {
                Id = Int32.Parse(data[0]),
                Rating = Int32.Parse(data[1]),
                Comment = data[2]
            };
        }
    }
}