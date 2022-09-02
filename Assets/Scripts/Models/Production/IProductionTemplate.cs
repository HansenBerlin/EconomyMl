using Enums;

namespace Models.Production
{



    public interface IProductionTemplate
    {
        ProductType TypeProduced { get; }
        ProductType ResourceTypeNeeded { get; }
        ProductType EnergyTypeNeeded { get; }
        int AvailableProductionResources { get; set; }
        int AvailableProductionEnergy { get; set; }
        decimal EnergyNeededPerPiece { get; }
        decimal ResourceNeededPerPiece { get; }
        decimal BaseCostPerPieceProduced { get; }
        decimal UnitsPerWorker { get; }
        decimal WorkerEfficiencyMultiplier { get; set; }
        decimal MachineEfficiencyMultiplier { get; set; }
    }
}