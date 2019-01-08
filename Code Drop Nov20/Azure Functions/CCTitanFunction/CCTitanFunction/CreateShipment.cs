using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Data.SqlClient;


namespace CCTitanFunction
{
    public static class CreateShipment
    {
        [FunctionName("AddShipmentDetails")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            
            List<Product> productList = new List<Product>();

            // Get request body

            dynamic body = await req.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(body))
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Value is null or empty");
            }

            var shipMaster = JsonConvert.DeserializeObject<ShipMaster>(body as string);        
           
            string shipmentID = shipMaster?.ShipmentID;
            string shipmentStatus = shipMaster?.ShipmentStatus;
            string createdBy = shipMaster?.CreatedBy;
            string sourceLoc = shipMaster?.SourceLoc;
            string destinationLoc = shipMaster?.DestinationLoc;
            string logisticPartner = shipMaster?.LogisticPartner;
            var dateofShipment = shipMaster?.DateofShipment;
            var deliveryDate = shipMaster?.DeliveryDate;
            string invoiceDocRef = shipMaster?.InvoiceDocRef;
            string pONumber = shipMaster?.PONumber;
            string blockchainStatus = shipMaster?.BlockchainStatus;
            string transactionHash = shipMaster?.TransactionHash;          

            productList = shipMaster?.ProductList;


            if (string.IsNullOrEmpty(shipmentID) || string.IsNullOrEmpty(blockchainStatus))
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Value is null or empty");
            }

            log.Info("Connecting to DataBase");

            var Connectionstring = Environment.GetEnvironmentVariable("SQLConnectionString");
            SqlConnection conn = new SqlConnection(Connectionstring);
            SqlCommand commanD;

            string SqlInsert = "INSERT INTO Shipping_Master([ShipmentID],[ShipmentStatus],[CreatedBy],[CreatedTime],[SourceLoc],[DestinationLoc],[LogisticPartner]"+
                             ",[DateofShipment],[DeliveryDate],[InvoiceDocRef],[PONumber],[BlockchainStatus],[TransactionHash],[IsActive],[GatewayCount],[PalletCount],[CartonCount],[BoxCount],[ProductCount],[BeaconCount])" +
                            " VALUES(@ShipmentID, @ShipmentStatus, @CreatedBy, @CreatedDateTime, @SourceLoc, @DestinationLoc, @LogisticPartner," +
                            " @DateofShipment, @DeliveryDate, @InvoiceDocRef, @PONumber, @BlockchainStatus, @TransactionHash, @IsActive,@GatewayCount,@PalletCount,@CartonCount,@BoxCount,@ProductCount,@BeaconCount)";
          

            try
            {
                commanD = new SqlCommand(SqlInsert, conn);

                commanD.Parameters.Add("@ShipmentID", SqlDbType.NVarChar).Value = shipmentID;
                commanD.Parameters.Add("@ShipmentStatus", SqlDbType.NVarChar).Value = shipmentStatus;
                commanD.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = createdBy;
                commanD.Parameters.Add("@CreatedDateTime", SqlDbType.DateTime).Value = DateTime.Now.ToString();
                commanD.Parameters.Add("@SourceLoc", SqlDbType.NVarChar).Value = sourceLoc;
                commanD.Parameters.Add("@DestinationLoc", SqlDbType.NVarChar).Value = destinationLoc;
                commanD.Parameters.Add("@LogisticPartner", SqlDbType.NVarChar).Value = logisticPartner;
                commanD.Parameters.Add("@DateofShipment", SqlDbType.DateTime).Value = dateofShipment;
                commanD.Parameters.Add("@DeliveryDate", SqlDbType.DateTime).Value = deliveryDate;
                commanD.Parameters.Add("@InvoiceDocRef", SqlDbType.NVarChar).Value = invoiceDocRef;
                commanD.Parameters.Add("@PONumber", SqlDbType.NVarChar).Value = pONumber;
                commanD.Parameters.Add("@BlockchainStatus", SqlDbType.NVarChar).Value = blockchainStatus;
                commanD.Parameters.Add("@TransactionHash", SqlDbType.NVarChar).Value = transactionHash;
                commanD.Parameters.Add("@IsActive", SqlDbType.Bit).Value = 1;

                commanD.Parameters.Add("@GatewayCount", SqlDbType.Int).Value = 0;
                commanD.Parameters.Add("@PalletCount", SqlDbType.Int).Value = 0;
                commanD.Parameters.Add("@CartonCount", SqlDbType.Int).Value = 0;
                commanD.Parameters.Add("@BoxCount", SqlDbType.Int).Value = 0;
                commanD.Parameters.Add("@ProductCount", SqlDbType.Int).Value = 0;
                commanD.Parameters.Add("@BeaconCount", SqlDbType.Int).Value = 0;
                
                conn.Open();
                commanD.ExecuteNonQuery();
               
                string qrySelectShipId = "SELECT ShipmasterID FROM Shipping_Master WHERE ShipmentID= @ShipmentID";
                commanD.Parameters.Clear();
                commanD = new SqlCommand(qrySelectShipId, conn);
                commanD.Parameters.Add("@ShipmentID", SqlDbType.NVarChar).Value = shipmentID;
                      
                var shipMasterId = commanD.ExecuteScalar();  

                if (productList.Count() > 0)
                {
                    SqlInsert = "INSERT INTO Shipment_Product([ShipMasterID] , [ProductID], [Product], [Quantity]) Values(@ShipMasterID, @ProductID, @Product, @Quantity)";
                   
                    foreach (Product item in productList)
                    {
                        commanD.Parameters.Clear();
                        commanD = new SqlCommand(SqlInsert, conn);
                        commanD.Parameters.Add("@ShipMasterID", SqlDbType.Int).Value = (int)shipMasterId;
                        commanD.Parameters.Add("@ProductID", SqlDbType.NVarChar).Value = item.ProductId;
                        commanD.Parameters.Add("@Product", SqlDbType.NVarChar).Value = item.ProductName;
                        commanD.Parameters.Add("@Quantity", SqlDbType.Int).Value = item.Quantity;
                        commanD.ExecuteNonQuery();
                    }

                }

                conn.Close();
                return req.CreateResponse(HttpStatusCode.OK, "Created shipment details.");
            }
            catch (Exception ex)
            {

                log.Info("Exception Occured", ex.ToString());
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Cannot add the record because of an Exception");
            }

        }
    }
    public class Product
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }

    }
    public class ShipMaster
    {
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
        public List<Product> ProductList { get; set; }
    }
}
