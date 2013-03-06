using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GX.IO
{
    public static class Formater
    {
        static string[] SizeUnit = { "B", "KB", "MB", "GB", "TB" };
        public static string FormatSize(this long size)
        {
            int unit = 0;
            double dsize = size;
            while (dsize > 1024 && unit < 4)
            {
                dsize = dsize / 1024.0;
                ++unit;
            }

            return string.Format("{0:G4}{1}", dsize, SizeUnit[unit]);
        }
    }
}
