#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"

using System;
using System.Net;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, ICollector<OrderInfo> Order, TraceWriter log)
{
    log.Info("C# HTTP trigger function processed a request.");
    string error = null;
    // parse query parameter
    string name = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
        .Value;
    
    string requester = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "requester", true) == 0)
        .Value;

    string guid = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "guid", true) == 0)
        .Value;

    string customer = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "customer", true) == 0)
        .Value;

    string network = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "network", true) == 0)
        .Value;

    string resourcetype = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "resourcetype", true) == 0)
        .Value;

    string resourcetypesubtype = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "resourcetypesubtype", true) == 0)
        .Value;

    string resourcegroup = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "resourcegroup", true) == 0)
        .Value;

    string approver = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "approver", true) == 0)
        .Value;

    // Get request body
    dynamic data = await req.Content.ReadAsAsync<object>();

    // Set name to query string or body data
    name = name ?? data?.name;
    requester = requester ?? data?.requester;
    guid = guid ?? data?.guid;
    

    
    try {
        Order.Add(
                new OrderInfo() { 
                    PartitionKey = requester, 
                    RowKey = guid, 
                    customer = customer ?? data?.customer,
                    network = network ?? data?.network,
                    resourcetype = resourcetype ?? data?.resourcetype,
                    resourcetypesubtype = resourcetypesubtype ?? data?.resourcetypesubtype,
                    resourcegroup = resourcegroup ?? data?.resourcegroup,
                    approver = approver ?? data?.approver,
                    status = "Waiting for approval"
                    }
                );
    }
    catch (Exception e){
        error = e.Message;
        log.Info("Error occured");
    }

    var myObj = new {Info = "Successfull", requester = requester, guid = guid};
    var jsonToReturn = JsonConvert.SerializeObject(myObj, Formatting.Indented);

    var myObjError = new {Info = "Failure", ErrorMessage = error};
    var jsonToReturnError = JsonConvert.SerializeObject(myObjError, Formatting.Indented);

    return error != null
        ? req.CreateResponse(HttpStatusCode.BadRequest, jsonToReturnError, "application/json")
        : req.CreateResponse(HttpStatusCode.OK, jsonToReturn, "application/json");
}

public class OrderInfo
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public string customer { get; set; }
    public string network { get; set; }
    public string resourcetype { get; set; }
    public string resourcetypesubtype { get; set; }
    public string resourcegroup { get; set; }
    public string approver { get; set; }
    public string status { get; set; }
}

class Record {
    public string GUID { get; set; }
}
