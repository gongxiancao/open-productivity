using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GX.Algorithm.Parser
{
    public class Token<TTokenType>
    {
        public TTokenType Type { get; set; }
        public string Value { get; set; }
        public Token(TTokenType type, string value)
        {
            Type = type;
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("{0},{1}", Type, Value);
        }
    }
}
