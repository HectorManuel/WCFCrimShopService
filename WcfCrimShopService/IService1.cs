using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
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
        //CompositeType GetDataUsingDataContract(CompositeType composite);

        [OperationContract]
        string InsertOrderDetails(OrderDetails orderInfo);
        // TODO: Add your service operations here
    }


    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    [DataContract]
    public class OrderDetails
    {
        int controlnumber = 0;
        string payResponse = string.Empty;
        string desc = string.Empty;
        [DataMember]
        public int ControlNumber
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
