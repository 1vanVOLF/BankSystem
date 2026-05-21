namespace BankSystem.Model.Entities
{
    public class DepositAccount : Account
    {
        public decimal InterestRate { get; set; }
        public override string AccountType => "Депозитный";

        public DepositAccount(decimal initialBalance, decimal interestRate = 0.05m) : base(initialBalance)
        {
            InterestRate = interestRate;
        }

        public void ApplyInterest()
        {
            decimal interest = Balance * InterestRate;
            Deposit(interest);
            NotifyObservers($"Начислены проценты по депозиту: {interest}");
        }
    }
}