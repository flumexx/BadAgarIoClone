using System;
using System.Collections.Generic;
using System.Text;

namespace HonorServer
{
    static class ParameterMap
    {
        public static string Stringify(params string[] keysAndValues)
        {
            string result = "";

            if (keysAndValues.Length % 2 != 0)
            {
                throw new ArgumentException("A key value pair is missing a value.");
            }

            for (int i = 0; i < keysAndValues.Length; i += 2)
            {
                if (i != 0)
                {
                    result += "&";
                }

                result += keysAndValues[i] + "=" + keysAndValues[i + 1];
            }

            return result;
        }

        public static string Stringify(Dictionary<string, string> map)
        {
            string result = "";

            foreach (string key in map.Keys)
            {
                if (result.Length != 0)
                {
                    result += "&";
                }

                result += key + "=" + map[key];
            }

            return result;
        }

        public static Dictionary<string, string> Parse(string str)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();

            string[] pairs = str.Split("&");

            foreach (string pair in pairs)
            {
                string[] parts = pair.Split("=");

                if (parts.Length == 2)
                {
                    map[parts[0].ToLower()] = parts[1];
                }
            }

            return map;
        }
    }
}
