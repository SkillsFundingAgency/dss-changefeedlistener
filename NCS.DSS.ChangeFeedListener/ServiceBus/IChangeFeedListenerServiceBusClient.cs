
using NCS.DSS.ChangeFeedListener.Model;
namespace NCS.DSS.ChangeFeedListener.ServiceBus
{
    public interface IChangeFeedListenerServiceBusClient
    {
        Task SendChangeFeedMessageAsync(string documentId, ChangeFeedMessageModel changeFeedMessageModel);
    }
}