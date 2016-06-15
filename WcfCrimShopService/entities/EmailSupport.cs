using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using Newtonsoft.Json;
using System.IO;
using System.Net.Mime;
using WcfCrimShopService.entities;
using System.Diagnostics;


namespace WcfCrimShopService.entities
{
    public class EmailSupport
    {
        Objects.ConfigObject config = JsonConvert.DeserializeObject<Objects.ConfigObject>(File.ReadAllText(System.AppDomain.CurrentDomain.BaseDirectory + @"Config.json"));
        public string EmailGenerator(string controlNumber)
        {
            DBConnection conForLog = new DBConnection();
            string message = string.Empty;
            AddHeaderToBody(controlNumber);
            MailMessage mail = new MailMessage();
            SmtpClient smtpServer = new SmtpClient(config.EmailConfiguration.SMTPClient);
            mail.From = new MailAddress(config.EmailConfiguration.MailAddress);
            mail.To.Add(config.SupportEmail);
            mail.Subject = "Producto Cartografíco no creado";
            mail.BodyEncoding = System.Text.Encoding.UTF8;
            mail.Body = "";

            ContentType mimeType = new ContentType("text/html");

            AlternateView alternate = AlternateView.CreateAlternateViewFromString(EmailBody(), mimeType);
            mail.AlternateViews.Add(alternate);

            smtpServer.Port = config.EmailConfiguration.port;
            smtpServer.Credentials = new System.Net.NetworkCredential(config.EmailConfiguration.Username, config.EmailConfiguration.Password);
            smtpServer.EnableSsl = false;

            try
            {
                smtpServer.Send(mail);
                conForLog.LogTransaction(controlNumber, "Support Email send");
                message = "Support email send";
                Debug.WriteLine("MailSend");
                Objects.bodyHtml = string.Empty;
                //Directory.Delete(orderFolderPath, true);
            }
            catch (Exception e)
            {
                conForLog.LogTransaction(controlNumber, e.Message);
                message = e.Message;
            }

            return message;
        }

        private string EmailBody()
        {
            string body = Objects.bodyHtml;
            return body;
        }

        public void AddHeaderToBody(string controlNumber)
        {
            DBConnection conForLog = new DBConnection();
            List<Objects.FullOrderInfo> orderInfo = new List<Objects.FullOrderInfo>();
            orderInfo = conForLog.OrderInformation(controlNumber);
            string htmlbody = string.Empty;
            if(orderInfo.Count > 0)
            {
                htmlbody = "<div>No se pudieron generar los productos de la siguiente orden:</div>";
                htmlbody += "<div><strong>Orden Número</strong> : "+orderInfo[0].ControlNumber+"</div>";
                htmlbody += "<div><strong>Fecha</strong> : "+ orderInfo[0].OrderDate+ "</div>";
                htmlbody += "<div><strong>Nombre cliente</strong> : "+ orderInfo[0].CustomerName +"</div>";
                htmlbody += "<div><strong>correo electrónico</strong> : "+orderInfo[0].CustomerEmail+"</div>";
                htmlbody += "<div><strong>Total</strong> : "+ orderInfo[0].Total+"</div>";
                htmlbody += "<div><strong>Número de confirmación</strong> : "+orderInfo[0].Confirmation+"</div>";

                Objects.bodyHtml = htmlbody + Objects.bodyHtml;
            }
        }

        public string AddFotoToBody(Objects.OrderItemPhoto item)
        {
            string htmlBody = "<p><strong>Mapas de foto aereas</strong>: "+ item.title +"</p>";
            htmlBody += "<p style=\"padding-left: 30px;\">Web_Map_as_JSON : "+ item.Item +"</p>";
            htmlBody += "<p style=\"padding-left: 30px;\">Format : "+ item.Format +"</p>";
            htmlBody += "<p style=\"padding-left: 30px;\">Layout_Template : "+ item.LayoutTemplate +"</p>";
            htmlBody += "<p style=\"padding-left: 30px;\">Georef_info : "+ item.GeorefInfo +"</p>";
            htmlBody += "<p style=\"padding-left: 30px;\">Title : "+ item.title +"</p>";
            htmlBody += "<p style=\"padding-left: 30px;\">Parcel : "+ item.Parcel +"</p>";
            htmlBody += "<p style=\"padding-left: 30px;\">Subtitle : "+ item.subtitle +"</p>";
            htmlBody += "<p style=\"padding-left: 30px;\">Control : "+ item.ControlNumber +"</p>";
            htmlBody += "<p style=\"padding-left: 30px;\">Buffer : "+item.buffer +"</p>";
            htmlBody += "<p style=\"padding-left: 30px;\">Parcelas : "+ item.parcelList +"</p>";
            htmlBody += "<p style=\"padding-left: 30px;\">Buffer_Distance_Units : "+ item.distance +"</p>";

            Objects.bodyHtml += htmlBody;

            Objects.NotCreated += "<div>" + item.title + " - " + item.Parcel + "</div>";
            return "failed item added to support email: "+ item.title +" "+item.ControlNumber;
        }

        public string AddCadToBody(List<Objects.OrderItemCatastral> cads)
        {
            List<Objects.Scale> listScale10 = new List<Objects.Scale>();
            List<Objects.Scale> listScale1 = new List<Objects.Scale>();
            
            foreach (var cad in cads)
            {
                if (cad.template == "MapaCatastral_10k")
                {
                    listScale10.Add(new Objects.Scale
                    {
                        template = cad.template,
                        geo = "true",
                        cuad = cad.cuadricula,
                        controlNum = cad.ControlNumber
                    });
                }
                if (cad.template == "MapaCatastral_1k")
                {
                    listScale1.Add(new Objects.Scale
                    {
                        template = cad.template,
                        geo = "true",
                        cuad = cad.cuadricula,
                        controlNum = cad.ControlNumber
                    });
                }
            }

            string array = string.Empty;
            string array2 = string.Empty;

            array = "(";
            bool firstItem = true;
            foreach (var cad in listScale10)
            {
                if (!firstItem)
                {
                    array += ",";
                }
                firstItem = false;
                array += "'" + cad.cuad + "'";
            }
            array += ")";

            array2 = "(";
            bool firstItem2 = true;
            foreach (var cad in listScale1)
            {
                if (!firstItem2)
                {
                    array2 += ",";
                }
                firstItem2 = false;
                array2 += "'" + cad.cuad + "'";
            }
            array2 += ")";

            if (listScale10.Count > 0)
            {
                string htmlBody = "<p style=\"padding-left: 30px;\">Layout_Template : "+ listScale10[0].template +"</p>";
                htmlBody += "<p style=\"padding-left: 30px;\">Page_Range : "+ array+"</p>";
                htmlBody += "<p style=\"padding-left: 30px;\">Georef_info : "+ listScale10[0].geo+"</p>";
                htmlBody += "<p style=\"padding-left: 30px;\">Control : "+ listScale10[0].controlNum+"</p>";
                Objects.bodyHtml += htmlBody;

                //catId.Add();
                Objects.NotCreated += "<div> Mapas catasatrales 1:10,000 no creados: " + array + "</div>";
            }

            if (listScale1.Count > 0)
            {
                string htmlBody = "<p style=\"padding-left: 30px;\">Layout_Template : " + listScale1[0].template + "</p>";
                htmlBody += "<p style=\"padding-left: 30px;\">Page_Range : " + array2 + "</p>";
                htmlBody += "<p style=\"padding-left: 30px;\">Georef_info : " + listScale1[0].geo + "</p>";
                htmlBody += "<p style=\"padding-left: 30px;\">Control : " + listScale1[0].controlNum + "</p>";
                Objects.bodyHtml += htmlBody;

                Objects.NotCreated += "<div> Mapas catasatrales 1:1,000 no creados: " + array2 + "</div>";
            }
            
            return Objects.bodyHtml;
        }

        public string AddExtractToBody(Objects.ElementoDeExtraccion elemento)
        {
            string htmlBody = "<p><strong>Elemento de extracci&oacute;n</strong>:</p>";
            htmlBody += "<p style=\"padding-left: 30px;\">Layers_to_Clip : "+ elemento.Layers_to_Clip +"</p>";
            htmlBody += "<p style=\"padding-left: 30px;\">Area_of_Interest : "+ elemento.Area_of_Interest +"</p>";
            htmlBody += "<p style=\"padding-left: 30px;\">Feature_Format : "+ elemento.Feature_Format +"</p>";
            htmlBody += "<p style=\"padding-left: 30px;\">Raster_Format : "+elemento.Raster_Format+"</p>";

            Objects.bodyHtml += htmlBody;

            Objects.NotCreated += "Elemento Extracci&oacuten, Formato " + elemento.Feature_Format + " - " + elemento.Qty + " parcelas en elemento";
            return Objects.bodyHtml;
        }

        public string AddListToBody(Objects.OrderItemList item, string client)
        {
            string htmlBody = "<p><strong>Lista de Colindantes:</strong></p>";
            htmlBody += "<p style=\"padding-left: 30px;\">Control: "+item.ControlNumber+"</p>";
            htmlBody += "<p style=\"padding-left: 30px;\">Cantidad de Parcelas: "+ item.itemQty+"</p>";
            htmlBody += "<p style=\"padding-left: 30px;\">Enlace para volver a general la Lista : "+ config.ServiceUrl + "GenerateList/"+item.ControlNumber+"/"+client+"</p>";

            Objects.bodyHtml += htmlBody;

            Objects.NotCreated += "<div>" + item.itemName + " - " + item.itemQty + " Colindantes</div>";

            return Objects.bodyHtml;
        }

    }
}