using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest
{
    public static class Tools
    {
        public static string FillCamelSpaces(string str)
        {
            string result = "";
            foreach (char ch in str)
            {
                if (char.IsUpper(ch) && result.Length > 0)
                    result += " ";
                result += ch;
            }
            return result;
        }
    }
}
