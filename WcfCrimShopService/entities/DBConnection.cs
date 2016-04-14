using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Specialized;
using WcfCrimShopService.entities;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using System.IO;

namespace WcfCrimShopService.entities
{
    public class DBConnection
    {
        Geoprocessing geo = new Geoprocessing();
        Objects.ConfigObject config = JsonConvert.DeserializeObject<Objects.ConfigObject>(File.ReadAllText(System.AppDomain.CurrentDomain.BaseDirectory + @"Config.json"));

        public SqlConnection Connection()
        {
            SqlConnection con = new SqlConnection("Data Source=GMTWKS13\\GMTWKS13DB;Initial Catalog=CRIMShopManagement;User ID=User;Password=user123;");
            return con;
        }

        public string PaymentResponseLogHandler(string PaymentResponse)
        {
            string Message = "things";
            NameValueCollection nvc = HttpUtility.ParseQueryString(PaymentResponse);

            //string val = nvc.Get("VPaymentDescription");
            string transactionId = nvc.Get("VTransactionId");
            string accountId = nvc.Get("VAccountId");
            string totalAmount = nvc.Get("VTotalAmount");
            string paymentMethod = nvc.Get("VPaymentMethod");
            string paymentDescription = nvc.Get("VPaymentDescription");
            string authorizationNum = nvc.Get("VAuthorizationNum");
            string confirmationNum = nvc.Get("VConfirmationNum");
            string merchantTransId = nvc.Get("VMerchantTransId");
            //string merchantTransId = nvc.Get("VMerchantTransId");

            SqlConnection con = Connection();
            //SqlConnection con = new SqlConnection(@"Data Source=HECTOR_CUSTOMS\MYOWNSQLSERVER;Initial Catalog=CRIMShopManagement;Trusted_Connection=Yes;");
            con.Open();
            //string queryString = "INSERT into dbo.Orders (ContorlNumber,PaymentResponse,Description)" +
            //        "VALUES (@control,@response,@description)";
            string queryString = "UPDATE dbo.Orders SET Confirmation=@confirm" +
                                " WHERE ControlNumber=@control";
            SqlCommand cmd = new SqlCommand(queryString, con);
            cmd.Parameters.AddWithValue("@control", merchantTransId);
            
            cmd.Parameters.AddWithValue("@confirm", confirmationNum);
            int result = cmd.ExecuteNonQuery();

            //string queryString = "INSERT into dbo.Orders (ContorlNumber,PaymentResponse,Description)" +
            //        "VALUES (@control,@response,@description)";
            string queryString2 = "INSERT into dbo.PaymentResponseLog (VTransactionID,VAccountId,VTotalAmount,VPaymentMethod,VPaymentDescription,VAuthorizationNum,VConfirmationNum, VMerchantTransId)" +
                                "VALUES (@VTransactionID,@VAccountId,@VTotalAmount,@VPaymentMethod,@VPaymentDescription,@VAuthorizationNum,@VConfirmationNum, @VMerchantTransId)";
            SqlCommand cmd2 = new SqlCommand(queryString2, con);
            //cmd.Parameters.AddWithValue("@control", ControlNumber);
            cmd2.Parameters.AddWithValue("@VTransactionID", transactionId);
            cmd2.Parameters.AddWithValue("@VAccountId", accountId);
            cmd2.Parameters.AddWithValue("@VTotalAmount", totalAmount);
            cmd2.Parameters.AddWithValue("@VPaymentMethod", paymentMethod);
            cmd2.Parameters.AddWithValue("@VPaymentDescription", paymentDescription);
            cmd2.Parameters.AddWithValue("@VAuthorizationNum", authorizationNum);
            cmd2.Parameters.AddWithValue("@VConfirmationNum", confirmationNum);
            cmd2.Parameters.AddWithValue("@VMerchantTransId", merchantTransId);
            //cmd2.Parameters.AddWithValue("@VMerchantTransId", merchantTransId);
            int result2 = cmd2.ExecuteNonQuery();

            if (result2 == 1 && result == 1)
            {
                Message = "ok";
                // run task to get information for the printing service
            }
            else
            {
                Message = "Order  not updated: " + PaymentResponse;
            }
            con.Close();

            if (result2 == 1 && result == 1)
            {

                Task.Run( () =>
                {
                   GetOrderDetails(merchantTransId);
                });
            }
            return Message;
        }

        public string InsertAerialPhotoHandler(string controlNumber, int itemQty, string item, string format, string layoutTemplate, string georefInfo, string parcel, string subtitle, string buffer, string parcelList, string bufferDistance)
        {
            SqlConnection con = Connection();
            con.Open();

            string query = "INSERT into dbo.OrderItemAerialphoto (ControlNumber,ItemQty,Item,Format,LayoutTemplate,GeorefInfo,Parcel,Subtitle,Buffer,ParcelList,BufferDistance) " +
                           "VALUES (@control,@qty,@item,@format,@template,@georef,@parcel,@sub,@buffer,@list,@bufferDistance)";
            SqlCommand cmd = new SqlCommand(query, con);
            if (string.IsNullOrEmpty(controlNumber))
            {
                return "Control Number Needed";
            }
            else
            {
                cmd.Parameters.AddWithValue("@control", controlNumber);
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
                parcel = "printing";
            }
            if (string.IsNullOrEmpty(subtitle))
            {
                subtitle = "zona seleccionada";
            }
            if (buffer.ToLower() != "false")
            {
                cmd.Parameters.AddWithValue("@buffer", buffer.ToLower());
                if (parcelList != string.Empty)
                {
                    cmd.Parameters.AddWithValue("@list", parcelList);
                }
                if (bufferDistance != string.Empty)
                {
                    cmd.Parameters.AddWithValue("@bufferDistance", bufferDistance);
                }
                               
            }
            else
            {
                cmd.Parameters.AddWithValue("@buffer", buffer.ToLower());
                parcelList = string.Empty;
                cmd.Parameters.AddWithValue("@list", parcelList);
                bufferDistance = string.Empty;
                cmd.Parameters.AddWithValue("@bufferDistance", bufferDistance);
            }
            
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

        public string InsertCatastralesHandler(string controlNumber, string itemName, int itemQty, string escala, string template, string cuadricula1, string cuadricula10)
        {
            
            decimal cost = Convert.ToDecimal(geo.CalculatePrice("catastrales", itemQty));
            SqlConnection con = Connection();
            con.Open();
            if (string.IsNullOrEmpty(cuadricula1))
            {
                string[] Cuadricula1k = cuadricula1.Split(',');
            }
            if (string.IsNullOrEmpty(cuadricula10))
            {
                string[] Cuadricula10k = cuadricula10.Split(',');
            }
            //*****************************************************
            //FALTA MODIFICAR ESTA FUNCTION CON UN FOR LOOP PARA LAS LISTAS Y TOMAR ESTA FUNCION Y COLOCARLA AFUERA
            //PARA EVITAR REPETIR CODIGO
            string query = "INSERT into dbo.OrderItemsCatastrales (ControlNumber,ItemName,ItemQty,Escala,Cuadricula,Template,Price) " +
                           "VALUES (@control,@itemName,@qty,@escala,@cuadricula, @template,@price) ";
            SqlCommand cmd = new SqlCommand(query, con);

            if (string.IsNullOrEmpty(controlNumber))
            {
                return "Control Number Needed";
            }
            else
            {
                cmd.Parameters.AddWithValue("@control", controlNumber);
            }

            if (string.IsNullOrEmpty(cuadricula1))
            {
                return "must include the map json on the item ";
            }
            else
            {
                cmd.Parameters.AddWithValue("@cuadricula", cuadricula1);
            }
            
            if (string.IsNullOrEmpty(itemName))
            {
                itemName = cuadricula1;
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
            cmd.Parameters.AddWithValue("@itemName", itemName);
            cmd.Parameters.AddWithValue("@qty", itemQty);
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
            con.Close();
            return message;
        }

        public string InsertListaColindanteItemHanlder(string controlNumber, string itemName, int itemQty, string item)
        {
            string message;
            try
            {
                decimal cost = Convert.ToDecimal(geo.CalculatePrice("colindante", itemQty));
                SqlConnection con = Connection();
                con.Open();

                string query = "INSERT into dbo.OrderItemsListaColindante (ControlNumber,Parcelas,ItemQty,Item,Price) " +
                               "VALUES (@control,@itemName,@qty,@item,@cost ";
                SqlCommand cmd = new SqlCommand(query, con);

                if (string.IsNullOrEmpty(controlNumber))
                {
                    return "Control Number Needed";
                }
                else
                {
                    cmd.Parameters.AddWithValue("@control", controlNumber);
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

                cmd.Parameters.AddWithValue("@cost", cost);

                var result = cmd.ExecuteNonQuery();
                
                if (result == 1)
                {
                    message = controlNumber;
                }
                else
                {
                    message = "error inserting Lista de colindante into DB";
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
            

            return message;
        }

        public string InsertOrderDetailsHandler(string ControlNumber, string Description, decimal tx, decimal sTotal, decimal Total, string CustomerName, string customerEmail, string hasPhoto, string hasCat, string hasList)
        {
            string Message;
            DateTime OrderDate = DateTime.Now;

            SqlConnection con = Connection();
            //SqlConnection con = new SqlConnection(@"Data Source=HECTOR_CUSTOMS\MYOWNSQLSERVER;Initial Catalog=CRIMShopManagement;Trusted_Connection=Yes;");
            con.Open();
            //string queryString = "INSERT into dbo.Orders (ContorlNumber,PaymentResponse,Description)" +
            //        "VALUES (@control,@response,@description)";
            string queryString = "INSERT into dbo.Orders (ControlNumber,Description,Confirmation,Tax,SubTotal,Total,OrderDate,CustomerName,CustomerEmail,HasPhoto,HasCat,HasList)" +
                                " VALUES (@control,@description,@confirmation,@tax,@subtotal,@total,@orderDate,@Name,@email,@photo,@cat,@list)";
            SqlCommand cmd = new SqlCommand(queryString, con);
            #region validations
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

            if (CustomerName == string.Empty)
            {
                return "Customer Name cannot be empty";
            }
            else
            {
                cmd.Parameters.AddWithValue("@Name", CustomerName);
            }

            if (customerEmail == string.Empty)
            {
                return "email cannot be empty";
            }
            else
            {
                cmd.Parameters.AddWithValue("@email", customerEmail);
            }

            if (hasPhoto == string.Empty)
            {
                cmd.Parameters.AddWithValue("@photo", "Y");
            }
            else
            {
                cmd.Parameters.AddWithValue("@photo", hasPhoto);
            }

            if (hasCat == string.Empty)
            {
                cmd.Parameters.AddWithValue("@cat", "Y");
            }
            else
            {
                cmd.Parameters.AddWithValue("@cat", hasCat);
            }

            if (hasList == string.Empty)
            {
                cmd.Parameters.AddWithValue("@list", "Y");
            }
            else
            {
                cmd.Parameters.AddWithValue("@list", hasList);
            }

            cmd.Parameters.AddWithValue("@confirmation", "Processing");
            cmd.Parameters.AddWithValue("@orderDate", OrderDate);
            #endregion

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


        //Asyncronous calls to the database to get the infromation of the order items and process it
        public void GetOrderDetails(string controlNumber)
        {
            SqlConnection con = Connection();
            con.Open();

            string query = "SELECT ControlNumber,Confirmation,CustomerName,CustomerEmail,HasPhoto,HasCat,HasList " +
                           "FROM dbo.Orders " +
                           "WHERE ControlNumber=@control ";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@control", controlNumber);
            
            List<Objects.Order> orderList = new List<Objects.Order>();
            using (SqlDataReader result = cmd.ExecuteReader())
            {
                while (result.Read())
                {
                    string cn = result["ControlNumber"].ToString();
                    string confirm = result["Confirmation"].ToString();
                    string name = result["CustomerName"].ToString();
                    string email = result["CustomerEmail"].ToString();
                    string haspic = result["HasPhoto"].ToString();
                    string hascat = result["HasCat"].ToString();
                    string haslist = result["HasList"].ToString();

                    orderList.Add(new Objects.Order
                    {
                        ControlNumber = cn,
                        Confirmation = confirm,
                        CustomerName = name,
                        CustomerEmail = email,
                        HasPhoto = haspic,
                        HasCat = hascat, 
                        HasList = haslist
                    });
                }
            }

            string ending = GetItems(orderList);

            con.Close();


        }

        public string GetItems(List<Objects.Order> myOrder)
        {

            string pictureContent = string.Empty;
            string cadastreContent = string.Empty;
            string listContent = string.Empty;

            if (myOrder[0].HasPhoto.ToUpper() == "Y")
            {
                pictureContent = ProcessPhotoProducts(myOrder[0].ControlNumber);
            }

            if (myOrder[0].HasCat.ToUpper() == "Y")
            {
                cadastreContent = ProcessCadastralProducts(myOrder[0].ControlNumber);
            }

            if (myOrder[0].HasList.ToUpper() == "Y")
            {
                listContent = ProcessListProducts(myOrder[0].ControlNumber, myOrder[0].CustomerName);
            }

            if (string.IsNullOrEmpty(pictureContent))
            {
                if (string.IsNullOrEmpty(cadastreContent))
                {
                    if (string.IsNullOrEmpty(listContent))
                    {
                        pictureContent = "Error: no items in order";
                    }
                    else
                    {
                        pictureContent = listContent;
                    }
                }
                else
                {
                    pictureContent = cadastreContent;
                }
            }

            geo.ZipAndSendEmail(pictureContent, myOrder[0].CustomerEmail);
            
            return pictureContent;
        }

        public string ProcessPhotoProducts(string controlNumber)
        {

            SqlConnection con = Connection();
            con.Open();

            string query = "SELECT ControlNumber,ItemQty,Item,Format,LayoutTemplate,GeorefInfo,Parcel,Subtitle,Buffer,ParcelList,BufferDistance " +
                            "FROM dbo.OrderItemAerialPhoto " +
                            "WHERE ControlNumber=@control ";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@control", controlNumber);

            List<Objects.OrderItemPhoto> orderList = new List<Objects.OrderItemPhoto>();
            using (SqlDataReader result = cmd.ExecuteReader())
            {
                while (result.Read())
                {
                    string cn = result["ControlNumber"].ToString();
                    string qty = result["ItemQty"].ToString();
                    string item = result["Item"].ToString();
                    string format = result["Format"].ToString();
                    string template = result["LayoutTemplate"].ToString();
                    string georef = result["GeorefInfo"].ToString();
                    string title = result["Parcel"].ToString();
                    string sub = result["Subtitle"].ToString();
                    string buffer = result["Buffer"].ToString();
                    string parcelList = result["ParcelList"].ToString();
                    string bufferDistance = result["BufferDistance"].ToString();

                    orderList.Add(new Objects.OrderItemPhoto
                    {
                        ControlNumber = cn,
                        ItemQty = qty,
                        Item = item,
                        Format = format,
                        LayoutTemplate = template,
                        GeorefInfo = georef,
                        Parcel = title,
                        subtitle = sub,
                        buffer = buffer,
                        parcelList = parcelList,
                        distance = bufferDistance
                    });
                }
            }
            string path = string.Empty;
            var task = Task.Run(async () => {
                var createPrinting = await geo.FotoAerea(orderList);
                path = createPrinting.ToString();
            });
            task.Wait();

            return path;
        }

        public string ProcessListProducts(string controlNumber, string customerName)
        {
            string path = string.Empty;
            SqlConnection con = Connection();
            con.Open();

            string query = "SELECT ControlNumber,Parcelas,ItemQty,Item " +
                            "FROM dbo.OrderItemsListaColindante " +
                            "WHERE ControlNumber=@control ";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@control", controlNumber);

            List<Objects.OrderItemList> orderList = new List<Objects.OrderItemList>();
            using (SqlDataReader result = cmd.ExecuteReader())
            {
                while (result.Read())
                {
                    string cn = result["ControlNumber"].ToString();
                    string itemName = result["Parcelas"].ToString();
                    string qty = result["ItemQty"].ToString();
                    string item = result["Item"].ToString();
                    
                    orderList.Add(new Objects.OrderItemList
                    {
                        ControlNumber = cn, 
                        itemName = itemName,
                        itemQty = qty, 
                        item = item
                    });
                }
                
                var createPrinting = geo.AdyacentListGenerator(orderList, customerName);
                path = createPrinting.ToString();


            }
            
            
            return path;
        }

        public string ProcessCadastralProducts(string controlNumber)
        {

            SqlConnection con = Connection();
            con.Open();

            string query = "SELECT ControlNumber,ItemName,ItemQty,Escala,Cuadricula,Template " +
                            "FROM dbo.OrderItemsCatastrales " +
                            "WHERE ControlNumber=@control ";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@control", controlNumber);

            List<Objects.OrderItemCatastral> orderList = new List<Objects.OrderItemCatastral>();
            using (SqlDataReader result = cmd.ExecuteReader())
            {
                while (result.Read())
                {
                    string cn = result["ControlNumber"].ToString();
                    string name = result["ItemName"].ToString();
                    string qty = result["ItemQty"].ToString();
                    string scale = result["Escala"].ToString();
                    string cuadro = result["Cuadricula"].ToString();
                    string template = result["Template"].ToString();

                    orderList.Add(new Objects.OrderItemCatastral
                    {
                        ControlNumber = cn,
                        itemName = name,
                        itemQty = qty,
                        escala = scale,
                        cuadricula = cuadro,
                        template = template
                    });
                }
            }

            string path = string.Empty;
            var task = Task.Run(async () =>
            {
                var createPrinting = await geo.OficialMaps(orderList);
                path = createPrinting.ToString();
            });
            task.Wait();
            
            return path;
        }

        public string LogTransaction()
        {
            return "this will save the service work on the log";
        }

        public Objects.ProductPrice PriceProduct(string product, int qty)
        {
            Objects.ProductPrice pp = new Objects.ProductPrice();
            SqlConnection con = Connection();//new SqlConnection("Data Source=GMTWKS13\\GMTWKS13DB;Initial Catalog=CRIMShopManagement;User ID=User;Password=user123;");
            con.Open();

            string query = "SELECT Product,Price " +
                           "FROM dbo.ProductPrice " +
                           "WHERE Product=@product ";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@product", product);

            
            using (SqlDataReader result = cmd.ExecuteReader())
            {
                while (result.Read())
                {
                    string cn = result["Product"].ToString();
                    decimal confirm = Convert.ToDecimal(result["Price"].ToString());

                    pp.price = confirm;
                    pp.product = cn;
                }
            }

            con.Close();

            return pp;
        }
    }
}