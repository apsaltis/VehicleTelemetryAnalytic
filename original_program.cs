// Decompiled with JetBrains decompiler
// Type: CarEventGenerator.Program
// Assembly: CarEventGenerator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: E6A588DA-B1D6-4953-AFEE-61A9C2B19D6C
// Assembly location: C:\Users\asus\Downloads\CarEventGenerator.exe

using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CarEventGenerator
{
  internal class Program
  {
    private static string eventHubName = ConfigurationManager.AppSettings["inputeventhub"];
    private static string connectionString = ConfigurationManager.AppSettings["servicebusconnectionstring"];
    private static Random random = new Random();
    private static List<string> VinList = new List<string>();

    private static void Main(string[] args)
    {
      Console.WriteLine("*******************************************************************************");
      Console.WriteLine("Virtual Car Telematics Application");
      Console.WriteLine("*******************************************************************************");
      Console.WriteLine("Press Ctrl-C to stop the sender process");
      Program.GetVINMasterList();
      Program.SendingCarEventData().Wait();
    }

    private static async Task SendingCarEventData()
    {
      EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(Program.connectionString, Program.eventHubName);
      while (true)
      {
        try
        {
          string city = Program.GetLocation();
          CarEvent info = new CarEvent()
          {
            vin = Program.GetRandomVIN(),
            city = city,
            outsideTemperature = Program.GetOutsideTemp(city),
            engineTemperature = Program.GetEngineTemp(city),
            speed = Program.GetSpeed(city),
            fuel = Program.random.Next(0, 40),
            engineoil = Program.GetOil(city),
            tirepressure = Program.GetTirePressure(city),
            odometer = Program.random.Next(0, 200000),
            accelerator_pedal_position = Program.random.Next(0, 100),
            parking_brake_status = Program.GetRandomBoolean(),
            headlamp_status = Program.GetRandomBoolean(),
            brake_pedal_status = Program.GetRandomBoolean(),
            transmission_gear_position = Program.GetGearPos(),
            ignition_status = Program.GetRandomBoolean(),
            windshield_wiper_status = Program.GetRandomBoolean(),
            abs = Program.GetRandomBoolean(),
            timestamp = DateTime.UtcNow
          };
          string serializedString = JsonConvert.SerializeObject((object) info);
          EventData data = new EventData(Encoding.UTF8.GetBytes(serializedString));
          data.get_Properties().Add("Type", (object) ("Telemetry_" + DateTime.Now.ToLongTimeString()));
          Console.WriteLine("{0} > Sending message: {1}", (object) DateTime.Now.ToString(), (object) serializedString.ToString());
          await eventHubClient.SendAsync(data);
        }
        catch (Exception ex)
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine("{0} > Exception: {1}", (object) DateTime.Now.ToString(), (object) ex.Message);
          Console.ResetColor();
        }
        await Task.Delay(200);
      }
    }

    private static async Task SendingRandomMessages()
    {
      EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(Program.connectionString, Program.eventHubName);
      while (true)
      {
        try
        {
          string message = Guid.NewGuid().ToString();
          Console.WriteLine("{0} > Sending message: {1}", (object) DateTime.Now.ToString(), (object) message);
          await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(message)));
        }
        catch (Exception ex)
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine("{0} > Exception: {1}", (object) DateTime.Now.ToString(), (object) ex.Message);
          Console.ResetColor();
        }
        await Task.Delay(200);
      }
    }

    private static int GetSpeed(string city)
    {
      if (city.ToLower() == "bellevue")
        return Program.GetRandomWeightedNumber(100, 0, Convert.ToDouble(ConfigurationManager.AppSettings["HighSpeedProbabilityPower"]));
      return Program.GetRandomWeightedNumber(100, 0, Convert.ToDouble(ConfigurationManager.AppSettings["LowSpeedProbabilityPower"]));
    }

    private static int GetOil(string city)
    {
      if (city.ToLower() == "seattle")
        return Program.GetRandomWeightedNumber(50, 0, Convert.ToDouble(ConfigurationManager.AppSettings["LowOilProbabilityPower"]));
      return Program.GetRandomWeightedNumber(50, 0, Convert.ToDouble(ConfigurationManager.AppSettings["HighOilProbabilityPower"]));
    }

    private static int GetTirePressure(string city)
    {
      if (city.ToLower() == "seattle")
        return Program.GetRandomWeightedNumber(50, 0, Convert.ToDouble(ConfigurationManager.AppSettings["LowTyrePressureProbabilityPower"]));
      return Program.GetRandomWeightedNumber(50, 0, Convert.ToDouble(ConfigurationManager.AppSettings["HighTyrePressureProbabilityPower"]));
    }

    private static int GetEngineTemp(string city)
    {
      if (city.ToLower() == "seattle")
        return Program.GetRandomWeightedNumber(500, 0, Convert.ToDouble(ConfigurationManager.AppSettings["HighEngineTempProbabilityPower"]));
      return Program.GetRandomWeightedNumber(500, 0, Convert.ToDouble(ConfigurationManager.AppSettings["LowEngineTempProbabilityPower"]));
    }

    private static int GetOutsideTemp(string city)
    {
      if (city.ToLower() == "seattle")
        return Program.GetRandomWeightedNumber(100, 0, Convert.ToDouble(ConfigurationManager.AppSettings["LowOutsideTempProbabilityPower"]));
      return Program.GetRandomWeightedNumber(100, 0, Convert.ToDouble(ConfigurationManager.AppSettings["HighOutsideTempProbabilityPower"]));
    }

    private static int GetRandomWeightedNumber(int max, int min, double probabilityPower)
    {
      double x = new Random().NextDouble();
      return (int) Math.Floor((double) min + (double) (max + 1 - min) * Math.Pow(x, probabilityPower));
    }

    private static void GetVINMasterList()
    {
      StreamReader streamReader = new StreamReader((Stream) File.OpenRead("VINMasterList.csv"));
      while (!streamReader.EndOfStream)
      {
        string[] strArray = streamReader.ReadLine().Split(';');
        Program.VinList.Add(strArray[0]);
      }
    }

    private static string GetRandomVIN()
    {
      int index = Program.random.Next(1, Program.VinList.Count - 1);
      return Program.VinList[index];
    }

    private static string GetLocation()
    {
      List<string> stringList = new List<string>()
      {
        "Seattle",
        "Redmond",
        "Bellevue",
        "Sammamish",
        "Bellevue",
        "Bellevue",
        "Seattle",
        "Seattle",
        "Seattle",
        "Redmond",
        "Bellevue",
        "Redmond"
      };
      int index = new Random().Next(stringList.Count);
      return stringList[index];
    }

    private static string GetGearPos()
    {
      List<string> stringList = new List<string>()
      {
        "first",
        "second",
        "third",
        "fourth",
        "fifth",
        "sixth",
        "seventh",
        "eight"
      };
      int index = new Random().Next(stringList.Count);
      return stringList[index];
    }

    private static bool GetRandomBoolean()
    {
      return new Random().Next(100) % 2 == 0;
    }
  }
}
