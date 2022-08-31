using System.Collections.Generic;
using System.Linq;
using Controller;
using Controller.Rewards;
using Models.Agents;
using Models.Observations;
using Models.Population;
using Settings;

namespace Factories
{



    public class PopulationFactory
    {
        private int ParentMinimumAge => _policies.AgeBoundaries.AdultMinAge;
        private AgeBoundaryPolicy AgePolicy => _policies.AgeBoundaries;
        private readonly PoliciesWrapper _policies;
        private readonly PopulationPropabilityController _propabilityController;
        private readonly JobMarketController _jobMarketController;
        private ActionsFactory _actionsFactory;
        private readonly List<int> _distributionOfAges;


        public PopulationFactory(PoliciesWrapper policies, PopulationPropabilityController propabilityController, JobMarketController jobMarketController)
        {
            _policies = policies;
            _propabilityController = propabilityController;
            _distributionOfAges = propabilityController.AgeDistribution;
            _jobMarketController = jobMarketController;

        }

        public void SetupActions(ActionsFactory actionsFactory)
        {
            _actionsFactory = actionsFactory;
            
        }

        private int FindMatchingGenerationBucketIndex(int age)
        {
            return age switch
            {
                < 18 => 0,
                >= 18 and < 46 => 1,
                >= 46 and < 74 => 2,
                _ => 3
            };
        }

        public List<IPersonBase> CreateInitialPopulation()
        {
            List<IPersonBase> persons = new();
            List<IPersonBase>[] buckets = CreateBuckets();

            var fbc = new List<IPersonBase>(buckets[0]);
            var fpb = new List<IPersonBase>(buckets[1]);
            var firstBucketChildren = AssignParentsToChildren(fpb, fbc);

            var sbc = new List<IPersonBase>(buckets[1]);
            sbc.AddRange(fbc);
            var tbc = new List<IPersonBase>(buckets[2]);
            var secondBucketChildren = AssignParentsToChildren(tbc, sbc);

            var spb = new List<IPersonBase>(buckets[2]);
            spb.AddRange(sbc);
            var tpb = new List<IPersonBase>(buckets[3]);
            var thirdfirstBucketChildren = AssignParentsToChildren(tpb, spb);

            persons.AddRange(firstBucketChildren);
            persons.AddRange(secondBucketChildren);
            persons.AddRange(thirdfirstBucketChildren);

            spb.AddRange(buckets[3]);

            foreach (var childLeft in spb)
            {
                childLeft.AddParents("dead", "dead");
                persons.Add(childLeft);
            }

            return persons;

        }

        public IPersonBase FindPersonWithinValueRange(int low, int high, List<IPersonBase> valuesToSearchIn,
            string excludeId)
        {
            double margin = 0;
            List<IPersonBase> matchingValues = new();
            int itts = 0;
            while (matchingValues.Count == 0)
            {
                double min = low - margin;
                var match = new List<IPersonBase>(valuesToSearchIn.Where(x => x.Age <= high && x.Age >= min));
                if (match != null)
                {
                    matchingValues.AddRange(match);
                    matchingValues = matchingValues.Where(p => p.Id != excludeId).ToList();
                    if (matchingValues.Count == 0)
                        return null;
                }

                margin += 0.5;
                itts++;
                if (itts > 100)
                    return null;
            }

            var randIndex = StatisticalDistributionController.CreateRandom(0, matchingValues.Count);
            return matchingValues[randIndex];
        }

        private List<IPersonBase> AssignParentsToChildren(List<IPersonBase> parentBucket, List<IPersonBase> childBucket)
        {
            List<IPersonBase> finalPersonsFromBucket = new();
            for (int i = parentBucket.Count - 1; i >= 0; i--)
            {
                if (childBucket.Count == 0)
                {
                    break;
                }

                IPersonBase parentOne = parentBucket[i];

                double childCount = (double) StatisticalDistributionController.ReproductionRate();
                int childCountRounded = ArithmeticController.RoundToInt(childCount);
                var potentialPartners = FindMatchingSecondParent(parentBucket, parentOne);
                IPersonBase parentTwo;
                if (potentialPartners.Count > 0)
                {
                    parentTwo = potentialPartners.First();
                    i--;
                }
                else
                {
                    parentTwo = new PersonDummy();
                }

                int youngerParentAge = parentOne.Age < parentTwo.Age ? parentOne.Age : parentTwo.Age;
                var potentialChildren = FindMatchingChildren(childBucket, youngerParentAge);
                int maxRange = childCountRounded > potentialChildren.Count
                    ? potentialChildren.Count
                    : childCountRounded;
                var children = potentialChildren.GetRange(0, maxRange);

                foreach (var c in children)
                {
                    c.AddParents(parentOne.Id, parentTwo.Id);
                    parentOne.AddChild(c);
                    parentTwo.AddChild(c);
                    childBucket.Remove(c);
                    finalPersonsFromBucket.Add(c);
                }

                parentBucket.Remove(parentOne);
                parentBucket.Remove(parentTwo);
            }



            return finalPersonsFromBucket;
        }

        private List<IPersonBase> FindMatchingChildren(IEnumerable<IPersonBase> bucket, int parentAge)
        {
            var potentialChildren = bucket.Where(
                p => p.Age <= parentAge - 18 && p.Age >= parentAge - 50).ToList();
            return potentialChildren.Count > 0 ? potentialChildren : new List<IPersonBase>();

        }

        private List<IPersonBase> FindMatchingSecondParent(IEnumerable<IPersonBase> bucket, IPersonBase parentOne)
        {
            var potentialPartners = bucket.Where(
                p => p.Age >= parentOne.Age - 5 && p.Age <= parentOne.Age + 5 && p != parentOne).ToList();
            return potentialPartners.Count > 0 ? potentialPartners : new List<IPersonBase>();

        }


        private List<IPersonBase>[] CreateBuckets()
        {
            List<IPersonBase>[] generationBuckets = {new(), new(), new(), new()};
            foreach (var age in _distributionOfAges)
            {
                var person = age >= ParentMinimumAge ? CreateAdult(age, "", "") : CreateChild(age, "", "");

                int bucketIndex = FindMatchingGenerationBucketIndex(age);
                generationBuckets[bucketIndex].Add(person);
            }

            return generationBuckets;
        }


        private PersonAgent CreateAdult(int age, string parentAId, string parentBId)
        {
            var financialData = _propabilityController.InitialIncomeAndCapital(age);
            decimal income = financialData[0];
            //Console.WriteLine(income);
            decimal capital = financialData[1];

            var observations = new PersonObservations(age, income, capital, _policies, _jobMarketController);
            var personController = new PersonController(_policies, _actionsFactory);
            var rewardController = new PersonRewardController(observations);
            var person = new PersonAgent(parentAId, parentBId, observations, personController, rewardController);
            return person;
        }

        public PersonAgent CreateChild(int age, string parentAId, string parentBId)
        {
            var observations = new PersonObservations(age, 0, 0, _policies, _jobMarketController);
            var personController = new PersonController(_policies, _actionsFactory);
            var rewardController = new PersonRewardController(observations);
            var person = new PersonAgent(parentAId, parentBId, observations, personController, rewardController);
            return person;
        }
    }
}