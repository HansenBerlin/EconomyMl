using System.Collections.Generic;
using Controller;
using Enums;
using Models.Market;
using Models.Meta;
using Models.Production;
using Repositories;
using Settings;

namespace Factories
{

    public static class ProductionFactory
    {
        public static IProductionTemplate CreateTemplate(ProductType type)
        {
            var production = new ProductionTemplate();

            if (type == ProductType.FossileEnergy)
            {
                production.EnergyNeededPerPiece = 0;
                production.ResourceNeededPerPiece = 0;
                production.TypeProduced = type;
                production.ResourceTypeNeeded = ProductType.None;
                production.EnergyTypeNeeded = ProductType.None;
                production.UnitsPerWorker = 5000;
                production.BaseCostPerPieceProduced = 0.1M;
                production.WorkerEfficiencyMultiplier = 1;
                production.MachineEfficiencyMultiplier = 1;
                production.AvailableProductionEnergy = 0;
                production.AvailableProductionResources = 0;
                //production.Product = CreateProduct(type, stats, env);
            }
            else if (type == ProductType.BaseProduct)
            {
                production.EnergyNeededPerPiece = 1;
                production.ResourceNeededPerPiece = 0;
                production.TypeProduced = type;
                production.ResourceTypeNeeded = ProductType.None;
                production.EnergyTypeNeeded = ProductType.FossileEnergy;
                production.UnitsPerWorker = 250;
                production.BaseCostPerPieceProduced = 0.5M;
                production.WorkerEfficiencyMultiplier = 1;
                production.MachineEfficiencyMultiplier = 1;
                production.AvailableProductionEnergy = 1000;
                production.AvailableProductionResources = 0;
                //production.Product = CreateProduct(type, stats, env);
            }
            else if (type == ProductType.IntermediateProduct)
            {
                production.EnergyNeededPerPiece = 10;
                production.ResourceNeededPerPiece = 0;
                production.TypeProduced = type;
                production.ResourceTypeNeeded = ProductType.None;
                production.EnergyTypeNeeded = ProductType.FossileEnergy;
                production.UnitsPerWorker = 125;
                production.BaseCostPerPieceProduced = 2;
                production.WorkerEfficiencyMultiplier = 1;
                production.MachineEfficiencyMultiplier = 1;
                production.AvailableProductionEnergy = 10000;
                production.AvailableProductionResources = 0;
                //production.Product = CreateProduct(type, stats, env);
            }
            else if (type == ProductType.LuxuryProduct)
            {
                production.EnergyNeededPerPiece = 5;
                production.ResourceNeededPerPiece = 5;
                production.TypeProduced = type;
                production.ResourceTypeNeeded = ProductType.IntermediateProduct;
                production.EnergyTypeNeeded = ProductType.FossileEnergy;
                production.UnitsPerWorker = 50;
                production.BaseCostPerPieceProduced = 5;
                production.WorkerEfficiencyMultiplier = 1;
                production.MachineEfficiencyMultiplier = 1;
                production.AvailableProductionEnergy = 100;
                production.AvailableProductionResources = 100;
                //production.Product = CreateProduct(type, stats, env);
            }
            else if (type == ProductType.FederalService)
            {
                production.EnergyNeededPerPiece = 2;
                production.ResourceNeededPerPiece = 2;
                production.TypeProduced = type;
                production.ResourceTypeNeeded = ProductType.IntermediateProduct;
                production.EnergyTypeNeeded = ProductType.FossileEnergy;
                production.UnitsPerWorker = 100;
                production.BaseCostPerPieceProduced = 2;
                production.WorkerEfficiencyMultiplier = 5000;
                production.MachineEfficiencyMultiplier = 5000;
                //production.Product = CreateProduct(type, stats, env);
            }

            return production;
        }

        public static List<ProductMarketModel> CreateMarkets(StatisticalDataRepository stats, EnvironmentModel env)
        {
            ProductType[] products =
            {
                ProductType.Energy, ProductType.FederalService, ProductType.FossileEnergy, ProductType.BaseProduct,
                ProductType.GreenEnergy, ProductType.IntermediateProduct, ProductType.LuxuryProduct,
                ProductType.PrivateService
            };
            List<ProductMarketModel> markets = new();
            foreach (var pr in products)
            {
                string id = IdGenerator.Create(env.Month, env.CountryName, pr);
                var dataRepo = new ProductMarketDataRepository(id);
                stats.AddProductMarketDataset(dataRepo);
                markets.Add(new ProductMarketModel(pr, dataRepo, CreateTemplate(pr)));
            }

            return markets;
        }

        public static ProductModel CreateProductModel(ProductType type, StatisticalDataRepository stats,
            EnvironmentModel env)
        {
            string id = IdGenerator.Create(env.Month, env.CountryName, type);
            var dataRepo = new ProductDataRepository(id);
            stats.AddProductDataset(dataRepo);
            return type switch
            {
                ProductType.FossileEnergy => new ProductModel(dataRepo, env, type, 0.8M),
                ProductType.BaseProduct => new ProductModel(dataRepo, env, type, 17M),
                ProductType.IntermediateProduct => new ProductModel(dataRepo, env, type, 45M),
                ProductType.LuxuryProduct => new ProductModel(dataRepo, env, type, 400M),
                ProductType.FederalService => new ProductModel(dataRepo, env, type, 100M),
                _ => new ProductModel(null, null, ProductType.None, 0M)
            };
        }

        public static ProductController CreateProductController(ProductModel model)
        {
            var template = CreateTemplate(model.Type);
            return model.Type switch
            {
                ProductType.FossileEnergy => new ProductController(ProductType.FossileEnergy, model, template),
                ProductType.BaseProduct => new ProductController(ProductType.BaseProduct, model, template),
                ProductType.IntermediateProduct => new ProductController(ProductType.IntermediateProduct, model, template),
                ProductType.LuxuryProduct => new ProductController(ProductType.LuxuryProduct, model, template),
                ProductType.FederalService => new ProductController(ProductType.FederalService, model, template),
                _ => new ProductController(ProductType.None, model, template)
            };
        }
    }
}