using System;

namespace System
{
    public delegate void VoidFunc();
    public delegate void VoidFunc<T>(T state);
    public delegate void VoidFunc<T1, T2>(T1 state1, T2 state2);
    public delegate void VoidFunc<T1, T2, T3>(T1 state1, T2 state2, T3 state3);
}
