using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Activation;
using System.Text;

namespace WcfCrimShopService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IService1
    {

        [OperationContract]
        string GetData(int value);

        //[OperationContract]
        //CompositeType GetDataUsingDataContract(CompositeType composite);  RequestFormat = WebMessageFormat.Json
        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string InsertOrderDetails(string ControlNumber, string PaymentResponse, string Description);
        // TODO: Add your service operations here

        [OperationContract]
        [WebInvoke(Method = "PUT", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string MakePaymentResponse(string ControlNumber, string PaymentResponse);

        //[OperationContract]
        //[WebInvoke(Method = "PUT", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        //string MakePaymentResponse(string ControlNumber, string PaymentResponse);

    }


    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    [DataContract]
    public class OrderDetails
    {
        string controlnumber = string.Empty;
        string payResponse = string.Empty;
        string desc = string.Empty;
        [DataMember]
        public string ControlNumber
        {
            get { return controlnumber; }
            set { controlnumber = value; }
        }

        [DataMember]
        public string PaymentResponse
        {
            get { return payResponse; }
            set { payResponse = value; }
        }

        [DataMember]
        public string Description
        {
            get { return desc; }
            set { desc = value; }
        }
    }
    //public class CompositeType
    //{
    //    bool boolValue = true;
    //    string stringValue = "Hello ";

    //    [DataMember]
    //    public bool BoolValue
    //    {
    //        get { return boolValue; }
    //        set { boolValue = value; }
    //    }

    //    [DataMember]
    //    public string StringValue
    //    {
    //        get { return stringValue; }
    //        set { stringValue = value; }
    //    }
    //}
}
