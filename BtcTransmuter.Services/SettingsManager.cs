using System;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Settings;
using BtcTransmuter.Data;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace BtcTransmuter.Services
{
    public class SettingsManager: ISettingsManager
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IMemoryCache _memoryCache;

        public SettingsManager(IServiceScopeFactory serviceScopeFactory, IMemoryCache memoryCache)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _memoryCache = memoryCache;
        }

        public async  Task<T> GetSettings<T>(string key)
        {
            return await _memoryCache.GetOrCreateAsync<T>(key, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromHours(1);
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    using (var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                    {
                        var result = await context.Settings.SingleOrDefaultAsync(settings =>
                            settings.Key == key);
                        return result == null ? Activator.CreateInstance<T>() : result.Get<T>();
                    }
                }
            });
        }

        public async Task SaveSettings<T>(string key, T settings)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    var existing = await context.Settings.SingleOrDefaultAsync(settings1 => settings1.Key == key);
                    if (existing == null)
                    {
                        existing = new Settings()
                        {
                            Key = key
                        };
                        
                        await context.AddAsync(existing);
                    }
                    existing.Set(settings);

                    await context.SaveChangesAsync();
                    _memoryCache.Remove(key);
                }
            }
        }
    }
}