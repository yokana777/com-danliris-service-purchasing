using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitPaymentCorrectionNoteViewModel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.PDFTemplates
{
    public class UnitPaymentQuantityCorrectionNotePDFTemplate
    {
        public MemoryStream GeneratePdfTemplate(UnitPaymentCorrectionNoteViewModel viewModel, int clientTimeZoneOffset)
        {
            Font header_font = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 14);
            Font normal_font = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 10);
            Font bold_font = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 10);

            double totalPPn=0;
            double total=0;
            string currencyCodePPn = "";
            string currencyCodeTotal = "";
            DateTime receiptDate = new DateTime();

            Document document = new Document(PageSize.A5.Rotate(), 40, 40, 40, 40);
            //document.SetPageSize(iTextSharp.text.PageSize.A4.Rotate())
            MemoryStream stream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, stream);
            document.Open();

            #region Header

            //string titleString = "NOTA KOREKSI";
            //Paragraph title = new Paragraph(titleString, bold_font) { Alignment = Element.ALIGN_CENTER };
            //document.Add(title);

            //string companyNameString = "PT DAN LIRIS";
            //Paragraph companyName = new Paragraph(companyNameString, header_font) { Alignment = Element.ALIGN_LEFT };
            //document.Add(companyName);

            PdfPTable tableHeader = new PdfPTable(3);
            tableHeader.SetWidths(new float[] { 4f, 4f, 4f });
            PdfPCell cellHeaderContentLeft = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT };
            PdfPCell cellHeaderContentRight = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT };
            PdfPCell cellHeaderContentCenter = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER };

            cellHeaderContentLeft.Phrase = new Phrase("BANARAN, GROGOL, SUKOHARJO", header_font);
            tableHeader.AddCell(cellHeaderContentLeft);

            cellHeaderContentCenter.Phrase = new Phrase("NOTA KOREKSI", header_font);
            tableHeader.AddCell(cellHeaderContentCenter);

            cellHeaderContentRight.Phrase = new Phrase("");
            tableHeader.AddCell(cellHeaderContentRight);

            cellHeaderContentLeft.Phrase = new Phrase("BANARAN, GROGOL, SUKOHARJO", normal_font);
            tableHeader.AddCell(cellHeaderContentLeft);

            cellHeaderContentCenter.Phrase = new Phrase("", header_font);
            tableHeader.AddCell(cellHeaderContentCenter);

            cellHeaderContentRight.Phrase = new Phrase("FM-PB-00-06-015/R2", bold_font);
            tableHeader.AddCell(cellHeaderContentRight);

            cellHeaderContentLeft.Phrase = new Phrase("");
            tableHeader.AddCell(cellHeaderContentLeft);

            cellHeaderContentCenter.Phrase = new Phrase("");
            tableHeader.AddCell(cellHeaderContentCenter);

            cellHeaderContentRight.Phrase = new Phrase($"SUKOHARJO, {viewModel.correctionDate.ToString("dd MMMM yyyy", new CultureInfo("id-ID"))}" + "\n" + "(A126) A. SARBINI" + "\n" + "JL.GREMET / JOHO, SOLO");
            tableHeader.AddCell(cellHeaderContentRight);



            PdfPCell cellHeader = new PdfPCell(tableHeader);
            tableHeader.ExtendLastRow = false;
            tableHeader.SpacingAfter = 20f;
            document.Add(tableHeader);

            LineSeparator lineSeparator = new LineSeparator(1f, 100f, BaseColor.Black, Element.ALIGN_CENTER, 1);
            document.Add(lineSeparator);


            #endregion

            #region Identity


            PdfPTable tableIdentity = new PdfPTable(4);
            tableIdentity.SetWidths(new float[] { 3f, 7f, 1f, 7f });
            PdfPCell cellIdentityContentLeft = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT };
            PdfPCell cellIdentityContentRight = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT };

            cellIdentityContentLeft.Phrase = new Phrase("Retur/Potongan", normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase(": " + viewModel.division.name, normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentRight.Phrase = new Phrase("", normal_font);
            tableIdentity.AddCell(cellIdentityContentRight);
            cellIdentityContentRight.Phrase = new Phrase(viewModel.uPCNo, bold_font);
            tableIdentity.AddCell(cellIdentityContentRight);
            cellIdentityContentLeft.Phrase = new Phrase("Untuk", normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase(": " + viewModel.category.name, normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("", normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("", normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            //cellIdentityContentLeft.Phrase = new Phrase("No.", normal_font);
            //tableIdentity.AddCell(cellIdentityContentLeft);
            //cellIdentityContentLeft.Phrase = new Phrase(": " + viewModel.no, normal_font);
            //tableIdentity.AddCell(cellIdentityContentLeft);

            PdfPCell cellIdentity = new PdfPCell(tableIdentity);
            tableIdentity.ExtendLastRow = false;
            tableIdentity.SpacingAfter = 10f;
            tableIdentity.SpacingBefore = 20f;
            document.Add(tableIdentity);

            #endregion

            #region TableContent

            PdfPCell cellCenter = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 5 };
            PdfPCell cellRight = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 5 };
            PdfPCell cellLeft = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_LEFT, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 5 };

            PdfPTable tableContent = new PdfPTable(6);
            tableContent.SetWidths(new float[] { 1f, 6f, 3f, 3f, 3f, 3f });

            cellCenter.Phrase = new Phrase("No", bold_font);
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

            //for (int a = 0; a < 20; a++) // coba kalau banyak baris ^_^
            for (int indexItem = 0; indexItem < viewModel.items.Count; indexItem++)
            {
                UnitPaymentCorrectionNoteItemViewModel item = viewModel.items[indexItem];

                cellCenter.Phrase = new Phrase((indexItem + 1).ToString(), normal_font);
                tableContent.AddCell(cellCenter);

                cellLeft.Phrase = new Phrase($"{item.product.code} - {item.product.name}", normal_font);
                tableContent.AddCell(cellLeft);

                cellCenter.Phrase = new Phrase($"{item.quantity} {item.product.uom.unit}", normal_font);
                tableContent.AddCell(cellCenter);

                cellLeft.Phrase = new Phrase($"{item.currency.code} {item.pricePerDealUnitAfter}", normal_font);
                tableContent.AddCell(cellLeft);

                cellCenter.Phrase = new Phrase($"{item.currency.code} {item.priceTotalAfter}", normal_font);
                tableContent.AddCell(cellCenter);

                cellCenter.Phrase = new Phrase(item.pRNo, normal_font);
                tableContent.AddCell(cellCenter);

                currencyCodePPn = item.currency.code;
                currencyCodeTotal = item.currency.code;
                totalPPn += (0.1 * item.priceTotalAfter);
                total += item.priceTotalAfter;

            }


            PdfPCell cellContent = new PdfPCell(tableContent);
            tableContent.ExtendLastRow = false;
            tableContent.SpacingAfter = 20f;
            document.Add(tableContent);

            #endregion

            #region TableTotal

            PdfPTable tableTotal = new PdfPTable(4);
            tableTotal.SetWidths(new float[] { 7f, 2f, 2f, 4f });
            PdfPCell cellIdentityTotalContentLeft = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT };
            PdfPCell cellIdentityTotalContentRight = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT };

            cellIdentityContentLeft.Phrase = new Phrase("", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("Jumlah PPn 10%", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentRight.Phrase = new Phrase($" : {currencyCodePPn}", normal_font);
            tableTotal.AddCell(cellIdentityContentRight);
            cellIdentityContentRight.Phrase = new Phrase(totalPPn.ToString("N", CultureInfo.InvariantCulture), normal_font);
            tableTotal.AddCell(cellIdentityContentRight);
            cellIdentityContentLeft.Phrase = new Phrase("", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("Total", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase($" : {currencyCodeTotal}", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentRight.Phrase = new Phrase(total.ToString("N", CultureInfo.InvariantCulture), normal_font);
            tableTotal.AddCell(cellIdentityContentRight);
            cellIdentityContentLeft.Phrase = new Phrase($"Terbilang : { NumberToTextIDN.terbilang(total)}", bold_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            //cellIdentityContentLeft.Phrase = new Phrase("No.", normal_font);
            //tableIdentity.AddCell(cellIdentityContentLeft);
            //cellIdentityContentLeft.Phrase = new Phrase(": " + viewModel.no, normal_font);
            //tableIdentity.AddCell(cellIdentityContentLeft);

            PdfPCell cellIdentityTotal = new PdfPCell(tableTotal);
            tableTotal.ExtendLastRow = false;
            tableTotal.SpacingAfter = 10f;
            tableTotal.SpacingBefore = 20f;
            document.Add(tableTotal);

            #endregion

            #region TableKeterangan

            PdfPTable tableKeterangan = new PdfPTable(4);
            tableTotal.SetWidths(new float[] { 3f, 4f, 3f, 4f });
            PdfPCell cellIdentityKeteranganContentLeft = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT };
            PdfPCell cellIdentityKeteranganContentRight = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT };

            cellIdentityContentLeft.Phrase = new Phrase("Perjanjian Pembayaran ", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase($" : {viewModel.duedate.ToString("dd MMMM yyyy", new CultureInfo("id-ID"))}", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentRight.Phrase = new Phrase("", normal_font);
            tableTotal.AddCell(cellIdentityContentRight);
            cellIdentityContentRight.Phrase = new Phrase("", normal_font);
            tableTotal.AddCell(cellIdentityContentRight);
            cellIdentityContentLeft.Phrase = new Phrase("Nota ", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase($" : {viewModel.uPONo}", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("Barang Datang ", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase($" : {receiptDate.ToString("dd MMMM yyyy", new CultureInfo("id-ID"))}", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentRight.Phrase = new Phrase("Keterangan ", normal_font);
            tableTotal.AddCell(cellIdentityContentRight);
            cellIdentityContentLeft.Phrase = new Phrase($" : {viewModel.remark}", bold_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("Nomor Nota Retur ", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase($" : {viewModel.returNoteNo}", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            //cellIdentityContentLeft.Phrase = new Phrase("No.", normal_font);
            //tableIdentity.AddCell(cellIdentityContentLeft);
            //cellIdentityContentLeft.Phrase = new Phrase(": " + viewModel.no, normal_font);
            //tableIdentity.AddCell(cellIdentityContentLeft);

            PdfPCell cellIdentityKeterangan = new PdfPCell(tableKeterangan);
            tableKeterangan.ExtendLastRow = false;
            tableKeterangan.SpacingAfter = 10f;
            tableKeterangan.SpacingBefore = 20f;
            document.Add(tableKeterangan);

            #endregion


            Paragraph date = new Paragraph($"Sukoharjo, {viewModel.correctionDate.ToString("dd MMMM yyyy", new CultureInfo("id-ID"))}", normal_font) { Alignment = Element.ALIGN_RIGHT };
            document.Add(date);

            #region TableSignature

            PdfPTable tableSignature = new PdfPTable(4);

            PdfPCell cellSignatureContent = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER };
            cellSignatureContent.Phrase = new Phrase("Diperiksa,\nVerifikasi\n\n\n\n\n\n(                  )", bold_font);
            tableSignature.AddCell(cellSignatureContent);
            cellSignatureContent.Phrase = new Phrase("Mengetahui,\nPimpinan Bagian\n\n\n\n\n\n(                  )", bold_font);
            tableSignature.AddCell(cellSignatureContent);
            cellSignatureContent.Phrase = new Phrase("Tanda Terima,\nBagian Pembelian\n\n\n\n\n\n(                  )", bold_font);
            tableSignature.AddCell(cellSignatureContent);
            cellSignatureContent.Phrase = new Phrase($"Dibuat Oleh,\n\n\n\n\n\n\n(  {viewModel.CreatedBy}  )", bold_font);
            tableSignature.AddCell(cellSignatureContent);


            PdfPCell cellSignature = new PdfPCell(tableSignature); // dont remove
            tableSignature.ExtendLastRow = false;
            tableSignature.SpacingBefore = 20f;
            tableSignature.SpacingAfter = 20f;
            document.Add(tableSignature);

            #endregion

            document.Close();
            byte[] byteInfo = stream.ToArray();
            stream.Write(byteInfo, 0, byteInfo.Length);
            stream.Position = 0;

            return stream;
        }

        public MemoryStream GeneratePdfNotaReturTemplate(UnitPaymentCorrectionNoteViewModel viewModel, int clientTimeZoneOffset)
        {
            Font header_font = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 14);
            Font normal_font = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 10);
            Font bold_font = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 10);

            double totalPPn = 0;
            double total = 0;
            string currencyCodePPn = "";
            string currencyCodeTotal = "";
            DateTime receiptDate = new DateTime();

            Document document = new Document(PageSize.A5, 40, 40, 40, 40);
            //document.SetPageSize(iTextSharp.text.PageSize.A4.Rotate())
            MemoryStream stream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, stream);
            document.Open();

            #region Header

            //string titleString = "NOTA KOREKSI";
            //Paragraph title = new Paragraph(titleString, bold_font) { Alignment = Element.ALIGN_CENTER };
            //document.Add(title);

            //string companyNameString = "PT DAN LIRIS";
            //Paragraph companyName = new Paragraph(companyNameString, header_font) { Alignment = Element.ALIGN_LEFT };
            //document.Add(companyName);

            PdfPTable tableHeader = new PdfPTable(3);
            tableHeader.SetWidths(new float[] { 4f, 4f, 4f });
            PdfPCell cellHeaderContentLeft = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT };
            PdfPCell cellHeaderContentRight = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT };
            PdfPCell cellHeaderContentCenter = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER };

            cellHeaderContentLeft.Phrase = new Phrase("BANARAN, GROGOL, SUKOHARJO", header_font);
            tableHeader.AddCell(cellHeaderContentLeft);

            cellHeaderContentCenter.Phrase = new Phrase("NOTA KOREKSI", header_font);
            tableHeader.AddCell(cellHeaderContentCenter);

            cellHeaderContentRight.Phrase = new Phrase("");
            tableHeader.AddCell(cellHeaderContentRight);

            cellHeaderContentLeft.Phrase = new Phrase("BANARAN, GROGOL, SUKOHARJO", normal_font);
            tableHeader.AddCell(cellHeaderContentLeft);

            cellHeaderContentCenter.Phrase = new Phrase("", header_font);
            tableHeader.AddCell(cellHeaderContentCenter);

            cellHeaderContentRight.Phrase = new Phrase("FM-PB-00-06-015/R2", bold_font);
            tableHeader.AddCell(cellHeaderContentRight);

            cellHeaderContentLeft.Phrase = new Phrase("");
            tableHeader.AddCell(cellHeaderContentLeft);

            cellHeaderContentCenter.Phrase = new Phrase("");
            tableHeader.AddCell(cellHeaderContentCenter);

            cellHeaderContentRight.Phrase = new Phrase($"SUKOHARJO, {viewModel.correctionDate.ToString("dd MMMM yyyy", new CultureInfo("id-ID"))}" + "\n" + "(A126) A. SARBINI" + "\n" + "JL.GREMET / JOHO, SOLO");
            tableHeader.AddCell(cellHeaderContentRight);



            PdfPCell cellHeader = new PdfPCell(tableHeader);
            tableHeader.ExtendLastRow = false;
            tableHeader.SpacingAfter = 20f;
            document.Add(tableHeader);

            LineSeparator lineSeparator = new LineSeparator(1f, 100f, BaseColor.Black, Element.ALIGN_CENTER, 1);
            document.Add(lineSeparator);


            #endregion

            #region Identity


            PdfPTable tableIdentity = new PdfPTable(4);
            tableIdentity.SetWidths(new float[] { 3f, 7f, 1f, 7f });
            PdfPCell cellIdentityContentLeft = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT };
            PdfPCell cellIdentityContentRight = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT };

            cellIdentityContentLeft.Phrase = new Phrase("Retur/Potongan", normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase(": " + viewModel.division.name, normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentRight.Phrase = new Phrase("", normal_font);
            tableIdentity.AddCell(cellIdentityContentRight);
            cellIdentityContentRight.Phrase = new Phrase(viewModel.uPCNo, bold_font);
            tableIdentity.AddCell(cellIdentityContentRight);
            cellIdentityContentLeft.Phrase = new Phrase("Untuk", normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase(": " + viewModel.category.name, normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("", normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("", normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            //cellIdentityContentLeft.Phrase = new Phrase("No.", normal_font);
            //tableIdentity.AddCell(cellIdentityContentLeft);
            //cellIdentityContentLeft.Phrase = new Phrase(": " + viewModel.no, normal_font);
            //tableIdentity.AddCell(cellIdentityContentLeft);

            PdfPCell cellIdentity = new PdfPCell(tableIdentity);
            tableIdentity.ExtendLastRow = false;
            tableIdentity.SpacingAfter = 10f;
            tableIdentity.SpacingBefore = 20f;
            document.Add(tableIdentity);

            #endregion

            #region TableContent

            PdfPCell cellCenter = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 5 };
            PdfPCell cellRight = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 5 };
            PdfPCell cellLeft = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_LEFT, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 5 };

            PdfPTable tableContent = new PdfPTable(6);
            tableContent.SetWidths(new float[] { 1f, 6f, 3f, 3f, 3f, 3f });

            cellCenter.Phrase = new Phrase("No", bold_font);
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

            //for (int a = 0; a < 20; a++) // coba kalau banyak baris ^_^
            for (int indexItem = 0; indexItem < viewModel.items.Count; indexItem++)
            {
                UnitPaymentCorrectionNoteItemViewModel item = viewModel.items[indexItem];

                cellCenter.Phrase = new Phrase((indexItem + 1).ToString(), normal_font);
                tableContent.AddCell(cellCenter);

                cellLeft.Phrase = new Phrase($"{item.product.code} - {item.product.name}", normal_font);
                tableContent.AddCell(cellLeft);

                cellCenter.Phrase = new Phrase($"{item.quantity} {item.product.uom.unit}", normal_font);
                tableContent.AddCell(cellCenter);

                cellLeft.Phrase = new Phrase($"{item.currency.code} {item.pricePerDealUnitAfter}", normal_font);
                tableContent.AddCell(cellLeft);

                cellCenter.Phrase = new Phrase($"{item.currency.code} {item.priceTotalAfter}", normal_font);
                tableContent.AddCell(cellCenter);

                cellCenter.Phrase = new Phrase(item.pRNo, normal_font);
                tableContent.AddCell(cellCenter);

                currencyCodePPn = item.currency.code;
                currencyCodeTotal = item.currency.code;
                totalPPn += (0.1 * item.priceTotalAfter);
                total += item.priceTotalAfter;

            }


            PdfPCell cellContent = new PdfPCell(tableContent);
            tableContent.ExtendLastRow = false;
            tableContent.SpacingAfter = 20f;
            document.Add(tableContent);

            #endregion

            #region TableTotal

            PdfPTable tableTotal = new PdfPTable(4);
            tableTotal.SetWidths(new float[] { 7f, 2f, 2f, 4f });
            PdfPCell cellIdentityTotalContentLeft = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT };
            PdfPCell cellIdentityTotalContentRight = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT };

            cellIdentityContentLeft.Phrase = new Phrase("", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("Jumlah PPn 10%", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentRight.Phrase = new Phrase($" : {currencyCodePPn}", normal_font);
            tableTotal.AddCell(cellIdentityContentRight);
            cellIdentityContentRight.Phrase = new Phrase(totalPPn.ToString("N", CultureInfo.InvariantCulture), normal_font);
            tableTotal.AddCell(cellIdentityContentRight);
            cellIdentityContentLeft.Phrase = new Phrase("", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("Total", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase($" : {currencyCodeTotal}", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentRight.Phrase = new Phrase(total.ToString("N", CultureInfo.InvariantCulture), normal_font);
            tableTotal.AddCell(cellIdentityContentRight);
            cellIdentityContentLeft.Phrase = new Phrase($"Terbilang : { NumberToTextIDN.terbilang(total)}", bold_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            //cellIdentityContentLeft.Phrase = new Phrase("No.", normal_font);
            //tableIdentity.AddCell(cellIdentityContentLeft);
            //cellIdentityContentLeft.Phrase = new Phrase(": " + viewModel.no, normal_font);
            //tableIdentity.AddCell(cellIdentityContentLeft);

            PdfPCell cellIdentityTotal = new PdfPCell(tableTotal);
            tableTotal.ExtendLastRow = false;
            tableTotal.SpacingAfter = 10f;
            tableTotal.SpacingBefore = 20f;
            document.Add(tableTotal);

            #endregion

            #region TableKeterangan

            PdfPTable tableKeterangan = new PdfPTable(4);
            tableTotal.SetWidths(new float[] { 3f, 4f, 3f, 4f });
            PdfPCell cellIdentityKeteranganContentLeft = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT };
            PdfPCell cellIdentityKeteranganContentRight = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT };

            cellIdentityContentLeft.Phrase = new Phrase("Perjanjian Pembayaran ", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase($" : {viewModel.duedate.ToString("dd MMMM yyyy", new CultureInfo("id-ID"))}", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentRight.Phrase = new Phrase("", normal_font);
            tableTotal.AddCell(cellIdentityContentRight);
            cellIdentityContentRight.Phrase = new Phrase("", normal_font);
            tableTotal.AddCell(cellIdentityContentRight);
            cellIdentityContentLeft.Phrase = new Phrase("Nota ", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase($" : {viewModel.uPONo}", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("Barang Datang ", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase($" : {receiptDate.ToString("dd MMMM yyyy", new CultureInfo("id-ID"))}", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentRight.Phrase = new Phrase("Keterangan ", normal_font);
            tableTotal.AddCell(cellIdentityContentRight);
            cellIdentityContentLeft.Phrase = new Phrase($" : {viewModel.remark}", bold_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("Nomor Nota Retur ", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase($" : {viewModel.returNoteNo}", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("", normal_font);
            tableTotal.AddCell(cellIdentityContentLeft);
            //cellIdentityContentLeft.Phrase = new Phrase("No.", normal_font);
            //tableIdentity.AddCell(cellIdentityContentLeft);
            //cellIdentityContentLeft.Phrase = new Phrase(": " + viewModel.no, normal_font);
            //tableIdentity.AddCell(cellIdentityContentLeft);

            PdfPCell cellIdentityKeterangan = new PdfPCell(tableKeterangan);
            tableKeterangan.ExtendLastRow = false;
            tableKeterangan.SpacingAfter = 10f;
            tableKeterangan.SpacingBefore = 20f;
            document.Add(tableKeterangan);

            #endregion


            Paragraph date = new Paragraph($"Sukoharjo, {viewModel.correctionDate.ToString("dd MMMM yyyy", new CultureInfo("id-ID"))}", normal_font) { Alignment = Element.ALIGN_RIGHT };
            document.Add(date);

            #region TableSignature

            PdfPTable tableSignature = new PdfPTable(4);

            PdfPCell cellSignatureContent = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER };
            cellSignatureContent.Phrase = new Phrase("Diperiksa,\nVerifikasi\n\n\n\n\n\n(                  )", bold_font);
            tableSignature.AddCell(cellSignatureContent);
            cellSignatureContent.Phrase = new Phrase("Mengetahui,\nPimpinan Bagian\n\n\n\n\n\n(                  )", bold_font);
            tableSignature.AddCell(cellSignatureContent);
            cellSignatureContent.Phrase = new Phrase("Tanda Terima,\nBagian Pembelian\n\n\n\n\n\n(                  )", bold_font);
            tableSignature.AddCell(cellSignatureContent);
            cellSignatureContent.Phrase = new Phrase($"Dibuat Oleh,\n\n\n\n\n\n\n(  {viewModel.CreatedBy}  )", bold_font);
            tableSignature.AddCell(cellSignatureContent);


            PdfPCell cellSignature = new PdfPCell(tableSignature); // dont remove
            tableSignature.ExtendLastRow = false;
            tableSignature.SpacingBefore = 20f;
            tableSignature.SpacingAfter = 20f;
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
