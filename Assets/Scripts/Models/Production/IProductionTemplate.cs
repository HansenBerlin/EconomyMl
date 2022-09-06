using Assets.Scripts.Enums;

namespace Assets.Scripts.Models.Production
{



    public interface IProductionTemplate
    {
        ProductType TypeProduced { get; }
        ProductType ResourceTypeNeeded { get; }
        ProductType EnergyTypeNeeded { get; }
        long AvailableProductionResources { get; set; }
        long AvailableProductionEnergy { get; set; }
        decimal EnergyNeededPerPiece { get; }
        decimal ResourceNeededPerPiece { get; }
        decimal BaseCostPerPieceProduced { get; }
        decimal UnitsPerWorker { get; }
        decimal WorkerEfficiencyMultiplier { get; set; }
        decimal MachineEfficiencyMultiplier { get; set; }
    }
}