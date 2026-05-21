namespace BankSystem
{
    public interface ISubject
    {
        void Attach(IObserver observer);
        void Detach(IObserver observer);
        void NotifyObservers(string message);
    }
}