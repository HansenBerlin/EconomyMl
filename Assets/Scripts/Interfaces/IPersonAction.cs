using Controller.Agents;
using Models.Observations;

namespace Interfaces
{
    public interface IPersonAction
    {
        void Init(PersonObservations observations, PersonRewardController rewardController, PersonController personController);
    }
}