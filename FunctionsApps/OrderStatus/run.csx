#r "Newtonsoft.Json"
#r "Microsoft.WindowsAzure.Storage"

using System.Net;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, CloudTable Orders, TraceWriter log)
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
    GUID = GUID ?? data?.GUID;
    Requester = Requester ?? data?.Requester;
    string error = "noerror";

    if (GUID == null) {

       error = "Input values are invalid";
    }
    if (Requester == null) {

       error = "Input values are invalid";
    }
    var myObj = new {Info = "Successfull", Requester = Requester, GUID = GUID, Status = ""};
    if (error == "noerror") {
        TableResult result = null;
        Order order = null;

        TableOperation operation = TableOperation.Retrieve<Order>(Requester, GUID);
        result = Orders.Execute(operation);
        if (result != null) {
            order = (Order)result.Result;
            if (order == null) {
                log.Info("Order is null");
                error = "Returned null value";
            }
            else {
                myObj = new {Info = "Successfull", Requester = Requester, GUID = GUID, Status = order.status};
                log.Info($"{order.PartitionKey} : {order.RowKey} : {order.status}");
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
