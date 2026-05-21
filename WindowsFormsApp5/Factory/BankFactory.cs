using BankSystem.Model.Entities;

namespace BankSystem.Factory
{
    public interface IBankFactory
    {
        Bank CreateBank(string name, decimal initialCreditFunds);
    }

    public class BankFactory : IBankFactory
    {
        public Bank CreateBank(string name, decimal initialCreditFunds)
        {
            return new Bank(name, initialCreditFunds);
        }
    }
}