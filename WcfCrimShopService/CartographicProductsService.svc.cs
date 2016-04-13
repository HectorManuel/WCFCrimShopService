﻿using System;
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
            
            SqlConnection con = new SqlConnection(@"Data Source=GMTWKS13\GMTWKS13DB;Initial Catalog=CRIMShopManagement;User ID=User;Password=user123;");
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
                    
                    //list.Add(new Objects.Order{ControlNumber= cn, Confirmation= confirm, Description = desc});
                }
                
            }

            //string ds = string.Empty;
            //test array object
            //var list = new List<KeyValuePair<string, string>>();
            //wb.DownloadFile("http://localhost:9001/test1234.zip", @"C:\Users\hasencio\Documents\MyProjects\Store\WebApp\pdfArchives\testing.zip");
            return ds;


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
            SqlConnection con = new SqlConnection(@"Data Source=GMTWKS13\GMTWKS13DB;Initial Catalog=CRIMShopManagement;User ID=User;Password=user123;");
            //SqlConnection con = new SqlConnection(@"Data Source=HECTOR_CUSTOMS\MYOWNSQLSERVER;Initial Catalog=CRIMShopManagement;Trusted_Connection=Yes;");
            WebClient wb = new WebClient();
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
                    Debug.WriteLine(time +" "+ ds);

                }
            } while (ds == "Processing" && time != 20);

            if (ds == "Processing")
            {
                ds = "error completing the payment";
            }

            return ds;
        }


        /// <summary>
        ///  InsertORderDetails is one of the main functions of te code, this funciton is in charge os creating the order item
        ///  and storing it in the database with all the information required.
        ///  
        /// cartographicProductsService.svc/InsertOrderDetails
        /// 
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
        /// <returns></returns>
        public string InsertOrderDetails(string ControlNumber, string Description, decimal tx,decimal sTotal, decimal Total, string CustomerName, string customerEmail, string hasPhoto, string hasCat, string hasList)
        {
            //DBConnection responseHandler = new DBConnection();

            var result = responseHandler.InsertOrderDetailsHandler(ControlNumber, Description, tx, sTotal, Total, CustomerName, customerEmail, hasPhoto,hasCat,hasList);

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
        /// this function is no longer in use
        /// 
        /// cartographicProductsService.svc/InsertClientDetails
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="address"></param>
        /// <param name="city"></param>
        /// <param name="zip"></param>
        /// <param name="tel"></param>
        /// <param name="fax"></param>
        /// <returns></returns>
        public string InsertClientDetails(string name, string email, string address, string city, string zip, string tel, string fax)
        {
            //DBConnection clientHandler = new DBConnection();

            var result = responseHandler.InsertClientDetailsHandler(name, email, address, city, zip, tel, fax);
            return result;
        }
        
        /// <summary>
        /// this service is in charge of retrieveing the response string from the online response
        /// POST from Evertec and process it. extracting all the information of the order and processing every product
        /// if the order payment was succesful
        /// 
        /// cartographicProductsService.svc/PaymentResponse
        /// 
        /// </summary>
        /// <param name="PaymentResponse"></param>
        /// <returns></returns>
        public string PaymentResponse(string PaymentResponse)
        {
            //DBConnection responseHandler = new DBConnection();

            var result = responseHandler.PaymentResponseLogHandler(PaymentResponse);

            return result;
        }

        
        /// <summary>
        /// StarGeorpocess if a function build to test the geoprocesses to observe the way they work.
        /// This function is later meant to work as the function to create the pdf archives and send them to the employee
        /// wihtout having to go through paying.
        /// 
        /// cartographicProductsService.svc/StarGeoprocess
        /// 
        /// </summary>
        /// <param name="jsonMap"></param>
        /// <param name="cNumber"></param>
        /// <param name="format"></param>
        /// <param name="template"></param>
        /// <param name="geoInfo"></param>
        /// <param name="parcelTitle"></param>
        /// <param name="sub_Title"></param>
        /// <param name="bf"></param>
        /// <param name="pr"></param>
        /// <param name="bf_distance_unit"></param>
        /// <param name="hasCat"></param>
        /// <param name="hasPhoto"></param>
        /// <param name="hasList"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public string StarGeoprocess(string jsonMap, string cNumber, string format, string template, string geoInfo, string parcelTitle, string sub_Title, string bf, string pr, string bf_distance_unit, string hasCat,  string hasPhoto, string hasList, string email)
        {

            Geoprocessing geo = new Geoprocessing();
            string res = string.Empty;
            string catRes = string.Empty;
            var result = Task.Run(async () =>
            {
                var test = await geo.FotoAerea(jsonMap, cNumber, format, template, geoInfo, parcelTitle, sub_Title, bf, pr, bf_distance_unit);
                res = test.ToString();
            });

            //var result = geo.FotoAerea(jsonMap, cNumber, format, template, geoInfo, parcelTitle, sub_Title);
            result.Wait();

            if (hasCat.ToUpper() != "Y")
            {
                string array = "('400','402')";
                string templated = "MapaCatastral_10k";
                var catastral = Task.Run(async () =>
                {
                    var cat = await geo.OficialMaps(templated, array, geoInfo, cNumber);
                    catRes = cat.ToString();
                });
                catastral.Wait();
            }

            //var catResult = geo.OficialMaps(template, array, geoInfo, cNumber);
            //string catRes = catResult.ToString();
            //string res = result.ToString();

            string zipFilePath = string.Empty;
            //email = "hasencio@gmtgis.com";
            if (res == catRes)
            {
                zipFilePath = geo.ZipAndSendEmail(catRes, email);
            }
            

            return zipFilePath;
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
        public string InsertAerialPhotoItem(string controlNumber, int itemQty, string item, string format, string layoutTemplate, string georefInfo, string parcel, string subtitle, string buffer, string parcelList, string bufferDistance)
        {
            //DBConnection AerialHandler = new DBConnection();
            var result = responseHandler.InsertAerialPhotoHandler(controlNumber, itemQty, item, format, layoutTemplate, georefInfo, parcel, subtitle, buffer, parcelList, bufferDistance);
            return result;
        }

        

        /// <summary>
        /// Insert the data for the Adjacent parcel list. the data will later be use to generate
        /// the pdf of the list to send to the user.
        /// 
        /// cartographicProductsService.svc/InsertListaColindanteItem
        /// 
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
        /// Insert the data for the cadastral item of the order. This information is later use
        /// to generate the Cadastral pdf after the order has been authorize.
        /// 
        /// cartographicProductsService.svc/InsertCatastralItem
        /// 
        /// </summary>
        /// <param name="controlNumber"></param>
        /// <param name="itemName"></param>
        /// <param name="itemQty"></param>
        /// <param name="escala"></param>
        /// <param name="cuadricula"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        public string InsertCatastralItem(string controlNumber, string itemName, int itemQty, string escala, string cuadricula, string template)
        {
            //DBConnection catastro = new DBConnection();
            var result = responseHandler.InsertCatastralesHandler(controlNumber, itemName, itemQty, escala, cuadricula, template);
            return result;
        }

        /// <summary>
        /// cartographicProductsService.svc/MakePayment
        /// this service is menat to send the request to evertec for the payment.
        /// </summary>
        /// <param name="controlNumber"></param>
        /// <returns></returns>
        public string MakePayment(string controlNumber)
        {
            //MerchantService web = new MerchantService();
            string Username ="required"; 
            string Password = "required"; 
            string CustomerName = "required"; 
            string CustomerID = "required";
            string CustomerEmail = "required"; 
            string Total = "1.50";
            string DescriptionBuy = "required"; 
            string TaxAmount1 = "1.00";
            string address1 = "optional";
            string address2 = "optional"; 
            string city = "optional";
            string zipcode = "optional"; 
            string telephone = "Optional"; 
            string fax = "optional";
            string ignoreValues = "optional";
            string language = "es";
            string TaxAmount2 = ""; 
            string TaxAmount3 = ""; 
            string TaxAmount4 = ""; 
            string TaxAmount5 = "merchantTransId"; 
            string filler1 = "";
            string filler2 = "";
            string filler3 = "";
            //string getValue = web.MakePayment(Username, Password, CustomerName, CustomerID, CustomerEmail, Total, DescriptionBuy, TaxAmount1, address1, address2, city, zipcode, telephone, fax, ignoreValues, language, TaxAmount2, TaxAmount3, TaxAmount4, TaxAmount5, filler1, filler2, filler3);

            string xmlEnvelope = @"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
            xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
            xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
            <soap:Body>
            <MakePayment xmlns=""http://tempuri.org/WebMerchant/MerchantService"">
            <Username>" + Username + @"</Username>
            <Password>" + Password + @"</Password>
            <CustomerName>" + CustomerName + @"</CustomerName>
            <CustomerID>" + CustomerID + @"</CustomerID>
            <CustomerEmail>" + CustomerEmail + @"</CustomerEmail>
            <Total>" + Total + @"</Total>
            <DescriptionBuy>" + DescriptionBuy + @"</DescriptionBuy>
            <TaxAmount1>" + TaxAmount1 + @"</TaxAmount1>
            <address1>" + address1 + @"</address1>
            <address2>" + address2 + @"</address2>
            <city>" + city + @"</city>
            <zipcode>" + zipcode + @"</zipcode>
            <telephone>" + telephone + @"</telephone>
            <fax>" + fax + @"</fax>
            <ignoreValues>" + ignoreValues + @"</ignoreValues>
            <language>" + language + @"</language>
            <TaxAmount2>" + TaxAmount2 + @"</TaxAmount2>
            <TaxAmount3>" + TaxAmount3 + @"</TaxAmount3>
            <TaxAmount4>" + TaxAmount4 + @"</TaxAmount4>
            <TaxAmount5>" + TaxAmount5 + @"</TaxAmount5>
            <filler1>" + filler1 + @"</filler1>
            <filler2>" + filler2 + @"</filler2>
            <filler3>" + filler3 + @"</filler3>
            </MakePayment>
            </soap:Body>
            </soap:Envelope>";

            //var url = "https://mmpay.evertecinc.com/webservicev2/wscheckoutpayment.asmx"; //?op=MakePayment
            //var action = "https://mmpay.evertecinc.com/webservicev2/wscheckoutpayment.asmx?op=MakePayment";

            //XmlDocument soapEnvelope = new XmlDocument();
            //soapEnvelope.LoadXml(xmlEnvelope);


            //HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            //webRequest.Headers.Add("SOAPAction", action);
            //webRequest.ContentType = "text/xml; charset=\"utf-8\"";
            //webRequest.Accept = "text/xml";
            //webRequest.Method = "POST";

            //using (Stream stream = webRequest.GetRequestStream())
            //{
            //    soapEnvelope.Save(stream);
            //}

            //IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);

            ////suspend this thread until  call is complete
            //asyncResult.AsyncWaitHandle.WaitOne();

            ////get the response from the completed web request
            //string soapResult;
            //using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult))
            //{
            //    using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
            //    {
            //        soapResult = rd.ReadToEnd();
            //    }
            //    Debug.WriteLine(soapResult);
                
            //}
            return xmlEnvelope;
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
                cNumber = "041120160001";
            }
            if (string.IsNullOrEmpty(customer))
            {
                customer = "Geographic Mapping Technology";
            }
            if (string.IsNullOrEmpty(parcela))
            {
                parcela = "000-000-000-00,111-111-111-11,222-222-222-22";
            }
            Geoprocessing geo = new Geoprocessing();
            //geo.AdyacentListGenerator(json, "041120160002", "GMT"); System.IO.Directory.GetCurrentDirectory()
            string zipPath = System.AppDomain.CurrentDomain.BaseDirectory + @"OrderFolder\";
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
                PdfPCell cell = new PdfPCell(new Phrase("Parcela de procedencia", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
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

                foreach (var item in lisCol.ListaColindante)
                {
                    table.AddCell(item.ParcelaProcedencia);
                    table.AddCell(item.Parcela);
                    table.AddCell(item.Catastro);
                    table.AddCell(item.Municipio);
                    table.AddCell(item.Dueno);
                    table.AddCell(item.DireccionFisica);
                    table.AddCell(item.DireccionPostal);

                }


                doc.Add(table);
                doc.NewPage();

                Paragraph par = new Paragraph("this is my first pdf new line");
                doc.Add(par);
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
        /// <param name="username"> user name</param>
        /// <param name="password"> password </param>
        /// <returns>confirmation</returns>
        public string Authentication(string username, string password)
        {
            AuthenticationClass auth = new AuthenticationClass();
            string result = auth.IsAuthenticated(username, password);
            return result;
        }

        public string GetItemPrice(string item, int qty)
        {
            string total = string.Empty;
            List<Objects.ProductPrice> price = responseHandler.PriceProduct(item, qty);

            switch (item)
            {
                case "Foto Aerea 1998":
                case "Foto Aerea 2004":
                case "Foto Aerea 2007":
                case "Foto Aerea 2010":
                    break;
            }

            return total;
        }
    }

}