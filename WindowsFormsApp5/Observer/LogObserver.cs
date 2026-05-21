using System;
using System.Collections.Generic;

namespace BankSystem.Observer
{
    public class LogObserver : IObserver
    {
        private List<string> _logs;
        private object _lock = new object();

        public LogObserver()
        {
            _logs = new List<string>();
        }

        public void Update(string message)
        {
            lock (_lock)
            {
                var logEntry = $"{DateTime.Now:HH:mm:ss.fff} - {message}";
                _logs.Insert(0, logEntry);

                // Ограничиваем размер лога
                if (_logs.Count > 1000)
                    _logs.RemoveAt(_logs.Count - 1);

                // Выводим в консоль для отладки
                Console.WriteLine(logEntry);
            }
        }

        public List<string> GetLogs()
        {
            lock (_lock)
            {
                return new List<string>(_logs);
            }
        }

        public void ClearLogs()
        {
            lock (_lock)
            {
                _logs.Clear();
            }
        }
    }
}