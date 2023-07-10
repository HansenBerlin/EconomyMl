using System;
using UnityEngine;

namespace NewScripts.DataModelling
{
    public class EquilibrieumPriceCalculator
    {
        public static decimal CalculateIntersectionY(decimal x1, decimal y1, decimal x2, decimal y2, decimal x3, decimal y3, decimal x4, decimal y4)
        {
            decimal yIntersection = 1.5M;
            try
            {
                decimal m1 = (y2 - y1) / (x2 - x1); 
                decimal b1 = y1 - m1 * x1;
                decimal m2 = (y4 - y3) / (x4 - x3);
                decimal b2 = y3 - m2 * x3;
                decimal xIntersection = (b2 - b1) / (m1 - m2);
                yIntersection = m1 * xIntersection + b1;
            }
            catch (Exception e)
            {
                //Debug.Log(e);
            }
            
            return yIntersection;
        }
    }
}