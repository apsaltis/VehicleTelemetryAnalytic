using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using RealtimeDashboardApp.Entity;

namespace RealtimeDashboardApp
{
    class EventProcessor : IEventProcessor
    {
        PartitionContext partitionContext;

        public Task OpenAsync(PartitionContext context)
        {
            Console.WriteLine(string.Format("Eventlistener OpenAsync.  Partition: '{0}', Offset: '{1}'", context.Lease.PartitionId, context.Lease.Offset));
            this.partitionContext = context;
            return Task.FromResult<object>(null);
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> events)
        {
            string temp;
            string eventdata = "";
            try
            {
               // EventIncreamenter.IncreamentEventCount(events.Count());
                foreach (EventData eventData in events)
                {
                    try
                    {
                        eventdata = Encoding.UTF8.GetString(eventData.GetBytes());
                        Console.WriteLine("Received event data");
                        temp = eventdata;
                        Program.PushEventHubAnomalyRecords(
                           JsonConvert.DeserializeObject<RealTimeVehicleHealthAnomalyReport[]>(eventdata).FirstOrDefault());
                    }
                    catch (Exception oops)
                    {
                        try
                        {
                            Program.PushEventHubAnomalyRecords(
                            JsonConvert.DeserializeObject<RealTimeVehicleHealthAnomalyReport>(eventdata));
                        }
                        catch (Exception e2)
                        {
                            Trace.TraceError(oops.Message);
                        }
                    }
                }

                await context.CheckpointAsync();

            }
            catch (Exception exp)
            {
                Console.WriteLine("Error in processing: " + exp.Message);
            }
        }

        public async Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Console.WriteLine(string.Format("Event Listener CloseAsync.  Partition '{0}', Reason: '{1}'.", this.partitionContext.Lease.PartitionId, reason.ToString()));
            if (reason == CloseReason.Shutdown)
            {
                await context.CheckpointAsync();
            }
        }
        
    }
}