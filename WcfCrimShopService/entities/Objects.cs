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
        }

        public class OrderItemList
        {
            public string ControlNumber { get; set; }
            public string itemName { get; set; }
            public string itemQty { get; set; }
            public string item { get; set; }
            public decimal cost { get; set; }
        }

        public class OrderItemCatastral
        {
            public string ControlNumber { get; set; }
            public string itemName { get; set; }
            public string itemQty { get; set; }
            public string escala { get; set; }
            public string cuadricula { get; set; }
            public string template { get; set; }
            public decimal cost { get; set; }
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

        public class ConfigObject
        {
            public ServerConnection ServerConnection { get; set; }
            public ActiveDirectoryInformation ActiveDirectoryInformation { get; set; }
            public string OrderDownloadStorage { get; set; }
        }

        public class ActiveDirectoryInformation
        {
            public string domain { get; set; }
            public string[] group { get; set; }

        }

        public class ServerConnection
        {
            public string source { get; set; }
            public string catalog { get; set; }
            public string id { get; set; }
            public string password { get; set; }
        }
    }
}