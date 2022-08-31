namespace Controller
{



    public static class WorkerAdaptionController
    {
        private static readonly decimal SupplyReductionModifier = 12;

        public static decimal CalculateWorkerModifier(decimal supply, decimal production, decimal sales)
        {
            if (supply == 0 && production == 0 && sales == 0)
                return 1;
            var prodModified = production + (supply - sales) / SupplyReductionModifier;
            if (prodModified == 0)
                return 1;
            //var prodModified = production;
            var reduceProd = (sales - prodModified) / prodModified;
            //var reducedWorkers = prodModified > sales ? 1 + reduceProd : reduceProd;
            return 1 + reduceProd;
        }

        public static decimal CalculateCapacityModifier(decimal capacityUsed)
        {
            decimal reductionRateCapacityUsed = 1;
            if (capacityUsed < 0.8M)
            {
                reductionRateCapacityUsed = capacityUsed * 1.2M;
            }

            return reductionRateCapacityUsed;
        }
    }
}