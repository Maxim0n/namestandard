#r "Newtonsoft.Json"
#r "Microsoft.WindowsAzure.Storage"
#r "Microsoft.ServiceBus"

using System.Net;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.ServiceBus.Messaging;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, CloudTable Orders, CloudTable OrdersOUT, TraceWriter log)
{
    log.Info("C# HTTP trigger function processed a request.");

    // parse query parameter
    string GUID = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "GUID", true) == 0)
        .Value;

    string Requester = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "Requester", true) == 0)
        .Value;

    // Get request body
    dynamic data = await req.Content.ReadAsAsync<object>();

    // Set name to query string or body data
    GUID = GUID ?? data?.GUID;
    Requester = Requester ?? data?.Requester;
    string error = "noerror";

    if (GUID == null) {

       error = "Input values are invalid";
    }
    if (Requester == null) {

       error = "Input values are invalid";
    }
    TableResult result = null;
    Order order = null;
    var myObj = new {Info = "Successfull", Requester = "", GUID = "", Customer ="", Network = "", Resourcetype = "", Resourcetypesubtype = "", Resourcegroup = "", Approver = "", Status = ""};

    if (error == "noerror") {
        TableOperation operation = TableOperation.Retrieve<Order>(Requester, GUID);
        try {
            log.Info("Executing Storage retrieve");
            result = Orders.Execute(operation);
            log.Info("Executing Storage retrieve finnished");
        }
        catch (Exception e) {
            error = "Error occured while reading table storage";
            log.Info(e.Message);
        }
        if (result != null) {
            log.Info("Parse result");
            try {
                order = (Order)result.Result;
            }
            catch (Exception e) {
                error = "Returned null value";
                log.Info(e.Message);
            }
            if (order == null) {
                log.Info("Order is null");
                error = "Returned null value";
            }
            else {
                // Change Storage Table entity status to 'Queued'
                order.status = "Queued";
                operation = TableOperation.Replace(order);
                result = OrdersOUT.Execute(operation);
                
                 // Create Message bus client
                var connectionString = ""; // TODO: Get ConnectionString from App Settings. Do not use hard coded keys
                var queueName = "namestandard";
                var client = QueueClient.CreateFromConnectionString(connectionString, queueName);

                // Create message,
                BrokeredMessage message = new BrokeredMessage("Order");
                message.Properties["Requester"] = Requester;
                message.Properties["GUID"] = GUID;

                // Add to Message Bus
                client.Send(message);

                // Create JSON responce
                myObj = new {Info = "Successfull", Requester = Requester, GUID = GUID, Customer = order.customer, Network = order.network, Resourcetype = order.resourcetype, Resourcetypesubtype = order.resourcetypesubtype, Resourcegroup = order.resourcegroup, Approver = order.approver, Status = "Queued"};
                log.Info($"{order.PartitionKey} : {order.RowKey} : {order.approver}");
            }
        }
    }

    var jsonToReturn = JsonConvert.SerializeObject(myObj, Formatting.Indented);

    var myObjError = new {Info = "Failure", ErrorMessage = error};
    var jsonToReturnError = JsonConvert.SerializeObject(myObjError, Formatting.Indented);

    return error != "noerror"
        ? req.CreateResponse(HttpStatusCode.BadRequest, jsonToReturnError, "application/json")
        : req.CreateResponse(HttpStatusCode.OK, jsonToReturn, "application/json");
}

public class Order : TableEntity
{
    public string Requester { get; set; }
    public string GUID { get; set; }
    public string customer { get; set; }
    public string network { get; set; }
    public string resourcetype { get; set; }
    public string resourcetypesubtype { get; set; }
    public string resourcegroup { get; set; }
    public string approver { get; set; }
    public string status { get; set; }
}
