using BankSystem.Observer;
using System;
using System.Collections.Generic;

namespace BankSystem.Model.Entities
{
    public abstract class Client : ISubject
    {
        private readonly object _lock = new object();
        private List<IObserver> _observers = new List<IObserver>();

        public Guid Id { get; private set; }
        public DateTime CreatedDate { get; private set; }
        public List<Account> Accounts { get; private set; }
        public Bank ParentBank { get; set; }
        public decimal TotalDepositedLimit { get; set; }
        public decimal TotalDeposited { get; private set; }

        protected Client(decimal totalLimit)
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.Now;
            Accounts = new List<Account>();
            TotalDepositedLimit = totalLimit;
            TotalDeposited = 0;
        }

        public abstract string GetFullName();
        public abstract string GetTypeName();

        public bool CanDeposit(decimal amount)
        {
            lock (_lock)
            {
                return TotalDeposited + amount <= TotalDepositedLimit;
            }
        }

        public void AddDeposit(decimal amount)
        {
            lock (_lock)
            {
                if (CanDeposit(amount))
                {
                    TotalDeposited += amount;
                    NotifyObservers($"{GetFullName()}: внесено {amount}, всего внесено {TotalDeposited}/{TotalDepositedLimit}");
                }
                else
                {
                    throw new InvalidOperationException("Превышен лимит внесения средств");
                }
            }
        }

        public void RemoveDeposit(decimal amount)
        {
            lock (_lock)
            {
                if (TotalDeposited >= amount)
                {
                    TotalDeposited -= amount;
                    NotifyObservers($"{GetFullName()}: снято {amount}, остаток внесённых средств {TotalDeposited}");
                }
                else
                {
                    throw new InvalidOperationException("Недостаточно внесённых средств для снятия");
                }
            }
        }

        public void AddAccount(Account account)
        {
            lock (_lock)
            {
                Accounts.Add(account);
                account.ParentClient = this;
                NotifyObservers($"{GetFullName()}: открыт новый счёт {account.AccountType}");
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