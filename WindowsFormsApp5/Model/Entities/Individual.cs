namespace BankSystem.Model.Entities
{
    public class Individual : Client
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Passport { get; set; }

        public Individual(string firstName, string lastName, string middleName, string passport, decimal totalLimit)
            : base(totalLimit)
        {
            FirstName = firstName;
            LastName = lastName;
            MiddleName = middleName;
            Passport = passport;
        }

        public override string GetFullName()
        {
            return $"{LastName} {FirstName} {MiddleName}".Trim();
        }

        public override string GetTypeName()
        {
            return "Физическое лицо";
        }
    }
}