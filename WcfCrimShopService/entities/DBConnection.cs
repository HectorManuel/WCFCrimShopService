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

using System.Threading;
using System.Diagnostics;

namespace WcfCrimShopService.entities
{
    public class DBConnection
    {
        Geoprocessing geo = new Geoprocessing();
        Objects.ConfigObject config = JsonConvert.DeserializeObject<Objects.ConfigObject>(File.ReadAllText(System.AppDomain.CurrentDomain.BaseDirectory + @"Config.json"));

        public SqlConnection Connection()
        {
            
            var configInfo = config.ServerConnection;
            SqlConnection con = new SqlConnection("Data Source="+configInfo.source+";Initial Catalog="+configInfo.catalog+";User ID="+configInfo.id+";Password="+configInfo.password+";");
            return con;
        }

        public string AwaitForResponse(string order)
        {
            SqlConnection con = Connection();
            //SqlConnection con = new SqlConnection(@"Data Source=HECTOR_CUSTOMS\MYOWNSQLSERVER;Initial Catalog=CRIMShopManagement;Trusted_Connection=Yes;");
            con.Open();
            string query = "SELECT Confirmation " +
                           "FROM dbo.Orders " +
                           "WHERE ControlNumber= @control";

            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@control", order);
            string ds = "Processing";
            //var list = new List<Objects.Order>();
            //var result = cmd.ExecuteNonQuery();
            int time = 0;

            do
            {
                using (SqlDataReader result = cmd.ExecuteReader())
                {
                    while (result.Read())
                    {
                        ds = result["Confirmation"].ToString();

                        //list.Add(new Objects.Order{ControlNumber= cn, Confirmation= confirm, Description = desc});
                    }
                    Thread.Sleep(2000); // modify to await 20min later
                    time += 2;
                    Debug.WriteLine(time + " " + ds);

                }
            } while (ds == "Processing" && time != 20);

            if (ds == "Processing")
            {
                ds = "error completing the payment";
            }

            return ds;
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

        public string PaymentResponseLogHandlerEmployee(string controlNumber)
        {
            string Message = string.Empty;


            SqlConnection con = Connection();
            //SqlConnection con = new SqlConnection(@"Data Source=HECTOR_CUSTOMS\MYOWNSQLSERVER;Initial Catalog=CRIMShopManagement;Trusted_Connection=Yes;");
            con.Open();
            //string queryString = "INSERT into dbo.Orders (ContorlNumber,PaymentResponse,Description)" +
            //        "VALUES (@control,@response,@description)";
            string queryString = "UPDATE dbo.Orders SET Confirmation=@confirm" +
                                " WHERE ControlNumber=@control";
            SqlCommand cmd = new SqlCommand(queryString, con);
            cmd.Parameters.AddWithValue("@control", controlNumber);

            cmd.Parameters.AddWithValue("@confirm", "Employee");
            int result = cmd.ExecuteNonQuery();

            if (result == 1)
            {
                Message = "ok";
                // run task to get information for the printing service
            }
            else
            {
                Message = "Order  not submitted: #order - " + controlNumber;
            }
            con.Close();

            if (result == 1)
            {

                Task.Run(() =>
                {
                    GetOrderDetails(controlNumber);
                });
            }
            return Message;
        }

        public string InsertAerialPhotoHandler(string title, string controlNumber, int itemQty, string item, string format, string layoutTemplate, string georefInfo, string parcel, string subtitle, string buffer, string parcelList, string bufferDistance)
        {
            decimal cost = Convert.ToDecimal(geo.CalculatePrice("fotoAerea", itemQty));
            SqlConnection con = Connection();

            con.Open();

            string query = "INSERT into dbo.OrderItemAerialphoto (Title,ControlNumber,ItemQty,Item,Format,LayoutTemplate,GeorefInfo,Parcel,Subtitle,Buffer,ParcelList,BufferDistance,Price) " +
                           "VALUES (@title,@control,@qty,@item,@format,@template,@georef,@parcel,@sub,@buffer,@list,@bufferDistance,@price)";
            SqlCommand cmd = new SqlCommand(query, con);

            if (string.IsNullOrEmpty(title))
            {
                cmd.Parameters.AddWithValue("@title", "Foto Aérea y Mapa De Catastro");
            }
            else
            {
                cmd.Parameters.AddWithValue("@title", title);
            }
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
            
            cmd.Parameters.AddWithValue("@price", cost);

            cmd.Parameters.AddWithValue("@qty", itemQty);
            cmd.Parameters.AddWithValue("@format", format);
            cmd.Parameters.AddWithValue("@georef", georefInfo);
            cmd.Parameters.AddWithValue("@parcel", parcel);
            cmd.Parameters.AddWithValue("@sub", subtitle);

            var result = cmd.ExecuteNonQuery();
            string message;
            if (result == 1)
            {
                message = "OK";
                LogTransaction(controlNumber, "Order Photo Added To DB");
            }
            else
            {
                message = "error inserting Photo item into DB";
            }

            return message;
        }

        public string InsertCatastralesHandler(string controlNumber, int itemQty, string cuadricula1, string cuadricula10)
        {
            
            //decimal cost = Convert.ToDecimal(geo.CalculatePrice("catastrales", itemQty));
            List<string> Cuadricula1k = new List<string>();
            List<string> Cuadricula10k = new List<string>();
            string message = string.Empty;
            if (!string.IsNullOrEmpty(cuadricula1))
            {
                var splitting = cuadricula1.Split(',');
                foreach(var cuad in splitting){
                    Cuadricula1k.Add(cuad);
                }
            }
            if (!string.IsNullOrEmpty(cuadricula10))
            {
                var splitting = cuadricula10.Split(',');
                foreach(var cuad in splitting){
                    Cuadricula10k.Add(cuad);
                }
            }

            if (Cuadricula10k.Count() != 0)
            {
                foreach (var cad in Cuadricula10k)
                {
                    message = CadastreHandler(controlNumber, 1, "1:10000", "MapaCatastral_10k", cad);
                   if (message != "sucess")
                   {
                       return message;
                   }
                }
            }
            if (Cuadricula1k.Count() != 0)
            {
                foreach (var cad in Cuadricula1k)
                {
                    message = CadastreHandler(controlNumber, 1, "1:1000", "MapaCatastral_1k", cad);
                    if (message != "sucess")
                    {
                        return message;
                    }
                }
            }
            
            return message;
        }

        public string CadastreHandler(string controlNumber, int itemQty, string escala, string template, string cuadricula)
        {
            string message = string.Empty;
            try
            {
                SqlConnection con = Connection();
                con.Open();
                string query = "INSERT into dbo.OrderItemsCatastrales (ControlNumber,ItemQty,Escala,Cuadricula,Template,Price) " +
                               "VALUES (@control,@qty,@escala,@cuadricula, @template,@price) ";
                SqlCommand cmd = new SqlCommand(query, con);

                if (string.IsNullOrEmpty(controlNumber))
                {
                    return "Control Number Needed";
                }
                else
                {
                    cmd.Parameters.AddWithValue("@control", controlNumber);
                }

                if (string.IsNullOrEmpty(cuadricula))
                {
                    return "must include the map json on the item ";
                }
                else
                {
                    cmd.Parameters.AddWithValue("@cuadricula", cuadricula);
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
                
                cmd.Parameters.AddWithValue("@qty", itemQty);
                cmd.Parameters.AddWithValue("@template", template);
                cmd.Parameters.AddWithValue("@escala", escala);

                decimal cost = Convert.ToDecimal(geo.CalculatePrice("catastrales", itemQty));
                cmd.Parameters.AddWithValue("@price", cost);

                var result = cmd.ExecuteNonQuery();

                if (result == 1)
                {
                    message = "sucess";
                    LogTransaction(controlNumber, "Order Cadastre Added To DB");
                }
                else
                {
                    message = "error inserting Item Catastral into DB";
                }
                con.Close();
            }
            catch (Exception e)
            {
                message = e.Message;
            }
            
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
                               "VALUES (@control,@itemName,@qty,@item,@cost) ";
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

                cmd.Parameters.AddWithValue("@itemName", itemName);
                cmd.Parameters.AddWithValue("@qty", itemQty);

                cmd.Parameters.AddWithValue("@cost", cost);

                var result = cmd.ExecuteNonQuery();
                
                if (result == 1)
                {
                    message = "OK";
                    LogTransaction(controlNumber, "Order ListaColindante Added To DB");
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
            string Message = string.Empty;
            DateTime OrderDate = DateTime.Now;

            SqlConnection con = Connection();
            try
            {
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
                    cmd.Parameters.AddWithValue("@tax", GetTax());
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
                    cmd.Parameters.AddWithValue("@total", Total);
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
                    Message = "ok";
                    LogTransaction(ControlNumber, "Order Added To DB");
                }
                else
                {
                    Message = "Order : " + ControlNumber + " not added";
                }
            }
            catch (Exception e)
            {
                LogTransaction(ControlNumber, e.Message);
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

            if (myOrder[0].HasList.ToUpper() == "Y")
            {
                listContent = ProcessListProducts(myOrder[0].ControlNumber, myOrder[0].CustomerName);
            }

            if (myOrder[0].HasCat.ToUpper() == "Y")
            {
                cadastreContent = ProcessCadastralProducts(myOrder[0].ControlNumber);
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

            if (pictureContent != "Error: no items in order")
            {
                geo.ZipAndSendEmail(pictureContent, myOrder[0].CustomerEmail, myOrder[0].ControlNumber);
            }
            
            
            return pictureContent;
        }

        public string ProcessPhotoProducts(string controlNumber)
        {

            SqlConnection con = Connection();
            con.Open();

            string query = "SELECT Title,ControlNumber,ItemQty,Item,Format,LayoutTemplate,GeorefInfo,Parcel,Subtitle,Buffer,ParcelList,BufferDistance,Price " +
                            "FROM dbo.OrderItemAerialPhoto " +
                            "WHERE ControlNumber=@control ";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@control", controlNumber);

            List<Objects.OrderItemPhoto> orderList = new List<Objects.OrderItemPhoto>();
            using (SqlDataReader result = cmd.ExecuteReader())
            {
                while (result.Read())
                {
                    string title = result["Title"].ToString();
                    string cn = result["ControlNumber"].ToString();
                    string qty = result["ItemQty"].ToString();
                    string item = result["Item"].ToString();
                    string format = result["Format"].ToString();
                    string template = result["LayoutTemplate"].ToString();
                    string georef = result["GeorefInfo"].ToString();
                    string parcel = result["Parcel"].ToString();
                    string sub = result["Subtitle"].ToString();
                    string buffer = result["Buffer"].ToString();
                    string parcelList = result["ParcelList"].ToString();
                    string bufferDistance = result["BufferDistance"].ToString();
                    decimal cost = Convert.ToDecimal(result["Price"].ToString());

                    orderList.Add(new Objects.OrderItemPhoto
                    {
                        ControlNumber = cn,
                        ItemQty = qty,
                        Item = item,
                        Format = format,
                        LayoutTemplate = template,
                        GeorefInfo = georef,
                        Parcel = parcel,
                        subtitle = sub,
                        buffer = buffer,
                        parcelList = parcelList,
                        distance = bufferDistance,
                        cost = cost,
                        title = title
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

            string query = "SELECT ControlNumber,Parcelas,ItemQty,Item,Price " +
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
                    decimal cost = Convert.ToDecimal(result["Price"].ToString());
                    
                    orderList.Add(new Objects.OrderItemList
                    {
                        ControlNumber = cn, 
                        itemName = itemName,
                        itemQty = qty, 
                        item = item,
                        cost = cost
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

            string query = "SELECT ControlNumber,ItemQty,Escala,Cuadricula,Template,Price " +
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
                    string qty = result["ItemQty"].ToString();
                    string scale = result["Escala"].ToString();
                    string cuadro = result["Cuadricula"].ToString();
                    string template = result["Template"].ToString();
                    decimal cost = Convert.ToDecimal(result["Price"].ToString());

                    orderList.Add(new Objects.OrderItemCatastral
                    {
                        ControlNumber = cn,
                        itemQty = qty,
                        escala = scale,
                        cuadricula = cuadro,
                        template = template,
                        cost = cost
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

        public string LogTransaction(string controlNumber, string description)
        {
            DateTime date = DateTime.Now;            
            SqlConnection con = Connection();
            con.Open();
            string query = "INSERT into dbo.ProductosCartograficosOrderLog (ControlNumber, Description, Date) " +
                           "VALUES (@control,@description,@date) ";
            SqlCommand cmd = new SqlCommand(query, con);

            cmd.Parameters.AddWithValue("@control", controlNumber);
            
            cmd.Parameters.AddWithValue("@description", description);
            cmd.Parameters.AddWithValue("@date", date);

            int result = cmd.ExecuteNonQuery();

            if (result != 1)
            {
                return "Log not processed";
            }
            con.Close();
            return "ok";
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

        public string PriceSubTotal(string controlNumber)
        {
            string message = string.Empty;
            List<Decimal> prices = new List<Decimal>();
            SqlConnection con = Connection();
            try
            {
                
                con.Open();

                string queryPhoto = "SELECT Price FROM dbo.OrderItemAerialphoto WHERE ControlNumber = @control";
                string queryCad = "SELECT Price FROM dbo.OrderItemsCatastrales WHERE ControlNumber = @control";
                string queryList = "SELECT Price FROM dbo.OrderItemsListaColindante WHERE ControlNumber = @control";

                SqlCommand cmd = new SqlCommand(queryPhoto, con);
                cmd.Parameters.AddWithValue("@control", controlNumber);

                using (SqlDataReader result = cmd.ExecuteReader())
                {
                    while (result.Read())
                    {
                        decimal price = Convert.ToDecimal(result["Price"].ToString());

                        prices.Add(price);

                    }
                }

                SqlCommand cmd2 = new SqlCommand(queryCad, con);
                cmd2.Parameters.AddWithValue("@control", controlNumber);

                using (SqlDataReader result = cmd2.ExecuteReader())
                {
                    while (result.Read())
                    {
                        decimal price = Convert.ToDecimal(result["Price"].ToString());

                        prices.Add(price);

                    }
                }

                SqlCommand cmd3 = new SqlCommand(queryList, con);
                cmd3.Parameters.AddWithValue("@control", controlNumber);

                using (SqlDataReader result = cmd3.ExecuteReader())
                {
                    while (result.Read())
                    {
                        decimal price = Convert.ToDecimal(result["Price"].ToString());

                        prices.Add(price);

                    }
                }

                decimal total = 0;
                foreach (decimal pr in prices)
                {
                    total += pr;
                }

                if (total != 0)
                {
                    message = Convert.ToString(total);
                }
                else
                {
                    message = "no items found";
                }
            }
            catch (Exception e)
            {
                message = e.Message;
            }
            con.Close();
            return message;
        }

        public string UpdateCost(string sub, string controlNumber)
        {
            string total=string.Empty;
            decimal tax = GetTax();
            decimal subTotal = Convert.ToDecimal(sub);

            SqlConnection con = Connection();
            try
            {
                con.Open();
                string query = "UPDATE dbo.Orders SET Tax=@tax,SubTotal=@sub,Total=@Total" +
                                " WHERE ControlNumber=@control";
                SqlCommand command = new SqlCommand(query, con);
                command.Parameters.AddWithValue("@control", controlNumber);
                command.Parameters.AddWithValue("@sub", subTotal);
                command.Parameters.AddWithValue("@tax", tax);

                decimal t = System.Math.Round(subTotal + (subTotal * (tax / 100)), 2);
                command.Parameters.AddWithValue("@Total", t);

                int result = command.ExecuteNonQuery();

                if (result == 1)
                {
                    total = t.ToString();
                }
                else
                {
                    total = "error updating the order total";
                }
            }
            catch (Exception e)
            {
                total = e.Message;
            }
            con.Close();
            return total;
        }

        public decimal GetTax()
        {
            decimal tax = 0;
            SqlConnection con = Connection();
            con.Open();
            string query = "SELECT Price FROM dbo.ProductPrice WHERE Product = 'Tax'";
            SqlCommand command = new SqlCommand(query, con);

            using (SqlDataReader read = command.ExecuteReader())
            {
                while (read.Read())
                {
                    tax = Convert.ToDecimal(read["Price"].ToString());
                }
            }
            con.Close();
            return tax;
        }

        public string UpdateFolderPath(string controlNumber, string path)
        {
            string Message = string.Empty;
            SqlConnection con = Connection();
            
            con.Open();

            string queryString = "UPDATE dbo.Orders SET OrderFilePath=@confirm" +
                                " WHERE ControlNumber=@control";
            SqlCommand cmd = new SqlCommand(queryString, con);
            cmd.Parameters.AddWithValue("@control", controlNumber);

            cmd.Parameters.AddWithValue("@confirm", path);
            int result = cmd.ExecuteNonQuery();

            if (result == 1)
            {
                Message = "ok";
                
            }
            else
            {
                Message = "Order  not submitted: #order - " + controlNumber;
            }
            con.Close();
            return Message;
        }
        
        public string GetControlNumberHandler()
        {
            string number = string.Empty;
            SqlConnection con = Connection();
            SqlCommand cmd = new SqlCommand();
            SqlDataReader data;

            cmd.CommandText = "ControlNumberHandler";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Connection = con;
            cmd.Parameters.AddWithValue("@result", "");

            con.Open();
            data = cmd.ExecuteReader();
            con.Close();
            con.Open();
            string getNumber = "SELECT TOP 1 * FROM dbo.ControlNumberTemp ORDER BY createDate DESC";
            SqlCommand cmd2 = new SqlCommand(getNumber, con);

            using (SqlDataReader read = cmd2.ExecuteReader())
            {
                while (read.Read())
                {
                    number = read["controlNumber"].ToString();
                }
            }
            con.Close();
            return number;
        }
    }
}