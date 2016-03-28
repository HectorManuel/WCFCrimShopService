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
using System.Web;
using System.Collections.Specialized;

using WcfCrimShopService.entities;

namespace WcfCrimShopService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    //NOte; two endpoits cannot have the same address name
 
    //this sets the compatibility mode for asp.net
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class Service1 : IService1
    {
        public string GetData(int value)
        {
            
            SqlConnection con = new SqlConnection(@"Data Source=GMTWKS13\GMTWKS13DB;Initial Catalog=CRIMShopManagement;User ID=User;Password=user123;");
            //SqlConnection con = new SqlConnection(@"Data Source=HECTOR_CUSTOMS\MYOWNSQLSERVER;Initial Catalog=CRIMShopManagement;Trusted_Connection=Yes;");
            con.Open();
            string query = "SELECT ControlNumber,PaymentRespone,Description" +
                           "FROM dbo.Orders" +
                           "WHERE ControlNumber=@control";
            
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@control", value);

            //var result = cmd.ExecuteNonQuery();
            SqlDataReader result = cmd.ExecuteReader();
            return "read";


        }

        public string InsertOrderDetails(string ControlNumber, string Description, string clientId, decimal tx,decimal sTotal, decimal Total)
        {
            string Message;
            DateTime OrderDate = DateTime.Now;

            SqlConnection con = new SqlConnection(@"Data Source=GMTWKS13\GMTWKS13DB;Initial Catalog=CRIMShopManagement;User ID=User;Password=user123;");
            //SqlConnection con = new SqlConnection(@"Data Source=HECTOR_CUSTOMS\MYOWNSQLSERVER;Initial Catalog=CRIMShopManagement;Trusted_Connection=Yes;");
            con.Open();
            //string queryString = "INSERT into dbo.Orders (ContorlNumber,PaymentResponse,Description)" +
            //        "VALUES (@control,@response,@description)";
            string queryString = "INSERT into dbo.Orders (ControlNumber,Description,ClientId,Confirmation,Tax,Subtotal,Total,OrderDate)" +
                                "VALUES (@control,@description,@clientId,@confirmation,@tax,@subtotal,@total,@orderDate)";
            SqlCommand cmd = new SqlCommand(queryString, con);
            if (ControlNumber == null)
            {
                Message = "Control number needed";
                return Message;
            }
            else
            {
                cmd.Parameters.AddWithValue("@control", ControlNumber);
            }
            if (Description == null)
            {
                Message = "Description cannot be empty, send Control number in it";
                return Message;
            }
            else
            {
                cmd.Parameters.AddWithValue("@description", Description);
            }
            if (clientId == null)
            {
                cmd.Parameters.AddWithValue("@cliendId", clientId);
            }
            else
            {
                cmd.Parameters.AddWithValue("@cliendId", clientId);
            }
            if (tx == 0)
            {
                cmd.Parameters.AddWithValue("@tax", 0);
            }
            else
            {
                cmd.Parameters.AddWithValue("@tax", tx);
            }
            if (sTotal == 0)
            {
                cmd.Parameters.AddWithValue("@subtotal", 0);
            }
            else
            {
                cmd.Parameters.AddWithValue("@subtotal", sTotal);
            }
            if (Total == 0)
            {
                Message = "A total amount must be added";
                return Message;
            }
            else
            {
                cmd.Parameters.AddWithValue("@total", Total);
            }

            cmd.Parameters.AddWithValue("@confirmation", "Processing");
            cmd.Parameters.AddWithValue("@orderDate",OrderDate);

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

        public string InsertClientDetails(string clientId, string name, string email, string address, string city, string zip, string tel, string fax)
        {
            string Message;
            SqlConnection con = new SqlConnection(@"Data Source=GMTWKS13\GMTWKS13DB;Initial Catalog=CRIMShopManagement;User ID=User;Password=user123;");
            //SqlConnection con = new SqlConnection(@"Data Source=HECTOR_CUSTOMS\MYOWNSQLSERVER;Initial Catalog=CRIMShopManagement;Trusted_Connection=Yes;");
            con.Open();
            //string queryString = "INSERT into dbo.Orders (ContorlNumber,PaymentResponse,Description)" +
            //        "VALUES (@control,@response,@description)";
            string queryString = "INSERT into dbo.Client (ClientId,Name, Email, Address, City, Zip, Telephone, Fax)" +
                                "VALUES (@clientId,@name, @email, @adress, @city, @zip, @telephone, @fax)";
            
            SqlCommand cmd = new SqlCommand(queryString, con);
            cmd.Parameters.AddWithValue("@clientId", clientId);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@adress", address);
            cmd.Parameters.AddWithValue("@city", city);
            cmd.Parameters.AddWithValue("@zip", zip);
            cmd.Parameters.AddWithValue("@telephone", tel);
            cmd.Parameters.AddWithValue("@fax", fax);

            int result = cmd.ExecuteNonQuery();
            if (result == 1)
            {
                Message = "Client: " + clientId + " aAdded successfully";
            }
            else
            {
                Message = "Client: " + clientId + " not added";
            }
            con.Close();
            return Message;

        }
        public string PaymentResponse(string PaymentResponse)
        {
            Geoprocessing test = new Geoprocessing();

            string Message = "things";
            NameValueCollection nvc = HttpUtility.ParseQueryString(PaymentResponse);

            string val = nvc.Get("VPaymentDescription");
            string transactionId = nvc.Get("VTransactionId");
            string accountId = nvc.Get("VAccountId");
            string totalAmount = nvc.Get("VTotalAmount");
            string paymentMethod = nvc.Get("VPaymentMethod");
            string paymentDescription = nvc.Get("VPaymentDescription");
            string authorizationNum = nvc.Get("VAuthorizationNum");
            string confirmationNum = nvc.Get("VConfirmationNum");
            //string merchantTransId = nvc.Get("VMerchantTransId");

            SqlConnection con = new SqlConnection(@"Data Source=GMTWKS13\GMTWKS13DB;Initial Catalog=CRIMShopManagement;Trusted_Connection=Yes;");
            //SqlConnection con = new SqlConnection(@"Data Source=HECTOR_CUSTOMS\MYOWNSQLSERVER;Initial Catalog=CRIMShopManagement;Trusted_Connection=Yes;");
            con.Open();
            //string queryString = "INSERT into dbo.Orders (ContorlNumber,PaymentResponse,Description)" +
            //        "VALUES (@control,@response,@description)";
            string queryString = "UPDATE dbo.Orders SET PaymentRespone=@response, Confirmation=@confirm" +
                                " WHERE ControlNumber=@control";
            SqlCommand cmd = new SqlCommand(queryString, con);
            cmd.Parameters.AddWithValue("@control", paymentDescription);
            cmd.Parameters.AddWithValue("@response", authorizationNum);
            cmd.Parameters.AddWithValue("@confirm", confirmationNum);
            int result = cmd.ExecuteNonQuery();
 
            //string queryString = "INSERT into dbo.Orders (ContorlNumber,PaymentResponse,Description)" +
            //        "VALUES (@control,@response,@description)";
            string queryString2 = "INSERT into dbo.PaymentResponseLog (ControlNumber,VTransactionID,VAccountId,VTotalAmount,VPaymentMethod,VPaymentDescription,VAuthorizationNum,VConfirmationNum)" +
                                "VALUES (@ControlNumber,@VTransactionID,@VAccountId,@VTotalAmount,@VPaymentMethod,@VPaymentDescription,@VAuthorizationNum,@VConfirmationNum)";
            SqlCommand cmd2 = new SqlCommand(queryString2, con);
            //cmd.Parameters.AddWithValue("@control", ControlNumber);
            cmd2.Parameters.AddWithValue("@ControlNumber", val);
            cmd2.Parameters.AddWithValue("@VTransactionID", transactionId);
            cmd2.Parameters.AddWithValue("@VAccountId", accountId);
            cmd2.Parameters.AddWithValue("@VTotalAmount", totalAmount);
            cmd2.Parameters.AddWithValue("@VPaymentMethod", paymentMethod);
            cmd2.Parameters.AddWithValue("@VPaymentDescription", paymentDescription);
            cmd2.Parameters.AddWithValue("@VAuthorizationNum", authorizationNum);
            cmd2.Parameters.AddWithValue("@VConfirmationNum", confirmationNum);
            //cmd2.Parameters.AddWithValue("@VMerchantTransId", merchantTransId);
            int result2 = cmd2.ExecuteNonQuery();

            if (result2 == 1 && result==1)
            {
                Message = "Order updated successfully: " + PaymentResponse;
            }
            else
            {
                Message = "Order  not updated: " + PaymentResponse;
            }
            con.Close();
            return Message;
        }

    }

}
