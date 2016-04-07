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
                    ds = result["ControlNumber"].ToString();
                    
                    //list.Add(new Objects.Order{ControlNumber= cn, Confirmation= confirm, Description = desc});
                }
            }

            //string ds = string.Empty;
            //test array object
            //var list = new List<KeyValuePair<string, string>>();
            //wb.DownloadFile("http://localhost:9001/test1234.zip", @"C:\Users\hasencio\Documents\MyProjects\Store\WebApp\pdfArchives\testing.zip");
            return ds;


        }

        //cartographicProductsService.svc/InsertOrderDetails
        public string InsertOrderDetails(string ControlNumber, string Description, decimal tx,decimal sTotal, decimal Total, string CustomerName, string customerEmail, string hasPhoto, string hasCat, string hasList)
        {
            DBConnection responseHandler = new DBConnection();

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

        //cartographicProductsService.svc/InsertClientDetails
        public string InsertClientDetails(string name, string email, string address, string city, string zip, string tel, string fax)
        {
            DBConnection clientHandler = new DBConnection();

            var result = clientHandler.InsertClientDetailsHandler(name, email, address, city, zip, tel, fax);
            return result;
        }
        
        //cartographicProductsService.svc/PaymentResponse
        public string PaymentResponse(string PaymentResponse)
        {
            DBConnection responseHandler = new DBConnection();

            var result = responseHandler.PaymentResponseLogHandler(PaymentResponse);

            return result;
        }

        //cartographicProductsService.svc/StarGeoprocess string jsonMap, string cNumber, string format, string template, string geoInfo, string parcelTitle, string sub_Title, string bf, string pr, string bf_distance_unit, string hasCat
        public string StarGeoprocess(string jsonMap, string cNumber, string format, string template, string geoInfo, string parcelTitle, string sub_Title, string bf, string pr, string bf_distance_unit, string hasCat, string email)
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

        //cartographicProductsService.svc/InsertAerialPhotoItem
        public string InsertAerialPhotoItem(string controlNumber, int itemQty, string item, string format, string layoutTemplate, string georefInfo, string parcel, string subtitle, string buffer, string parcelList, string bufferDistance)
        {
            DBConnection AerialHandler = new DBConnection();
            var result = AerialHandler.InsertAerialPhotoHandler(controlNumber, itemQty, item, format, layoutTemplate, georefInfo, parcel, subtitle, buffer, parcelList, bufferDistance);
            return result;
        }

        //cartographicProductsService.svc/InsertListaColindanteItem
        public string InsertListaColindanteItem(string controlNumber, string itemName, int itemQty, string item)
        {
            DBConnection lista = new DBConnection();
            var result = lista.InsertListaColindanteItemHanlder(controlNumber, itemName, itemQty, item);
            return result;
        }

        //cartographicProductsService.svc/InsertCatastralItem
        public string InsertCatastralItem(string controlNumber, string itemName, int itemQty, string escala, string cuadricula, string template)
        {
            DBConnection catastro = new DBConnection();
            var result = catastro.InsertCatastralesHandler(controlNumber, itemName, itemQty, escala, cuadricula, template);
            return result;
        }

        //cartographicProductsService.svc/MakePayment
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
    }

}
