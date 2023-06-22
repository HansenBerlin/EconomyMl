namespace NewScripts
{
    public class Blacklist
    {
        public Company BlackListedCompany { get; }
        private int _monthLeft;

        public bool CanBeWhitlisted()
        {
            _monthLeft--;
            return _monthLeft == 0;
        }
    }
}