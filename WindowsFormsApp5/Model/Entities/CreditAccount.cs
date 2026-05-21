using System;

namespace BankSystem.Model.Entities
{
    public class CreditAccount : Account
    {
        public decimal CreditAmount { get; private set; }
        public decimal RemainingDebt { get; private set; }
        public decimal MonthlyPayment { get; private set; }
        public decimal InterestRate { get; set; }
        public override string AccountType => "Кредитный";

        public CreditAccount(decimal creditAmount, decimal interestRate = 0.1m, int months = 12) : base(0)
        {
            CreditAmount = creditAmount;
            RemainingDebt = creditAmount;
            InterestRate = interestRate;
            MonthlyPayment = creditAmount / months * (1 + interestRate);
        }

        public void MakeMonthlyPayment()
        {
            if (RemainingDebt <= 0)
            {
                NotifyObservers("Кредит уже полностью погашен");
                return;
            }

            decimal payment = Math.Min(MonthlyPayment, RemainingDebt + MonthlyPayment * InterestRate);

            if (Balance >= payment)
            {
                Balance -= payment;
                RemainingDebt -= payment;

                if (ParentClient?.ParentBank != null)
                {
                    ParentClient.ParentBank.ReceiveCreditPayment(payment);
                }

                NotifyObservers($"Произведён ежемесячный платёж по кредиту: {payment}, остаток долга: {Math.Max(0, RemainingDebt)}");

                if (RemainingDebt <= 0)
                {
                    CloseCredit();
                }
            }
            else
            {
                NotifyObservers($"Недостаточно средств для ежемесячного платежа по кредиту. Начислены проценты на остаток {RemainingDebt}");
                decimal interest = RemainingDebt * InterestRate;
                RemainingDebt += interest;
            }
        }

        private void CloseCredit()
        {
            NotifyObservers($"Кредит полностью погашен! Остаток на счёте: {Balance}");

            if (Balance > 0)
            {
                var depositAccount = ParentClient.Accounts.Find(a => a is DepositAccount);
                if (depositAccount != null)
                {
                    depositAccount.Deposit(Balance);
                    NotifyObservers($"Остаток {Balance} перенесён на депозитный счёт");
                }
                else
                {
                    var newDeposit = new DepositAccount(0);
                    ParentClient.AddAccount(newDeposit);
                    newDeposit.Deposit(Balance);
                    NotifyObservers($"Создан новый депозитный счёт, остаток {Balance} перенесён на него");
                }
                Balance = 0;
            }
        }

        public override void Withdraw(decimal amount)
        {
            throw new InvalidOperationException("Нельзя снимать средства с кредитного счёта");
        }
    }
}