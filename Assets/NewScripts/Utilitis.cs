using System.Collections.Generic;
using System.Linq;

namespace NewScripts
{
    public static class Utilitis
    {
        private static readonly System.Random Random = new(42);
        
        public static int[] GenerateRandomArray(int startRange, int endRange, int maxElements = -1)
        {
            int length = endRange - startRange;
            int[] array = new int[length];

            for (int i = 0; i < length; i++)
            {
                array[i] = startRange + i;
            }

            
            for (int i = 0; i < length - 1; i++)
            {
                int j = Random.Next(i, length);
                (array[i], array[j]) = (array[j], array[i]);
            }

            int range = maxElements == -1 ? length : maxElements > length ? length : maxElements;

            return array
                .ToList()
                .GetRange(0, range)
                .ToArray();
        }
        
        public static List<T> GenerateRandomLoop<T>(List<T> listToShuffle)
        {
            for (int i = listToShuffle.Count - 1; i > 0; i--)
            {
                var k = Random.Next(i + 1);
                (listToShuffle[k], listToShuffle[i]) = (listToShuffle[i], listToShuffle[k]);
            }
            return listToShuffle;
        }
    }
}