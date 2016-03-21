using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.ServiceModel.Activation;


namespace WcfCrimShopService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    //NOte; two endpoits cannot have the same address name 
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class Service1 : IService1
    {
        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public string InsertOrderDetails(string ControlNumber, string PaymentResponse, string Description)
        {
            string Message;
            SqlConnection con = new SqlConnection(@"Data Source=GMTWKS13\GMTWKS13DB;Initial Catalog=CRIMShopManagement;Trusted_Connection=Yes;");
            //SqlConnection con = new SqlConnection(@"Data Source=HECTOR_CUSTOMS\MYOWNSQLSERVER;Initial Catalog=CRIMShopManagement;Trusted_Connection=Yes;");
            con.Open();
            //string queryString = "INSERT into dbo.Orders (ContorlNumber,PaymentResponse,Description)" +
            //        "VALUES (@control,@response,@description)";
            string queryString = "INSERT into dbo.Orders (ControlNumber,PaymentRespone,Description)" +
                                "VALUES (@control,@response,@description)";
            SqlCommand cmd = new SqlCommand(queryString, con);
            cmd.Parameters.AddWithValue("@control", ControlNumber);
            cmd.Parameters.AddWithValue("@response", PaymentResponse);
            cmd.Parameters.AddWithValue("@description", Description);
            //string queryString = "INSERT into dbo.Orders (ContorlNumber,PaymentResponse,Description)" +
            //                    "VALUES (@control,@response,@description)";
            ////string queryString = "INSERT into dbo.Orders (ControlNumber,PaymentRespone,Description)" +
            ////                    "VALUES (@control,@response,@description)";
            //SqlCommand cmd = new SqlCommand(queryString, con);
            //cmd.Parameters.AddWithValue("@control", orderInfo.ControlNumber);
            //cmd.Parameters.AddWithValue("@response", orderInfo.PaymentResponse);
            //cmd.Parameters.AddWithValue("@description", orderInfo.Description);
            int result = cmd.ExecuteNonQuery();
            if(result == 1)
            {
                Message = "Order number: " + ControlNumber + " aAdded successfully";
            }
            else
            {
                Message = "Order : " + ControlNumber + " not added";
            }
            con.Close();
            return Message;

        }

        public string MakePaymentResponse(string ControlNumber, string PaymentResponse)
        {
            string Message;
            SqlConnection con = new SqlConnection(@"Data Source=GMTWKS13\GMTWKS13DB;Initial Catalog=CRIMShopManagement;Trusted_Connection=Yes;");
            //SqlConnection con = new SqlConnection(@"Data Source=HECTOR_CUSTOMS\MYOWNSQLSERVER;Initial Catalog=CRIMShopManagement;Trusted_Connection=Yes;");
            con.Open();
            //string queryString = "INSERT into dbo.Orders (ContorlNumber,PaymentResponse,Description)" +
            //        "VALUES (@control,@response,@description)";
            string queryString = "UPDATE dbo.Orders SET PaymentRespone=@response" +
                                " WHERE ControlNumber=@control";
            SqlCommand cmd = new SqlCommand(queryString, con);
            cmd.Parameters.AddWithValue("@control", ControlNumber);
            cmd.Parameters.AddWithValue("@response", PaymentResponse);
            int result = cmd.ExecuteNonQuery();
            if (result == 1)
            {
                Message = "Order : " + ControlNumber + " updated successfully: "+ PaymentResponse;
            }
            else
            {
                Message = "Order : " + ControlNumber + " not updated: "+ PaymentResponse;
            }
            con.Close();
            return Message;
        }

        //public CompositeType GetDataUsingDataContract(CompositeType composite)
        //{
        //    if (composite == null)
        //    {
        //        throw new ArgumentNullException("composite");
        //    }
        //    if (composite.BoolValue)
        //    {
        //        composite.StringValue += "Suffix";
        //    }
        //    return composite;
        //}
    }
}
