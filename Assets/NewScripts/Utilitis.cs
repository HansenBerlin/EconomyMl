using System.Linq;

namespace NewScripts
{
    public static class Utilitis
    {
        public static int[] GenerateRandomArray(int startRange, int endRange, int maxElements = -1)
        {
            int length = endRange - startRange + 1;
            int[] array = new int[length];

            for (int i = 0; i < length; i++)
            {
                array[i] = startRange + i;
            }

            var random = new System.Random();
            for (int i = 0; i < length - 1; i++)
            {
                int j = random.Next(i, length);
                (array[i], array[j]) = (array[j], array[i]);
            }

            return maxElements == -1 ? array : array
                .ToList()
                .GetRange(0, maxElements)
                .ToArray();
        }
        
    }
}