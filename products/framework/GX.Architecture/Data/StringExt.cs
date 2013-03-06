using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GX.Architecture.Data
{
    public static class StringExt
    {
        public static IEnumerable<KeyValuePair<string, string>> ParseNameValues(this string input, char delimiter, char indicator)
        {
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            foreach (string nameValue in input.Split(new char[] { delimiter }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = nameValue.Split(new char[] { indicator }, 2);
                if (parts.Length >= 1)
                {
                    string name = parts[0];
                    if (parts.Length > 1)
                    {
                        string value = parts[1];
                        result.Add(new KeyValuePair<string, string>(name, value));
                    }
                }
            }
            return result;
        }
    }
}
