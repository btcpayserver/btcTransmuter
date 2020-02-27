using System.Collections.Generic;
using System.Threading.Tasks;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Entities.U2F;

namespace BtcTransmuter.Abstractions.U2F
{
    public interface IU2FService
    {
        Task<List<U2FDevice>> GetDevices(string userId);
        Task RemoveDevice(string id, string userId);
        Task<bool> HasDevices(string userId);
        ServerRegisterResponse StartDeviceRegistration(string userId, string appId);
        Task<bool> CompleteRegistration(string userId, string deviceResponse, string name);
        Task<bool> AuthenticateUser(string userId, string deviceResponse);
        Task<List<ServerChallenge>> GenerateDeviceChallenges(string userId, string appId);
    }
}