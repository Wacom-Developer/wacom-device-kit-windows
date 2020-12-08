using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace WintabDN
{
    // \cond IGNORED_BY_DOXYGEN
    // This is a dummy class to test doing P/Invoke.
    // TODO: remove this class
    public class TestOnly
    {
        // You NEED the Cdecl attribute to avoid getting PInvokeStackImbalance error.
        [DllImport("msvcrt.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int puts(
        [MarshalAs(UnmanagedType.LPStr)] string m);

        [DllImport("msvcrt.dll")]
        public static extern int _flushall();
    }
    // \endcond IGNORED_BY_DOXYGEN

}
