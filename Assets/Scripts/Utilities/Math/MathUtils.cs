using System.Collections;
using System.Collections.Generic;

namespace Utilities
{
    public struct MathUtils
    {
        public static ulong PairingF(int a, int b)
        {

            ulong k1 = (ulong)(a < 0 ? (a * (-2)) - 1 : 2 * a);
            ulong k2 = (ulong)(b < 0 ? (b * (-2)) - 1 : 2 * b);

            return (k1 + k2) * (k1 + k2 + 1) / 2 + k2 + 1;

        }
    }
}

