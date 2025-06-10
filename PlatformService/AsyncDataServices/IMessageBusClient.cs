using PlatformService.DTOs;

namespace PlatformService.AsyncDataServices
{
    public interface IMessageBusClient
    {
        Task PublishNewPlatform(PlatformPublishedDto publishedDto);
    }
}