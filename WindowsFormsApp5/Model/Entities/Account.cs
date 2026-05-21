using BankSystem.Observer;
using System;
using System.Collections.Generic;

namespace BankSystem.Model.Entities
{
    public abstract class Account : ISubject
    {
        private readonly object _lock = new object();
        private List<IObserver> _observers = new List<IObserver>();

        public Guid Id { get; private set; }
        public decimal Balance { get; protected set; }
        public Client ParentClient { get; set; }
        public abstract string AccountType { get; }

        protected Account(decimal initialBalance)
        {
            Id = Guid.NewGuid();
            Balance = initialBalance;
        }

        public virtual void Deposit(decimal amount)
        {
            lock (_lock)
            {
                if (amount <= 0)
                    throw new ArgumentException("Сумма должна быть положительной");

                Balance += amount;
                NotifyObservers($"{ParentClient?.GetFullName()}: пополнение счёта {AccountType} на {amount}, баланс: {Balance}");
            }
        }

        public virtual void Withdraw(decimal amount)
        {
            lock (_lock)
            {
                if (amount <= 0)
                    throw new ArgumentException("Сумма должна быть положительной");

                if (Balance >= amount)
                {
                    Balance -= amount;
                    NotifyObservers($"{ParentClient?.GetFullName()}: снятие со счёта {AccountType} {amount}, баланс: {Balance}");
                }
                else
                {
                    throw new InvalidOperationException("Недостаточно средств на счете");
                }
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