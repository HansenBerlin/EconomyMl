using EconomyBase.Enums;

namespace EconomyBase.Controller
{



    public interface IEpisodeUpdateController
    {
        void DailyData(UpdateEpisodeType type);
        void MontlyData(UpdateEpisodeType type, List<decimal> data);
        void QuarterlyData(UpdateEpisodeType type);
        void YearlyData(UpdateEpisodeType type);
    }
}