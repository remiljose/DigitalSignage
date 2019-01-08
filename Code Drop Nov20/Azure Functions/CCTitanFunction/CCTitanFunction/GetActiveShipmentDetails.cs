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
    public static class GetActiveShipmentDetails
    {
        [FunctionName("GetActiveShipmentDetails")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            // parse query parameter
            string shipmentID = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "ShipmentID", true) == 0)
                .Value;
            log.Info("Inside beacon data");

            if (string.IsNullOrEmpty(shipmentID))
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Input is Null or Empty");

            }

            log.Info("Connecting to DataBase");
            
            var Connectionstring = Environment.GetEnvironmentVariable("SQLConnectionString");

            ActiveShipment shipMent = new ActiveShipment();

            string SqlSelect = "SELECT[ShipmasterID],[ShipmentID],[ShipmentStatus],[CreatedBy],[CreatedTime],[SourceLoc],[DestinationLoc],[LogisticPartner],[DateofShipment],[DeliveryDate],"
                + "[InvoiceDocRef],[PONumber],[BlockchainStatus],[GatewayCount],[PalletCount],[CartonCount],[BoxCount],[ProductCount],[BeaconCount],"
                + "[CurrrentLat],[CurrrentLong],[TemperatureBreach],[HumidityBreach],[TamperBreach],[VibrationBreach],[UnreachableDevice],[IsActive] "
                + " FROM Shipping_Master WHERE  [IsActive]=1 and [ShipmentStatus] in ('Associated') and [ShipmentID] =@ShipmentID";

            using (SqlConnection conn = new SqlConnection(Connectionstring))
            {
                using (SqlCommand cmd = new SqlCommand(SqlSelect, conn))
                {
                    cmd.Parameters.Add("@ShipmentID", SqlDbType.Char);
                    cmd.Parameters["@ShipmentID"].Value = shipmentID;
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                  
                    while (reader.Read())
                    {
                        var lat = reader["CurrrentLat"];
                        var longt = reader["CurrrentLong"];
                        shipMent.ShipmasterID = (int)reader["ShipmasterID"];
                        shipMent.ShipmentID = reader["ShipmentID"].ToString();
                        shipMent.ShipmentStatus = reader["ShipmentStatus"].ToString();
                        shipMent.CreatedBy = reader["CreatedBy"].ToString();
                        shipMent.CreatedDateTime = (DateTime)reader["CreatedTime"];
                        shipMent.SourceLoc = reader["SourceLoc"].ToString();
                        shipMent.DestinationLoc = reader["DestinationLoc"].ToString();
                        shipMent.LogisticPartner = reader["LogisticPartner"].ToString();
                        shipMent.DateofShipment = (DateTime)reader["DateofShipment"];
                        shipMent.DeliveryDate = (DateTime)reader["DeliveryDate"];
                        shipMent.InvoiceDocRef = reader["InvoiceDocRef"].ToString();
                        shipMent.PONumber = reader["PONumber"].ToString();
                        shipMent.IsActive = (bool)reader["IsActive"];
                        shipMent.GatewayCount = (int)reader["GatewayCount"];
                        shipMent.PalletCount = (int)reader["PalletCount"];
                        shipMent.CartonCount = (int)reader["CartonCount"];
                        shipMent.BoxCount = (int)reader["BoxCount"];
                        shipMent.ProductCount = (int)reader["ProductCount"];
                        shipMent.BeaconCount = (int)reader["BeaconCount"];
                        shipMent.BlockchainStatus = reader["BlockchainStatus"].ToString();
                        shipMent.TemperatureBreachCount = (int)reader["TemperatureBreach"];
                        shipMent.HumidityBreachCount = (int)reader["HumidityBreach"];
                        shipMent.ShockVibrationCount = (int)reader["VibrationBreach"];
                        shipMent.TamperBreachCount = (int)reader["TamperBreach"];
                        shipMent.UnreachableDeviceCount = (int)reader["UnreachableDevice"];
                        shipMent.CurrentLatitude = (double)reader["CurrrentLat"];
                        shipMent.CurrentLongitude = (double)reader["CurrrentLong"];

                        break;
                    }
                    reader.Close();
                    conn.Close();
                }
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(shipMent, Formatting.Indented), Encoding.UTF8, "application/json")
            };
        }


    }

    public class ActiveShipment
    {
        public int ShipmasterID { get; set; }
        public string ShipmentID { get; set; }
        public string ShipmentStatus { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string SourceLoc { get; set; }
        public string DestinationLoc { get; set; }
        public string LogisticPartner { get; set; }
        public DateTime DateofShipment { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string InvoiceDocRef { get; set; }
        public string PONumber { get; set; }
        public string BlockchainStatus { get; set; }
        public string TransactionHash { get; set; }
        public bool IsActive { get; set; }
        public int GatewayCount { get; set; }
        public int PalletCount { get; set; }
        public int CartonCount { get; set; }
        public int BoxCount { get; set; }
        public int ProductCount { get; set; }
        public int BeaconCount { get; set; }
        public int TemperatureBreachCount { get; set; }
        public int HumidityBreachCount { get; set; }
        public int ShockVibrationCount { get; set; }
        public int TamperBreachCount { get; set; }
        public int UnreachableDeviceCount { get; set; }
        public double CurrentLatitude { get; set; }
        public double CurrentLongitude { get; set; }

    }
}
