namespace LilaSoft.Architectures.Domains.API.Application.IntegrationEvents
{
    using LilaSoft.Patterns.EventBus.EntityFrameworkIntegrationEventLog.Services;
    using LilaSoft.Patterns.EventBus.EntityFrameworkIntegrationEventLog.Utilities;
    using LilaSoft.Patterns.EventBus.EventBus.Abstractions;
    using LilaSoft.Patterns.EventBus.EventBus.Events;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
    using System;
    using System.Data.Common;
    using System.Threading.Tasks;

    public class DomainDrivenIntegrationEventService : IDomainDrivenIntegrationEventService
    {
        private readonly Func<DbConnection, IIntegrationEventLogService> _integrationEventLogServiceFactory;
        private readonly IEventBus _eventBus;
        private readonly DbContext _orderingContext;
        private readonly IIntegrationEventLogService _eventLogService;

        public DomainDrivenIntegrationEventService(IEventBus eventBus, DbContext orderingContext,
        Func<DbConnection, IIntegrationEventLogService> integrationEventLogServiceFactory)
        {
            _orderingContext = orderingContext ?? throw new ArgumentNullException(nameof(orderingContext));
            _integrationEventLogServiceFactory = integrationEventLogServiceFactory ?? throw new ArgumentNullException(nameof(integrationEventLogServiceFactory));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _eventLogService = _integrationEventLogServiceFactory(_orderingContext.Database.GetDbConnection());
        }

        public async Task PublishThroughEventBusAsync(IntegrationEvent evt)
        {
            await SaveEventAndOrderingContextChangesAsync(evt);
            _eventBus.Publish(evt);
            await _eventLogService.MarkEventAsPublishedAsync(evt);
        }

        private async Task SaveEventAndOrderingContextChangesAsync(IntegrationEvent evt)
        {
            //Use of an EF Core resiliency strategy when using multiple DbContexts within an explicit BeginTransaction():
            //See: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency            
            await ResilientTransaction.New(_orderingContext)
                .ExecuteAsync(async () =>
                {
                    // Achieving atomicity between original ordering database operation and the IntegrationEventLog thanks to a local transaction
                    await _orderingContext.SaveChangesAsync();
                    await _eventLogService.SaveEventAsync(evt, _orderingContext.Database.CurrentTransaction.GetDbTransaction());
                });
        }
    }
}
