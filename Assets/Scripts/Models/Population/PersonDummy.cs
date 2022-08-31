using Models.Agents;

namespace Models.Population
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