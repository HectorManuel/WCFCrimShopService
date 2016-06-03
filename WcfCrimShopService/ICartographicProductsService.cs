using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Activation;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace WcfCrimShopService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface ICartographicProductsService
    {

        [OperationContract]
        [WebGet(UriTemplate = "WebGet/{value}", ResponseFormat = WebMessageFormat.Json)]
        string GetData(string value);

        [OperationContract]
        [WebGet(UriTemplate = "AwaitConfirmation/{order}", ResponseFormat = WebMessageFormat.Json)]
        string AwaitConfirmation(string order);

        [OperationContract]
        [WebGet(UriTemplate = "GetControlNumber", ResponseFormat = WebMessageFormat.Json)]
        string GetControlNumber();

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string InsertOrderDetails(string ControlNumber, string Description, decimal tx, decimal sTotal, decimal Total, string CustomerName, string customerEmail, string hasPhoto, string hasCat, string hasList, string hasExtract);


        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string PaymentResponse(string PaymentResponse);

        //[OperationContract]
        //[WebInvoke(UriTemplate="PaymentResponse2/{response}")]
        //string PaymentResponse2(string response,System.IO.Stream PaymentResponse);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string InsertAerialPhotoItem(string title,string controlNumber, int itemQty, string item, string format, string layoutTemplate, string georefInfo, string parcel, string subtitle, string buffer, string parcelList, string bufferDistance);


        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string InsertListaColindanteItem(string controlNumber, string parcelas, int itemQty, string item);


        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string InsertCatastralItem(string controlNumber, int itemQty, string cuadricula1, string cuadricula10);

        [OperationContract]
        [WebInvoke(Method="POST", ResponseFormat= WebMessageFormat.Json, BodyStyle= WebMessageBodyStyle.Wrapped)]
        string InsertExtractDataService(string controlNumber, int qty, string layer, string area, string format, string raster);



        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string StarGeoprocess(string cNumber);

        [OperationContract]
        [WebInvoke(Method = "POST",RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        string MakePayment(string controlNumber);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        string CreatePdfListaColindante(string json, string cNumber, string customer, string parcela);

        [OperationContract]
        [WebGet(UriTemplate="Authentication/{username}/{password}", ResponseFormat = WebMessageFormat.Json)]
        string Authentication(string username, string password);

        [OperationContract]
        [WebInvoke(Method="POST", RequestFormat= WebMessageFormat.Json, ResponseFormat= WebMessageFormat.Json, BodyStyle= WebMessageBodyStyle.WrappedRequest)]
        string GetItemPrice(string item, int qty);

        [OperationContract]
        [WebGet(UriTemplate = "GetTax", ResponseFormat = WebMessageFormat.Json)]
        decimal GetTax();

        [OperationContract]
        [WebGet(UriTemplate = "GenerateList/{control}/{customer}", ResponseFormat = WebMessageFormat.Json)]
        string GenerateList(string control, string customer);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        string test(string cn);

        [OperationContract]
        [WebGet(UriTemplate = "GetPriceList", ResponseFormat = WebMessageFormat.Json)]
        string GetPriceList();

        [OperationContract]
        [WebGet(UriTemplate = "GetIPAdress", ResponseFormat = WebMessageFormat.Json)]
        string GetIP();

    }

}
