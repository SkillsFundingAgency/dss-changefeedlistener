using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using NCS.DSS.ChangeFeedListener.Model;

namespace NCS.DSS.ChangeFeedListener.ServiceBus
{
    public interface IServiceBusClient
    {
        Task SendChangeFeedMessageAsync(Document document, ChangeFeedMessageModel changeFeedMessageModel);
    }
}