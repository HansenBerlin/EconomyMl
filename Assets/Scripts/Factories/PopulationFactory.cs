using System.Collections.Generic;
using System.Linq;
using Controller;
using Models.Agents;
using Models.Market;
using Models.Observations;
using Settings;
using UnityEngine;

namespace Factories
{



    public class PopulationFactory : MonoBehaviour
    {
        private int ParentMinimumAge => _policies.AgeBoundaries.AdultMinAge;
        private PoliciesWrapper _policies;
        private PopulationPropabilityController _propabilityController;
        private List<int> _distributionOfAges;
        private ActionsFactory _actionsFactory;
        private JobMarketController _jobMarketController;
        private ICountryEconomy _markets;
        
        public GameObject personAgentPrefab;
        
        
        public void Init(ActionsFactory factory, JobMarketController jobMarketController, PoliciesWrapper policies, 
            PopulationPropabilityController propabilityController, ICountryEconomy markets)
        {
            _actionsFactory = factory;
            _jobMarketController = jobMarketController;
            _policies = policies;
            _propabilityController = propabilityController;
            _distributionOfAges = _propabilityController.AgeDistribution;
            _markets = markets;
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

        public List<PersonAgent> CreateInitialPopulation()
        {
            List<PersonAgent> persons = new();
            List<PersonAgent>[] buckets = CreateBuckets();

            var fbc = new List<PersonAgent>(buckets[0]);
            var fpb = new List<PersonAgent>(buckets[1]);
            var firstBucketChildren = AssignParentsToChildren(fpb, fbc);

            var sbc = new List<PersonAgent>(buckets[1]);
            sbc.AddRange(fbc);
            var tbc = new List<PersonAgent>(buckets[2]);
            var secondBucketChildren = AssignParentsToChildren(tbc, sbc);

            var spb = new List<PersonAgent>(buckets[2]);
            spb.AddRange(sbc);
            var tpb = new List<PersonAgent>(buckets[3]);
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

        public PersonAgent FindPersonWithinValueRange(int low, int high, List<PersonAgent> valuesToSearchIn,
            string excludeId)
        {
            double margin = 0;
            List<PersonAgent> matchingValues = new();
            int itts = 0;
            while (matchingValues.Count == 0)
            {
                double min = low - margin;
                var match = new List<PersonAgent>(valuesToSearchIn.Where(x => x.Age <= high && x.Age >= min));
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

        private List<PersonAgent> AssignParentsToChildren(List<PersonAgent> parentBucket, List<PersonAgent> childBucket)
        {
            List<PersonAgent> finalPersonsFromBucket = new();
            for (int i = parentBucket.Count - 1; i >= 0; i--)
            {
                if (childBucket.Count == 0)
                {
                    break;
                }

                PersonAgent parentOne = parentBucket[i];

                double childCount = (double) StatisticalDistributionController.ReproductionRate();
                int childCountRounded = ArithmeticController.RoundToInt(childCount);
                var potentialPartners = FindMatchingSecondParent(parentBucket, parentOne);
                PersonAgent parentTwo = null;
                int parentTwoAge;
                string parentTwoId;
                if (potentialPartners.Count > 0)
                {
                    parentTwo = potentialPartners.First();
                    parentTwoAge = parentTwo.Age;
                    parentTwoId = parentTwo.Id;
                    i--;
                }
                else
                {
                    parentTwoAge = parentOne.Age;
                    parentTwoId = "dead";
                }

                int youngerParentAge = parentOne.Age < parentTwoAge ? parentOne.Age : parentTwoAge;
                var potentialChildren = FindMatchingChildren(childBucket, youngerParentAge);
                int maxRange = childCountRounded > potentialChildren.Count
                    ? potentialChildren.Count
                    : childCountRounded;
                var children = potentialChildren.GetRange(0, maxRange);

                foreach (var c in children)
                {
                    c.AddParents(parentOne.Id, parentTwoId);
                    parentOne.AddChild(c);
                    if (parentTwo != null)
                    {
                        parentTwo.AddChild(c);
                    }
                    childBucket.Remove(c);
                    finalPersonsFromBucket.Add(c);
                }

                parentBucket.Remove(parentOne);
                parentBucket.Remove(parentTwo);
            }



            return finalPersonsFromBucket;
        }

        private List<PersonAgent> FindMatchingChildren(IEnumerable<PersonAgent> bucket, int parentAge)
        {
            var potentialChildren = bucket.Where(
                p => p.Age <= parentAge - 18 && p.Age >= parentAge - 50).ToList();
            return potentialChildren.Count > 0 ? potentialChildren : new List<PersonAgent>();

        }

        private List<PersonAgent> FindMatchingSecondParent(IEnumerable<PersonAgent> bucket, PersonAgent parentOne)
        {
            var potentialPartners = bucket.Where(
                p => p.Age >= parentOne.Age - 5 && p.Age <= parentOne.Age + 5 && p != parentOne).ToList();
            return potentialPartners.Count > 0 ? potentialPartners : new List<PersonAgent>();

        }


        private List<PersonAgent>[] CreateBuckets()
        {
            List<PersonAgent>[] generationBuckets = {new(), new(), new(), new()};
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

            var bankAccount = _markets.OpenBankAccount(capital, true);
            var observations = new PersonObservations(age, income, bankAccount, _policies, _jobMarketController, new NormalizationController());
            var controller = new PersonController(observations, _policies, _actionsFactory, _markets);
            var go = Instantiate(personAgentPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            PersonAgent person = go.GetComponent<PersonAgent>();
            person.Init(parentAId, parentBId, observations, controller);
            return person;
        }

        public PersonAgent CreateChild(int age, string parentAId, string parentBId)
        {
            var bankAccount = _markets.OpenBankAccount(10, true);
            var observations = new PersonObservations(age, 0, bankAccount, _policies, _jobMarketController, new NormalizationController());
            var controller = new PersonController(observations, _policies, _actionsFactory, _markets);
            var go = Instantiate(personAgentPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            PersonAgent person = go.GetComponent<PersonAgent>();
            person.Init(parentAId, parentBId, observations, controller);
            return person;
        }
    }
}