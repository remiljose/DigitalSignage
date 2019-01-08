using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CCTitanFunction
{
    public static class DeviceRegister
    {
        [FunctionName("RegisterDevice")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info($"Device Registration Function Triggered - {DateTime.Now}");

            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();
            string Mode = data?.Mode;
            string MacId = data?.MacId;
            string Type = data?.DeviceType;

            if (string.IsNullOrEmpty(Mode) || string.IsNullOrEmpty(MacId) || string.IsNullOrEmpty(Type))
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Value is null or empty");
            }

            string mode = Mode;

            var ConnectionstrinG = Environment.GetEnvironmentVariable("SQLConnectionString");
            string NewquerY = "INSERT INTO DeviceInfo VALUES(@MacId, @Type, @IsActive, @Status, @ConnectionString, @CreatedBy, @CreatedDateTime, @UpdatedBy, @UpdatedDateTime)";
            string UpdatequerY = "UPDATE DeviceInfo SET MacId = @MacId, Type = @Type, UpdatedDateTime = @UpdatedDateTime, UpdatedBy = @UpdatedBy WHERE MacId = @MacId";
            string DeletequerY = "DELETE FROM DeviceInfo WHERE MacId = @MacId";
            SqlConnection conn = new SqlConnection(ConnectionstrinG);
            SqlCommand commanD;
            try
            {
                if (mode == "New")
                {
                    commanD = new SqlCommand(NewquerY, conn);

                    commanD.Parameters.Add("@MacId", SqlDbType.NVarChar).Value = MacId;
                    commanD.Parameters.Add("@Type", SqlDbType.NVarChar).Value = Type;
                    commanD.Parameters.Add("@IsActive", SqlDbType.Bit).Value = 0;
                    commanD.Parameters.Add("@Status", SqlDbType.NVarChar).Value = "Online";
                    commanD.Parameters.Add("@ConnectionString", SqlDbType.NVarChar).Value = "whatever the value is";
                    commanD.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = 234;
                    commanD.Parameters.Add("@CreatedDateTime", SqlDbType.DateTime).Value = DateTime.Now.ToString();
                    commanD.Parameters.Add("@UpdatedBy", SqlDbType.Int).Value = 28;
                    commanD.Parameters.Add("@UpdatedDateTime", SqlDbType.DateTime).Value = DateTime.Now.ToString();
                    conn.Open();
                    commanD.ExecuteNonQuery();
                    conn.Close();

                    return req.CreateResponse(HttpStatusCode.OK, "New details have been updated");
                }

                else if (mode == "Update")
                {
                    commanD = new SqlCommand(UpdatequerY, conn);
                    commanD.Parameters.Add("@MacId", SqlDbType.NVarChar).Value = MacId;
                    commanD.Parameters.Add("@Type", SqlDbType.NVarChar).Value = Type;
                    commanD.Parameters.Add("@UpdatedBy", SqlDbType.Int).Value = 28;
                    commanD.Parameters.Add("@UpdatedDateTime", SqlDbType.DateTime).Value = DateTime.Now.ToString();
                    conn.Open();
                    commanD.ExecuteNonQuery();
                    conn.Close();

                    return req.CreateResponse(HttpStatusCode.OK, "Details have been updated");
                }
                else if (mode == "Delete")
                {
                    commanD = new SqlCommand(DeletequerY, conn);
                    commanD.Parameters.AddWithValue("@MacId", SqlDbType.NVarChar).Value = MacId;
                    conn.Open();
                    commanD.ExecuteNonQuery();
                    conn.Close();

                    return req.CreateResponse(HttpStatusCode.OK, "Selected row has been deleted");
                }

                else if (mode != "New" && mode != "Update" && mode != "Delete")
                {
                    return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid Operation");
                }

                return req.CreateResponse(HttpStatusCode.OK, "Details have been Updated");

            }
            catch (Exception ex)
            {
                log.Info("Exception Occured", ex.ToString());
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Cannot Update the Table because of an Exception");
            }

        }
    }
}
