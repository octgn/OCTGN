namespace Octgn.Play
{
    using System.ComponentModel;

    public interface IPlayCounter
    {
        string Name { get; }

        int Value { get; set; }

        DataNew.Entities.Counter Definition { get; }

        int Id { get; }

        string ToString();

        void SetValue(int value, IPlayPlayer who, bool notifyServer);

        void Reset();

        event PropertyChangedEventHandler PropertyChanged;
    }
}