using EconomyBase.Models.Agents;

namespace EconomyBase.Models.Population
{



    public class PersonDummy : PersonAgent
    {
        public new string Id => "dead";

        public PersonDummy()
        {
            IsDummy = true;
        }
    }
}