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
using System.Threading.Tasks;

namespace CCTitanFunction
{
    public static class GetObjectList
    {
        [FunctionName("GetObjectList")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            // parse query parameter
            string ObjectType = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "ObjectType", true) == 0)
                .Value;

            string ShipmentId = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "ShipmentId", true) == 0)
                .Value;

            log.Info("Inside beacon data");

            if (string.IsNullOrEmpty(ObjectType) && string.IsNullOrEmpty(ShipmentId))
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Input is Null or Empty");

            }

            List<Objectlist> objectlisT = new List<Objectlist>();
            var ConnectionstrinG = Environment.GetEnvironmentVariable("SQLConnectionString");
            var objecTlist = "SELECT ObjectId FROM Beacon_Object_Info where ObjectType = @ObjectType AND ShipMasterId = @ShipmentId "+
                "and Beacon_Obj_Id not in (SELECT [Associated_Object_Id] FROM Shipping_Association where ObjectType = @ObjectType AND ShipMasterId = @ShipmentId )";

            SqlConnection conn = new SqlConnection(ConnectionstrinG);
            SqlCommand commanD;
            commanD = new SqlCommand(objecTlist, conn);
            conn.Open();
            commanD.Parameters.Add("@ObjectType", SqlDbType.NVarChar);
            commanD.Parameters["@ObjectType"].Value = ObjectType;
            commanD.Parameters.Add("@ShipmentId", SqlDbType.NVarChar);
            commanD.Parameters["@ShipmentId"].Value = ShipmentId;
            SqlDataReader reader = commanD.ExecuteReader();

            while (reader.Read())
            {
                Objectlist objectlist = new Objectlist();
                objectlist.ObjectiD = (string)reader[0];
                objectlisT.Add(objectlist);

            }
            reader.Close();
            conn.Close();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(objectlisT, Formatting.Indented), Encoding.UTF8, "application/json")
            };

        }

        public class Objectlist
        {
            public string ObjectiD { get; set; }
        }
    }
}
