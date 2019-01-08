using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace CCTitanFunction
{
    public static class GetBeaconList
    {
        public class Beacondata
        {
            public string BeaconID { get; set; }
            public double MaxTempLimit { get; set; }
            public double MinTempLimit { get; set; }
            public double MaxHumLimit { get; set; }
            public double MinHumLimit { get; set; }
            public string Status { get; set; }
        }

        [FunctionName("GetBeaconList")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info($"C# HTTP trigger function processed a request - {DateTime.Now}");            

            // parse query parameter
            string deviceid = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "deviceid", true) == 0)
                .Value;
            log.Info("Inside beacon data");
           
            if (string.IsNullOrEmpty(deviceid))
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Input is Null or Empty");

            }
            List<Beacondata> BeaconList = new List<Beacondata>();
            // Connect to Sql Database           
            var ConnectionstrinG = Environment.GetEnvironmentVariable("SQLConnectionString");
            var queryString = "SELECT BeaconId,TemperatureUpperLimit,TemperatureLowerLimit,HumidityUpperLimit,HumidityLowerLimit, Status FROM Beacon_Object_Info WHERE ObjectId= @deviceid";

            using (SqlConnection conn = new SqlConnection(ConnectionstrinG))
            {
                using (SqlCommand cmd = new SqlCommand(queryString, conn))
                {
                    cmd.Parameters.Add("@deviceid", SqlDbType.Char);
                    cmd.Parameters["@deviceid"].Value = deviceid;
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Beacondata beacon = new Beacondata();
                        beacon.BeaconID = (string)reader[0];
                        beacon.MaxTempLimit = (double)reader[1];
                        beacon.MinTempLimit = (double)reader[2];
                        beacon.MaxHumLimit = (double)reader[3];
                        beacon.MinHumLimit = (double)reader[4];
                        beacon.Status = (string)reader[5];
                        BeaconList.Add(beacon);
                        break;
                    }
                    reader.Close();
                }
            }
           
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(BeaconList[0], Formatting.Indented), Encoding.UTF8, "application/json")
            };
        }

    }
}

