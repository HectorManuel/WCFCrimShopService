using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Specialized;
using System.Web;


namespace WcfCrimShopService.entities
{
    public class DBConnection
    {
        public string PaymentResponseLogHandler(string PaymentResponse)
        {
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

            if (result2 == 1 && result == 1)
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

        public string InsertAerialPhotoHandler(string controlNumber, int clientId, string itemName, int itemQty, string item, string format, string layoutTemplate, string georefInfo, string parcel, string subtitle)
        {
            SqlConnection con = new SqlConnection(@"Data Source=GMTWKS13\GMTWKS13DB;Initial Catalog=CRIMShopManagement;User ID=User;Password=user123;");
            con.Open();

            string query = "INSERT into dbo.OrderItemAerialphoto (ControlNumber,ClientId,ItemName,ItemQty,Item,Format,LayoutTemplate,GeorefInfo,Parcel,Subtitle)" +
                           "VALUES (@control,@client,@itemName,@qty,@item,@format,@template,@georef,@parcel,@sub)";
            SqlCommand cmd = new SqlCommand(query, con);
            if (string.IsNullOrEmpty(controlNumber))
            {
                return "Control Number Needed";
            }
            else
            {
                cmd.Parameters.AddWithValue("@control", controlNumber);
            }

            if (clientId == 0)
            {
                return "Client ID missing";
            }
            else
            {
                cmd.Parameters.AddWithValue("@client", clientId);
            }
            if (string.IsNullOrEmpty(item))
            {
                return "must include the map json on the item ";
            }
            else
            {
                cmd.Parameters.AddWithValue("@item", item);
            }
            if (string.IsNullOrEmpty(layoutTemplate))
            {
                return "must specify a layout template";
            }
            else
            {
                cmd.Parameters.AddWithValue("@template", layoutTemplate);
            }
            if (string.IsNullOrEmpty(itemName))
            {
                itemName = "Foto Aérea";
            }
            if (itemQty == 0)
            {
                itemQty = 1;
            }
            if (string.IsNullOrEmpty(format))
            {
                format = "PDF";
            }
            if (string.IsNullOrEmpty(georefInfo))
            {
                georefInfo = "false";
            }
            if (string.IsNullOrEmpty(parcel))
            {
                parcel = "----";
            }
            if (string.IsNullOrEmpty(subtitle))
            {
                subtitle = "zona seleccionada";
            }
            cmd.Parameters.AddWithValue("@itemName", itemName);
            cmd.Parameters.AddWithValue("@qty", itemQty);
            cmd.Parameters.AddWithValue("@format", format);
            cmd.Parameters.AddWithValue("@georef", georefInfo);
            cmd.Parameters.AddWithValue("@parcel", parcel);
            cmd.Parameters.AddWithValue("@sub", subtitle);

            var result = cmd.ExecuteNonQuery();
            string message;
            if (result == 1)
            {
                message = controlNumber;
            }
            else
            {
                message = "error inserting Photo item into DB";
            }

            return message;
        }

        public string InsertCatastralesHandler(string controlNumber, int clientId, string itemName, int itemQty, string escala, string cuadricula, string template)
        {
            SqlConnection con = new SqlConnection(@"Data Source=GMTWKS13\GMTWKS13DB;Initial Catalog=CRIMShopManagement;User ID=User;Password=user123;");
            con.Open();

            string query = "INSERT into dbo.OrderItemsCatastrales (ControlNumber,ClientId,ItemName,ItemQty,Escala,Cuadricula,Template)" +
                           "VALUES (@control,@client,@itemName,@qty,@escala,@cuadricula, @template";
            SqlCommand cmd = new SqlCommand(query, con);

            if (string.IsNullOrEmpty(controlNumber))
            {
                return "Control Number Needed";
            }
            else
            {
                cmd.Parameters.AddWithValue("@control", controlNumber);
            }

            if (clientId == 0)
            {
                return "Client ID missing";
            }
            else
            {
                cmd.Parameters.AddWithValue("@client", clientId);
            }

            if (string.IsNullOrEmpty(cuadricula))
            {
                return "must include the map json on the item ";
            }
            else
            {
                cmd.Parameters.AddWithValue("@cuadricula", cuadricula);
            }

            if (string.IsNullOrEmpty(itemName))
            {
                itemName = cuadricula;
            }

            if (itemQty == 0)
            {
                itemQty = 1;
            }

            if (string.IsNullOrEmpty(escala))
            {
                escala = "unknown";
            }
            if (string.IsNullOrEmpty(template))
            {
                template = "Peticiones-11x17";
            }
            cmd.Parameters.Add("@itemName", itemName);
            cmd.Parameters.Add("@qty", itemQty);
            cmd.Parameters.AddWithValue("@template", template);
            cmd.Parameters.AddWithValue("@escala", escala);


            var result = cmd.ExecuteNonQuery();
            string message;
            if (result == 1)
            {
                message = controlNumber;
            }
            else
            {
                message = "error inserting Item Catastral into DB";
            }

            return message;
        }

        public string InsertListaColindanteItemHanlder(string controlNumber, int clientId, string itemName, int itemQty, string item)
        {
            SqlConnection con = new SqlConnection(@"Data Source=GMTWKS13\GMTWKS13DB;Initial Catalog=CRIMShopManagement;User ID=User;Password=user123;");
            con.Open();

            string query = "INSERT into dbo.OrderItemsListaColindante (ControlNumber,ClientId,ItemName,ItemQty,Item)" +
                           "VALUES (@control,@client,@itemName,@qty,@item";
            SqlCommand cmd = new SqlCommand(query, con);

            if (string.IsNullOrEmpty(controlNumber))
            {
                return "Control Number Needed";
            }
            else
            {
                cmd.Parameters.AddWithValue("@control", controlNumber);
            }

            if (clientId == 0)
            {
                return "Client ID missing";
            }
            else
            {
                cmd.Parameters.AddWithValue("@client", clientId);
            }

            if (string.IsNullOrEmpty(item))
            {
                return "must include the json for the item ";
            }
            else
            {
                cmd.Parameters.AddWithValue("@item", item);
            }

            if (string.IsNullOrEmpty(itemName))
            {
                itemName = "Lista De Colindante";
            }

            if (itemQty == 0)
            {
                itemQty = 1;
            }

            cmd.Parameters.AddWithValue("@itemName", itemName);
            cmd.Parameters.AddWithValue("@qty", itemQty);


            var result = cmd.ExecuteNonQuery();
            string message;
            if (result == 1)
            {
                message = controlNumber;
            }
            else
            {
                message = "error inserting Lista de colindante into DB";
            }

            return message;
        }

        public string InsertOrderDetailsHandler(string ControlNumber, string Description, string clientId, decimal tx, decimal sTotal, decimal Total)
        {
            string Message;
            DateTime OrderDate = DateTime.Now;

            SqlConnection con = new SqlConnection(@"Data Source=GMTWKS13\GMTWKS13DB;Initial Catalog=CRIMShopManagement;User ID=User;Password=user123;");
            //SqlConnection con = new SqlConnection(@"Data Source=HECTOR_CUSTOMS\MYOWNSQLSERVER;Initial Catalog=CRIMShopManagement;Trusted_Connection=Yes;");
            con.Open();
            //string queryString = "INSERT into dbo.Orders (ContorlNumber,PaymentResponse,Description)" +
            //        "VALUES (@control,@response,@description)";
            string queryString = "INSERT into dbo.Orders (ControlNumber,Description,ClientId,Confirmation,Tax,Subtotal,Total,OrderDate)" +
                                " VALUES (@control,@description,@clientId,@confirmation,@tax,@subtotal,@total,@orderDate)";
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
                cmd.Parameters.AddWithValue("@clientId", clientId);
            }
            else
            {
                cmd.Parameters.AddWithValue("@clientId", clientId);
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
            cmd.Parameters.AddWithValue("@orderDate", OrderDate);

            int result = cmd.ExecuteNonQuery();
            if (result == 1)
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


        public string InsertClientDetailsHandler(string name, string email, string address, string city, string zip, string tel, string fax)
        {
            string Message;
            SqlConnection con = new SqlConnection(@"Data Source=GMTWKS13\GMTWKS13DB;Initial Catalog=CRIMShopManagement;User ID=User;Password=user123;");
            //SqlConnection con = new SqlConnection(@"Data Source=HECTOR_CUSTOMS\MYOWNSQLSERVER;Initial Catalog=CRIMShopManagement;Trusted_Connection=Yes;");
            con.Open();
            //string queryString = "INSERT into dbo.Orders (ContorlNumber,PaymentResponse,Description)" +
            //        "VALUES (@control,@response,@description)";
            string queryString = "INSERT into dbo.Client (Name, Email, Address, City, Zip, Telephone, Fax)" +
                                "VALUES (@name, @email, @adress, @city, @zip, @telephone, @fax)";

            SqlCommand cmd = new SqlCommand(queryString, con);
            //if (string.IsNullOrEmpty(clientId))
            //{
            //    return "client Id cannot be empty";
            //}
            //else
            //{
            //    cmd.Parameters.AddWithValue("@clientId", clientId);
            //}
            if (string.IsNullOrEmpty(name))
            {
                return "name cannot be empty";
            }
            else
            {
                cmd.Parameters.AddWithValue("@name", name);
            }

            if (string.IsNullOrEmpty(email))
            {
                return "email cannot be empty";
            }
            else
            {
                cmd.Parameters.AddWithValue("@email", email);
            }
            if (string.IsNullOrEmpty(address))
            {
                address = string.Empty;
            }
            
            if (string.IsNullOrEmpty(city))
            {
                city = string.Empty;
            }
            
            if (string.IsNullOrEmpty(tel))
            {
                tel = string.Empty;
            }
            if (string.IsNullOrEmpty(fax))
            {
                fax = string.Empty;
            }
            if (string.IsNullOrEmpty(zip))
            {
                zip = "00000";
            }
            cmd.Parameters.AddWithValue("@city", city);
            cmd.Parameters.AddWithValue("@zip", zip);
            cmd.Parameters.AddWithValue("@adress", address);
            cmd.Parameters.AddWithValue("@telephone", tel);
            cmd.Parameters.AddWithValue("@fax", fax);

            int result = cmd.ExecuteNonQuery();

            if (result == 1)
            {
                Message = "Client: " + name + " Added successfully";
            }
            else
            {
                Message = "Client: " + name + " not added";
            }
            con.Close();
            return Message;
        }
    }
}