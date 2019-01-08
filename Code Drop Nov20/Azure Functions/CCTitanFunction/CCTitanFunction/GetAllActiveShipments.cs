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
    public static class GetAllActiveShipments
    {
        [FunctionName("GetAllActiveShipments")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            log.Info("Connecting to DataBase");

            var Connectionstring = Environment.GetEnvironmentVariable("SQLConnectionString");

            List<ActiveShipments> shipMasterList = new List<ActiveShipments>();

            string SqlSelect = "SELECT[ShipmasterID],[ShipmentID],[ShipmentStatus],[CreatedBy],[CreatedTime],[SourceLoc],[DestinationLoc],[LogisticPartner],[DateofShipment],[DeliveryDate],"
                + "[InvoiceDocRef],[PONumber],[BlockchainStatus],[GatewayCount],[PalletCount],[CartonCount],[BoxCount],[ProductCount],[BeaconCount],"
                + "[CurrrentLat],[CurrrentLong],[TemperatureBreach],[HumidityBreach],[TamperBreach],[VibrationBreach],[UnreachableDevice],[IsActive] "
                + " FROM Shipping_Master WHERE  [IsActive]=1 and [ShipmentStatus] in ('Associated')";

            using (SqlConnection conn = new SqlConnection(Connectionstring))
            {
                using (SqlCommand cmd = new SqlCommand(SqlSelect, conn))
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        ActiveShipments shipMaster = new ActiveShipments();
                        var lat = reader["CurrrentLat"];
                        var longt = reader["CurrrentLong"];
                        shipMaster.ShipmasterID = (int)reader["ShipmasterID"];
                        shipMaster.ShipmentID = reader["ShipmentID"].ToString();
                        shipMaster.ShipmentStatus = reader["ShipmentStatus"].ToString();
                        shipMaster.CreatedBy = reader["CreatedBy"].ToString();
                        shipMaster.CreatedDateTime = (DateTime)reader["CreatedTime"];
                        shipMaster.SourceLoc = reader["SourceLoc"].ToString();
                        shipMaster.DestinationLoc = reader["DestinationLoc"].ToString();
                        shipMaster.LogisticPartner = reader["LogisticPartner"].ToString();
                        shipMaster.DateofShipment = (DateTime)reader["DateofShipment"];
                        shipMaster.DeliveryDate = (DateTime)reader["DeliveryDate"];
                        shipMaster.InvoiceDocRef = reader["InvoiceDocRef"].ToString();
                        shipMaster.PONumber = reader["PONumber"].ToString();
                        shipMaster.IsActive = (bool)reader["IsActive"];
                        shipMaster.GatewayCount = (int)reader["GatewayCount"];
                        shipMaster.PalletCount = (int)reader["PalletCount"];
                        shipMaster.CartonCount = (int)reader["CartonCount"];
                        shipMaster.BoxCount = (int)reader["BoxCount"];
                        shipMaster.ProductCount = (int)reader["ProductCount"];
                        shipMaster.BeaconCount = (int)reader["BeaconCount"];
                        shipMaster.BlockchainStatus = reader["BlockchainStatus"].ToString();
                        shipMaster.TemperatureBreachCount = (int)reader["TemperatureBreach"];
                        shipMaster.HumidityBreachCount = (int)reader["HumidityBreach"];
                        shipMaster.ShockVibrationCount = (int)reader["VibrationBreach"];
                        shipMaster.TamperBreachCount = (int)reader["TamperBreach"];
                        shipMaster.UnreachableDeviceCount = (int)reader["UnreachableDevice"];
                        shipMaster.CurrentLatitude = (double)reader["CurrrentLat"];
                        shipMaster.CurrentLongitude = (double)reader["CurrrentLong"];

                        shipMasterList.Add(shipMaster);
                    }
                    reader.Close();
                    conn.Close();
                }
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(shipMasterList, Formatting.Indented), Encoding.UTF8, "application/json")
            };
        }
    }

    public class ActiveShipments
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
        public double CurrentLatitude{ get; set; }
        public double CurrentLongitude { get; set; }

    }
}
