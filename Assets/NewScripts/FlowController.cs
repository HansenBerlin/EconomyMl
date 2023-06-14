using System;

namespace NewScripts
{
    public class FlowController
    {
        public int Year { get; set; } = 1;
        public int Month { get; set; } = 1;
        public int Day { get; set; } = 1;

        public void Increment()
        {
            if (Month == 12)
            {
                Year++;
                Month = 1;
            }
            if (Day == 30)
            {
                Day = 1;
                Month++;
            }
            else
            {
                Day++;
            }
        }

        public void Reset()
        {
            Year = 1;
            Month = 1;
            Day = 1;
        }

        public string Current()
        {
            return $"{Day}.{Month}.{Year}";
        }
    }
}