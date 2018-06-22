using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNote;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.PDFTemplates
{
    public class UnitReceiptNotePDFTemplate
    {
        public MemoryStream GeneratePdfTemplate(UnitReceiptNoteViewModel viewModel)
        {
            Font header_font = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 18);
            Font normal_font = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 10);
            Font bold_font = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 10);

            Document document = new Document(PageSize.A4, 40, 40, 40, 40);
            MemoryStream stream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, stream);
            document.Open();

            #region Header

            string titleString = "BON PENERIMAAN BARANG";
            Paragraph title = new Paragraph(titleString, bold_font) { Alignment = Element.ALIGN_CENTER };
            document.Add(title);

            string companyNameString = "PT DAN LIRIS";
            Paragraph companyName = new Paragraph(companyNameString, header_font) { Alignment = Element.ALIGN_LEFT };
            document.Add(companyName);

            string companyAddressString = "BANARAN, GROGOL, SUKOHARJO";
            Paragraph companyAddress = new Paragraph(companyAddressString, normal_font) { Alignment = Element.ALIGN_LEFT };
            companyAddress.SpacingAfter = 10f;
            document.Add(companyAddress);


            string codeNoString = "FM-PB-00-06-006/R1";
            Paragraph codeNo = new Paragraph(codeNoString, bold_font) { Alignment = Element.ALIGN_RIGHT };
            codeNo.SpacingBefore = 5f;
            document.Add(codeNo);

            LineSeparator lineSeparator = new LineSeparator(1f, 100f, BaseColor.Black, Element.ALIGN_CENTER, 1);
            document.Add(lineSeparator);


            #endregion

            document.Close();
            byte[] byteInfo = stream.ToArray();
            stream.Write(byteInfo, 0, byteInfo.Length);
            stream.Position = 0;

            return stream;
        }
    }
}
