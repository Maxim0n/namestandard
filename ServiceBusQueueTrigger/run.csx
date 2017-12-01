#r "Microsoft.ServiceBus"
#r "Microsoft.WindowsAzure.Storage"

using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

public static void Run(BrokeredMessage myMessage, CloudTable Orders, TraceWriter log)
{
    log.Info($"C# ServiceBus queue trigger function processed message");

    string sRequester = myMessage.Properties["Requester"].ToString();
    string sGUID = myMessage.Properties["GUID"].ToString();
    
    if (GUID == null) {
       myMessage.Abandon();
    }
    if (Requester == null) {
       myMessage.Abandon();
    }

    TableResult result = null;
    Order order = null;
    string error = "";
    
    TableOperation operation = TableOperation.Retrieve<Order>(sRequester, sGUID);
    result = Orders.Execute(operation);
    if (result != null) {
        order = (Order)result.Result;

        if (order == null) {
            log.Info("Order is null");
            error = "Returned null value";
            myMessage.Abandon();
        }
        else {
            // Call name generator and pass to AddNewResource with generated name
            log.Info($"{order.PartitionKey} : {order.RowKey} : {order.approver} : {order.status}");
        }
    }
    else {
        log.Info("Storage entity returned null value");
        myMessage.Abandon();
    }
    log.Info($"sRequester - " + sRequester);
    log.Info($"sGUID - " + sGUID);
    //log.Info($"C# ServiceBus queue trigger function processed message: {myMessage}");
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
