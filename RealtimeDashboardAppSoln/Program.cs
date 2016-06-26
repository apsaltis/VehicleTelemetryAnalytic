//Copyright Microsoft 2014

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.ExceptionHandling;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using RealtimeDashboardApp.Entity;
using RealtimeDashboardApp.Extensions;

namespace RealtimeDashboardApp
{

    //Sample to show how to use the Power BI API
    //  See also, http://docs.powerbi.apiary.io/reference

    //To run this sample

    //See How to register an app (http://go.microsoft.com/fwlink/?LinkId=519361)

    //Step 1 - Replace clientID with your client app ID. To learn how to get a client app ID, see How to register an app (http://go.microsoft.com/fwlink/?LinkId=519361)
    //Step 2 - Replace redirectUri with your redirect uri.

    class Program
    {

        //Step 1 - Replace client app ID 
        //private static string clientID = "a687b12e-3830-416b-a8ea-ab0e64a980f3";
        private static string clientID = "51d8e193-d6b8-4678-8a40-f9b3cfbbd66e";

        //Step 2 - Replace redirectUri with the uri you used when you registered your app
        private static string redirectUri = "https://login.live.com/oauth20_desktop.srf";
        
        //Power BI resource uri
        private static string resourceUri = "https://analysis.windows.net/powerbi/api";             
        
        //OAuth2 authority
        //private static string authority = "https://login.windows.net/microsoft.onmicrosoft.com/oauth2/authorize";
        private static string authority = "https://login.windows.net/common/oauth2/authorize";

        //Uri for Power BI datasets
        private static string datasetsUri = "https://api.powerbi.com/v1.0/myorg/datasets";

        private static AuthenticationContext authContext = null;
        private static string token = String.Empty;

        //.NET Class Example:
        private static string datasetName = ConfigurationManager.AppSettings["PowerBI_DataSetName"];
        private static string AmlDatatable = ConfigurationManager.AppSettings["PowerBI_RealTimeVehicleHealthAnomaly_Table"];

        private static string EventHubName = ConfigurationManager.AppSettings["EventHubName"];
        private static string EventHubConnString = ConfigurationManager.AppSettings["EventHubConnString"];

        private static string AMLUrl = ConfigurationManager.AppSettings["AmlUrl"];
        private static string AMLKey = ConfigurationManager.AppSettings["AMLKey"];

        private static System.Timers.Timer _timer;
        private static int currentRecord;
        private static bool FlushData = false;

        static void Main(string[] args)
        {
            //_timer = new System.Timers.Timer(1000);
            //_timer.Elapsed += _timer_Elapsed;
            //_timer.Interval = 1000;
            //_timer.Enabled = true;

            if (args.Length > 0)
            {
                foreach (var arg in args)
                {
                    if (arg.ToLower().Contains("flushdata"))
                    {
                        FlushData = true;
                    }
                }
            }

            try
            {
                datasetsUri = TestConnection();

            }
            catch (Exception ex)
            {
                datasetsUri = "";
            }
            
            if (!string.IsNullOrEmpty(datasetsUri))
            {
                CreateDataset();
                
                if (FlushData)
                {
                    ClearRows();
                }

                Console.Write("Registering eventhub listeners...\n");
                var r = new EventReceiver(EventHubName, EventHubConnString);
                r.MessageProcessingWithPartitionDistribution();
                Console.Write("registered.\n");

                Console.WriteLine("Waiting for events...");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Action cancelled...Press any key to exit.");
                Console.ReadLine();
            }
        }

        static void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timer.Enabled = false;
            Console.WriteLine("Processed events " + EventIncreamenter.EventCounter);
            EventIncreamenter.EventCounter = 0;
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Enabled = true;
        }

        private static void ExtractAPIOutPut()
        {
            var result =
                "{\"Results\":{\"output1\":{\"type\":\"table\",\"value\":{\"ColumnNames\":[\"vin\",\"model\",\"timestamp\",\"outsideTemperature\",\"engineTemperature\",\"speed\",\"fuel\",\"engineoil\",\"tirepressure\",\"odometer\",\"city\",\"accelerator_pedal_position\",\"parking_brake_status\",\"headlamp_status\",\"brake_pedal_status\",\"transmission_gear_position\",\"ignition_status\",\"windshield_wiper_status\",\"abs\",\"MaintenanceLabel\",\"MaintenanceProbability\",\"RecallLabel\",\"RecallProbability\"],\"ColumnTypes\":[\"String\",\"String\",\"DateTime\",\"Double\",\"Double\",\"Double\",\"Int32\",\"Double\",\"Double\",\"Int32\",\"String\",\"Int32\",\"Int32\",\"Int32\",\"Int32\",\"String\",\"Int32\",\"Int32\",\"Int32\",\"Double\",\"Double\",\"Double\",\"Double\"],\"Values\":[[\"O1NWZUDN720THHPDX\",\"Station Wagon\",\"6/18/2015 4:10:32 PM\",\"76\",\"282\",\"58\",\"6\",\"24\",\"27\",\"20557\",\"Seattle\",\"73\",\"1\",\"1\",\"1\",\"fifth\",\"1\",\"1\",\"1\",\"0\",\"0.0240385048091412\",\"0\",\"0.102526672184467\"]]}}}}";
            var obj = JsonConvert.DeserializeObject<dynamic>(result.ToString());
            string[] finalResult = obj.Results.output1.value.Values.ToString().Split(',');

            string maintenanceLabel = finalResult[19].Replace("\r", "").Replace("\n", "").Replace("\"", "");
            maintenanceLabel = maintenanceLabel.Trim();

            string maintenanceProbability = finalResult[20].Replace("\r", "")
                .Replace("\n", "")
                .Replace("]", "")
                .Replace("\"", "");
            maintenanceProbability = maintenanceProbability.Trim();

            string recallLabel = finalResult[21].Replace("\r", "").Replace("\n", "").Replace("\"", "");
            recallLabel = recallLabel.Trim();

            string recallProbability = finalResult[22].Replace("\r", "").Replace("\n", "").Replace("]", "").Replace("\"", "");
            recallProbability = recallProbability.Trim();
        }

        /// <summary>
        /// Create a Power BI schema from a SQL View.
        /// </summary>

        private static string TestConnection()
        {
            // Check the connection for redirects
            HttpWebRequest request = System.Net.WebRequest.Create(datasetsUri) as System.Net.HttpWebRequest;
            request.KeepAlive = true;
            request.Method = "GET";
            request.ContentLength = 0;
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", String.Format("Bearer {0}", AccessToken));
            request.AllowAutoRedirect = false;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
           if (response.StatusCode == HttpStatusCode.TemporaryRedirect)
            {
                return response.Headers["Location"];
            }
            return datasetsUri;

        }

        static string AccessToken
        {
            get
            {
                if (token == String.Empty)
                {
                    TokenCache TC = new TokenCache();
                    authContext = new AuthenticationContext(authority,TC);
                    token = authContext.AcquireToken(resourceUri, clientID, new Uri(redirectUri)).AccessToken.ToString();
                    
                }
                else
                {
                    token = authContext.AcquireTokenSilent(resourceUri, clientID).AccessToken;
                }

                return token;
            }
        }

        static List<Object> GetAllDatasets()
        {
            List<Object> datasets = null;

            //In a production application, use more specific exception handling.
            try
            {
                //Create a GET web request to list all datasets
                HttpWebRequest request = DatasetRequest(datasetsUri, "GET", AccessToken);

                //Get HttpWebResponse from GET request
                string responseContent = GetResponse(request);

                //Get list from response
                datasets = responseContent.ToObject<List<Object>>();

            }
            catch (Exception ex)
            {               
                //In a production application, handle exception
            }

            return datasets;
        }

        public static void PushEventHubAnomalyRecords(RealTimeVehicleHealthAnomalyReport record)
        {
            //Get dataset id from a table name
            string datasetId = GetAllDatasets().Datasets(datasetName).First()["id"].ToString();
            
            record= CallMlApi(record);

            var records = new List<RealTimeVehicleHealthAnomalyReport> {record};

            HttpWebRequest request = DatasetRequest(String.Format("{0}/{1}/tables/{2}/rows", datasetsUri, datasetId, AmlDatatable), "POST", AccessToken);
            var requestData = new SendRequest
            {
                rows = records
            };
            
            PostRequest(request, JsonConvert.SerializeObject(requestData));
        }

        private static RealTimeVehicleHealthAnomalyReport CallMlApi(RealTimeVehicleHealthAnomalyReport record)
        {
            
            using (var client = new HttpClient())
            {
                var scoreRequest = new
                {
                    Inputs = new Dictionary<string, StringTable>()
                    {
                        {
                            "data",
                            new StringTable()
                            {
                                ColumnNames =
                                    new[]
                                    {
                                        "vin", "model", "timestamp", "outsidetemperature", "enginetemperature", "speed",
                                        "fuel", "engineoil", "tirepressure", "odometer", "city",
                                        "accelerator_pedal_position", "parking_brake_status", "headlamp_status",
                                        "brake_pedal_status", "transmission_gear_position", "ignition_status",
                                        "windshield_wiper_status", "abs"
                                    },
                                Values =
                                    new[,]
                                    {
                                        {
                                            record.vin, record.Model, record.timestamp, record.outsideTemperature.ToString(),
                                            record.engineTemperature.ToString(), record.speed.ToString(), record.fuel.ToString(), record.engineoil.ToString(),
                                            record.tirepressure.ToString(), record.odometer.ToString(), record.city,
                                            record.accelerator_pedal_position.ToString(),

                                            record.parking_brake_status.ToString().ToLower()
                                                .Replace("true", "1")
                                                .Replace("false", "0"),
                                            record.headlamp_status.ToString().ToLower().Replace("true", "1").Replace("false", "0"),
                                            record.brake_pedal_status.ToString().ToLower()
                                                .Replace("true", "1")
                                                .Replace("false", "0"),
                                            record.transmission_gear_position.ToLower(),
                                            record.ignition_status.ToString().ToLower().Replace("true", "1").Replace("false", "0"),
                                            record.windshield_wiper_status.ToString().ToLower()
                                                .Replace("true", "1")
                                                .Replace("false", "0"),
                                            record.abs.ToString().ToLower().Replace("true", "1").Replace("false", "0")
                                        },
                                    }
                            }
                        },
                    },
                    GlobalParameters = new Dictionary<string, string>()
                    {}
                };

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AMLKey);

                client.BaseAddress = new Uri(AMLUrl);

                HttpResponseMessage response = client.PostAsJsonAsync("", scoreRequest).Result;
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var result = response.Content.ReadAsStringAsync();
                        string responsefromml = result.Result;
                        var obj = JsonConvert.DeserializeObject<dynamic>(responsefromml);

                        string[] finalResult = obj.Results.data.value.Values.ToString().Split(',');

                        string maintenanceLabel = finalResult[19].Replace("\r", "").Replace("\n", "").Replace("\"", "");
                        maintenanceLabel = maintenanceLabel.Trim();
                        record.MaintenanceLabel = Convert.ToInt32(maintenanceLabel);

                        string maintenanceProbability =
                            finalResult[20].Replace("\r", "").Replace("\n", "").Replace("]", "").Replace("\"", "");
                        maintenanceProbability = maintenanceProbability.Trim();
                        record.MaintenanceProbability = Convert.ToDouble(maintenanceProbability);

                        string recallLabel = finalResult[21].Replace("\r", "").Replace("\n", "").Replace("\"", "");
                        recallLabel = recallLabel.Trim();
                        record.RecallLabel = Convert.ToInt32(recallLabel);

                        string recallProbability =
                            finalResult[22].Replace("\r", "").Replace("\n", "").Replace("]", "").Replace("\"", "");
                        recallProbability = recallProbability.Trim();
                        record.RecallProbability = Convert.ToDouble(recallProbability);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception while parsing AML output: {0}", response.StatusCode);
                    }
                }
                else
                {
                    Console.WriteLine("Failed AML calling with status code: {0}", response.StatusCode);
                }
            }


            return record;
        }

        static void CreateDataset()
        {
            Console.Write("Checking dataset {0}...", datasetName);
            //In a production application, use more specific exception handling.           
            try
            {               
                //Create a POST web request to list all datasets
                HttpWebRequest request = DatasetRequest(datasetsUri, "POST", AccessToken);

                var datasets = GetAllDatasets().Datasets(datasetName);

                if (!datasets.Any())
                {
                    Console.Write("does not exist.\n");
                    Console.Write("Creating dataset {0}...", datasetName);
                    var tables = new List<object> { new RealTimeVehicleHealthAnomalyReport() };

                    string jsonString = JSONBuilder.CreateTableSchema(datasetName, tables);

                    PostRequest(request, jsonString);
                    //POST request using the json schema from Product
                   Console.Write("created.\n");
                }
                else
                {
                    Console.Write("exists.\n");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            } 
        }

        static void ClearRows()
        {
            //Get dataset id from a table name
            string datasetId = GetAllDatasets().Datasets(datasetName).First()["id"].ToString();
            Console.Write("Flushing data...");
            //In a production application, use more specific exception handling. 
            try
            {
                //Create a DELETE web request
                HttpWebRequest request1 = DatasetRequest(String.Format("{0}/{1}/tables/{2}/rows", datasetsUri, datasetId, AmlDatatable), "DELETE", AccessToken);
                request1.ContentLength = 0;

                GetResponse(request1);
                Console.Write("done.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            } 
        }

        private static string PostRequest(HttpWebRequest request, string json)
        {
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(json);
            request.ContentLength = byteArray.Length;

            //Write JSON byte[] into a Stream
            using (Stream writer = request.GetRequestStream())
            {
                writer.Write(byteArray, 0, byteArray.Length);
            }
            return GetResponse(request);
        }

        private static string GetResponse(HttpWebRequest request)
        {
            string response = string.Empty;

            using (HttpWebResponse httpResponse = request.GetResponse() as System.Net.HttpWebResponse)
            {
                //Get StreamReader that holds the response stream
                using (StreamReader reader = new System.IO.StreamReader(httpResponse.GetResponseStream()))
                {
                    response = reader.ReadToEnd();                 
                }
            }

            return response;
        }

        private static HttpWebRequest DatasetRequest(string datasetsUri, string method, string authorizationToken)
        {
            HttpWebRequest request = System.Net.WebRequest.Create(datasetsUri) as System.Net.HttpWebRequest;
            request.KeepAlive = true;
            request.Method = method;
            request.ContentLength = 0;
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", String.Format("Bearer {0}", authorizationToken));

            return request;
        }

        private static HttpWebRequest DatasetRequest(string datasetsUri, string method, string refreshToken,bool refresh)
        {

            HttpWebRequest request = System.Net.WebRequest.Create(datasetsUri) as System.Net.HttpWebRequest;

            request.KeepAlive = true;

            request.Method = method;

            request.ContentLength = 0;

            request.ContentType = "application/json";

            request.Headers.Add("Authorization", String.Format("grant_type=refresh_token&refresh_token={0}", refreshToken));

            return request;

        }
    }

    public class EventIncreamenter
    {
        public static int EventCounter = 0;

        public static void IncreamentEventCount(int i)
        {
            EventCounter = EventCounter + i;
        }

        public static void ResetCounter()
        {
            EventCounter = 0;
        }

    }
    public class StringTable
    {
        public string[] ColumnNames { get; set; }
        public string[,] Values { get; set; }
    }
}
