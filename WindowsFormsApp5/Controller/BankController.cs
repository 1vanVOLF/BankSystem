using System;
using BankSystem.Model;
using BankSystem.Model.Entities;
using BankSystem.Factory;

namespace BankSystem.Controller
{
    public class BankController
    {
        private BankSystemModel _model;
        private BankFactory _bankFactory;
        private ClientFactory _clientFactory;

        public BankController(BankSystemModel model)
        {
            _model = model;
            _bankFactory = new BankFactory();
            _clientFactory = new ClientFactory();
        }

        public Bank CreateBank(string name)
        {
            var bank = _bankFactory.CreateBank(name, 500000);
            _model.AddBank(bank);
            return bank;
        }

        public Individual CreateIndividual(string firstName, string lastName, string middleName,
            string passport, decimal totalLimit, Bank bank)
        {
            var individual = _clientFactory.CreateIndividual(firstName, lastName, middleName, passport, totalLimit);
            bank.AddClient(individual);
            individual.ParentBank = bank;

            var deposit = new DepositAccount(totalLimit * 0.1m);
            individual.AddAccount(deposit);
            individual.AddDeposit(totalLimit * 0.1m);

            _model.AddLog($"Создан новый клиент: {individual.GetFullName()} в банке {bank.Name}");
            return individual;
        }

        public LegalEntity CreateLegalEntity(string companyName, string inn, string address,
            decimal totalLimit, Bank bank)
        {
            var legalEntity = _clientFactory.CreateLegalEntity(companyName, inn, address, totalLimit);
            bank.AddClient(legalEntity);
            legalEntity.ParentBank = bank;

            var credit = new CreditAccount(bank.AvailableCreditFunds * 0.1m);
            legalEntity.AddAccount(credit);
            bank.IssueCredit(bank.AvailableCreditFunds * 0.1m);

            _model.AddLog($"Создан новый клиент: {legalEntity.GetFullName()} в банке {bank.Name}");
            return legalEntity;
        }

        public void DepositToAccount(Account account, decimal amount, Client client)
        {
            try
            {
                if (client.CanDeposit(amount))
                {
                    account.Deposit(amount);
                    client.AddDeposit(amount);
                    _model.AddLog($"[{client.ParentBank?.Name}] {client.GetFullName()} ({client.GetTypeName()}) - ВНЕСЕНИЕ: +{amount:N2} руб. на счёт {account.AccountType}");
                }
                else
                {
                    _model.AddLog($"ОШИБКА: {client.GetFullName()} превысил лимит внесения средств! Лимит: {client.TotalDepositedLimit}, уже внесено: {client.TotalDeposited}");
                }
            }
            catch (Exception ex)
            {
                _model.AddLog($"Ошибка при внесении: {ex.Message}");
            }
        }

        public void WithdrawFromAccount(Account account, decimal amount, Client client)
        {
            try
            {
                account.Withdraw(amount);
                client.RemoveDeposit(amount);
                _model.AddLog($"[{client.ParentBank?.Name}] {client.GetFullName()} ({client.GetTypeName()}) - СНЯТИЕ: -{amount:N2} руб. со счёта {account.AccountType}");
            }
            catch (Exception ex)
            {
                _model.AddLog($"Ошибка при снятии: {ex.Message}");
            }
        }

        public void TakeCredit(Client client, decimal amount, Bank bank)
        {
            try
            {
                if (bank.IssueCredit(amount))
                {
                    var creditAccount = new CreditAccount(amount);
                    client.AddAccount(creditAccount);
                    client.AddDeposit(amount); // Клиент получает деньги на руки
                    _model.AddLog($"[{bank.Name}] {client.GetFullName()} ({client.GetTypeName()}) - ВЗЯТ КРЕДИТ: {amount:N2} руб. Ставка: {creditAccount.InterestRate * 100}%, ежемесячный платёж: {creditAccount.MonthlyPayment:N2} руб.");
                }
                else
                {
                    _model.AddLog($"ОШИБКА: Банк {bank.Name} не может выдать кредит {amount:N2} руб. Доступно: {bank.AvailableCreditFunds:N2} руб.");
                }
            }
            catch (Exception ex)
            {
                _model.AddLog($"Ошибка при выдаче кредита: {ex.Message}");
            }
        }

        public void RepayCredit(CreditAccount creditAccount, decimal amount, Client client)
        {
            try
            {
                if (amount <= 0)
                {
                    _model.AddLog($"Ошибка: сумма погашения должна быть положительной");
                    return;
                }

                if (client.TotalDeposited >= amount)
                {
                    // Вносим деньги на кредитный счёт
                    creditAccount.Deposit(amount);
                    client.RemoveDeposit(amount);

                    // Производим платёж
                    decimal oldDebt = creditAccount.RemainingDebt;
                    creditAccount.MakeMonthlyPayment();

                    _model.AddLog($"[{client.ParentBank?.Name}] {client.GetFullName()} ({client.GetTypeName()}) - ПОГАШЕНИЕ КРЕДИТА: {amount:N2} руб. Долг был: {oldDebt:N2}, стал: {creditAccount.RemainingDebt:N2}");

                    // Проверяем, закрылся ли кредит
                    if (creditAccount.RemainingDebt <= 0)
                    {
                        _model.AddLog($"🎉 ПОЗДРАВЛЯЕМ! {client.GetFullName()} полностью погасил кредит в банке {client.ParentBank?.Name}!");
                    }
                }
                else
                {
                    _model.AddLog($"ОШИБКА: {client.GetFullName()} недостаточно средств для погашения кредита. Нужно: {amount:N2}, доступно: {client.TotalDeposited:N2}");
                }
            }
            catch (Exception ex)
            {
                _model.AddLog($"Ошибка при погашении кредита: {ex.Message}");
            }
        }

        public void MakeMonthlyPaymentOnCredit(CreditAccount creditAccount, Client client)
        {
            try
            {
                decimal oldDebt = creditAccount.RemainingDebt;
                creditAccount.MakeMonthlyPayment();
                _model.AddLog($"[{client.ParentBank?.Name}] {client.GetFullName()} - АВТОПЛАТЁЖ ПО КРЕДИТУ: Долг был {oldDebt:N2}, стал {creditAccount.RemainingDebt:N2}");
            }
            catch (Exception ex)
            {
                _model.AddLog($"Ошибка при автоплатеже по кредиту: {ex.Message}");
            }
        }

        public void ApplyInterestToDeposit(DepositAccount depositAccount, Client client)
        {
            try
            {
                decimal oldBalance = depositAccount.Balance;
                depositAccount.ApplyInterest();
                decimal interest = depositAccount.Balance - oldBalance;
                _model.AddLog($"[{client.ParentBank?.Name}] {client.GetFullName()} - НАЧИСЛЕНЫ ПРОЦЕНТЫ: +{interest:N2} руб. на депозит. Баланс: {depositAccount.Balance:N2}");
            }
            catch (Exception ex)
            {
                _model.AddLog($"Ошибка при начислении процентов: {ex.Message}");
            }
        }
    }
}