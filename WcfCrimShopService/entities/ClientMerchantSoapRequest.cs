using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;
using Newtonsoft.Json;
using System.Xml.Linq;

namespace WcfCrimShopService.entities
{
    public class ClientMerchantSoapRequest
    {
        Objects.ConfigObject config = JsonConvert.DeserializeObject<Objects.ConfigObject>(File.ReadAllText(System.AppDomain.CurrentDomain.BaseDirectory + @"Config.json"));
        public string SoapRequest(string total, string tax, string desc, string clientName, string clientEmail, string controlNumber)
        {
            string Username = config.MerchantInfo.user;
            string Password = config.MerchantInfo.pass;
            string CustomerName = clientName; //base de dato
            string CustomerID = controlNumber; //numero de control?
            string CustomerEmail = clientEmail;//base de datos
            string Total = total; //base de datos
            string DescriptionBuy = desc; // base de datos
            string TaxAmount1 = tax; //base de datos
            string address1 = "";
            string address2 = "";
            string city = "";
            string zipcode = "";
            string telephone = "";
            string fax = "";
            string ignoreValues = "";
            string language = "es";
            string TaxAmount2 = "";
            string TaxAmount3 = "";
            string TaxAmount4 = "";
            string TaxAmount5 = controlNumber;
            string filler1 = "";
            string filler2 = "";
            string filler3 = "";

            var url = config.MerchantInfo.serviceUrl; //?op=MakePayment  "https://mmpay.evertecinc.com/webservicev2/wscheckoutpayment.asmx"
            var action = config.MerchantInfo.serviceAction; // "https://mmpay.evertecinc.com/webservicev2/wscheckoutpayment.asmx?op=MakePayment"


            Dictionary<string, string> dc = new Dictionary<string, string>();
            dc.Add("Username", Username);
            dc.Add("Password", Password);
            dc.Add("CustomerName", CustomerName);
            dc.Add("CustomerID", CustomerID);
            dc.Add("CustomerEmail", CustomerEmail);
            dc.Add("Total",Total);
            dc.Add("DescriptionBuy",DescriptionBuy);
            dc.Add("TaxAmount1",TaxAmount1);
            dc.Add("address1","");
            dc.Add("address2","");
            dc.Add("city","");
            dc.Add("zipcode","");
            dc.Add("telephone","");
            dc.Add("fax","");
            dc.Add("ignoreValues","");
            dc.Add("language","es");
            dc.Add("TaxAmount2","");
            dc.Add("TaxAmount3","");
            dc.Add("TaxAmount4","");
            dc.Add("TaxAmount5",controlNumber);
            dc.Add("filler1","");
            dc.Add("filler2","");
            dc.Add("filler3","");

            string soapResult = HttpPostRequest(dc);
            //XmlDocument soapEnvelope = CreateSoapEnvelope(Username, Password, CustomerName, CustomerID,
            // CustomerEmail, Total, DescriptionBuy, TaxAmount1,
            // address1, address2, city, zipcode,
            // telephone, fax, ignoreValues, language,
            // TaxAmount2, TaxAmount3, TaxAmount4, TaxAmount5,
            // filler1, filler2, filler3);

            //HttpWebRequest webRequest = CreateWebRequest(url, action);

            //using (Stream stream = webRequest.GetRequestStream())
            //{
            //    soapEnvelope.Save(stream);
            //}

            //IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);

            ////suspend this thread until  call is complete
            //asyncResult.AsyncWaitHandle.WaitOne();

            ////get the response from the completed web request
            //string soapResult;
            ////using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult))
            //using (WebResponse webResponse = webRequest.GetResponse())
            //{
            //    using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
            //    {
            //        soapResult = rd.ReadToEnd();
            //    }
            //    Debug.WriteLine(soapResult);

            //}
            return soapResult;
        }

        private string HttpPostRequest(Dictionary<string, string> postParams)
        {
            string pageContent = string.Empty;
            string postData = string.Empty;//"Username=CERT4549444000001&Password=W04085j2&CustomerName=test&CustomerID=130520160005&CustomerEmail=hmelendez@gmtgis.com&Total=66.90&DescriptionBuy=controlNumber&TaxAmount1=6.90&address1=&address2=&city=&zipcode=&telephone=&fax=&ignoreValues=&language=es&TaxAmount2=&TaxAmount3=&TaxAmount4=&TaxAmount5=130520160005&filler1=&filler2=&filler3=&";
            foreach (string key in postParams.Keys)
            {
                postData += HttpUtility.UrlEncode(key) + "=" + HttpUtility.UrlEncode(postParams[key]) + "&";
            }

            HttpWebRequest myHttpWebrequest = (HttpWebRequest)System.Net.WebRequest.Create(config.MerchantInfo.serviceAction);
            myHttpWebrequest.Method = "POST";
            //WebProxy myproxy = new WebProxy("", true);
            //myHttpWebrequest.Proxy = myproxy;
            byte[] data = Encoding.ASCII.GetBytes(postData);

            myHttpWebrequest.ContentType = "application/x-www-form-urlencoded";
            myHttpWebrequest.ContentLength = data.Length;
            myHttpWebrequest.AllowAutoRedirect = true;
            myHttpWebrequest.Timeout = 1000 * 30;
            myHttpWebrequest.PreAuthenticate = true;
            myHttpWebrequest.Credentials = CredentialCache.DefaultCredentials;
            

            try
            {
                Stream requestStream = myHttpWebrequest.GetRequestStream();
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();


                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebrequest.GetResponse();
            
                Stream responseStream = myHttpWebResponse.GetResponseStream();

            //WebResponse myWebResponse = myHttpWebrequest.GetResponse();


                StreamReader myStreamReader = new StreamReader(responseStream, Encoding.Default);

                pageContent = myStreamReader.ReadToEnd();
                XElement parserXML = XElement.Parse(pageContent);

                if (!parserXML.IsEmpty)
                {
                    pageContent = parserXML.Value;
                }

                myStreamReader.Close();
                responseStream.Close();
                myHttpWebResponse.Close();
 
            }
            catch (WebException e)
            {
                Console.WriteLine("This program is expected to throw WebException on successful run." +
                                    "\n\nException Message :" + e.Message);
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    Console.WriteLine("Status Code : {0}", ((HttpWebResponse)e.Response).StatusCode);
                    Console.WriteLine("Status Description : {0}", ((HttpWebResponse)e.Response).StatusDescription);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }



            return pageContent;
        }


        private static HttpWebRequest CreateWebRequest(string url, string action)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Headers.Add("SOAPAction", action);
            webRequest.ContentType = "text/xml; charset=utf-8";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            webRequest.AllowAutoRedirect = true;
            webRequest.Timeout = 1000 * 30;
            webRequest.PreAuthenticate = true;
            webRequest.Credentials = CredentialCache.DefaultCredentials;

            return webRequest;
        }

        private static XmlDocument CreateSoapEnvelope(string Username, string Password, string CustomerName, string CustomerID,
            string CustomerEmail, string Total, string DescriptionBuy, string TaxAmount1,
            string address1, string address2, string city, string zipcode, 
            string telephone, string fax, string ignoreValues, string language,
            string TaxAmount2, string TaxAmount3, string TaxAmount4, string TaxAmount5,
            string filler1, string filler2, string filler3)
        {

            //string getValue = web.MakePayment(Username, Password, CustomerName, CustomerID, CustomerEmail, Total, DescriptionBuy, TaxAmount1, address1, address2, city, zipcode, telephone, fax, ignoreValues, language, TaxAmount2, TaxAmount3, TaxAmount4, TaxAmount5, filler1, filler2, filler3);

            string xmlEnvelope = @"<?xml version=""1.0"" encoding=""utf-8""?>";
            xmlEnvelope += @"<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""";
            xmlEnvelope += @" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""";
            xmlEnvelope += @" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">";
            xmlEnvelope += @"<soap:Body>";
            xmlEnvelope += @"<MakePayment xmlns=""http://tempuri.org/WebMerchant/MerchantService"">";
            xmlEnvelope += @"<Username>" + Username + @"</Username>";
            xmlEnvelope += @"<Password>" + Password + @"</Password>";
            xmlEnvelope += @"<CustomerName>" + CustomerName + @"</CustomerName>";
            xmlEnvelope += @"<CustomerID>" + CustomerID + @"</CustomerID>";
            xmlEnvelope += @"<CustomerEmail>" + CustomerEmail + @"</CustomerEmail>";
            xmlEnvelope += @"<Total>" + Total + @"</Total>";
            xmlEnvelope += @"<DescriptionBuy>" + DescriptionBuy + @"</DescriptionBuy>";
            xmlEnvelope += @"<TaxAmount1>" + TaxAmount1 + @"</TaxAmount1>";
            xmlEnvelope += @"<address1>" + address1 + @"</address1>";
            xmlEnvelope += @"<address2>" + address2 + @"</address2>";
            xmlEnvelope += @"<city>" + city + @"</city>";
            xmlEnvelope += @"<zipcode>" + zipcode + @"</zipcode>";
            xmlEnvelope += @"<telephone>" + telephone + @"</telephone>";
            xmlEnvelope += @"<fax>" + fax + @"</fax>";
            xmlEnvelope += @"<ignoreValues>" + ignoreValues + @"</ignoreValues>";
            xmlEnvelope += @"<language>" + language + @"</language>";
            xmlEnvelope += @"<TaxAmount2>" + TaxAmount2 + @"</TaxAmount2>";
            xmlEnvelope += @"<TaxAmount3>" + TaxAmount3 + @"</TaxAmount3>";
            xmlEnvelope += @"<TaxAmount4>" + TaxAmount4 + @"</TaxAmount4>";
            xmlEnvelope += @"<TaxAmount5>" + TaxAmount5 + @"</TaxAmount5>";
            xmlEnvelope += @"<filler1>" + filler1 + @"</filler1>";
            xmlEnvelope += @"<filler2>" + filler2 + @"</filler2>";
            xmlEnvelope += @"<filler3>" + filler3 + @"</filler3>";
            xmlEnvelope += @"</MakePayment>";
            xmlEnvelope += @"</soap:Body>";
            xmlEnvelope += @"</soap:Envelope>";

            XmlDocument envelop = new XmlDocument();
            envelop.LoadXml(xmlEnvelope);

            return envelop;

        }
    }
}