using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace CCTitanFunction
{
    public static class AssignPalletAndGateways
    {
        [FunctionName("AssignPalletAndGateways")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            List<PalletGateway> palletList = new List<PalletGateway>();

            // Get request body

            dynamic body = await req.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(body))
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Value is null or empty");
            }
            try
            {
                var palletObject = JsonConvert.DeserializeObject<AssociatedPallet>(body as string);

                palletList = palletObject?.PalletList;

                var Connectionstring = Environment.GetEnvironmentVariable("SQLConnectionString");
                SqlConnection conn = new SqlConnection(Connectionstring);
                SqlCommand command;

                if (palletList.Count() > 0)
                {

                    conn.Open();
                    foreach (PalletGateway item in palletList)
                    {
                        string qrySelectId = "SELECT [Beacon_Obj_Id] FROM [Beacon_Object_Info] WHERE [ObjectId] = @PalletId and ShipmasterID= @ShipmasterID";

                        command = new SqlCommand(qrySelectId, conn);
                        command.Parameters.Clear();
                        command.Parameters.Add("@PalletId", SqlDbType.NVarChar).Value = item.PalletId;
                        command.Parameters.Add("@ShipmasterID", SqlDbType.Int).Value = palletObject?.ShipmasterID;
                        var palletBeacObjId = command.ExecuteScalar();

                        qrySelectId = "SELECT [DeviceId] FROM [DeviceInfo] WHERE [MacId] = @MacId";
                        command = new SqlCommand(qrySelectId, conn);
                        command.Parameters.Clear();
                        command.Parameters.Add("@MacId", SqlDbType.NVarChar).Value = item.MacId;
                        var gatewayDeviceId = command.ExecuteScalar();

                        qrySelectId = "SELECT [DeviceId] FROM [DeviceInfo] WHERE [MacId] = @trackerId";
                        command = new SqlCommand(qrySelectId, conn);
                        command.Parameters.Clear();
                        command.Parameters.Add("@trackerId", SqlDbType.NVarChar).Value = item.TrackerId;
                        var trackerDeviceId = command.ExecuteScalar();

                        string SqlInsert = "INSERT INTO [Pallet_Gateway_Association]([ShipMasterID],[Pallet_Id],[Gateway_Mac_Id],[Tracker_Id],[CreatedBy],[CreatedTime])" +
                                     " VALUES(@ShipMasterID,@Pallet_ObjectId, @Gateway_Object_Id,@Trackr_Object_Id, @CreatedBy, @CreatedDateTime)";

                        command = new SqlCommand(SqlInsert, conn);
                        command.Parameters.Clear();
                        command.Parameters.Add("@ShipMasterID", SqlDbType.Int).Value = palletObject?.ShipmasterID;
                        command.Parameters.Add("@Pallet_ObjectId", SqlDbType.Int).Value = (int)palletBeacObjId;
                        command.Parameters.Add("@Gateway_Object_Id", SqlDbType.Int).Value = (int)gatewayDeviceId;
                        command.Parameters.Add("@Trackr_Object_Id", SqlDbType.Int).Value = (int)trackerDeviceId;
                        command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = "r";
                        command.Parameters.Add("@CreatedDateTime", SqlDbType.DateTime).Value = DateTime.Now.ToString();

                        command.ExecuteNonQuery();
                    }

                }
                conn.Close();

                int productCount = 0, cartonCount = 0, boxCount = 0;
                int palletCount = 0, gatewayCount = 0, trackerCount = 0;

                string qrySelect = "SELECT count(Associated_Object_Id) as ObjectCount,associatedType from Shipping_Association where ShipmasterId = @ShipMasterID group by associatedType";
                using (conn = new SqlConnection(Connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand(qrySelect, conn))
                    {
                        conn.Open();
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@ShipMasterID", SqlDbType.Int).Value = palletObject?.ShipmasterID;
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            if (((string)reader["associatedType"]).ToLower() == "product")
                                productCount = (int)reader["ObjectCount"];
                            if (((string)reader["associatedType"]).ToLower() == "box")
                                boxCount = (int)reader["ObjectCount"];
                            if (((string)reader["associatedType"]).ToLower() == "carton")
                                cartonCount = (int)reader["ObjectCount"];
                            break;
                        }
                        reader.Close();
                    }
                }

                qrySelect = "SELECT count([Pallet_Id]) as PalletCount,count([Tracker_Id]) as TrackerCount ,count([Gateway_Mac_Id]) as GatewayCount " +
                        "from [dbo].[Pallet_Gateway_Association] where ShipmasterId = @ShipMasterID ";
                using (conn = new SqlConnection(Connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand(qrySelect, conn))
                    {
                        conn.Open();
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@ShipMasterID", SqlDbType.Int).Value = palletObject?.ShipmasterID;
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {

                            palletCount = (int)reader["PalletCount"];
                            gatewayCount = (int)reader["GatewayCount"];
                            trackerCount = (int)reader["TrackerCount"];
                            break;
                        }
                        reader.Close();
                    }
                }


                //Updating Shipping Mater

                string quryUpdate = "UPDATE [Shipping_Master] SET [GatewayCount] = @gatewayCount, [PalletCount] = @palletCount," +
                    "[CartonCount] = @cartonCount, [BoxCount] = @boxCount ,[ProductCount]=@productCount WHERE [ShipmasterID] = @ShipMasterID";

                command = new SqlCommand(quryUpdate, conn);
                command.Parameters.Clear();
                command.Parameters.Add("@gatewayCount", SqlDbType.Int).Value = gatewayCount;
                command.Parameters.Add("@palletCount", SqlDbType.Int).Value = palletCount;
                command.Parameters.Add("@cartonCount", SqlDbType.Int).Value = cartonCount;
                command.Parameters.Add("@boxCount", SqlDbType.Int).Value = boxCount;
                command.Parameters.Add("@productCount", SqlDbType.Int).Value = productCount;
                command.Parameters.Add("@ShipMasterID", SqlDbType.Int).Value = palletObject?.ShipmasterID;
                conn.Open();
                command.ExecuteNonQuery();
                conn.Close();
                return req.CreateResponse(HttpStatusCode.OK, "Successfully Created Association.");
            }            
             catch (Exception ex)
            {

                log.Info("Exception Occured", ex.ToString());
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Cannot Update the Association Table because of an Exception");
            }
        }
    }

    public class PalletGateway
    {
        public string PalletId { get; set; }
        public string MacId { get; set; }
        public string TrackerId { get; set; }

    }
    public class AssociatedPallet
    {
        public int ShipmasterID { get; set; }
        public string ShipmentID { get; set; }     
        public List<PalletGateway> PalletList { get; set; }
    }
}
