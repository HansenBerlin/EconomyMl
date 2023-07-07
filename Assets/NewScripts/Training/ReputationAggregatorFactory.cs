using System.Collections.Generic;

namespace NewScripts.Training
{
    public class ReputationAggregatorFactory
    {
        private readonly List<RewardNormalizer> _normalizers = new();
        private ReputationAggregator _reputationAggregator;
        
        public ReputationAggregatorFactory()
        {
            for (int i = 0; i < 5; i++)
            {
                _normalizers.Add(new RewardNormalizer());
            }
        }
        
        public ReputationAggregator Create()
        {
            List<RewardNormalizer> normalizers = new();
            for (int i = 0; i < 5; i++)
            {
                normalizers.Add(new RewardNormalizer());
            }
            return new ReputationAggregator(normalizers);
        }
    }
}