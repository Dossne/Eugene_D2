using System.Runtime.CompilerServices;

namespace Infrastructure.Utilities
{
    //read more https://dev.to/sky_noc__16618949385255bd/practical-bitwise-operations-and-bitmasks-in-unity-5h6d
    public static class BitUtils
    {
        /// <summary>
        /// Enable or disable a feature, unlock content, set a status.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint SetBit(uint flags, int i, bool value)
        {
            return value ? SetBit(flags, i) : ClearBit(flags, i);
        }


        /// <summary>
        /// Set bit i to 1.
        /// Use cases: enable a feature, unlock content, set a status.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint SetBit(uint flags, int i)
        {
            return flags | (1u << i);
        }


        /// <summary>
        /// Clear bit i to 0.
        /// Use cases: disable a feature, lock content, remove a status.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ClearBit(uint flags, int i)
        {
            return flags & ~(1u << i);
        }


        /// <summary>
        /// Toggle bit i.
        /// Use cases: on/off toggles, state inversion, UI interactions.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ToggleBit(uint flags, int i)
        {
            return flags ^ (1u << i);
        }


        /// <summary>
        /// Test if bit i is 1.
        /// Use cases: status checks, permission checks, branching.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TestBit(uint bits, int bit)
        {
            return (bits & (1 << bit)) != 0;
        }


        /// <summary>
        /// Clear the lowest set bit (rightmost 1).
        /// Use cases: process active flags one-by-one, priority queues, resource allocation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ClearLowestSetBit(uint x)
        {
            return x &= x - 1;
        }


        /// <summary>
        /// Isolate the lowest set bit (rightmost 1).
        /// Use cases: find the next item to process, bit scan, priority handling.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint IsolateLowestSetBit(uint x)
        {
            return x & (uint)-(int)x;
        }


        /// <summary>
        /// Count number of set bits (population count).
        /// Use cases: count actives, resource usage, degree of fulfillment.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountSetBits(uint x)
        {
            int count = 0;
            while (x != 0)
            {
                x = ClearLowestSetBit(x);
                count++;
            }

            return count;
        }


        /// <summary>
        /// Find index of the lowest set bit.
        /// Use cases: find the first matching index.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FindLowestSetBitIndex(uint x)
        {
            if (x == 0) return -1;

            int index = 0;
            uint isolated = IsolateLowestSetBit(x);
            while (isolated > 1)
            {
                isolated >>= 1;
                index++;
            }

            return index;
        }
    }
}