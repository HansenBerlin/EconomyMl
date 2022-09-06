namespace Assets.Scripts.Controller
{



    public static class IdController
    {
        private static int _id = 1;

        public static int GetId => _id++;
    }
}
