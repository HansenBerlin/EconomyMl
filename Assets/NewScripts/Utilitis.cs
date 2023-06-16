using System.Collections.Generic;
using System.Linq;

namespace NewScripts
{
    public static class Utilitis
    {
        
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
                int j = new System.Random().Next(i, length);
                (array[i], array[j]) = (array[j], array[i]);
            }
            
            return maxElements == -1 ? array : array
                .ToList()
                .GetRange(0, maxElements)
                .ToArray();
        }
        
        public static List<T> GenerateRandomLoop<T>(List<T> listToShuffle)
        {
            for (int i = listToShuffle.Count - 1; i > 0; i--)
            {
                var k = new System.Random().Next(i + 1);
                (listToShuffle[k], listToShuffle[i]) = (listToShuffle[i], listToShuffle[k]);
            }
            return listToShuffle;
        }
    }
}