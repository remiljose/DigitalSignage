using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace CCTitanFunction
{
    public static class Assocciation
    {
        [FunctionName("Assocciation")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            string ShipmentId = req.GetQueryNameValuePairs()
                   .FirstOrDefault(q => string.Compare(q.Key, "ShipmentId", true) == 0)
                   .Value;

            if (string.IsNullOrEmpty(ShipmentId))
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Value is null or empty");
            }

            //var flatList = new List<Group>() {
            //    new Group() { ID = 1, ParentID = null , ObjectId = "1"},
            //    new Group() { ID = 2, ParentID = 1 ,ObjectId = "2"},
            //    new Group() { ID = 3, ParentID = 1 ,ObjectId = "3"},
            //    new Group() { ID = 4, ParentID = 3 ,ObjectId = "4"},
            //    new Group() { ID = 5, ParentID = 4 ,ObjectId = "5"},
            //    new Group() { ID = 6, ParentID = 4, ObjectId = "6"}
            //};

            //var tree = flatList.BuildTree();

            ////////////// parse query parameter


            string sprocname = "Sp_GetAssociation";

            var ConnectionstrinG = Environment.GetEnvironmentVariable("SQLConnectionString");
            List<Group> flatAssociatedList = new List<Group>();
            SqlConnection conn = new SqlConnection(ConnectionstrinG);
            SqlCommand commanD;
            SqlParameter parameteR;

            commanD = new SqlCommand(sprocname, conn);
            commanD.CommandType = CommandType.StoredProcedure;

            parameteR = commanD.Parameters.Add("@ShipmentId", SqlDbType.VarChar, 100);
            parameteR.Direction = ParameterDirection.Input;
            parameteR.Value = ShipmentId;

            conn.Open();
            SqlDataReader myReader = commanD.ExecuteReader();
            while (myReader.Read())
            {
                Group flatGroup = new Group();
                flatGroup.ID = (int)myReader["Id"];
               
                flatGroup.ObjectId = (string)myReader["ObjectId"];
                flatGroup.Type = (string)myReader["Type"];
                flatGroup.Level = (int)myReader["Level"];


                if ((int)myReader["Level"] == 0)
                {
                    flatGroup.ParentID = null;
                }
                else
                {
                    flatGroup.ParentID = (int?)myReader["parent"];
                }


                flatAssociatedList.Add(flatGroup);
            }
            myReader.Close();
            conn.Close();

            var tree = flatAssociatedList.BuildTree();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(tree, Formatting.Indented), Encoding.UTF8, "application/json")
            };
        }
    }

    public static class GroupEnumerable
    {
        public static IList<Group> BuildTree(this IEnumerable<Group> source)
        {
            var groups = source.GroupBy(i => i.ParentID);

            var roots = groups.FirstOrDefault(g => g.Key.HasValue == false).ToList();

            if (roots.Count > 0)
            {
                var dict = groups.Where(g => g.Key.HasValue).ToDictionary(g => g.Key.Value, g => g.ToList());
                for (int i = 0; i < roots.Count; i++)
                    AddChildren(roots[i], dict);
            }

            return roots;
        }

        private static void AddChildren(Group node, IDictionary<int, List<Group>> source)
        {
            if (source.ContainsKey(node.ID))
            {
                node.Children = source[node.ID];
                for (int i = 0; i < node.Children.Count; i++)
                    AddChildren(node.Children[i], source);
            }
            else
            {
                node.Children = new List<Group>();
            }
        }
    }


    public class Group
    {
        public int ID { get; set; }
        public int? ParentID { get; set; }
        public string ObjectId { get; set; }
        public string Type { get; set; }
        public int Level { get; set; }
        public List<Group> Children { get; set; }

    }

}
