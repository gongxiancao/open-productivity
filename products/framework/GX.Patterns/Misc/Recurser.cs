using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GX.Patterns.Misc
{
    public class Recurser
    {
        public delegate void VoidRecursive<T>(VoidRecursive<T> r, T item);
        public delegate void Recursive<T>(Recursive<T> r, T item);

        public static void Do<T>(T state, Recursive<T> routine)
        {
            routine(routine, state);
        }
    }
}
