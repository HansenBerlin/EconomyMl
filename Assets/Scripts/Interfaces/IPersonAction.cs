using Agents;
using Controller.Agents;

namespace Interfaces
{
    public interface IPersonAction
    {
        void Init(PersonObservations observations, PersonRewardController rewardController, PersonController personController);
    }
}