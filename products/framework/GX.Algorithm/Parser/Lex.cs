using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GX.Algorithm.Parser
{
    interface ILex<TToken>
    {
        TToken ReadToken(string source, ref int position);
    }
}
