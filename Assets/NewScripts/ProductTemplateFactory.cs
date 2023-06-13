using NewScripts;

namespace NewScripts
{
    public static class ProductTemplateFactory
    {
        public static int CompanysPerType { get; set; } = 3;
        public static ProductTemplate Create(ProductType type)
        {
            float defaultPrice = 0.33F;
            int workerSalary = 10;
            int startWorkerCount = 333 / CompanysPerType;
            int unitsPerWorker = 30;
            int startCapital = 3333 / CompanysPerType * 12;
            int avgConsumptionPerCompany = 10000 / CompanysPerType;
            ProductType input = ProductType.None;
            
            if (type == ProductType.Intermediate)
            {
                defaultPrice = 10;
                workerSalary = 20;
                unitsPerWorker = 2;
                startCapital = 6660 / CompanysPerType * 12;
                avgConsumptionPerCompany = 666 / CompanysPerType;
            }
            if (type == ProductType.Luxury)
            {
                defaultPrice = 25;
                workerSalary = 30;
                unitsPerWorker = 2;
                startCapital = 16650 / CompanysPerType * 12;
                input = ProductType.Intermediate;
                avgConsumptionPerCompany = 666 / CompanysPerType;
            }

            return new ProductTemplate(new Product
                {
                    ProductTypeInput = input,
                    ProductTypeOutput = type,
                    Price = defaultPrice
                },
                workerSalary, unitsPerWorker, defaultPrice, startCapital, avgConsumptionPerCompany, startWorkerCount);
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