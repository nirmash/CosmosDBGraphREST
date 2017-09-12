using System;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Data;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Graphs;
using Microsoft.Azure.Graphs.Elements;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;



namespace GraphFunc
{
    public static class AddGraphEntity
    {
        [FunctionName("GraphInteract")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "{collection}/{entity}/{action}/{id}/{property}/{value}")]HttpRequestMessage req, string collection, string entity, string action,string id, string property, string value, TraceWriter log, ExecutionContext context)
        {
            log.Info("AddGraphEntity entring");
            GraphAction grpAct;
            try
            {
                grpAct = new GraphAction(collection, entity, action, id, property, value);
            }
            catch(Exception err)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, err.Message);
            }

            string endpoint = ConfigurationManager.AppSettings["Endpoint"];
            string authKey = ConfigurationManager.AppSettings["AuthKey"];
            string retval = "";

            using (DocumentClient client = new DocumentClient(
                new Uri(endpoint),
                authKey,
                new ConnectionPolicy { ConnectionMode = ConnectionMode.Direct, ConnectionProtocol = Protocol.Tcp }))
            {
                try
                {
                    Talk2Graph t2g = new Talk2Graph();
                    t2g.RunAsync(client, grpAct, log, context).Wait();
                    retval = t2g.JSONResults;

                } catch (Exception err)
                {
                    log.Info(err.Message);
                    return req.CreateResponse(HttpStatusCode.OK, retval, "application/json");
                }
            }
            return req.CreateResponse(HttpStatusCode.OK, retval, "application/json");
        }
    }

    public class GraphAction
    {
        public string collection { get; set; }
        public string entity { get; set; }
        public string action { get; set; }
        public string id { get; set; }
        public string property { get; set; }
        public string value { get; set; }

        public GraphAction(string collection, string entity, string action, string id, string property, string value)
        {
            string errMsg = "";
            this.collection = collection;
            this.entity = entity;
            this.action = action;
            this.id = id;
            this.property = property;
            this.value = value;

            errMsg = (collection == "") ? "collection is missing" : "";
            errMsg = (entity == "") ? "entity is missing" : "";
            errMsg = (action == "") ? "action is missing" : "";
            errMsg = (id == "") ? "id is missing" : "";
            errMsg = (property == "") ? "property is missing" : "";
            errMsg = (value == "") ? "value is missing" : "";

            if (action.ToLower() == "del" || action.ToLower() == "drop")
                errMsg = "";

            if (errMsg != "")
            {
                throw (new Exception(errMsg));
            }
        }
    }

    public class Talk2Graph
    {
        public string JSONResults;
        public async Task RunAsync(DocumentClient client, GraphAction act, TraceWriter log, ExecutionContext context)
        {
            //load query map from local file - could be changed to Azure Storage or http endpoint
            //debug file location 
            //string queriesFilePath = context.FunctionDirectory.Replace(context.FunctionName, "content\\querymaps.json");
            //production file location
            string queriesFilePath = context.FunctionDirectory.Replace(context.FunctionName, "bin\\content\\querymaps.json");
            string jsontxt = (File.ReadAllText(queriesFilePath));
            string GremlinQuery = "";
            try {
                //find the correct query by entity and action 
                DataSet queries = JsonConvert.DeserializeObject<DataSet>(jsontxt);
                DataTable qryTbl = queries.Tables["queries"];
                GremlinQuery = qryTbl.Select($"entity='{act.entity.ToLower()}' and action='{act.action.ToLower()}'")[0]["query"].ToString();
                //id = 0, property = 1, value = 2
                //do the string replacement
                GremlinQuery = String.Format(GremlinQuery, act.id, act.property, act.value);
            }
            catch (Exception err)
            {
                log.Info(err.Message);
                this.JSONResults = "{'error': '" + err.Message + "'}";
                return;
            }
            //setup query 
            //DB and Collection Connections
            string databaseName = ConfigurationManager.AppSettings["databaseName"];
            string collectionName = act.collection;

            Database database = await client.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseName });

            DocumentCollection graph = await client.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(databaseName),
                new DocumentCollection { Id = collectionName },
                new RequestOptions { OfferThroughput = 1000 });
            JArray lst = new JArray();
            // Execute the Gremlin Query
            try
            {
                IDocumentQuery<dynamic> query = client.CreateGremlinQuery<dynamic>(graph, GremlinQuery);
                while (query.HasMoreResults)
                {
                    foreach (dynamic result in await query.ExecuteNextAsync())
                    {
                        log.Info($"\t {JsonConvert.SerializeObject(result)}");
                        lst.Add(JObject.Parse(result.ToString()));
                    }
                }
            }catch(Exception err)
            {
                log.Info(err.Message);
                lst.Add(err.Message);
            }finally
            {
                this.JSONResults = lst.ToString(Newtonsoft.Json.Formatting.Indented);
            }
        }
    }
}