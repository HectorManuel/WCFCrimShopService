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
using System.Diagnostics;

using System.Net;
using System.IO;
using System.Xml;

using WcfCrimShopService.entities;
using WcfCrimShopService.com.evertecinc.mmpay;
using System.Threading.Tasks;
using System.Threading;
using System.Timers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web.Script.Serialization;
using System.DirectoryServices.AccountManagement;
using System.Text.RegularExpressions;


namespace WcfCrimShopService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    //NOTE: two endpoits cannot have the same address name

    //NOTE: Service endpoints are written on top of each function
    //this sets the compatibility mode for asp.net
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class CartographicProductsService : ICartographicProductsService
    {
        DBConnection responseHandler = new DBConnection();
        Objects.ConfigObject config = JsonConvert.DeserializeObject<Objects.ConfigObject>(File.ReadAllText(System.AppDomain.CurrentDomain.BaseDirectory + @"Config.json"));

        /// <summary>
        /// Service meant to test the GET url this function can and might be overwritten 
        /// wiht a functionallity that we might need.
        /// 
        /// cartographicProductsService.svc/WebGet/{value}
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string GetData(string value)
        {

            SqlConnection con = new SqlConnection(@"Data Source=" + config.ServerConnection.source + ";Initial Catalog=" + config.ServerConnection.catalog + ";User ID=" + config.ServerConnection.id + ";Password=" + config.ServerConnection.password + ";");
            //SqlConnection con = new SqlConnection(@"Data Source=HECTOR_CUSTOMS\MYOWNSQLSERVER;Initial Catalog=CRIMShopManagement;Trusted_Connection=Yes;");
            WebClient wb = new WebClient();
            con.Open();
            string query = "SELECT ControlNumber, Confirmation, Description " +
                           "FROM dbo.Orders " +
                           "WHERE ControlNumber= @control";
            
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@control", value);
            string ds = string.Empty;
            //var list = new List<Objects.Order>();
            //var result = cmd.ExecuteNonQuery();
            using (SqlDataReader result = cmd.ExecuteReader())
            {
                while (result.Read())
                {
                    ds = result["Confirmation"].ToString();
                    
                }
                
            }
            return ds;


        }

        /// <summary>
        /// This functionis in charge of running the Sp to generate a 
        /// Control number to assign to the order
        /// </summary>
        /// <returns>control Number</returns>
        public string GetControlNumber()
        {
            string controlNumber = responseHandler.GetControlNumberHandler();
            return controlNumber;
        }
        
        /// <summary>
        /// GET function in charge of waiting for a change in the confirmation column of the order.
        /// to wait for it to change from 'Processing' to tell the widget that the order was processed or cancelled
        /// or an error occured based on values the call will evaluate.
        /// 
        /// cartographicProductsService.svc/AwaitConfirmation/{order}
        /// 
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public string AwaitConfirmation(string order)
        {
            string ds = responseHandler.AwaitForResponse(order);
            return ds;
        }

        /// <summary>
        ///  InsertORderDetails is one of the main functions of te code, this funciton is in charge os creating the order item
        ///  and storing it in the database with all the information required.
        ///  
        /// cartographicProductsService.svc/InsertOrderDetails
        /// </summary>
        /// <param name="ControlNumber"></param>
        /// <param name="Description"></param>
        /// <param name="tx"></param>
        /// <param name="sTotal"></param>
        /// <param name="Total"></param>
        /// <param name="CustomerName"></param>
        /// <param name="customerEmail"></param>
        /// <param name="hasPhoto"></param>
        /// <param name="hasCat"></param>
        /// <param name="hasList"></param>
        /// <param name="hasExtract"></param>
        /// <returns></returns>
        public string InsertOrderDetails(string ControlNumber, string Description, decimal tx,decimal sTotal, decimal Total, string CustomerName, string customerEmail, string hasPhoto, string hasCat, string hasList, string hasExtract)
        {
            //DBConnection responseHandler = new DBConnection();

            var result = responseHandler.InsertOrderDetailsHandler(ControlNumber, Description, tx, sTotal, Total, CustomerName, customerEmail, hasPhoto,hasCat,hasList, hasExtract);

            return result;
            #region commented Code
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
            #endregion
        }
        
        /// <summary>
        /// this service is in charge of retrieveing the response string from the online response
        /// POST from Evertec and process it. extracting all the information of the order and processing every product
        /// if the order payment was succesful. this service is for the use of evertec response
        /// 
        /// cartographicProductsService.svc/PaymentResponse
        /// 
        /// </summary>
        /// <param name="PaymentResponse"></param>
        /// <returns></returns>
        public string PaymentResponse(Stream PaymentResponse)
        {
            // ********* PARTE DE PRUEBA DE STREAM
            DBConnection con = new DBConnection();
            
            StreamReader streamReader = new StreamReader(PaymentResponse);

            string rawString = streamReader.ReadToEnd();
            streamReader.Dispose();
            //DBConnection responseHandler = new DBConnection();
            con.LogTransaction("Response", rawString);
            string result = string.Empty;
            if (!string.IsNullOrEmpty(rawString))
            {
                result = responseHandler.PaymentResponseLogHandler(rawString).Result;
            }
            else
            {
                result = "PaymentResponse null or empty";
            }

            return result;
        }
        //VTransactionId=IdTrans12345&VAccountId=CustomerAccount12345&VTotalAmount=10.52&VPaymentMethod=V&VPaymentDescription=InternetPayment&VAuthorizationNum=AutNum12345&VConfirmationNum=ConfNum12345
        //https://msdn.microsoft.com/en-us/library/cc656724(v=vs.110).aspx
        //http://www.codeproject.com/Articles/35982/REST-WCF-and-Streams-Getting-Rid-of-those-Names-Sp
        //
        /// <summary>
        /// this service is in charge of retrieveing the response string from the online response
        /// POST from Evertec and process it. extracting all the information of the order and processing every product
        /// if the order payment was succesful. this service is for the use of evertec response
        /// 
        /// cartographicProductsService.svc/PaymentResponse
        /// 
        /// </summary>
        /// <param name="PaymentResponse"></param>
        /// <returns></returns>
        public string PaymentResponse2(Stream PaymentResponse)
        {
            //DBConnection responseHandler = new DBConnection();
            DBConnection con = new DBConnection();
            StreamReader streamReader = new StreamReader(PaymentResponse);

            string rawString = streamReader.ReadToEnd();
            streamReader.Dispose();
            con.LogTransaction("Response", rawString);

            NameValueCollection queryString = HttpUtility.ParseQueryString(rawString);

            string confirm = queryString["VConfirmationNum"];

            return confirm +"    " + rawString;
            //if (!string.IsNullOrEmpty(PaymentResponse))
            //{
            //    result = responseHandler.PaymentResponseLogHandler(PaymentResponse).Result;
            //}
            //else
            //{
            //    result = "PaymentResponse null or empty";
            //}


            //return result;
        }




        //    return result;
        //}

        /// <summary>
        /// StarGeorpocess if a function build to test the geoprocesses to observe the way they work.
        /// This function is later meant to work as the function to create the pdf archives and send them to the employee
        /// wihtout having to go through paying.
        /// 
        /// cartographicProductsService.svc/StarGeoprocess
        /// 
        /// </summary>
        /// <param name="cNumber"></param>
        /// <returns></returns>
        public string StarGeoprocess(string cNumber)
        {
            string response = string.Empty;

            if (!string.IsNullOrEmpty(cNumber))
            {
                response = responseHandler.PaymentResponseLogHandlerEmployee(cNumber).Result;
                if (response == "ok")
                {
                    responseHandler.LogTransaction(cNumber, "Order Submitted");
                }
                
            }
            else
            {
                response = "no control number";
            }
           

            return response;
        }

        /// <summary>
        /// Insert the data for the Aerial Photo item of the order. This information is later use
        /// to generate the Cadastral pdf after the order has been authorize.
        /// 
        /// cartographicProductsService.svc/InsertAerialPhotoItem
        /// 
        /// </summary>
        /// <param name="controlNumber"></param>
        /// <param name="itemQty"></param>
        /// <param name="item"></param>
        /// <param name="format"></param>
        /// <param name="layoutTemplate"></param>
        /// <param name="georefInfo"></param>
        /// <param name="parcel"></param>
        /// <param name="subtitle"></param>
        /// <param name="buffer"></param>
        /// <param name="parcelList"></param>
        /// <param name="bufferDistance"></param>
        /// <returns></returns>
        public string InsertAerialPhotoItem(string title, string controlNumber, int itemQty, string item, string format, string layoutTemplate, string georefInfo, string parcel, string subtitle, string buffer, string parcelList, string bufferDistance)
        {
            //DBConnection AerialHandler = new DBConnection();
            var result = responseHandler.InsertAerialPhotoHandler(title, controlNumber, itemQty, item, format, layoutTemplate, georefInfo, parcel, subtitle, buffer, parcelList, bufferDistance);
            return result;
        }

        /// <summary>
        /// Insert the data for the Adjacent parcel list. the data will later be use to generate
        /// the pdf of the list to send to the user.
        /// 
        /// cartographicProductsService.svc/InsertListaColindanteItem
        /// </summary>
        /// <param name="controlNumber"></param>
        /// <param name="itemName"></param>
        /// <param name="itemQty"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public string InsertListaColindanteItem(string controlNumber, string itemName, int itemQty, string item)
        {
            //DBConnection lista = new DBConnection();
            var result = responseHandler.InsertListaColindanteItemHanlder(controlNumber, itemName, itemQty, item);
            return result;
        }

        /// <summary>
        ///         /// Insert the data for the cadastral item of the order. This information is later use
        /// to generate the Cadastral pdf after the order has been authorize.
        /// 
        /// cartographicProductsService.svc/InsertCatastralItem
        /// </summary>
        /// <param name="controlNumber"></param>
        /// <param name="itemQty"></param>
        /// <param name="cuadricula1"></param>
        /// <param name="cuadricula10"></param>
        /// <returns></returns>
        public string InsertCatastralItem(string controlNumber, int itemQty, string cuadricula1, string cuadricula10)
        {
            //DBConnection catastro = new DBConnection();
            var result = responseHandler.InsertCatastralesHandler(controlNumber, itemQty, cuadricula1, cuadricula10);
            return result;
        }

        /// <summary>
        /// cartographicProductsService.svc/MakePayment
        /// this service is meant to send the request to evertec for the payment.
        /// </summary>
        /// <param name="controlNumber"></param>
        /// <returns></returns>
        public string MakePayment(string controlNumber)
        {
            string subtotal = responseHandler.PriceSubTotal(controlNumber);
            string TotalCost = responseHandler.UpdateCost(subtotal, controlNumber);
            decimal tax = responseHandler.GetTax();

            decimal st = Convert.ToDecimal(subtotal);
            string taxAmount = (System.Math.Round(st * (tax/100), 2)).ToString();

            List<Objects.FullOrderInfo> OrderInfo = responseHandler.OrderInformation(controlNumber);
            string description = OrderInfo[0].Description;
            string clientName = OrderInfo[0].CustomerName;
            string email = OrderInfo[0].CustomerEmail;

            var handler = new ClientMerchantSoapRequest();

            string soap = handler.SoapRequest(TotalCost, taxAmount, description, clientName, email, controlNumber);
            //MerchantService web = new MerchantService();
            return soap;
        }

        /// <summary>
        /// Test service for the List of adyacent parcels of the selection
        /// </summary>
        /// <param name="json"></param>
        /// <param name="cNumber"></param>
        /// <param name="customer"></param>
        /// <param name="parcela"></param>
        /// <returns></returns>
        public string CreatePdfListaColindante(string json, string cNumber, string customer, string parcela)
        {
            if (string.IsNullOrEmpty(cNumber))
            {
                cNumber = "No Asignado";
            }
            if (string.IsNullOrEmpty(customer))
            {
                customer = "N/A";
            }
            if (string.IsNullOrEmpty(parcela))
            {
                parcela = "N_A";
            }
            Geoprocessing geo = new Geoprocessing();
            //geo.AdyacentListGenerator(json, "041120160002", "GMT"); System.IO.Directory.GetCurrentDirectory()
            string zipPath = System.AppDomain.CurrentDomain.BaseDirectory + @"OrderFolder\";


            string csvPath = Path.Combine(zipPath, parcela + "_colindante.csv");
            if (!File.Exists(csvPath))
            {
                File.Create(csvPath).Dispose();
            }
            else
            {
                File.Delete(csvPath);
            }

            StringBuilder csvContent = new StringBuilder();
            
            Objects.ListaCol lisCol = JsonConvert.DeserializeObject<Objects.ListaCol>(json);
            using (Document doc = new Document(new RectangleReadOnly(1191, 842), 25, 25, 45, 35))//A3 (842,1191) nearest to 11x17, A4 (595,842) nearest to 8.5x11
            {
                PdfWriter wr = PdfWriter.GetInstance(doc, new FileStream(zipPath + parcela+ @"_colindantes.pdf", FileMode.Create));

                ColindantePdfEventHandler e = new ColindantePdfEventHandler()
                {
                    cantidad = lisCol.ListaColindante.Count.ToString(),
                    controlNumber = cNumber,
                    contribuyente = customer,
                    Parcela = parcela
                };

                wr.PageEvent = e;

                doc.Open();



                //completar eso, el json y el prionting del pdf
                PdfPTable table = new PdfPTable(7);
                table.WidthPercentage = 100;
                float[] widths = { 12.00F, 8.00F, 10.00F, 10.00F, 12.00F, 20.00F, 20.00F };
                //table.SetWidthPercentage(widths,new RectangleReadOnly(1191, 842));
                table.SetTotalWidth(widths);
                PdfPCell cell = new PdfPCell(new Phrase("Parcela de Procedencia", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                cell.Colspan = 1;
                cell.HorizontalAlignment = 0;//0=left 1=center 2=right
                PdfPCell cell1 = new PdfPCell(new Phrase("Parcela", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                cell1.Colspan = 1;
                cell1.HorizontalAlignment = 0;//0=left 1=center 2=right
                PdfPCell cell2 = new PdfPCell(new Phrase("Catastro", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                cell2.Colspan = 1;
                cell2.HorizontalAlignment = 0;//0=left 1=center 2=right
                PdfPCell cell3 = new PdfPCell(new Phrase("Municipio", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                cell3.Colspan = 1;
                cell3.HorizontalAlignment = 0;//0=left 1=center 2=right
                PdfPCell cell4 = new PdfPCell(new Phrase("Dueño", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                cell4.Colspan = 1;
                cell4.HorizontalAlignment = 0;//0=left 1=center 2=right
                PdfPCell cell5 = new PdfPCell(new Phrase("Dirección Física", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                cell5.Colspan = 1;
                cell5.HorizontalAlignment = 0;//0=left 1=center 2=right
                PdfPCell cell6 = new PdfPCell(new Phrase("Dirección Postal", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                cell6.Colspan = 1;
                cell6.HorizontalAlignment = 0;//0=left 1=center 2=right

                table.AddCell(cell);
                table.AddCell(cell1);
                table.AddCell(cell2);
                table.AddCell(cell3);
                table.AddCell(cell4);
                table.AddCell(cell5);
                table.AddCell(cell6);

                string headers = "Parcela de Procedencia" + "," + "Parcela" + "," + "Catastro" + "," + "Municipio" + "," + "Dueño" + "," + "Dirección Física" + "," + "Dirección Postal";
                csvContent.AppendLine(headers);

                foreach (var item in lisCol.ListaColindante)
                {
                    table.AddCell(item.ParcelaProcedencia);
                    table.AddCell(item.Parcela);
                    table.AddCell(item.Catastro);
                    table.AddCell(item.Municipio);
                    table.AddCell(item.Dueno);
                    table.AddCell(item.DireccionFisica);
                    table.AddCell(item.DireccionPostal);

                    string row = item.ParcelaProcedencia + "," + item.Parcela + "," + item.Catastro + "," + item.Municipio + "," + item.Dueno + "," + item.DireccionFisica + "," + item.DireccionPostal;
                    csvContent.AppendLine(row);

                }


                doc.Add(table);

                using (StreamWriter file = new StreamWriter(new FileStream(csvPath, FileMode.Create),Encoding.UTF8))
                {
                    file.Write(csvContent.ToString());
                }
                

               
               
                //JObject obj = JObject.Parse(json);

                doc.Close();
            }
            return "complete";
        }

        /// <summary>
        ///  this service is in charge os authentication the user with the active directory
        ///  once the username and password are matched , it will evaluate the user to the required group
        ///  or groups.
        /// </summary>
        /// <param name="username"> username</param>
        /// <param name="password"> password </param>
        /// <returns>confirmation</returns>
        public string Authentication(string username, string password)
        {
            AuthenticationClass auth = new AuthenticationClass();
            string result = auth.IsAuthenticated(username, password);
            return result;
        }

        /// <summary>
        /// This function is in charge of getting the price per item from the DB and 
        /// calculate the price for the interface and later to save in the db
        /// </summary>
        /// <param name="item"></param>
        /// <param name="qty"></param>
        /// <returns></returns>
        public string GetItemPrice(string item, int qty)
        {
            
            string total = string.Empty;

            //Objects.ConfigObject config = JsonConvert.DeserializeObject<Objects.ConfigObject>(File.ReadAllText( System.AppDomain.CurrentDomain.BaseDirectory+ @"Config.json"));
            Geoprocessing gp = new Geoprocessing();
            total = gp.CalculatePrice(item, qty);
            
            return total;
        }

        /// <summary>
        /// Thi small function is used to get the Tax value
        /// </summary>
        /// <returns>tax value</returns>
        public decimal GetTax()
        {
            decimal tax = responseHandler.GetTax();
            return tax;
        }

        /// <summary>
        /// insert the data for the Extract Data Items
        /// </summary>
        /// <param name="controlNumber"></param>
        /// <param name="qty"></param>
        /// <param name="layer"></param>
        /// <param name="areaa"></param>
        /// <param name="format"></param>
        /// <param name="raster"></param>
        /// <returns></returns>
        public string InsertExtractDataService(string controlNumber, int qty, string layer, string areaa, string format, string raster)
        {
            string response = string.Empty;
            if (raster != "ESRI GRID - GRID")
            {
                raster = "ESRI GRID - GRID";
            }
            if (controlNumber != null && qty != 0 && layer != null && areaa != null && format != null)
            {
                response = responseHandler.InsertExtractDataHandler(controlNumber, qty, layer, areaa, format, raster);
            }
            else{
                response = "Missing parameter";
            }
            
            return response;
        }

        /// <summary>
        /// process the failed List products
        /// </summary>
        /// <param name="control"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        public string GenerateList(string control, string customer)
        {
            string file = responseHandler.ProcessFailedListProducts(control, customer);
            return file;
        }

        /// <summary>
        /// Get the list of prices per unit 
        /// </summary>
        /// <returns></returns>
        public string GetPriceList()
        {
            string jsonList = responseHandler.GetPriceList();

            return jsonList;
        }

        public string test(string cn)
        {
            string test = "'087-001','087-002','087-003','087-004','087-005','087-006','087-007','087-008','087-009','087-010','087-011','087-012','087-013','087-014','087-015','087-016','087-017','087-018','087-019','087-020','087-021','087-022','087-023','087-024','087-025','087-026','087-027','087-028','087-029','087-030','087-031','087-032','087-033','087-034','087-035','087-036','087-037','087-038','087-039','087-040','087-041','087-042','087-043','087-044','087-045','087-046','087-047','087-048','087-049','087-050'";
           //string result = responseHandler.CheckForFailedItems(cn, "test").Result;
            responseHandler.UpdateFailedCad(cn, test, "false");
            //string result = responseHandler.GetExtractDataItem(cn);
            return test;
        }

        /// <summary>
        /// obtiene el mac adrress, Physical IP, public Ip
        /// </summary>
        /// <returns></returns>
        public string GetIP()
        {
            string ip = HttpContext.Current.Request.UserHostAddress;
            string name = HttpContext.Current.Request.UserHostName;
            //string nameIp = ip + "  " + name;
            string nameIp = string.Empty;
            IPHostEntry heserver = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var id in heserver.AddressList)
            {
                nameIp += id.ToString() + " , ";
            }
            try
            {
                string externalIP;
                externalIP = (new WebClient()).DownloadString("http://checkip.dyndns.org/");
                externalIP = (new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")).Matches(externalIP)[0].ToString();
                nameIp += externalIP;
            }
            catch
            {

            }
            return nameIp;
        }
    }
}
