﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Activation;
using System.Text;
using System.Diagnostics;

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

        [OperationContract] //ya cree ajax
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string InsertOrderDetails(string ControlNumber, string Description, decimal tx, decimal sTotal, decimal Total, string CustomerName, string customerEmail, string hasPhoto, string hasCat, string hasList);
        // TODO: Add your service operations here

        [OperationContract] // ya cree ajax pa pruebas
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string PaymentResponse(string PaymentResponse);


        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string InsertClientDetails(string name, string email, string address, string city, string zip, string tel, string fax);


        [OperationContract] //ya cree ajax
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string InsertAerialPhotoItem(string controlNumber, int itemQty, string item, string format, string layoutTemplate, string georefInfo, string parcel, string subtitle, string buffer, string parcelList, string bufferDistance);


        [OperationContract] // ya cree ajax
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string InsertListaColindanteItem(string controlNumber, string itemName, int itemQty, string item);


        [OperationContract] // ya cree ajax
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string InsertCatastralItem(string controlNumber, string itemName, int itemQty, string escala, string cuadricula, string template);


        [OperationContract] // ya cree ajax en espera a ver que otras variables le agrego
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string StarGeoprocess(string jsonMap, string cNumber, string format, string template, string geoInfo, string parcelTitle, string sub_Title, string bf, string pr, string bf_distance_unit, string hasCat, string hasPhoto, string hasList, string email);

        [OperationContract]
        [WebInvoke(Method = "POST",RequestFormat=WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        string MakePayment(string controlNumber);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        string CreatePdf(string controlNumber);

    }


    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    //[DataContract]
    //public class OrderDetails
    //{
    //    string controlnumber = string.Empty;
    //    string payResponse = string.Empty;
    //    string desc = string.Empty;
    //    [DataMember]
    //    public string ControlNumber
    //    {
    //        get { return controlnumber; }
    //        set { controlnumber = value; }
    //    }

    //    [DataMember]
    //    public string PaymentResponse
    //    {
    //        get { return payResponse; }
    //        set { payResponse = value; }
    //    }

    //    [DataMember]
    //    public string Description
    //    {
    //        get { return desc; }
    //        set { desc = value; }
    //    }
    //}
}
