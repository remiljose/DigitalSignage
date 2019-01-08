using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace CCTitanFunction
{
    public static class UpdateBeacon
    {
        [FunctionName("UpdateBeaconDetails")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
           log.Info($"Updating Beacon Details Function Triggered - {DateTime.Now}");

            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();
            string BeaconId = data?.BeaconId;
            string ObjectId = data?.ObjectId;
            string ObjectType = data?.ObjectType;
            string Content = data?.Content;
            string TemperatureMin = data?.TemperatureMin;
            string TemperatureMax = data?.TemperatureMax;
            string HumidityMin = data?.HumidityMin;
            string HumidityMax = data?.HumidityMax;

            string BeaconID = BeaconId;
            string ObjectiD = ObjectId;

            if (string.IsNullOrEmpty(BeaconId) || string.IsNullOrEmpty(ObjectId))
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Value is null or empty");
            }

            log.Info("Connecting to DataBase");

            var ConnectionstrinG = Environment.GetEnvironmentVariable("SQLConnectionString");
            string UpdateTempHum = "UPDATE Beacon_Object_Info SET TemperatureLowerLimit = @TemperatureLowerLimit, TemperatureUpperLimit = @TemperatureUpperLimit, HumidityUpperLimit = @HumidityUpperLimit, HumidityLowerLimit = @HumidityLowerLimit WHERE BeaconId = @BeaconId";
            SqlConnection conn = new SqlConnection(ConnectionstrinG);
            SqlCommand commanD;            

            try
            {
                if (BeaconId == BeaconID && ObjectId == ObjectiD)
                {
                    commanD = new SqlCommand(UpdateTempHum, conn);
                    commanD.Parameters.Add("@BeaconId", SqlDbType.NVarChar).Value = BeaconId;
                    //commanD.Parameters.Add("@ObjectId", SqlDbType.NVarChar).Value = ObjectId;
                    //commanD.Parameters.Add("@ObjectType", SqlDbType.NVarChar).Value = ObjectType;
                    commanD.Parameters.Add("@TemperatureLowerLimit", SqlDbType.Float).Value = TemperatureMin;
                    commanD.Parameters.Add("@TemperatureUpperLimit", SqlDbType.Float).Value = TemperatureMax;
                    commanD.Parameters.Add("@HumidityUpperLimit", SqlDbType.Float).Value = HumidityMax;
                    commanD.Parameters.Add("@HumidityLowerLimit", SqlDbType.Float).Value = HumidityMin;
                    //commanD.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = "MyName";
                    //commanD.Parameters.Add("@CreatedDateTime", SqlDbType.DateTime).Value = DateTime.Now.ToString();
                    //commanD.Parameters.Add("@UpdatedBy", SqlDbType.NVarChar).Value = "HisName";
                    //commanD.Parameters.Add("@UpdatedDateTime", SqlDbType.DateTime).Value = DateTime.Now.ToString();
                    //commanD.Parameters.Add("@Content", SqlDbType.NVarChar).Value = Content;
                    conn.Open();
                    commanD.ExecuteNonQuery();
                    conn.Close();

                    return req.CreateResponse(HttpStatusCode.OK, "Added Beacon details to the dB");
                }
                else
                    return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Cannot Update the details");
            }
            catch (Exception ex)
            {

                log.Info("Exception Occured", ex.ToString());
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Cannot Update the Table because of an Exception");
            }
            
          
        }
    }
}
