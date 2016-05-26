using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using Newtonsoft.Json;
using System.IO;
using System.Net.Mime;
using WcfCrimShopService.entities;


namespace WcfCrimShopService.entities
{
    public class EmailSupport
    {
        Objects.ConfigObject config = JsonConvert.DeserializeObject<Objects.ConfigObject>(File.ReadAllText(System.AppDomain.CurrentDomain.BaseDirectory + @"Config.json"));
        public string EmailGenerator()
        {
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



            return "";
        }

        private string EmailBody()
        {
            string body = Objects.bodyHtml;
            return body;
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
            }

            if (listScale1.Count > 0)
            {
                string htmlBody = "<p style=\"padding-left: 30px;\">Layout_Template : " + listScale1[0].template + "</p>";
                htmlBody += "<p style=\"padding-left: 30px;\">Page_Range : " + array2 + "</p>";
                htmlBody += "<p style=\"padding-left: 30px;\">Georef_info : " + listScale1[0].geo + "</p>";
                htmlBody += "<p style=\"padding-left: 30px;\">Control : " + listScale1[0].controlNum + "</p>";
                Objects.bodyHtml += htmlBody;
            }
            
            return Objects.bodyHtml;
        }

        public string AddExtractToBody(Objects.ElementoDeExtraccion elemento)
        {
            string htmlBody = "<p><strong>Elemento de extraccion</strong>:</p>";
            htmlBody += "<p style=\"padding-left: 30px;\">Layers_to_Clip : "+ elemento.Layers_to_Clip +"</p>";
            htmlBody += "<p style=\"padding-left: 30px;\">Area_of_Interest : "+ elemento.Area_of_Interest +"</p>";
            htmlBody += "<p style=\"padding-left: 30px;\">Feature_Format : "+ elemento.Feature_Format +"</p>";
            htmlBody += "<p style=\"padding-left: 30px;\">Raster_Format : "+elemento.Raster_Format+"</p>";

            Objects.bodyHtml += htmlBody;
            return Objects.bodyHtml;
        }

        public string AddListToBody(Objects.OrderItemList item)
        {
            string htmlBody = "<p><strong>Lista de Colindantes:</strong></p>";
            htmlBody += "<p style=\"padding-left: 30px;\">Control:</p>";
            htmlBody += "<p style=\"padding-left: 30px;\">Parcelas:</p>";
            htmlBody += "<p style=\"padding-left: 30px;\">Item:</p>";

            Objects.bodyHtml += htmlBody;
            return Objects.bodyHtml;
        }

    }
}