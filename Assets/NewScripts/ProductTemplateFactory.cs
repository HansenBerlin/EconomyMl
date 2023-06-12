using NewScripts;

namespace NewScripts
{
    public class ProductTemplateFactory
    {
        public static ProductTemplate Create(ProductType type)
        {
            float defaultPrice = 0.33F;
            int workerSalary = 10;
            int unitsPerWorker = 30;
            int startCapital = 45000;
            int avgConsumption = 10000;
            ProductType input = ProductType.None;
            
            if (type == ProductType.Intermediate)
            {
                defaultPrice = 10;
                workerSalary = 20;
                unitsPerWorker = 2;
                startCapital = 90000;
                avgConsumption = 666;
            }
            if (type == ProductType.Luxury)
            {
                defaultPrice = 25;
                workerSalary = 30;
                unitsPerWorker = 2;
                startCapital = 240000;
                input = ProductType.Intermediate;
                avgConsumption = 666;
            }

            return new ProductTemplate(new Product
                {
                    ProductTypeInput = input,
                    ProductTypeOutput = type,
                    Price = defaultPrice
                },
                workerSalary, unitsPerWorker, defaultPrice, startCapital, avgConsumption);
        }
    }

    public class ProductTemplate
    {
        public ProductTemplate(Product product, int workerSalary, int unitsPerWorker, float defaultPrice, float startCapital, int averageConsumption)
        {
            Product = product;
            WorkerSalary = workerSalary;
            UnitsPerWorker = unitsPerWorker;
            DefaultPrice = defaultPrice;
            StartCapital = startCapital;
            AverageConsumption = averageConsumption;
        }

        public Product Product { get; }
        public float DefaultPrice { get; }
        public float StartCapital { get; }
        public int WorkerSalary { get; }
        public int AverageConsumption { get; }
        public int UnitsPerWorker { get; }
    }
}