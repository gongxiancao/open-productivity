using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GX.Algorithm.Parser
{
    public class CommandLineLex : ILex<CommandLineLex.Token>
    {
        public enum TokenType
        {
            ArgumentName,
            String,
            Colon,
            EOF
        }

        public class Token : Token<TokenType>
        {
            public Token(TokenType tokenType, string value)
                : base(tokenType, value)
            {
            }
        }

        public Token ReadToken(string source, ref int position)
        {
            int length = source.Length;
            while (position < length && char.IsSeparator(source[position]))
            {
                ++position;
            }

            if (position >= length)
            {
                return new Token(TokenType.EOF, null);
            }

            char ch = source[position];

            if (ch == '/' || ch == '-')
            {
                int start = position + 1;
                while (++position < length)
                {
                    ch = source[position];
                    if (!(char.IsLetterOrDigit(ch) || ch == '_'))
                    {
                        break;
                    }
                }
                return new Token(TokenType.ArgumentName, source.Substring(start, position - start));
            }
            if (char.IsLetterOrDigit(ch))
            {
                int start = position;
                while (++position < length)
                {
                    ch = source[position];
                    if (char.IsSeparator(ch))
                    {
                        break;
                    }
                }
                return new Token(TokenType.String, source.Substring(start, position - start));
            }
            if (ch == '"')
            {
                StringBuilder value = new StringBuilder(length);
                while (++position < length)
                {
                    ch = source[position];
                    if (ch == '"')
                    {
                        ++position;
                        if (position < length)
                        {
                            ch = source[position];
                            if (ch == '"')
                            {
                                value.Append(ch);
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        value.Append(ch);
                    }
                }

                return new Token(TokenType.String, value.ToString());
            }
            if (ch == ':')
            {
                ++position;
                return new Token(TokenType.Colon, ":");
            }
            throw new ArgumentException(string.Format("Invalid source: {0}, position: {1}", source, position), "source");
        }

    }
}
