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
    public static class CCBeaconList
    {
        public class CCBeaconListData
        {
            public string BeaconId { get; set; }            
            public double TemperatureUpperLimit { get; set; }
            public double TemperatureLowerLimit { get; set; }
            public double HumidityUpperLimit { get; set; }
            public double HumidityLowerLimit { get; set; }
        }
        [FunctionName("GetAssociateBeaconList")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info($"C# HTTP trigger function processed a request - {DateTime.UtcNow}");

            // parse query parameter
            string MacId = req.GetQueryNameValuePairs()
                    .FirstOrDefault(q => string.Compare(q.Key, "MacId", true) == 0)
                    .Value;

            if (string.IsNullOrEmpty(MacId))
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Value is null or empty");
            }

            string sprocname = "Sp_GetBeacons";            

            var ConnectionstrinG = Environment.GetEnvironmentVariable("SQLConnectionString");
            List<CCBeaconListData> CCBeaconLists = new List<CCBeaconListData>();
            SqlConnection conn = new SqlConnection(ConnectionstrinG);
            SqlCommand commanD;
            SqlParameter parameteR;

            commanD = new SqlCommand(sprocname, conn);
            commanD.CommandType = CommandType.StoredProcedure;

            parameteR = commanD.Parameters.Add("@MacId", SqlDbType.VarChar, 100);
            parameteR.Direction = ParameterDirection.Input;           
            parameteR.Value = MacId;

            conn.Open();
            SqlDataReader myReader = commanD.ExecuteReader();
            while (myReader.Read())
            {
                CCBeaconListData beaconList = new CCBeaconListData {

                    BeaconId = (string)myReader[0],
                    TemperatureUpperLimit = (double)myReader[1],
                    TemperatureLowerLimit = (double)myReader[2],
                    HumidityUpperLimit = (double)myReader[3],
                    HumidityLowerLimit = (double)myReader[4]                    
                };

                CCBeaconLists.Add(beaconList);                                                                                                    
            }
            myReader.Close();           
            conn.Close();                     

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(CCBeaconLists, Formatting.Indented), Encoding.UTF8, "application/json")
            };

        }
    }


}