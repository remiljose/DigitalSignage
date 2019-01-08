using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CCTitanFunction
{
    public static class GetDasboardSummary
    {
        [FunctionName("GetDasboardSummary")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            var Connectionstring = Environment.GetEnvironmentVariable("SQLConnectionString");

            DasboardSummary summary = new DasboardSummary();

            string SqlSelect = "SELECT COUNT([ShipmentID]) as CurrentShipments,SUM([TemperatureBreach]) as TemperatureBreach,"
                + "SUM([HumidityBreach]) as HumidityBreach ,SUM([VibrationBreach]) as ShockVibration,SUM([TamperBreach]) as TamperBreach,"
                + "SUM([UnreachableDevice]) as UnreachableDevice  FROM [dbo].[Shipping_Master] "
                + " WHERE  [IsActive]=1 and [ShipmentStatus] in ('Associated')";

            using (SqlConnection conn = new SqlConnection(Connectionstring))
            {
                using (SqlCommand cmd = new SqlCommand(SqlSelect, conn))
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {                      
                        summary.CurrentShipments = (int)reader["CurrentShipments"];
                        summary.TemperatureBreachCount = (int)reader["TemperatureBreach"];
                        summary.HumidityBreachCount = (int)reader["HumidityBreach"];
                        summary.ShockVibrationCount = (int)reader["ShockVibration"];
                        summary.TamperBreachCount = (int)reader["TamperBreach"];
                        summary.UnreachableDeviceCount = (int)reader["UnreachableDevice"];
                       
                        break;                       
                    }
                    reader.Close();
                    conn.Close();
                }
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(summary, Formatting.Indented), Encoding.UTF8, "application/json")
            };
        }
    }

    public class DasboardSummary
    {
        public int CurrentShipments { get; set; }
        public int TemperatureBreachCount { get; set; }
        public int HumidityBreachCount { get; set; }
        public int ShockVibrationCount { get; set; }
        public int TamperBreachCount { get; set; }
        public int UnreachableDeviceCount { get; set; }
       
    }
}
