namespace LilaSoft.Architectures.Domains.API.Application.IntegrationEvents
{
    using LilaSoft.Patterns.EventBus.EventBus.Events;
    using System.Threading.Tasks;

    public interface IDomainDrivenIntegrationEventService
    {
        Task PublishThroughEventBusAsync(IntegrationEvent evt);
    }
}
