using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions;
using BtcTransmuter.Abstractions.ExternalServices;
using BtcTransmuter.Data;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BtcTransmuter.Services
{
    public class ExternalServiceManager : IExternalServiceManager
    {
        private IServiceScopeFactory _serviceScopeFactory;
        public event EventHandler<UpdatedItem<ExternalServiceData>> ExternalServiceDataUpdated;

        public ExternalServiceManager(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }


        public async Task<ExternalServiceData> GetExternalServiceData(string id, string userId)
        {
            return (await GetExternalServicesData(new ExternalServicesDataQuery()
            {
                ExternalServiceId = id,
                UserId = userId
            })).FirstOrDefault();
        }

        public async Task RemoveExternalServiceData(string id)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    var externalService = await context.ExternalServices.FindAsync(id);
                    if (externalService == null)
                    {
                        throw new ArgumentException();
                    }

                    context.Remove(externalService);
                    await context.SaveChangesAsync();
                    OnExternalServiceDataUpdated(new UpdatedItem<ExternalServiceData>()
                    {
                        Item = externalService,
                        Action = UpdatedItem<ExternalServiceData>.UpdateAction.Removed
                    });
                }
            }
        }

        public async Task AddOrUpdateExternalServiceData(ExternalServiceData externalServiceData)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    var created = false;
                    if (string.IsNullOrEmpty(externalServiceData.Id))
                    {
                        await context.ExternalServices.AddAsync(externalServiceData);
                        created = true;
                    }
                    else
                    {
                        context.Attach(externalServiceData).State = EntityState.Modified;
                        context.ExternalServices.Update(externalServiceData);
                    }

                    await context.SaveChangesAsync();
                    OnExternalServiceDataUpdated(new UpdatedItem<ExternalServiceData>()
                    {
                        Item = externalServiceData,
                        Action = created
                            ? UpdatedItem<ExternalServiceData>.UpdateAction.Added
                            : UpdatedItem<ExternalServiceData>.UpdateAction.Updated
                    });
                }
            }
        }

        public async Task UpdateInternalData(string id, object data)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    var externalService = await context.ExternalServices.FindAsync(id);
                    if (externalService == null)
                    {
                        throw new ArgumentException();
                    }

                    externalService.Set(data);

                    await context.SaveChangesAsync();
                    OnExternalServiceDataUpdated(new UpdatedItem<ExternalServiceData>()
                    {
                        Item = externalService,
                        Action = UpdatedItem<ExternalServiceData>.UpdateAction.Updated
                    });
                }
            }
        }


        public async Task<IEnumerable<ExternalServiceData>> GetExternalServicesData(ExternalServicesDataQuery query)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                {
                    var queryable = context.ExternalServices
                        .Include(data => data.RecipeActions)
                        .Include(data => data.RecipeTriggers)
                        .AsQueryable();

                    if (query.Type != null && query.Type.Any())
                    {
                        queryable = queryable.Where(x => query.Type.Contains(x.Type));
                    }

                    if (!string.IsNullOrEmpty(query.UserId))
                    {
                        queryable = queryable.Where(x => x.UserId == query.UserId);
                    }

                    if (!string.IsNullOrEmpty(query.ExternalServiceId))
                    {
                        queryable = queryable.Where(x => x.Id == query.ExternalServiceId);
                    }

                    return await queryable.ToListAsync();
                }
            }
        }

        protected virtual void OnExternalServiceDataUpdated(UpdatedItem<ExternalServiceData> e)
        {
            ExternalServiceDataUpdated?.Invoke(this, e);
        }
    }
}