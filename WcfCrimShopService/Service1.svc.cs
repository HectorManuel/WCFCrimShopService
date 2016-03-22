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
 
    //this sets the compatibility mode for asp.net
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class Service1 : IService1
    {
        public string GetData(int value)
        {
            
            SqlConnection con = new SqlConnection(@"Data Source=GMTWKS13\GMTWKS13DB;Initial Catalog=CRIMShopManagement;User ID=User;Password=user123;");
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

        public string InsertOrderDetails(string ControlNumber, string PaymentResponse, string Description)
        {
            string Message;
            SqlConnection con = new SqlConnection(@"Data Source=GMTWKS13\GMTWKS13DB;Initial Catalog=CRIMShopManagement;User ID=User;Password=user123;");
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

        public string InsertClientDetails(string clientId, string name, string lName, string mName, string email, string address, string city, string zip, string tel, string fax)
        {
            string Message;
            SqlConnection con = new SqlConnection(@"Data Source=GMTWKS13\GMTWKS13DB;Initial Catalog=CRIMShopManagement;User ID=User;Password=user123;");
            //SqlConnection con = new SqlConnection(@"Data Source=HECTOR_CUSTOMS\MYOWNSQLSERVER;Initial Catalog=CRIMShopManagement;Trusted_Connection=Yes;");
            con.Open();
            //string queryString = "INSERT into dbo.Orders (ContorlNumber,PaymentResponse,Description)" +
            //        "VALUES (@control,@response,@description)";
            string queryString = "INSERT into dbo.Client (ClientId,Name,LastName, MiddleName, Email, Adress, City, Zip, Telephone, Fax)" +
                                "VALUES (@clientId,@name,@lastName, @middleName, @email, @adress, @city, @zip, @telephone, @fax)";
            
            SqlCommand cmd = new SqlCommand(queryString, con);
            cmd.Parameters.AddWithValue("@clientId", clientId);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@lastName", lName);
            cmd.Parameters.AddWithValue("@middleName", mName);
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
        public string MakePaymentResponse(string PaymentResponse)
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
            //cmd.Parameters.AddWithValue("@control", ControlNumber);
            cmd.Parameters.AddWithValue("@response", PaymentResponse);
            int result = cmd.ExecuteNonQuery();
            if (result == 1)
            {
                Message = "Order updated successfully: "+ PaymentResponse;
            }
            else
            {
                Message = "Order  not updated: "+ PaymentResponse;
            }
            con.Close();
            return Message;
        }

    }
}
