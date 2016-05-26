using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WcfCrimShopService.entities
{
    public class Objects
    {
        public class Order
        {
            public string ControlNumber { get; set; }
            public string Confirmation { get; set; }
            public string CustomerName { get; set; }
            public string CustomerEmail { get; set; }
            public string HasPhoto { get; set; }
            public string HasCat { get; set; }
            public string HasList { get; set; }
            public string HasExtract { get; set; }
        }

        public class FullOrderInfo
        {
            public string ControlNumber { get; set; }
            public string Description { get; set; }
            public string Confirmation { get; set; }
            public decimal Tax { get; set; }
            public decimal Subtotal { get; set; }
            public decimal Total { get; set; }
            public DateTime OrderDate { get; set; }
            public string CustomerName { get; set; }
            public string CustomerEmail { get; set; }
            public string HasPhoto { get; set; }
            public string HasCat { get; set; }
            public string HasList { get; set; }
            public string HasExtract { get; set; }
            public string OrderFilePath { get; set; }
        }

        public class OrderItemPhoto
        {
            public string ControlNumber { get; set; }
            public string ItemQty { get; set; }
            public string Item { get; set; }
            public string Format { get; set; }
            public string LayoutTemplate { get; set; }
            public string GeorefInfo { get; set; }
            public string Parcel { get; set; }
            public string subtitle { get; set; }
            public string buffer { get; set; }
            public string parcelList { get; set; }
            public string distance { get; set; }
            public decimal cost { get; set; }
            public string title { get; set; }
            public string created { get; set; }
        }

        public class OrderItemList
        {
            public string ControlNumber { get; set; }
            public string itemName { get; set; }
            public string itemQty { get; set; }
            public string item { get; set; }
            public decimal cost { get; set; }
            public string created { get; set; }
        }

        public class OrderItemCatastral
        {
            public string ControlNumber { get; set; }
            public string itemQty { get; set; }
            public string escala { get; set; }
            public string cuadricula { get; set; }
            public string template { get; set; }
            public decimal cost { get; set; }
            public string created { get; set; }
        }

        public class Scale
        {
            public string template { get; set; }
            public string geo { get; set; }
            public string cuad { get; set; }
            public string controlNum { get; set; }

        }
        public class listaColindante
        {
            public string ParcelaProcedencia { get; set; }
            public string Parcela { get; set; }
            public string Catastro { get; set; }
            public string Municipio { get; set; }
            public string Dueno { get; set; }
            public string DireccionFisica { get; set; }
            public string DireccionPostal { get; set; }
        }

        public class ListaCol
        {
            public List<listaColindante> ListaColindante { get; set; }
        }

        public class ProductPrice
        {
            public string product { get; set; }
            public decimal price { get; set; }
        }

        /**
         * 
         * Configuration objects
         * 
         * **/
        public class ConfigObject
        {
            public ServerConnection ServerConnection { get; set; }
            public ActiveDirectoryInformation ActiveDirectoryInformation { get; set; }
            public EmailConfiguration EmailConfiguration { get; set; }
            public MerchantInfo MerchantInfo { get; set; }
            public PortalAuthentication PortalAuthentication { get; set; }
            public string OrderDownloadStorage { get; set; }
            public string MailDownloadPath { get; set; }
            public string ExtractDataUrl { get; set; }
            public string FotoAereaUrl { get; set; }
            public string MapasCatastral { get; set; }
            public string SupportEmail { get; set; }
        }

        public class ActiveDirectoryInformation
        {
            public string domain { get; set; }
            public string[] group { get; set; }

        }

        public class EmailConfiguration
        {
            public string SMTPClient { get; set; }
            public string MailAddress { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string port { get; set; }
        }
        public class MerchantInfo
        {
            public string user { get; set; }
            public string pass { get; set; }
            public string serviceUrl { get; set; }
            public string serviceAction { get; set; }
        }

        public class ServerConnection
        {
            public string source { get; set; }
            public string catalog { get; set; }
            public string id { get; set; }
            public string password { get; set; }
        }

        public class PortalAuthentication
        {
            public string ServiceUri { get; set; }
            public string username { get; set; }
            public string password { get; set; }
        }

        /**
         * 
         * End of configuration objects
         * 
         * **/
        static string pathOfZip;
        public static string path
        {
            get{
                return pathOfZip;
            }
            set{
                pathOfZip = value;
            }
        }

        static string htmlBody;
        public static string bodyHtml
        {
            get
            {
                return htmlBody;
            }
            set
            {
                htmlBody = value;
            }
        }
        public class ElementoDeExtraccion
        {
            public string ControlNumber { get; set; }
            public int Qty { get; set; }
            public string Layers_to_Clip { get; set; }
            public string Area_of_Interest { get; set; }
            public string Feature_Format { get; set; }
            public string Raster_Format { get; set; }
            public string Created { get; set; }
            public decimal Price { get; set; }
        }


    }
}