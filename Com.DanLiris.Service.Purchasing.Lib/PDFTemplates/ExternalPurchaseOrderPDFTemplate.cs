using Com.DanLiris.Service.Purchasing.Lib.ViewModels.ExternalPurchaseOrderViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;

namespace Com.DanLiris.Service.Purchasing.Lib.PDFTemplates
{
    public class ExternalPurchaseOrderPDFTemplate
    {
        public MemoryStream GeneratePdfTemplate(ExternalPurchaseOrderViewModel viewModel, int clientTimeZoneOffset)
        {
            Font header_font = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 18);
            Font normal_font = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 9);
            Font bold_font = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 8);

            Document document = new Document(PageSize.A5, 40, 40, 40, 40);
            MemoryStream stream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, stream);
            document.Open();

            #region Header

            PdfPTable tableHeader = new PdfPTable(2);
            tableHeader.SetWidths(new float[] { 4f,4f });
            PdfPCell cellHeaderContentLeft = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT };
            PdfPCell cellHeaderContentRight = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT };

            cellHeaderContentLeft.Phrase = new Phrase("PT DAN LIRIS" + "\n" + "Head Office: Kelurahan Banaran" + "\n" + "Kecamatan Grogol" + "\n" + "Sukoharjo 57193 - INDONESIA" + "\n" +  "PO.BOX 166 Solo 57100" + "\n" + "Telp. (0271) 740888, 714400" + "\n" + "Fax. (0271) 735222, 740777", bold_font);
            tableHeader.AddCell(cellHeaderContentLeft);

            cellHeaderContentRight.Phrase = new Phrase("FM-PB-00-06-009/R1" + "\n" + "Nomor PO: " + viewModel.no, bold_font);
            tableHeader.AddCell(cellHeaderContentRight);

            PdfPCell cellHeader = new PdfPCell(tableHeader); // dont remove
            tableHeader.ExtendLastRow = false;
            tableHeader.SpacingAfter = 10f;
            document.Add(tableHeader);

            string titleString = "ORDER PEMBELIAN";
            bold_font.SetStyle(Font.UNDERLINE);
            Paragraph title = new Paragraph(titleString, bold_font) { Alignment = Element.ALIGN_CENTER };
            document.Add(title);
            bold_font.SetStyle(Font.NORMAL);

            #endregion

            #region Identity

            PdfPTable tableIdentity = new PdfPTable(3);
            tableIdentity.SetWidths(new float[] { 1.2f, 2.5f, 2.9f });
            PdfPCell cellIdentityContentLeft = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT };
            PdfPCell cellIdentityContentRight = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT };
            cellIdentityContentLeft.Phrase = new Phrase("Kepada Yth", normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase(": " + viewModel.supplier.name, normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentRight.Phrase = new Phrase("Sukoharjo, " + viewModel.orderDate.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("dd MMMM yyyy", new CultureInfo("id-ID")), normal_font);
            tableIdentity.AddCell(cellIdentityContentRight);
            cellIdentityContentLeft.Phrase = new Phrase(" ", normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("  " + "Attn. \n  Telp.", normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentRight.Phrase = new Phrase("Mohon di-fax kembali setelah ditandatangani dan distempel perusahaan. Terima Kasih.", normal_font);
            tableIdentity.AddCell(cellIdentityContentRight);
            //cellIdentityContentLeft.Phrase = new Phrase(" ", normal_font);
            //tableIdentity.AddCell(cellIdentityContentLeft);
            //cellIdentityContentLeft.Phrase = new Phrase(" " + "Telp.", normal_font);
            //tableIdentity.AddCell(cellIdentityContentLeft);
            //cellIdentityContentRight.Phrase = new Phrase("ditandatangani dan distempel", normal_font);
            //tableIdentity.AddCell(cellIdentityContentRight);
            //cellIdentityContentLeft.Phrase = new Phrase(" ", normal_font);
            //tableIdentity.AddCell(cellIdentityContentLeft);
            //cellIdentityContentLeft.Phrase = new Phrase(" " + " ", normal_font);
            //tableIdentity.AddCell(cellIdentityContentLeft);
            //cellIdentityContentRight.Phrase = new Phrase("perusahaan. Terima Kasih.", normal_font);
            //tableIdentity.AddCell(cellIdentityContentRight);
            PdfPCell cellIdentity = new PdfPCell(tableIdentity); // dont remove
            tableIdentity.ExtendLastRow = false;
            tableIdentity.SpacingBefore = 5f;
            document.Add(tableIdentity);

            #endregion

            document.Add(new Paragraph("Dengan Hormat,", normal_font) { Alignment = Element.ALIGN_LEFT });
            string firstParagraphString = "Yang bertanda tangan di bawah ini, PT. DAN LIRIS, SOLO (selanjutnya disebut sebagai pihak Pembeli) dan " + viewModel.supplier.name + "(selanjutnya disebut sebagai pihak Penjual) saling menyetujui untuk mengadaan kontrak jual beli dengan ketentuan sebagai berikut : ";
            Paragraph firstParagraph = new Paragraph(firstParagraphString, normal_font) { Alignment = Element.ALIGN_LEFT };
            
            firstParagraph.SpacingAfter = 10f;
            document.Add(firstParagraph);

            #region Items

            PdfPCell cellCenter = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 5 };
            PdfPCell cellRight = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 5 };
            PdfPCell cellLeft = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_LEFT, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 5 };

            PdfPTable tableContent = new PdfPTable(4);
            tableContent.SetWidths(new float[] { 7f, 3.5f, 4f, 4.5f });

            cellCenter.Phrase = new Phrase("NAMA DAN JENIS BARANG", bold_font);
            tableContent.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("JUMLAH", bold_font);
            tableContent.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("HARGA SATUAN", bold_font);
            tableContent.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("SUB TOTAL", bold_font);
            tableContent.AddCell(cellCenter);

            double total = 0;
            //for (int a = 0; a < 20; a++) // coba kalau banyak baris ^_^
            foreach (ExternalPurchaseOrderItemViewModel item in viewModel.items)
            {
                for (int indexItem = 0; indexItem < item.details.Count; indexItem++)
                {
                    ExternalPurchaseOrderDetailViewModel detail = item.details[indexItem];

                    string line1 = detail.product.name + "\n";
                    string line2 = detail.productRemark + "\n";
                    string line3 = item.prNo;

                    Paragraph p1 = new Paragraph();
                    Phrase ph1 = new Phrase(line1, normal_font);
                    Phrase ph2 = new Phrase(line2, normal_font);
                    Phrase ph3 = new Phrase(line3, normal_font);

                    p1.Add(ph1);
                    p1.Add(ph2);
                    p1.Add(ph3);

                    tableContent.AddCell(p1);

                    cellLeft.Phrase = new Phrase(string.Format("{0:n2}", detail.dealQuantity) +$"{detail.dealUom.unit}", normal_font);
                    tableContent.AddCell(cellLeft);

                    cellCenter.Phrase = new Phrase($"{viewModel.currency.code} {detail.pricePerDealUnit.ToString("N", new CultureInfo("id-ID"))}", normal_font);
                    tableContent.AddCell(cellCenter);

                    double subtotalPrice = detail.pricePerDealUnit * detail.dealQuantity;

                    cellCenter.Phrase = new Phrase($"{viewModel.currency.code} {subtotalPrice.ToString("N", new CultureInfo("id-ID"))}", normal_font);
                    tableContent.AddCell(cellCenter);

                    total += subtotalPrice;
                }
            }


            cellRight.Colspan = 3;
            cellRight.Phrase = new Phrase("Total", bold_font);
            tableContent.AddCell(cellRight);
            cellLeft.Phrase = new Phrase($"{viewModel.currency.code} {total.ToString("N", new CultureInfo("id-ID"))}", normal_font);
            tableContent.AddCell(cellLeft);

            cellRight.Colspan = 3;
            cellRight.Phrase = new Phrase("PPN 10%", bold_font);
            tableContent.AddCell(cellRight);
            string ppn = "IDR   0.00";
            double ppnNominal = 0;
            if (viewModel.useVat)
            {
                ppnNominal = total * 10 / 100;
                ppn = $"{viewModel.currency.code} {ppnNominal.ToString("N", new CultureInfo("id-ID"))}";
            }
            cellLeft.Phrase = new Phrase(ppn, normal_font);
            tableContent.AddCell(cellLeft);

            cellRight.Colspan = 3;
            cellRight.Phrase = new Phrase("Grand Total", bold_font);
            tableContent.AddCell(cellRight);
            cellLeft.Phrase = new Phrase($"{viewModel.currency.code} {(total+ ppnNominal).ToString("N", new CultureInfo("id-ID"))}", normal_font);
            tableContent.AddCell(cellLeft);

            PdfPCell cellContent = new PdfPCell(tableContent); // dont remove
            tableContent.ExtendLastRow = false;
            tableContent.SpacingAfter = 20f;
            document.Add(tableContent);

            #endregion

            #region Footer

            PdfPTable tableFooter = new PdfPTable(4);
            tableFooter.SetWidths(new float[] { 3.3f, 5.5f, 2f, 4.8f });

            //PdfPCell cellFooterContentLeft = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT };
            //PdfPCell cellFooterContentRight = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT };
            PdfPCell cellFooterContent = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT };

            

            cellFooterContent.Phrase = new Phrase("Ongkos Kirim", normal_font);
            tableFooter.AddCell(cellFooterContent);
            cellFooterContent.Phrase = new Phrase(": " + viewModel.freightCostBy, normal_font);
            tableFooter.AddCell(cellFooterContent);
            cellFooterContent.Phrase = new Phrase("Delivery", normal_font);
            tableFooter.AddCell(cellFooterContent);
            cellFooterContent.Phrase = new Phrase(": " + (viewModel.deliveryDate.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("dd MMMM yyyy", new CultureInfo("id-ID"))), normal_font);
            tableFooter.AddCell(cellFooterContent);

            string duedays = "";
            if (Convert.ToInt32( viewModel.paymentDueDays) > 0)
            {
                duedays = viewModel.paymentDueDays + " hari setelah terima barang";
            }

            cellFooterContent.Phrase = new Phrase("Pembayaran", normal_font);
            tableFooter.AddCell(cellFooterContent);
            cellFooterContent.Phrase = new Phrase(": " + viewModel.paymentMethod + "\n"+ "  "+ duedays, normal_font);
            tableFooter.AddCell(cellFooterContent);

            cellFooterContent.Phrase = new Phrase("Lain-lain", normal_font);
            tableFooter.AddCell(cellFooterContent);
            cellFooterContent.Phrase = new Phrase(": " + viewModel.remark, normal_font);
            tableFooter.AddCell(cellFooterContent);

            PdfPCell cellFooter = new PdfPCell(tableFooter); // dont remove
            tableFooter.ExtendLastRow = false;
            tableContent.SpacingAfter = 20f;
            document.Add(tableFooter);

            #endregion


            #region TableSignature

            PdfPTable tableSignature = new PdfPTable(2);

            PdfPCell cellSignatureContent = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER };
            cellSignatureContent.Phrase = new Phrase("Pembeli\n\n\n\n\n\n\n(  "+ viewModel.CreatedBy + "  )", bold_font);
            tableSignature.AddCell(cellSignatureContent);
            cellSignatureContent.Phrase = new Phrase("Penjual\n\n\n\n\n\n\n(  " + viewModel.supplier.name + "  )", bold_font);
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
