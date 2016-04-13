using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace WcfCrimShopService.entities
{
    public class ColindantePdfEventHandler : PdfPageEventHelper
    {
        public string controlNumber { get; set; }
        public string contribuyente { get; set; }
        public string cantidad { get; set; }
        public string Parcela { get; set; }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            //cell height
            float cellHeight = document.TopMargin - 5;
            //pdf document size
            Rectangle page = document.PageSize;

            //Create two column table
            PdfPTable head = new PdfPTable(3);
            head.TotalWidth = page.Width - 50.0F;
           
            //add the header text
            PdfPCell c = new PdfPCell(new Phrase("Contribuyente: " + contribuyente, FontFactory.GetFont(FontFactory.HELVETICA_BOLD))); //DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " GMT"
            c.Border = PdfPCell.NO_BORDER;
            c.VerticalAlignment = Element.ALIGN_BOTTOM;
            c.FixedHeight = cellHeight;
            head.AddCell(c);

            c = new PdfPCell(new Phrase("Lista De Propiedades", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
            c.Border = PdfPCell.NO_BORDER;
            c.VerticalAlignment = Element.ALIGN_BOTTOM;
            c.HorizontalAlignment = Element.ALIGN_CENTER;
            c.FixedHeight = cellHeight;
            head.AddCell(c);

            c = new PdfPCell(new Phrase("Parcela : " + Parcela, FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
            c.Border = PdfPCell.NO_BORDER;
            c.VerticalAlignment = Element.ALIGN_BOTTOM;
            c.HorizontalAlignment = Element.ALIGN_RIGHT;
            c.FixedHeight = cellHeight;
            head.AddCell(c);

            head.WriteSelectedRows(0, -1, 25, page.Height - cellHeight + head.TotalHeight, writer.DirectContent);

            //************FOOTER
            //cell height
            float cellHeightFooter = document.BottomMargin;
            //pdf document size
            //Rectangle page = document.PageSize;

            //Create two column table
            PdfPTable footer = new PdfPTable(3);
            footer.TotalWidth = page.Width - 50.0F;
            
            //add the header text
            PdfPCell d = new PdfPCell(new Phrase(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " GMT", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
            d.Border = PdfPCell.NO_BORDER;
            d.VerticalAlignment = Element.ALIGN_TOP;
            d.FixedHeight = cellHeightFooter;
            footer.AddCell(d);

            d = new PdfPCell(new Phrase("num.Control : " + controlNumber, FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
            d.Border = PdfPCell.NO_BORDER;
            d.VerticalAlignment = Element.ALIGN_TOP;
            d.HorizontalAlignment = Element.ALIGN_RIGHT;
            d.FixedHeight = cellHeightFooter;
            footer.AddCell(d);

            d = new PdfPCell(new Phrase(cantidad + " colindantes", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
            d.Border = PdfPCell.NO_BORDER;
            d.VerticalAlignment = Element.ALIGN_TOP;
            d.HorizontalAlignment = Element.ALIGN_RIGHT;
            d.FixedHeight = cellHeightFooter;
            footer.AddCell(d);

            footer.WriteSelectedRows(0, -1, 25, cellHeightFooter, writer.DirectContent);//page.Height - cellHeightFooter + head.TotalHeight
        }
    }
}