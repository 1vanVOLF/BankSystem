namespace BankSystem.Model.Entities
{
    public class LegalEntity : Client
    {
        public string CompanyName { get; set; }
        public string INN { get; set; }
        public string Address { get; set; }

        public LegalEntity(string companyName, string inn, string address, decimal totalLimit)
            : base(totalLimit)
        {
            CompanyName = companyName;
            INN = inn;
            Address = address;
        }

        public override string GetFullName()
        {
            return CompanyName;
        }

        public override string GetTypeName()
        {
            return "Юридическое лицо";
        }
    }
}