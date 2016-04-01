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
            DBConnection responseHandler = new DBConnection();

            var result = responseHandler.InsertOrderDetailsHandler(ControlNumber, Description, clientId, tx, sTotal, Total);

            return result;

            //string Message;
            //DateTime OrderDate = DateTime.Now;

            //SqlConnection con = new SqlConnection(@"Data Source=GMTWKS13\GMTWKS13DB;Initial Catalog=CRIMShopManagement;User ID=User;Password=user123;");
            ////SqlConnection con = new SqlConnection(@"Data Source=HECTOR_CUSTOMS\MYOWNSQLSERVER;Initial Catalog=CRIMShopManagement;Trusted_Connection=Yes;");
            //con.Open();
            ////string queryString = "INSERT into dbo.Orders (ContorlNumber,PaymentResponse,Description)" +
            ////        "VALUES (@control,@response,@description)";
            //string queryString = "INSERT into dbo.Orders (ControlNumber,Description,ClientId,Confirmation,Tax,Subtotal,Total,OrderDate)" +
            //                    "VALUES (@control,@description,@clientId,@confirmation,@tax,@subtotal,@total,@orderDate)";
            //SqlCommand cmd = new SqlCommand(queryString, con);
            //if (ControlNumber == null)
            //{
            //    Message = "Control number needed";
            //    return Message;
            //}
            //else
            //{
            //    cmd.Parameters.AddWithValue("@control", ControlNumber);
            //}
            //if (Description == null)
            //{
            //    Message = "Description cannot be empty, send Control number in it";
            //    return Message;
            //}
            //else
            //{
            //    cmd.Parameters.AddWithValue("@description", Description);
            //}
            //if (clientId == null)
            //{
            //    cmd.Parameters.AddWithValue("@clientId", clientId);
            //}
            //else
            //{
            //    cmd.Parameters.AddWithValue("@clientId", clientId);
            //}
            //if (tx == 0)
            //{
            //    cmd.Parameters.AddWithValue("@tax", 0);
            //}
            //else
            //{
            //    cmd.Parameters.AddWithValue("@tax", tx);
            //}
            //if (sTotal == 0)
            //{
            //    cmd.Parameters.AddWithValue("@subtotal", 0);
            //}
            //else
            //{
            //    cmd.Parameters.AddWithValue("@subtotal", sTotal);
            //}
            //if (Total == 0)
            //{
            //    Message = "A total amount must be added";
            //    return Message;
            //}
            //else
            //{
            //    cmd.Parameters.AddWithValue("@total", Total);
            //}

            //cmd.Parameters.AddWithValue("@confirmation", "Processing");
            //cmd.Parameters.AddWithValue("@orderDate",OrderDate);

            //int result = cmd.ExecuteNonQuery();
            //if(result == 1)
            //{
            //    Message = "Order number: " + ControlNumber + " aAdded successfully";
            //}
            //else
            //{
            //    Message = "Order : " + ControlNumber + " not added";
            //}
            //con.Close();
            //return Message;

        }

        public string InsertClientDetails(string name, string email, string address, string city, string zip, string tel, string fax)
        {
            DBConnection clientHandler = new DBConnection();

            var result = clientHandler.InsertClientDetailsHandler(name, email, address, city, zip, tel, fax);
            return result;
        }
        
        public string PaymentResponse(string PaymentResponse)
        {
            DBConnection responseHandler = new DBConnection();

            var result = responseHandler.PaymentResponseLogHandler(PaymentResponse);

            return result;
        }

        public string StarGeoprocess(string jsonMap)
        {
            Geoprocessing geo = new Geoprocessing();
            var result = geo.FotoAerea(jsonMap);
            var res = result.ToString();
            return res;
        }

        public string InsertAerialPhotoItem(string controlNumber, int clientId, string itemName, int itemQty, string item, string format, string layoutTemplate, string georefInfo, string parcel, string subtitle)
        {
            DBConnection AerialHandler = new DBConnection();
            var result = AerialHandler.InsertAerialPhotoHandler(controlNumber, clientId, itemName, itemQty, item, format, layoutTemplate, georefInfo, parcel, subtitle);
            return result;
        }


        public string InsertListaColindanteItem(string controlNumber, int clientId, string itemName, int itemQty, string item)
        {
            DBConnection lista = new DBConnection();
            var result = lista.InsertListaColindanteItemHanlder(controlNumber, clientId, itemName, itemQty, item);
            return result;
        }


        public string InsertCatastralItem(string controlNumber, int clientId, string itemName, int itemQty, string escala, string cuadricula, string template)
        {
            DBConnection catastro = new DBConnection();
            var result = catastro.InsertCatastralesHandler(controlNumber, clientId, itemName, itemQty, escala, cuadricula, template);
            return "here goes the message";
        }
    }

}
