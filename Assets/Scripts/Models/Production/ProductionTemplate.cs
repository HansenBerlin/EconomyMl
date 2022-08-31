using Enums;

namespace Models.Production
{



    public class ProductionTemplate : IProductionTemplate
    {
        public ProductType TypeProduced { get; set; }
        public ProductType ResourceTypeNeeded { get; set; }
        public ProductType EnergyTypeNeeded { get; set; }
        public decimal AvailableProductionResources { get; set; }
        public decimal AvailableProductionEnergy { get; set; }
        public decimal EnergyNeededPerPiece { get; set; }
        public decimal ResourceNeededPerPiece { get; set; }
        public decimal BaseCostPerPieceProduced { get; set; }
        public decimal UnitsPerWorker { get; set; }
        public decimal WorkerEfficiencyMultiplier { get; set; }
        public decimal MachineEfficiencyMultiplier { get; set; }
    }
}