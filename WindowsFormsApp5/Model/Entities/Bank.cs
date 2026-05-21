using BankSystem.Observer;
using System;
using System.Collections.Generic;

namespace BankSystem.Model.Entities
{
    public class Bank : ISubject
    {
        private readonly object _lock = new object();
        private List<IObserver> _observers = new List<IObserver>();

        public string Name { get; private set; }
        public decimal AvailableCreditFunds { get; private set; }
        public List<Client> Clients { get; private set; }
        public Guid Id { get; private set; }

        public Bank(string name, decimal initialCreditFunds)
        {
            Id = Guid.NewGuid();
            Name = name;
            AvailableCreditFunds = initialCreditFunds;
            Clients = new List<Client>();
        }

        public bool IssueCredit(decimal amount)
        {
            lock (_lock)
            {
                if (AvailableCreditFunds >= amount)
                {
                    AvailableCreditFunds -= amount;
                    NotifyObservers($"Банк {Name}: выдан кредит на сумму {amount}");
                    return true;
                }
                NotifyObservers($"Банк {Name}: недостаточно средств для выдачи кредита {amount}");
                return false;
            }
        }

        public void ReceiveCreditPayment(decimal amount)
        {
            lock (_lock)
            {
                AvailableCreditFunds += amount;
                NotifyObservers($"Банк {Name}: получен платёж по кредиту {amount}");
            }
        }

        public void AddClient(Client client)
        {
            lock (_lock)
            {
                Clients.Add(client);
                NotifyObservers($"Банк {Name}: добавлен новый клиент {client.GetFullName()}");
            }
        }

        public void Attach(IObserver observer)
        {
            lock (_observers)
            {
                if (!_observers.Contains(observer))
                    _observers.Add(observer);
            }
        }

        public void Detach(IObserver observer)
        {
            lock (_observers)
            {
                _observers.Remove(observer);
            }
        }

        public void NotifyObservers(string message)
        {
            List<IObserver> observersCopy;
            lock (_observers)
            {
                observersCopy = new List<IObserver>(_observers);
            }
            foreach (var observer in observersCopy)
            {
                observer.Update(message);
            }
        }
    }
}