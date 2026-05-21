using System;
using System.Collections.Generic;
using BankSystem.Model.Entities;

namespace BankSystem.Model
{
    public class BankSystemModel
    {
        private readonly object _lock = new object();
        public List<Bank> Banks { get; private set; }
        public List<string> GlobalLog { get; private set; }
        public decimal GlobalCreditPool { get; set; }

        public BankSystemModel()
        {
            Banks = new List<Bank>();
            GlobalLog = new List<string>();
            GlobalCreditPool = 1000000;
        }

        public void AddBank(Bank bank)
        {
            lock (_lock)
            {
                Banks.Add(bank);
                AddLog($"Создан новый банк: {bank.Name}");
            }
        }

        public void AddLog(string message)
        {
            lock (_lock)
            {
                var logEntry = $"{DateTime.Now:HH:mm:ss} - {message}";
                GlobalLog.Insert(0, logEntry);
                if (GlobalLog.Count > 1000)
                    GlobalLog.RemoveAt(GlobalLog.Count - 1);
            }
        }

        public List<Bank> GetBanks()
        {
            lock (_lock)
            {
                return new List<Bank>(Banks);
            }
        }
    }
}