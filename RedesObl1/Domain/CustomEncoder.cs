using System.Linq;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace Domain
{
    public class CustomEncoder
    {
        public static string Encode(List<string> list, string separator)
        {
            string encodedData = "";
            foreach (string item in list)
            {
                encodedData += separator;
                encodedData += item;
            }
            return encodedData;
        }

        public static List<string> Decode(string encodedData, string separator)
        {
            return encodedData.Split(separator).ToList();
        }

        public static string EncodeList<T>(IList<T> list, string separator) where T : Encodable
        {
            string encodedData = "";
            foreach (Encodable item in list)
            {
                encodedData += separator;
                encodedData += item.Encode();
            }
            return encodedData;
        }

        public static List<string> DecodeList(string encodedData, string separator)
        {
            return encodedData.Split(separator).ToList();
        }
    }

    public interface Encodable
    {
        public string Encode();
    }
}