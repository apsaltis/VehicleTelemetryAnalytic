using System.Configuration;
using Microsoft.ServiceBus.Messaging;

namespace RealtimeDashboardApp
{
    class EventReceiver
    {
        #region Fields
        string eventHubName;
        EventHubConsumerGroup defaultConsumerGroup;
        string eventHubConnectionString;
        EventProcessorHost eventProcessorHost;
        #endregion

        public EventReceiver(string eventHubName, string eventHubConnectionString)
        {
            this.eventHubConnectionString = eventHubConnectionString;
            this.eventHubName =eventHubName;
        }

        public void MessageProcessingWithPartitionDistribution()
        {
            EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(eventHubConnectionString, this.eventHubName);

            // Get the default Consumer Group 
            defaultConsumerGroup = eventHubClient.GetDefaultConsumerGroup();
            string blobConnectionString = ConfigurationManager.AppSettings["BlobStorageConnString"]; // Required for checkpoint/state 
            eventProcessorHost = new EventProcessorHost("singleworker", eventHubClient.Path, defaultConsumerGroup.GroupName, this.eventHubConnectionString, blobConnectionString);
            eventProcessorHost.RegisterEventProcessorAsync<EventProcessor>().Wait();
        }

        public void UnregisterEventProcessor()
        {
            eventProcessorHost.UnregisterEventProcessorAsync().Wait();
        }
    } 
}
