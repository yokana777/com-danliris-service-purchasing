using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentOrderModel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Newtonsoft.Json;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Lib.Utilities;

namespace Com.DanLiris.Service.Purchasing.Lib.PDFTemplates
{
    public class UnitPaymentOrderPDFTemplate
    {
        public MemoryStream Generate(UnitPaymentOrder model, IServiceProvider serviceProvider, int clientTimeZoneOffset = 7, string userName = null)
        {
            PurchasingDbContext purchasingDbContext = (PurchasingDbContext)serviceProvider.GetService(typeof(PurchasingDbContext));

            Font header_font = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 14);
            Font normal_font = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 10);
            Font bold_font = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 10);

            Document document = new Document(PageSize.A4.Rotate(), 40, 40, 40, 40);
            MemoryStream stream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, stream);
            document.Open();

            PdfPCell cellLeftNoBorder = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT };
            PdfPCell cellCenterNoBorder = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER };
            PdfPCell cellCenterTopNoBorder = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_TOP };
            PdfPCell cellRightNoBorder = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT };
            PdfPCell cellJustifyNoBorder = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_JUSTIFIED };
            PdfPCell cellJustifyAllNoBorder = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_JUSTIFIED_ALL };

            PdfPCell cellCenter = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 5 };
            PdfPCell cellRight = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 5 };
            PdfPCell cellLeft = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_LEFT, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 5 };

            #region Header

            PdfPTable tableHeader = new PdfPTable(3);
            tableHeader.SetWidths(new float[] { 1f, 1f, 1f });

            PdfPCell cellHeaderContentLeft = new PdfPCell() { Border = Rectangle.NO_BORDER };
            cellHeaderContentLeft.AddElement(new Phrase("PT DAN LIRIS", header_font));
            cellHeaderContentLeft.AddElement(new Phrase("BANARAN, GROGOL, SUKOHARJO", normal_font));
            tableHeader.AddCell(cellHeaderContentLeft);

            PdfPCell cellHeaderContentCenter = new PdfPCell() { Border = Rectangle.NO_BORDER };
            cellHeaderContentCenter.AddElement(new Paragraph("NOTA KREDIT", header_font) { Alignment = Element.ALIGN_CENTER });
            cellHeaderContentCenter.AddElement(new Paragraph(model.PaymentMethod.ToUpper().Trim().Equals("CASH") ? "CASH" : "", normal_font) { Alignment = Element.ALIGN_CENTER });
            tableHeader.AddCell(cellHeaderContentCenter);

            PdfPCell cellHeaderContentRight = new PdfPCell() { Border = Rectangle.NO_BORDER };
            cellHeaderContentRight.AddElement(new Phrase("FM-PB-00-06-014/R1", normal_font));
            cellHeaderContentRight.AddElement(new Phrase($"SUKOHARJO, {model.Date.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("dd MMMM yyyy", new CultureInfo("id-ID"))}", normal_font));
            cellHeaderContentRight.AddElement(new Phrase(model.SupplierName, normal_font));
            cellHeaderContentRight.AddElement(new Phrase(model.SupplierAddress, normal_font));
            tableHeader.AddCell(cellHeaderContentRight);

            PdfPCell cellHeader = new PdfPCell(tableHeader);
            tableHeader.ExtendLastRow = false;
            tableHeader.SpacingAfter = 15f;
            document.Add(tableHeader);

            #endregion

            #region Identity

            PdfPTable tableIdentity = new PdfPTable(3);
            tableIdentity.SetWidths(new float[] { 1.5f, 4.5f, 3f });

            cellLeftNoBorder.Phrase = new Phrase("Nota Pembelian", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase($":   {model.CategoryName}", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase($"Nomor   {model.UPONo}", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase("Untuk", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase($":   {model.DivisionName}", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase("", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);

            PdfPCell cellIdentity = new PdfPCell(tableIdentity);
            tableIdentity.ExtendLastRow = false;
            tableIdentity.SpacingAfter = 15f;
            document.Add(tableIdentity);

            #endregion

            #region TableContent

            PdfPTable tableContent = new PdfPTable(8);
            tableContent.SetWidths(new float[] { 1.5f, 6f, 3f, 5f, 5f, 5f, 5f, 3f });

            cellCenter.Phrase = new Phrase("No.", bold_font);
            tableContent.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Nama Barang", bold_font);
            tableContent.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Jumlah", bold_font);
            tableContent.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Harga Satuan", bold_font);
            tableContent.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Harga Total", bold_font);
            tableContent.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Nomor Order", bold_font);
            tableContent.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Nomor Bon Unit", bold_font);
            tableContent.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Unit", bold_font);
            tableContent.AddCell(cellCenter);

            int no = 0;
            double total = 0;

            //foreach (var f in new float[15])
            foreach (var item in model.Items)
            {
                foreach (var detail in item.Details)
                {
                    cellCenter.Phrase = new Phrase($"{++no}", normal_font);
                    tableContent.AddCell(cellCenter);

                    cellLeft.Phrase = new Phrase(detail.ProductName, normal_font);
                    tableContent.AddCell(cellLeft);

                    cellRight.Phrase = new Phrase($"{detail.ReceiptQuantity} {detail.UomUnit}", normal_font);
                    tableContent.AddCell(cellRight);

                    cellRight.Phrase = new Phrase($"{model.CurrencyCode} {detail.PricePerDealUnit.ToString("n", new CultureInfo("id-ID"))}", normal_font);
                    tableContent.AddCell(cellRight);

                    cellRight.Phrase = new Phrase($"{model.CurrencyCode} {detail.PriceTotal.ToString("n", new CultureInfo("id-ID"))}", normal_font);
                    tableContent.AddCell(cellRight);

                    cellCenter.Phrase = new Phrase($"{detail.PRNo}", normal_font);
                    tableContent.AddCell(cellCenter);

                    cellCenter.Phrase = new Phrase($"{item.URNNo}", normal_font);
                    tableContent.AddCell(cellCenter);

                    cellCenter.Phrase = new Phrase($"{purchasingDbContext.UnitReceiptNotes.Single(m => m.Id == item.URNId).UnitName}", normal_font);
                    tableContent.AddCell(cellCenter);

                    total += detail.PriceTotal;
                }
            }

            PdfPCell cellContent = new PdfPCell(tableContent);
            tableContent.ExtendLastRow = false;
            tableContent.SpacingAfter = 10f;
            document.Add(tableContent);

            #endregion

            #region Tax

            PdfPTable tableTax = new PdfPTable(3);
            tableTax.SetWidths(new float[] { 1f, 0.3f, 1f });

            var pph = total * model.IncomeTaxRate / 100;
            var pphTotal = total + pph;

            if (!model.UseIncomeTax)
            {
                tableTax.AddCell(new PdfPCell() { Border = Rectangle.NO_BORDER });
            }
            else
            {
                PdfPTable tableIncomeTax = new PdfPTable(3);
                tableIncomeTax.SetWidths(new float[] { 5f, 2f, 3f });

                tableIncomeTax.AddCell(new PdfPCell(new Phrase(" ", normal_font)) { Border = Rectangle.NO_BORDER, Colspan = 3 });

                cellLeftNoBorder.Phrase = new Phrase($"PPh {model.IncomeTaxName} {model.IncomeTaxRate}%", normal_font);
                tableIncomeTax.AddCell(cellLeftNoBorder);

                cellLeftNoBorder.Phrase = new Phrase($":   {model.CurrencyCode}", normal_font);
                tableIncomeTax.AddCell(cellLeftNoBorder);

                cellRightNoBorder.Phrase = new Phrase($"{pph.ToString("n", new CultureInfo("id-ID"))}", normal_font);
                tableIncomeTax.AddCell(cellRightNoBorder);

                cellLeftNoBorder.Phrase = new Phrase("Jumlah dibayar Ke Supplier", normal_font);
                tableIncomeTax.AddCell(cellLeftNoBorder);

                cellLeftNoBorder.Phrase = new Phrase($":   {model.CurrencyCode}", normal_font);
                tableIncomeTax.AddCell(cellLeftNoBorder);

                cellRightNoBorder.Phrase = new Phrase($"{pphTotal.ToString("n", new CultureInfo("id-ID"))}", normal_font);
                tableIncomeTax.AddCell(cellRightNoBorder);

                tableTax.AddCell(new PdfPCell(tableIncomeTax) { Border = Rectangle.NO_BORDER });
            }

            tableTax.AddCell(new PdfPCell() { Border = Rectangle.NO_BORDER });

            PdfPTable tableVat = new PdfPTable(2);

            var ppn = total / 10;
            var ppnTotal = total + ppn;

            cellJustifyAllNoBorder.Phrase = new Phrase($"Jumlah . . . . . . . . . . . . . . .   {model.CurrencyCode}", normal_font);
            tableVat.AddCell(cellJustifyAllNoBorder);

            cellRightNoBorder.Phrase = new Phrase($"{total.ToString("n", new CultureInfo("id-ID"))}", normal_font);
            tableVat.AddCell(cellRightNoBorder);

            cellJustifyAllNoBorder.Phrase = new Phrase($"PPn 10 % . . . . . . . . . . . . . .   {model.CurrencyCode}", normal_font);
            tableVat.AddCell(cellJustifyAllNoBorder);

            cellRightNoBorder.Phrase = new Phrase($"{ppn.ToString("n", new CultureInfo("id-ID"))}", normal_font);
            tableVat.AddCell(cellRightNoBorder);

            cellJustifyAllNoBorder.Phrase = new Phrase($"T O T A L. . . . . . . . . . . . . .   {model.CurrencyCode}", normal_font);
            tableVat.AddCell(cellJustifyAllNoBorder);

            cellRightNoBorder.Phrase = new Phrase($"{ppnTotal.ToString("n", new CultureInfo("id-ID"))}", normal_font);
            tableVat.AddCell(cellRightNoBorder);

            tableTax.AddCell(new PdfPCell(tableVat) { Border = Rectangle.NO_BORDER });

            PdfPCell taxCell = new PdfPCell(tableTax);
            tableTax.ExtendLastRow = false;
            tableTax.SpacingAfter = 15f;
            document.Add(tableTax);

            #endregion

            Paragraph paragraphTerbilang = new Paragraph($"Terbilang : {NumberToTextIDN.terbilang(model.UseIncomeTax ? pphTotal : ppnTotal)} {model.CurrencyDescription.ToLower()}", bold_font) { SpacingAfter = 15f };
            document.Add(paragraphTerbilang);

            #region Footer

            PdfPTable tableFooter = new PdfPTable(2);
            tableFooter.SetWidths(new float[] { 1.3f, 1f });

            PdfPTable tableFooterLeft = new PdfPTable(2);
            tableFooterLeft.SetWidths(new float[] { 5f, 7.7f });

            cellLeftNoBorder.Phrase = new Phrase("Perjanjian Pembayaran", normal_font);
            tableFooterLeft.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Phrase = new Phrase($":   {model.DueDate.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("dd MMMM yyyy", new CultureInfo("id-ID"))}", normal_font);
            tableFooterLeft.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Phrase = new Phrase("Invoice", normal_font);
            tableFooterLeft.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Phrase = new Phrase($":   {model.InvoiceNo ?? "-"}", normal_font);
            tableFooterLeft.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Phrase = new Phrase("No PIB", normal_font);
            tableFooterLeft.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Phrase = new Phrase($":   {model.PibNo ?? "-"}", normal_font);
            tableFooterLeft.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Phrase = new Phrase("Ket.", normal_font);
            tableFooterLeft.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Phrase = new Phrase($":   {model.Remark ?? "-"}", normal_font);
            tableFooterLeft.AddCell(cellLeftNoBorder);

            tableFooter.AddCell(new PdfPCell(tableFooterLeft) { Border = Rectangle.NO_BORDER });

            PdfPTable tableFooterRight = new PdfPTable(2);
            tableFooterRight.SetWidths(new float[] { 5f, 7.3f });

            cellLeftNoBorder.Phrase = new Phrase("Barang Datang", normal_font);
            tableFooterRight.AddCell(cellLeftNoBorder);

            List<DateTimeOffset> UnitReceiptNoteDates = new List<DateTimeOffset>();
            foreach (var item in model.Items)
            {
                var unitReceiptNoteDate = purchasingDbContext.UnitReceiptNotes.Single(m => m.Id == item.URNId).ReceiptDate;
                UnitReceiptNoteDates.Add(unitReceiptNoteDate);
            }
            var maxUnitReceiptNoteDate = UnitReceiptNoteDates.Max();
            cellLeftNoBorder.Phrase = new Phrase($":   {maxUnitReceiptNoteDate.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("dd MMMM yyyy", new CultureInfo("id-ID"))}", normal_font);
            tableFooterRight.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Phrase = new Phrase("Nomor Faktur Pajak PPN", normal_font);
            tableFooterRight.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Phrase = new Phrase($":   {model.VatNo ?? "-"}", normal_font);
            tableFooterRight.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Phrase = new Phrase("Pembayaran", normal_font);
            tableFooterRight.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Phrase = new Phrase($":   {model.PaymentMethod ?? "-"}", normal_font);
            tableFooterRight.AddCell(cellLeftNoBorder);

            tableFooter.AddCell(new PdfPCell(tableFooterRight) { Border = Rectangle.NO_BORDER });

            PdfPCell taxFooter = new PdfPCell(tableFooter);
            tableFooter.ExtendLastRow = false;
            tableFooter.SpacingAfter = 30f;
            document.Add(tableFooter);

            #endregion

            #region TableSignature

            PdfPTable tableSignature = new PdfPTable(4);

            cellCenterTopNoBorder.Phrase = new Paragraph("Diperiksa,\nVerifkasi\n\n\n\n\n\n\n\n(                                   )", normal_font);
            tableSignature.AddCell(cellCenterTopNoBorder);
            cellCenterTopNoBorder.Phrase = new Paragraph("Mengetahui,\nPimpinan Bagian\n\n\n\n\n\n\n\n(                                   )", normal_font);
            tableSignature.AddCell(cellCenterTopNoBorder);
            cellCenterTopNoBorder.Phrase = new Paragraph("Tanda Terima,\nBagian Pembelian\n\n\n\n\n\n\n\n(                                   )", normal_font);
            tableSignature.AddCell(cellCenterTopNoBorder);
            cellCenterTopNoBorder.Phrase = new Paragraph($"Dibuat Oleh,\n\n\n\n\n\n\n\n\n( {userName ?? "                                 "} )", normal_font);
            tableSignature.AddCell(cellCenterTopNoBorder);

            PdfPCell cellSignature = new PdfPCell(tableSignature);
            tableSignature.ExtendLastRow = false;
            document.Add(tableSignature);

            #endregion

            document.Close();
            byte[] byteInfo = stream.ToArray();
            stream.Write(byteInfo, 0, byteInfo.Length);
            stream.Position = 0;

            return stream;
        }
    }
}
