using NewScripts;

namespace NewScripts
{
    public static class ProductTemplateFactory
    {
        public static int CompanysPerType { get; set; } = 3;
        public static ProductTemplate Create(ProductType type)
        {
            float defaultPrice = 0.33F;
            int workerSalary = 300;
            int startWorkerCount = 30 / CompanysPerType;
            int unitsPerWorker = 30;
            int startCapital = 333 / CompanysPerType * 12;
            int avgConsumptionFromAllWorkers = 10000 / CompanysPerType;
            ProductType input = ProductType.None;
            
            if (type == ProductType.Intermediate)
            {
                defaultPrice = 10;
                workerSalary = 600;
                unitsPerWorker = 2;
                startCapital = 666 / CompanysPerType * 12;
                avgConsumptionFromAllWorkers = 666 / CompanysPerType;
            }
            if (type == ProductType.Luxury)
            {
                defaultPrice = 25;
                workerSalary = 900;
                unitsPerWorker = 2;
                startCapital = 1650 / CompanysPerType * 12;
                input = ProductType.Intermediate;
                avgConsumptionFromAllWorkers = 666 / CompanysPerType;
            }

            return new ProductTemplate(new Product
                {
                    ProductTypeInput = input,
                    ProductTypeOutput = type,
                    //Price = defaultPrice
                },
                workerSalary, unitsPerWorker, defaultPrice, startCapital, avgConsumptionFromAllWorkers, startWorkerCount);
        }
    }

    public class ProductTemplate
    {
        public ProductTemplate(Product product, int workerSalary, int unitsPerWorker, 
            float defaultPrice, float startCapital, int averageConsumptionPerCompany, int startWorkerCount)
        {
            Product = product;
            WorkerSalary = workerSalary;
            UnitsPerWorker = unitsPerWorker;
            DefaultPrice = defaultPrice;
            StartCapital = startCapital;
            AverageConsumptionPerCompany = averageConsumptionPerCompany;
            StartWorkerCount = startWorkerCount;
        }

        public Product Product { get; }
        public float DefaultPrice { get; }
        public float StartCapital { get; }
        public int WorkerSalary { get; }
        public int StartWorkerCount { get; }
        public int AverageConsumptionPerCompany { get; }
        public int UnitsPerWorker { get; }
    }
}