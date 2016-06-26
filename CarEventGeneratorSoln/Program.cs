using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System.IO;
using System.Configuration;

namespace CarEventGenerator
{
    class Program
    {

        static string eventHubName = ConfigurationManager.AppSettings["inputeventhub"];
        //static string connectionString = "Endpoint=sb://tisensordemosh.servicebus.windows.net/;SharedAccessKeyName=all;SharedAccessKey=mcxBEocF6f0KHQGhP2MtT7A44tUC+zhqyDcetP/Jt0o=";
        static string connectionString = ConfigurationManager.AppSettings["servicebusconnectionstring"];
        static Random random = new Random();
        static List<string> VinList = new List<string>();
        
        static void Main(string[] args)
        {
            Console.WriteLine("*******************************************************************************");
            Console.WriteLine("Virtual Car Telematics Application");
            Console.WriteLine("*******************************************************************************");
            Console.WriteLine("Press Ctrl-C to stop the sender process");
            //Console.ReadLine();
            GetVINMasterList();    
            //SendingRandomMessages().Wait();     
            SendingCarEventData().Wait() ;
        }
     
        static async Task SendingCarEventData()
        {
            var eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, eventHubName);
            
            while (true)
            {
                try
                {
                    var city = GetLocation();

                    var info = new CarEvent() {

                            vin = GetRandomVIN(),
                            city = city,
                            outsideTemperature = GetOutsideTemp(city),
                            engineTemperature  = GetEngineTemp(city),
                            speed = GetSpeed(city), 
                            fuel = random.Next(0,40),
                            engineoil = GetOil(city),
                            tirepressure = GetTirePressure(city),
                            odometer = random.Next (0,200000),
                            accelerator_pedal_position = random.Next (0,100),
                            parking_brake_status = GetRandomBoolean(),
                            headlamp_status = GetRandomBoolean(),
                            brake_pedal_status = GetRandomBoolean(),
                            transmission_gear_position = GetGearPos(),
                            ignition_status = GetRandomBoolean(),
                            windshield_wiper_status = GetRandomBoolean(),
                            abs = GetRandomBoolean(),   
                            timestamp = DateTime.UtcNow
                    };
                    var serializedString = JsonConvert.SerializeObject(info);
                    EventData data = new EventData(Encoding.UTF8.GetBytes(serializedString));
                    //{
                    //    PartitionKey = info.productId.ToString()
                    //};

                    // Set user properties if needed
                    data.Properties.Add("Type", "Telemetry_" + DateTime.Now.ToLongTimeString());
                    //Console.WriteLine("{0} > Sending message: {1}", DateTime.Now.ToString(), data.ToString());
                    Console.WriteLine("{0} > Sending message: {1}", DateTime.Now.ToString(), serializedString.ToString());
                    //await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(data.ToString())));
                    await eventHubClient.SendAsync(data);
                }
                catch (Exception exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0} > Exception: {1}", DateTime.Now.ToString(), exception.Message);
                    Console.ResetColor();
                }

                await Task.Delay(200);
            }
        }

        static async Task SendingRandomMessages()
        {
            var eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, eventHubName);
            while (true)
            {
                try
                {
                    var message = Guid.NewGuid().ToString();
                    Console.WriteLine("{0} > Sending message: {1}", DateTime.Now.ToString(), message);
                    await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(message)));
                }
                catch (Exception exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0} > Exception: {1}", DateTime.Now.ToString(), exception.Message);
                    Console.ResetColor();
                }

                await Task.Delay(200);
            }
        }

        static int GetSpeed(string city)
        {
            if (city.ToLower() == "bellevue")
            {
                return GetRandomWeightedNumber(100, 0, Convert.ToDouble(ConfigurationManager.AppSettings["HighSpeedProbabilityPower"]));
            }
            return GetRandomWeightedNumber(100, 0, Convert.ToDouble(ConfigurationManager.AppSettings["LowSpeedProbabilityPower"]));
        }

        static int GetOil(string city)
        {
            if (city.ToLower() == "seattle")
            {
                return GetRandomWeightedNumber(50, 0, Convert.ToDouble(ConfigurationManager.AppSettings["LowOilProbabilityPower"]));
            }
            return GetRandomWeightedNumber(50, 0, Convert.ToDouble(ConfigurationManager.AppSettings["HighOilProbabilityPower"]));
        }

        static int GetTirePressure(string city)
        {
            if (city.ToLower() == "seattle")
            {
                return GetRandomWeightedNumber(50, 0, Convert.ToDouble(ConfigurationManager.AppSettings["LowTyrePressureProbabilityPower"]));
            }
            return GetRandomWeightedNumber(50, 0, Convert.ToDouble(ConfigurationManager.AppSettings["HighTyrePressureProbabilityPower"]));
        }
        static int GetEngineTemp(string city)
        {
            if (city.ToLower() == "seattle")
            {
                return GetRandomWeightedNumber(500, 0, Convert.ToDouble(ConfigurationManager.AppSettings["HighEngineTempProbabilityPower"]));
            }
            return GetRandomWeightedNumber(500, 0, Convert.ToDouble(ConfigurationManager.AppSettings["LowEngineTempProbabilityPower"]));
        }
        static int GetOutsideTemp(string city)
        {
            if (city.ToLower() == "seattle")
            {
                return GetRandomWeightedNumber(100, 0, Convert.ToDouble(ConfigurationManager.AppSettings["LowOutsideTempProbabilityPower"]));
            }
            return GetRandomWeightedNumber(100, 0, Convert.ToDouble(ConfigurationManager.AppSettings["HighOutsideTempProbabilityPower"]));
        }


        private static int GetRandomWeightedNumber(int max, int min, double probabilityPower)
        {
            var randomizer = new Random();
            var randomDouble = randomizer.NextDouble();

            var result = Math.Floor(min + (max + 1 - min) * (Math.Pow(randomDouble, probabilityPower)));
            return (int)result;
        }

        static void GetVINMasterList()
        {
            var reader = new StreamReader(File.OpenRead(@"VINMasterList.csv"));            
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(';');

                VinList.Add(values[0]);
                
            }
        }
        static string GetRandomVIN()
        {
            int RandomIndex = random.Next(1, VinList.Count -1);
            return VinList[RandomIndex];
        }

        static string GetLocation()
        {
            List<string> list = new List<string>() { "North Point", "Quaseway Bay", "Tsim Sha Tsui ", "Mong Kok", "Kowloon Bay", "Tseung Kwan O", "Main On Shan", "Tsuen Wan", "Admirality", "Central", "Wan Cahi", "Quarry Bay" };
            int l = list.Count;
            Random r = new Random();
            int num = r.Next(l);
            return list[num];
        }
        static string GetGearPos()
        {
            List<string> list = new List<string>() { "first", "second", "third", "fourth", "fifth", "sixth", "seventh", "eight"};
            int l = list.Count;
            Random r = new Random();
            int num = r.Next(l);
            return list[num];
        }
        static bool GetRandomBoolean()
        {
            return new Random().Next(100) % 2 == 0;
        }

    
    

    }
}
