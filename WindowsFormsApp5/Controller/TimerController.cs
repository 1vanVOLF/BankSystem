using System;
using System.Windows.Forms;
using BankSystem.Model;
using BankSystem.Model.Entities;

namespace BankSystem.Controller
{
    public class TimerController
    {
        private BankSystemModel _model;
        private BankController _controller;
        private Timer _timer;
        private int _tickCount = 0;

        public TimerController(BankSystemModel model, BankController controller)
        {
            _model = model;
            _controller = controller;
            _timer = new Timer { Interval = 7000 };
            _timer.Tick += OnTimerTick;
        }

        public void Start()
        {
            _timer.Start();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            _tickCount++;

            if (_tickCount == 1)
            {
                ProcessFirstTick();
            }
            else if (_tickCount == 2)
            {
                ProcessSecondTick();
                _tickCount = 0;
            }
        }

        private void ProcessFirstTick()
        {
            _model.AddLog("=== Начисление процентов и ежемесячных платежей ===");

            foreach (var bank in _model.GetBanks())
            {
                foreach (var client in bank.Clients)
                {
                    foreach (var account in client.Accounts)
                    {
                        if (account is DepositAccount deposit)
                        {
                            deposit.ApplyInterest();
                        }
                        else if (account is CreditAccount credit)
                        {
                            credit.MakeMonthlyPayment();
                        }
                    }
                }
            }
        }

        private void ProcessSecondTick()
        {
            _model.AddLog("=== Пополнение депозитов на 1% ===");

            foreach (var bank in _model.GetBanks())
            {
                foreach (var client in bank.Clients)
                {
                    foreach (var account in client.Accounts)
                    {
                        if (account is DepositAccount deposit)
                        {
                            var amount = client.TotalDepositedLimit * 0.01m;
                            _controller.DepositToAccount(deposit, amount, client);
                        }
                    }
                }
            }
        }
    }
}