namespace Octgn.Library
{
    public interface ISimpleConfig
    {
        string DataDirectory { get; }
        string ConfigPath { get; }

        T ReadValue<T>(string valName, T def);

        void WriteValue<T>(string valName, T value);
    }
}