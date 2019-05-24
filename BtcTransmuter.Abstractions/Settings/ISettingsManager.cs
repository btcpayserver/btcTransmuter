using System.Threading.Tasks;

namespace BtcTransmuter.Abstractions.Settings
{
    public interface ISettingsManager
    {
        Task<T> GetSettings<T>(string key);
        Task SaveSettings<T>(string key,T settings);
    }
}