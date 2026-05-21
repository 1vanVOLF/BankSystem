using BankSystem.Model.Entities;

namespace BankSystem.Factory
{
    public interface IClientFactory
    {
        Individual CreateIndividual(string firstName, string lastName, string middleName,
            string passport, decimal totalLimit);
        LegalEntity CreateLegalEntity(string companyName, string inn, string address,
            decimal totalLimit);
    }

    public class ClientFactory : IClientFactory
    {
        public Individual CreateIndividual(string firstName, string lastName, string middleName,
            string passport, decimal totalLimit)
        {
            return new Individual(firstName, lastName, middleName, passport, totalLimit);
        }

        public LegalEntity CreateLegalEntity(string companyName, string inn, string address,
            decimal totalLimit)
        {
            return new LegalEntity(companyName, inn, address, totalLimit);
        }
    }
}