using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace CCTitanFunction
{
    public static class GetDeviceConnectionString
    {                             
        [FunctionName("GetDeviceConnectionString")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // parse query parameter
            string DeviceId = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "DeviceId", true) == 0)
                .Value;           

            var IoTConnectionstrinG = Environment.GetEnvironmentVariable("IOTHubConnectionString");

            // string cnnstrinG = "HostName=CCTitanIOTHub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=FxHEkERJ/XXLlIFI3nD/sXkN44tq6pDUONbYQBgw7tY=";

             RegistryManager registryManager  = RegistryManager.CreateFromConnectionString(IoTConnectionstrinG);

            // Device device;

            Device device;
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(DeviceId));
            }
            catch (DeviceAlreadyExistsException ex)
            {
                device = await registryManager.GetDeviceAsync(DeviceId);
                var err = ex.ToString();
            }
            string deviceKey = device.Authentication.SymmetricKey.PrimaryKey;          


            return req.CreateResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(deviceKey));
        }
    }
}
