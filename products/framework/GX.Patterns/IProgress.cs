using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GX.Patterns
{
    public interface IProgress<T>
    {
        void UpdateProgresss(T progress);
    }
}
