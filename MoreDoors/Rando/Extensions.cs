using System;
using System.Collections.Generic;

namespace MoreDoors.Rando
{
    public static class Extensions
    {
        public static void Shuffle<T>(this List<T> list, Random r)
        {
            for (int i = 0; i < list.Count - 1; ++i)
            {
                int j = i + r.Next(0, list.Count - i);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}
