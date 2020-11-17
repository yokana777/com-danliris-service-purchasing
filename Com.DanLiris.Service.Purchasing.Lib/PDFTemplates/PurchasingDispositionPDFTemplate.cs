using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.PurchasingDispositionViewModel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.PDFTemplates
{
    public class PurchasingDispositionPDFTemplate
    {
        public MemoryStream GeneratePdfTemplate(PurchasingDispositionViewModel viewModel, int clientTimeZoneOffset)
        {
            Font header_font = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 18);
            Font normal_font = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 9);
            Font small_font = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 8);
            Font smaller_font = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 7);
            Font bold_font = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 7);
            Font bold_font2 = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 8);
            Font bold_font3 = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 9);
            Font bold_font4 = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 10);

            PdfPCell cellLeftNoBorder = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT };
            PdfPCell cellCenterNoBorder = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER };
            PdfPCell cellCenterTopNoBorder = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_TOP };
            PdfPCell cellRightNoBorder = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT };
            PdfPCell cellJustifyNoBorder = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_JUSTIFIED };
            PdfPCell cellJustifyAllNoBorder = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_JUSTIFIED_ALL };

            PdfPCell cellCenter = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 5 };
            PdfPCell cellRight = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 5 };
            PdfPCell cellLeft = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_LEFT, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 5 };

            PdfPCell cellRightMerge = new PdfPCell() { Border = Rectangle.NO_BORDER | Rectangle.NO_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, VerticalAlignment = Element.ALIGN_TOP, Padding = 5 };
            PdfPCell cellLeftMerge = new PdfPCell() { Border = Rectangle.NO_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT, VerticalAlignment = Element.ALIGN_TOP, Padding = 5 };


            Document document = new Document(PageSize.A4, 30, 30, 30, 30);
            MemoryStream stream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, stream);
            document.Open();

            string fmString = "FM-PB-00-06-011";
            Paragraph fm = new Paragraph(fmString, bold_font4) { Alignment = Element.ALIGN_RIGHT };

            string titleString = "DISPOSISI PEMBAYARAN";
            Paragraph title = new Paragraph(titleString, bold_font4) { Alignment = Element.ALIGN_CENTER };

            document.Add(title);
            bold_font.SetStyle(Font.NORMAL);


            string NoString = "NO : " + viewModel.DispositionNo;
            Paragraph dispoNumber = new Paragraph(NoString, bold_font4) { Alignment = Element.ALIGN_CENTER };
            dispoNumber.SpacingAfter = 20f;
            document.Add(dispoNumber);



            #region Identity

            PdfPTable tableIdentity = new PdfPTable(5);
            tableIdentity.SetWidths(new float[] { 5f, 0.5f, 2f, 7f, 4f });

            double dpp = 0;
            foreach (var item in viewModel.Items)
            {
                foreach (var detail in item.Details)
                {
                    dpp += detail.PaidPrice;
                }
            }

            double ppn = (dpp * 0.1);
            string pph = "";
            double pphRate = 0;

            foreach (var item in viewModel.Items)
            {
                if (!item.UseVat)
                {
                    ppn = 0;
                }
                if (item.UseIncomeTax)
                {
                    pph = item.IncomeTax.name;
                    pphRate = dpp * (Convert.ToDouble(item.IncomeTax.rate) / 100);
                }
                break;
            }

            //Jumlah dibayar ke Supplier
            double paidToSupp = dpp + ppn - pphRate;
            if (viewModel.IncomeTaxBy == "Dan Liris")
            {
                paidToSupp = dpp + ppn;
            }

            double amount = dpp + ppn;

            if (viewModel.IncomeTaxBy == "Dan Liris")
            {
                amount = dpp + ppn + pphRate;
            }

            var payingDisposition = Math.Round((paidToSupp + viewModel.PaymentCorrection + pphRate), 2, MidpointRounding.AwayFromZero);
            cellLeftNoBorder.SetLeading(13f, 0f);
            cellLeftNoBorder.Phrase = new Phrase("Mohon Disposisi Pembayaran", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase(":", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase(viewModel.PaymentMethod + "  " + viewModel.Currency.code + " " + $"{payingDisposition.ToString("N", new CultureInfo("id-ID"))}", normal_font);
            cellLeftNoBorder.Colspan = 2;
            tableIdentity.AddCell(cellLeftNoBorder);
            //cellLeftNoBorder.Phrase = new Phrase( viewModel.Currency.code + " " +  $"{(paidToSupp + viewModel.PaymentCorrection + pphRate).ToString("N", new CultureInfo("id-ID")) }", normal_font);
            //tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase("", normal_font);
            cellLeftNoBorder.Colspan = 0;
            tableIdentity.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Phrase = new Phrase("Terbilang", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase(":", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase($"{ NumberToTextIDN.terbilang(payingDisposition) }" + " " + viewModel.Currency.description.ToLower(), normal_font);
            cellLeftNoBorder.Colspan = 2;
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase("", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Phrase = new Phrase("", normal_font);
            cellLeftNoBorder.Colspan = 4;
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase("", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Phrase = new Phrase("Perhitungan :", bold_font3);
            cellLeftNoBorder.Colspan = 4;
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase("", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);


            cellLeftNoBorder.Colspan = 0;
            cellLeftNoBorder.Phrase = new Phrase("Biaya", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase(":", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Colspan = 3;
            cellLeftNoBorder.Phrase = new Phrase(viewModel.Currency.code + "  " + $"{viewModel.DPP.ToString("N", new CultureInfo("id-ID")) }", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Colspan = 0;
            cellLeftNoBorder.Phrase = new Phrase("(PPn)", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase(":", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Colspan = 3;
            cellLeftNoBorder.Phrase = new Phrase(viewModel.Currency.code + "  " + $"{ppn.ToString("N", new CultureInfo("id-ID")) }", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Colspan = 0;
            cellLeftNoBorder.Phrase = new Phrase("Total", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase(":", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Colspan = 3;
            cellLeftNoBorder.Phrase = new Phrase(viewModel.Currency.code + "  " + $"{(dpp + ppn).ToString("N", new CultureInfo("id-ID")) }", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Colspan = 0;
            cellLeftNoBorder.Phrase = new Phrase("", normal_font);
            cellLeftNoBorder.Colspan = 4;
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase("", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Colspan = 0;
            cellLeftNoBorder.Phrase = new Phrase("", normal_font);
            cellLeftNoBorder.Colspan = 4;
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase("", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);

            var pphDanliris = pphRate;
            if (viewModel.IncomeTaxBy == "Dan Liris")
            {
                pphDanliris = 0;
            }

            cellLeftNoBorder.Colspan = 0;
            cellLeftNoBorder.Phrase = new Phrase("PPh pasal " + pph, normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase(":", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Colspan = 3;
            cellLeftNoBorder.Phrase = new Phrase(viewModel.Currency.code + "  " + $"{pphDanliris.ToString("N", new CultureInfo("id-ID")) }", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);



            cellLeftNoBorder.Colspan = 0;
            cellLeftNoBorder.Phrase = new Phrase("Jumlah dibayar ke Supplier ", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase(":", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Colspan = 2;
            cellLeftNoBorder.Phrase = new Phrase(viewModel.Currency.code + "  " + $"{(paidToSupp).ToString("N", new CultureInfo("id-ID")) }", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase("", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Colspan = 0;
            cellLeftNoBorder.Phrase = new Phrase("Koreksi Bayar", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase(":", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Colspan = 3;
            cellLeftNoBorder.Phrase = new Phrase(viewModel.Currency.code + "  " + $"{viewModel.PaymentCorrection.ToString("N", new CultureInfo("id-ID"))}", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Colspan = 0;
            cellLeftNoBorder.Phrase = new Phrase("", normal_font);
            cellLeftNoBorder.Colspan = 4;
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase("", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Colspan = 0;
            cellLeftNoBorder.Phrase = new Phrase("", normal_font);
            cellLeftNoBorder.Colspan = 4;
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase("", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);

            PdfPCell cellSuppLeft = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER, HorizontalAlignment = Element.ALIGN_LEFT, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 5 };
            PdfPCell cellSuppMid = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER, HorizontalAlignment = Element.ALIGN_LEFT, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 5 };
            PdfPCell cellSuppRight = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_LEFT, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 5 };

            cellSuppLeft.Phrase = new Phrase("Total dibayar ke Supplier", normal_font);
            tableIdentity.AddCell(cellSuppLeft);
            cellSuppMid.Phrase = new Phrase(":", normal_font);
            tableIdentity.AddCell(cellSuppMid);
            cellSuppRight.Colspan = 2;
            cellSuppRight.Phrase = new Phrase(viewModel.Currency.code + "  " + $"{(paidToSupp + viewModel.PaymentCorrection).ToString("N", new CultureInfo("id-ID"))}", normal_font);
            tableIdentity.AddCell(cellSuppRight);
            cellLeftNoBorder.Colspan = 0;
            cellLeftNoBorder.Phrase = new Phrase("", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Colspan = 0;
            cellLeftNoBorder.Phrase = new Phrase("", normal_font);
            cellLeftNoBorder.Colspan = 4;
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase("", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Colspan = 0;
            cellLeftNoBorder.Phrase = new Phrase("", normal_font);
            cellLeftNoBorder.Colspan = 4;
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase("", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Colspan = 0;
            cellLeftNoBorder.Phrase = new Phrase("Pembayaran ditransfer ke", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase(":", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase(viewModel.Bank, normal_font);
            cellLeftNoBorder.Colspan = 3;
            tableIdentity.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Colspan = 0;
            cellLeftNoBorder.Phrase = new Phrase("Dibayar ke Kas Negara", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase(":", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Colspan = 3;
            cellLeftNoBorder.Phrase = new Phrase(viewModel.Currency.code + "  " + $"{(pphRate).ToString("N", new CultureInfo("id-ID")) }", normal_font);
            tableIdentity.AddCell(cellLeftNoBorder);


            PdfPCell cellIdentity = new PdfPCell(tableIdentity);
            tableIdentity.ExtendLastRow = false;
            tableIdentity.SpacingAfter = 15f;
            document.Add(tableIdentity);
            #endregion

            #region Content
            PdfPTable tableContent = new PdfPTable(8);
            tableContent.SetWidths(new float[] { 6f, 5f, 4f, 3f, 3f, 2.5f, 2f, 3.5f });

            cellCenter.Phrase = new Phrase("Nama Barang", bold_font);
            tableContent.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("No PR", bold_font);
            tableContent.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("No PO Eksternal", bold_font);
            tableContent.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Unit", bold_font);
            tableContent.AddCell(cellCenter);
            //cellCenter.Phrase = new Phrase("Kategori", bold_font);
            //tableContent.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Quantity", bold_font);
            tableContent.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Satuan", bold_font);
            tableContent.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Harga Satuan", bold_font);
            cellCenter.Colspan = 2;
            tableContent.AddCell(cellCenter);
            //cellCenter.Phrase = new Phrase("Harga yang dibayar", bold_font);
            //cellCenter.Colspan = 2;
            //tableContent.AddCell(cellCenter);

            double total = 0;
            double totalPurchase = 0;
            
            foreach (PurchasingDispositionItemViewModel item in viewModel.Items)
            {
                for (int indexItem = 0; indexItem < item.Details.Count; indexItem++)
                {
                    PurchasingDispositionDetailViewModel detail = item.Details[indexItem];
                    //var unitName = detail.Unit._id == "50" ? "WEAVING" : detail.Unit.name;
                    var unitName = "";
                    var unitId = detail.Unit._id;
                    if (unitId == "50")
                    {
                        unitName = "WEAVING";
                    }
                    else if (unitId == "35")
                    {
                        unitName = "SPINNING 1";
                    }
                    else
                    {
                        unitName = detail.Unit.name ;
                    }
                    cellLeft.Colspan = 0;
                    cellLeft.Phrase = new Phrase($"{detail.Product.name}", smaller_font);
                    tableContent.AddCell(cellLeft);
                    cellCenter.Colspan = 0;
                    cellCenter.Phrase = new Phrase($"{detail.PRNo}", smaller_font);
                    tableContent.AddCell(cellCenter);

                    cellCenter.Phrase = new Phrase($"{item.EPONo}", smaller_font);
                    tableContent.AddCell(cellCenter);

                    cellCenter.Phrase = new Phrase($"{unitName}", smaller_font);
                    tableContent.AddCell(cellCenter);

                    //cellCenter.Phrase = new Phrase($"{detail.Category.name}", smaller_font);
                    //tableContent.AddCell(cellCenter);

                    cellCenter.Phrase = new Phrase(string.Format("{0:n2}", detail.PaidQuantity), smaller_font);
                    tableContent.AddCell(cellCenter);

                    cellCenter.Phrase = new Phrase($"{detail.DealUom.unit}", smaller_font);
                    tableContent.AddCell(cellCenter);

                    cellLeftMerge.Phrase = new Phrase($"{viewModel.Currency.code}", smaller_font);
                    tableContent.AddCell(cellLeftMerge);

                    cellRightMerge.Phrase = new Phrase($"{detail.PricePerDealUnit.ToString("N", new CultureInfo("id-ID"))}", smaller_font);
                    tableContent.AddCell(cellRightMerge);

                    double subtotalPrice = detail.PaidPrice * detail.PaidQuantity;

                    //cellLeftMerge.Phrase = new Phrase($"{viewModel.Currency.code}", smaller_font);
                    //tableContent.AddCell(cellLeftMerge);

                    //cellRightMerge.Phrase = new Phrase($"{detail.PaidPrice.ToString("N", new CultureInfo("id-ID"))}", smaller_font);
                    //tableContent.AddCell(cellRightMerge);

                    total += detail.PaidPrice;

                    totalPurchase += (detail.PricePerDealUnit * detail.DealQuantity);
                }
            }


            //cellRight.Colspan = 8;
            //cellRight.Phrase = new Phrase("Total Amount", bold_font);
            //tableContent.AddCell(cellRight);

            //cellLeftMerge.Phrase = new Phrase($"{viewModel.Currency.code}", smaller_font);
            //tableContent.AddCell(cellLeftMerge);
            //cellRightMerge.Phrase = new Phrase($"{total.ToString("N", new CultureInfo("id-ID"))}", smaller_font);
            //tableContent.AddCell(cellRightMerge);


            PdfPCell cellContent = new PdfPCell(tableContent); // dont remove
            tableContent.ExtendLastRow = false;
            tableContent.SpacingAfter = 20f;
            document.Add(tableContent);
            #endregion

            #region note

            PdfPTable tableNote = new PdfPTable(3);
            tableNote.SetWidths(new float[] { 4f, 0.5f, 11f });

            cellLeftNoBorder.Phrase = new Phrase("Note :", bold_font3);
            cellLeftNoBorder.Colspan = 4;
            tableNote.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Colspan = 0;
            cellLeftNoBorder.Phrase = new Phrase("Kategori", normal_font);
            tableNote.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase(":", normal_font);
            tableNote.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase(viewModel.Category.name, normal_font);
            tableNote.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Colspan = 0;
            cellLeftNoBorder.Phrase = new Phrase("Supplier / Agent", normal_font);
            tableNote.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase(":", normal_font);
            tableNote.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase(viewModel.Supplier.name, normal_font);
            tableNote.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Phrase = new Phrase("No Order Confirmation", normal_font);
            tableNote.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase(":", normal_font);
            tableNote.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase(viewModel.ConfirmationOrderNo, normal_font);
            tableNote.AddCell(cellLeftNoBorder);

            //cellLeftNoBorder.Phrase = new Phrase("No Invoice", normal_font);
            //tableNote.AddCell(cellLeftNoBorder);
            //cellLeftNoBorder.Phrase = new Phrase(":", normal_font);
            //tableNote.AddCell(cellLeftNoBorder);
            //cellLeftNoBorder.Phrase = new Phrase(viewModel.InvoiceNo, normal_font);
            //tableNote.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Phrase = new Phrase("No Proforma", normal_font);
            tableNote.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase(":", normal_font);
            tableNote.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase(viewModel.ProformaNo, normal_font);
            tableNote.AddCell(cellLeftNoBorder);

            //cellLeftNoBorder.Phrase = new Phrase("Investasi", normal_font);
            //tableNote.AddCell(cellLeftNoBorder);
            //cellLeftNoBorder.Phrase = new Phrase(":", normal_font);
            //tableNote.AddCell(cellLeftNoBorder);
            //cellLeftNoBorder.Phrase = new Phrase(viewModel.Investation, normal_font);
            //tableNote.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Phrase = new Phrase("Mohon dibayar Tanggal", normal_font);
            tableNote.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase(":", normal_font);
            tableNote.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase(viewModel.PaymentDueDate.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("dd MMMM yyyy", new CultureInfo("id-ID")), normal_font);
            tableNote.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Phrase = new Phrase("Perhitungan", normal_font);
            tableNote.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase(":", normal_font);
            tableNote.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase(viewModel.Calculation, normal_font);
            tableNote.AddCell(cellLeftNoBorder);

            cellLeftNoBorder.Phrase = new Phrase("Keterangan", normal_font);
            tableNote.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase(":", normal_font);
            tableNote.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase(viewModel.Remark, normal_font);
            tableNote.AddCell(cellLeftNoBorder);

            var ppnPurchase = viewModel.VatValue > 0 ? (totalPurchase * 10 / 100) : 0;


            cellLeftNoBorder.Phrase = new Phrase("Total Pembelian", normal_font);
            tableNote.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase(":", normal_font);
            tableNote.AddCell(cellLeftNoBorder);
            cellLeftNoBorder.Phrase = new Phrase($"{viewModel.Currency.code}" + " " + $"{(totalPurchase + ppnPurchase).ToString("N", new CultureInfo("id-ID"))}", normal_font);
            tableNote.AddCell(cellLeftNoBorder);

            PdfPCell cellNote = new PdfPCell(tableNote); // dont remove
            tableNote.ExtendLastRow = false;
            tableNote.SpacingAfter = 20f;
            document.Add(tableNote);
            #endregion

            #region signature
            PdfPTable tableSignature = new PdfPTable(4);
            tableSignature.SetWidths(new float[] { 4f, 4f, 4f, 4.1f });

            PdfPCell cellSignatureContent = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER };

            cellSignatureContent.Phrase = new Phrase("", bold_font3);
            tableSignature.AddCell(cellSignatureContent);
            cellSignatureContent.Phrase = new Phrase("", bold_font3);
            cellSignatureContent.Colspan = 2;
            tableSignature.AddCell(cellSignatureContent);

            cellSignatureContent.Colspan = 0;
            cellSignatureContent.Phrase = new Phrase("Sukoharjo, " + viewModel.CreatedUtc.ToString("dd MMMM yyyy", new CultureInfo("id-ID")), bold_font3);
            tableSignature.AddCell(cellSignatureContent);

            cellSignatureContent.Phrase = new Phrase("Menyetujui,", bold_font3);
            tableSignature.AddCell(cellSignatureContent);
            cellSignatureContent.Phrase = new Phrase("Mengetahui,", bold_font3);
            cellSignatureContent.Colspan = 2;
            tableSignature.AddCell(cellSignatureContent);

            cellSignatureContent.Colspan = 0;
            cellSignatureContent.Phrase = new Phrase("Hormat Kami,", bold_font3);
            tableSignature.AddCell(cellSignatureContent);

            cellSignatureContent.Phrase = new Phrase("\n\n\n\n\n\n\n(                                       )", bold_font3);
            tableSignature.AddCell(cellSignatureContent);
            cellSignatureContent.Phrase = new Phrase("\n\n\n\n\n\n\n(                                       )", bold_font3);
            tableSignature.AddCell(cellSignatureContent);
            cellSignatureContent.Phrase = new Phrase("\n\n\n\n\n\n\n(                                       )", bold_font3);
            tableSignature.AddCell(cellSignatureContent);
            cellSignatureContent.Phrase = new Phrase("\n\n\n\n\n\n\n(                                       )", bold_font3);
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
