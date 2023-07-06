using System.Collections.Generic;

namespace NewScripts.Training
{
    public class ReputationAggregatorFactory
    {
        private readonly List<RewardNormalizer> _normalizers = new();
        
        public ReputationAggregatorFactory()
        {
            for (int i = 0; i < 5; i++)
            {
                _normalizers.Add(new RewardNormalizer());
            }
        }
        
        public ReputationAggregator Create()
        {
            return new ReputationAggregator(_normalizers);
        }
    }
}