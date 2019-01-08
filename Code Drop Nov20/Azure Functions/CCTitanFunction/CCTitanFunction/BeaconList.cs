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
    public static class BeaconList
    {
        public class BeaconIdList
        {
            public string BeaconID { get; set; }
            
        }
        [FunctionName("BeaconList")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info($"C# HTTP trigger function processed a request - {DateTime.Now}");

            // parse query parameter
            string ObjectID = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "ObjectID", true) == 0)
                .Value;
            log.Info("Inside beacon data");

            if (string.IsNullOrEmpty(ObjectID))
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Input is Null or Empty");

            }

            List<BeaconIdList> beaconlisT = new List<BeaconIdList>();

            var ConnectionstrinG = Environment.GetEnvironmentVariable("SQLConnectionString");
            var listBeacon = "SELECT BeaconId FROM Beacon_Object_Info WHERE ObjectId= @ObjectID";

            SqlConnection conn = new SqlConnection(ConnectionstrinG);
            SqlCommand commanD;
            commanD = new SqlCommand(listBeacon, conn);
            conn.Open();
            commanD.Parameters.Add("@ObjectID", SqlDbType.NVarChar);
            commanD.Parameters["@ObjectID"].Value = ObjectID;
            SqlDataReader reader = commanD.ExecuteReader();
            while (reader.Read())
            {                              
                BeaconIdList beacon = new BeaconIdList();
                beacon.BeaconID = (string)reader[0];
                beaconlisT.Add(beacon);
                
            }

            reader.Close();
            conn.Close();


            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(beaconlisT[0], Formatting.Indented), Encoding.UTF8, "application/json")
            };
        }
    }
}
