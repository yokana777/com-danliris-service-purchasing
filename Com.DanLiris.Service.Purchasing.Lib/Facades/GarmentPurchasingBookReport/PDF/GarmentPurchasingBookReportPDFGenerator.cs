using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchasingBookReport.PDF
{
    public static class GarmentPurchasingBookReportPDFGenerator
    {

        private static readonly Font _headerFont = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 11);
        private static readonly Font _subHeaderFont = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 10);
        private static readonly Font _normalFont = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 9);
        private static readonly Font _smallFont = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 8);
        private static readonly Font _smallerFont = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 7);
        private static readonly Font _normalBoldFont = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 9);
        private static readonly Font _normalBoldWhiteFont = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 9, 0, BaseColor.White);
        private static readonly Font _smallBoldFont = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 8);
        private static readonly Font _smallerBoldFont = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 7);
        private static readonly Font _smallerBoldWhiteFont = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 7, 0, BaseColor.White);

        public static MemoryStream Generate(ReportDto report, DateTimeOffset startDate, DateTimeOffset endDate, bool isForeignCurrency, bool isImportSupplier, int timezoneOffset)
        {
            var document = new Document(PageSize.A4.Rotate(), 20, 20, 20, 20);
            var stream = new MemoryStream();
            var writer = PdfWriter.GetInstance(document, stream);
            document.Open();

            SetTitle(document, startDate, endDate, isForeignCurrency, isImportSupplier, timezoneOffset);
            SetTable(document, report, isForeignCurrency, isImportSupplier, timezoneOffset);
            SetCategory(document, report.Categories);
            SetCurrency(document, report.Currencies, isForeignCurrency, isImportSupplier);
            document.Close();
            byte[] byteInfo = stream.ToArray();
            stream.Write(byteInfo, 0, byteInfo.Length);
            stream.Position = 0;

            return stream;
        }

        private static void SetCurrency(Document document, List<ReportCurrencyDto> currencies, bool isForeignCurrency, bool isImportSupplier)
        {
            if (isForeignCurrency || isImportSupplier)
            {
                var table = new PdfPTable(3)
                {
                    WidthPercentage = 100,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };

                var cellCenter = new PdfPCell()
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_CENTER
                };

                var cellLeft = new PdfPCell()
                {
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_CENTER
                };

                var cellRight = new PdfPCell()
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    VerticalAlignment = Element.ALIGN_CENTER
                };

                cellCenter.Phrase = new Phrase("Mata Uang", _subHeaderFont);
                table.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase("Total", _subHeaderFont);
                table.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase("Total (IDR)", _subHeaderFont);
                table.AddCell(cellCenter);

                foreach (var currency in currencies)
                {
                    cellLeft.Phrase = new Phrase(currency.CurrencyCode, _normalFont);
                    table.AddCell(cellLeft);
                    cellRight.Phrase = new Phrase(currency.CurrencyAmount.ToString(), _normalFont);
                    table.AddCell(cellRight);
                    cellRight.Phrase = new Phrase(currency.Amount.ToString(), _normalFont);
                    table.AddCell(cellRight);
                }

                document.Add(table);

                document.Add(new Paragraph("\n"));
            }
            else
            {
                var table = new PdfPTable(2)
                {
                    WidthPercentage = 100,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };

                var cellCenter = new PdfPCell()
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_CENTER
                };

                var cellLeft = new PdfPCell()
                {
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_CENTER
                };

                var cellRight = new PdfPCell()
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    VerticalAlignment = Element.ALIGN_CENTER
                };

                cellCenter.Phrase = new Phrase("Mata Uang", _subHeaderFont);
                table.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase("Total (IDR)", _subHeaderFont);
                table.AddCell(cellCenter);

                foreach (var currency in currencies)
                {
                    cellLeft.Phrase = new Phrase(currency.CurrencyCode, _normalFont);
                    table.AddCell(cellLeft);
                    cellRight.Phrase = new Phrase(currency.Amount.ToString(), _normalFont);
                    table.AddCell(cellRight);
                }

                document.Add(table);

                document.Add(new Paragraph("\n"));
            }
        }

        private static void SetCategory(Document document, List<ReportCategoryDto> categories)
        {
            var table = new PdfPTable(2)
            {
                WidthPercentage = 100,
                HorizontalAlignment = Element.ALIGN_LEFT
            };

            var cellCenter = new PdfPCell()
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_CENTER
            };

            var cellLeft = new PdfPCell()
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                VerticalAlignment = Element.ALIGN_CENTER
            };

            var cellRight = new PdfPCell()
            {
                HorizontalAlignment = Element.ALIGN_RIGHT,
                VerticalAlignment = Element.ALIGN_CENTER
            };

            cellCenter.Phrase = new Phrase("Kategori", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Total (IDR)", _subHeaderFont);
            table.AddCell(cellCenter);

            foreach (var category in categories)
            {
                cellLeft.Phrase = new Phrase(category.CategoryName, _normalFont);
                table.AddCell(cellLeft);
                cellRight.Phrase = new Phrase(category.Amount.ToString(), _normalFont);
                table.AddCell(cellRight);
            }

            document.Add(table);

            document.Add(new Paragraph("\n"));
        }

        private static void SetTable(Document document, ReportDto report, bool isForeignCurrency, bool isImportSupplier, int timezoneOffset)
        {
            if (!isForeignCurrency && !isImportSupplier)
            {
                SetTableLocal(document, report, timezoneOffset);
            }

            if (isForeignCurrency)
            {
                SetTableLocalForeignCurrency(document, report, timezoneOffset);
            }

            if (isImportSupplier)
            {
                SetTableImport(document, report, timezoneOffset);
            }

        }

        private static void SetTableImport(Document document, ReportDto report, int timezoneOffset)
        {
            var table = new PdfPTable(19)
            {
                WidthPercentage = 100,
                HorizontalAlignment = Element.ALIGN_LEFT
            };

            var cellCenter = new PdfPCell()
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_CENTER
            };

            var cellLeft = new PdfPCell()
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                VerticalAlignment = Element.ALIGN_CENTER
            };

            var cellRight = new PdfPCell()
            {
                HorizontalAlignment = Element.ALIGN_RIGHT,
                VerticalAlignment = Element.ALIGN_CENTER
            };

            cellCenter.Rowspan = 2;
            cellCenter.Phrase = new Phrase("Tanggal Bon", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Supplier", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Keterangan", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("No. Surat Jalan", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("No. BP Besar", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("No. BP Kecil", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("No. Invoice", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("No. Faktur Pajak", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("No. NI", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Kategori Pembelian", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Kategori Pembukuan", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Rowspan = 1;
            cellCenter.Colspan = 4;
            cellCenter.Phrase = new Phrase("Bea Cukai", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Colspan = 3;
            cellCenter.Phrase = new Phrase("Pembelian", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Rowspan = 2;
            cellCenter.Colspan = 1;
            cellCenter.Phrase = new Phrase("Total (IDR)", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Rowspan = 1;
            cellCenter.Colspan = 1;
            cellCenter.Phrase = new Phrase("Tanggal BC", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("No. BC", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Jenis BC", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Ket Nilai Impor", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Mata Uang", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("DPP Valas", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Rate", _subHeaderFont);
            table.AddCell(cellCenter);

            foreach (var item in report.Data)
            {
                cellCenter.Rowspan = 1;
                cellCenter.Colspan = 1;
                cellCenter.Phrase = new Phrase(item.CustomsArrivalDate.AddHours(timezoneOffset).ToString("dd/MMMM/yyyy"), _normalFont);
                table.AddCell(cellCenter);
                cellLeft.Phrase = new Phrase(item.SupplierName, _normalFont);
                table.AddCell(cellLeft);
                cellLeft.Phrase = new Phrase(item.ProductName, _normalFont);
                table.AddCell(cellLeft);
                cellCenter.Phrase = new Phrase(item.GarmentDeliveryOrderNo, _normalFont);
                table.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase(item.BillNo, _normalFont);
                table.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase(item.PaymentBill, _normalFont);
                table.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase(item.InvoiceNo, _normalFont);
                table.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase(item.VATNo, _normalFont);
                table.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase(item.InternalNoteNo, _normalFont);
                table.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase(item.PurchasingCategoryName, _normalFont);
                table.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase(item.AccountingCategoryName, _normalFont);
                table.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase(item.CustomsDate.AddHours(timezoneOffset).ToString("dd/MMMM/yyyy"), _normalFont);
                table.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase(item.CustomsNo, _normalFont);
                table.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase(item.CustomsType, _normalFont);
                table.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase(item.ImportValueRemark, _normalFont);
                table.AddCell(cellCenter);
                cellRight.Phrase = new Phrase(item.CurrencyCode, _normalFont);
                table.AddCell(cellRight);
                cellRight.Phrase = new Phrase(item.CurrencyDPPAmount.ToString(), _normalFont);
                table.AddCell(cellRight);
                cellRight.Phrase = new Phrase(item.CurrencyRate.ToString(), _normalFont);
                table.AddCell(cellRight);
                cellRight.Phrase = new Phrase(item.Total.ToString(), _normalFont);
                table.AddCell(cellRight);
            }

            document.Add(table);

            document.Add(new Paragraph("\n"));
        }

        private static void SetTableLocalForeignCurrency(Document document, ReportDto report, int timezoneOffset)
        {
            var table = new PdfPTable(19)
            {
                WidthPercentage = 100,
                HorizontalAlignment = Element.ALIGN_LEFT
            };

            var cellCenter = new PdfPCell()
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_CENTER
            };

            var cellLeft = new PdfPCell()
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                VerticalAlignment = Element.ALIGN_CENTER
            };

            var cellRight = new PdfPCell()
            {
                HorizontalAlignment = Element.ALIGN_RIGHT,
                VerticalAlignment = Element.ALIGN_CENTER
            };

            cellCenter.Rowspan = 2;
            cellCenter.Phrase = new Phrase("Tanggal Bon", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Supplier", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Keterangan", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("No. Surat Jalan", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("No. BP Besar", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("No. BP Kecil", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("No. Invoice", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("No. Faktur Pajak", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("No. NI", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Kategori Pembelian", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Kategori Pembukuan", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Kuantitas", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Mata Uang", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Kurs", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Rowspan = 1;
            cellCenter.Colspan = 4;
            cellCenter.Phrase = new Phrase("Pembelian", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Rowspan = 2;
            cellCenter.Colspan = 1;
            cellCenter.Phrase = new Phrase("Total (IDR)", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Rowspan = 1;
            cellCenter.Colspan = 1;
            cellCenter.Phrase = new Phrase("DPP Valas", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("DPP (IDR)", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("PPN (IDR)", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("PPh (IDR)", _subHeaderFont);
            table.AddCell(cellCenter);

            foreach (var item in report.Data)
            {
                cellCenter.Rowspan = 1;
                cellCenter.Colspan = 1;
                cellCenter.Phrase = new Phrase(item.CustomsArrivalDate.AddHours(timezoneOffset).ToString("dd/MMMM/yyyy"), _normalFont);
                table.AddCell(cellCenter);
                cellLeft.Phrase = new Phrase(item.SupplierName, _normalFont);
                table.AddCell(cellLeft);
                cellLeft.Phrase = new Phrase(item.ProductName, _normalFont);
                table.AddCell(cellLeft);
                cellCenter.Phrase = new Phrase(item.GarmentDeliveryOrderNo, _normalFont);
                table.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase(item.BillNo, _normalFont);
                table.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase(item.PaymentBill, _normalFont);
                table.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase(item.InvoiceNo, _normalFont);
                table.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase(item.VATNo, _normalFont);
                table.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase(item.InternalNoteNo, _normalFont);
                table.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase(item.PurchasingCategoryName, _normalFont);
                table.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase(item.AccountingCategoryName, _normalFont);
                table.AddCell(cellCenter);
                cellRight.Phrase = new Phrase(item.InternalNoteQuantity.ToString(), _normalFont);
                table.AddCell(cellRight);
                cellRight.Phrase = new Phrase(item.CurrencyCode, _normalFont);
                table.AddCell(cellRight);
                cellRight.Phrase = new Phrase(item.CurrencyRate.ToString(), _normalFont);
                table.AddCell(cellRight);
                cellRight.Phrase = new Phrase(item.CurrencyDPPAmount.ToString(), _normalFont);
                table.AddCell(cellRight);
                cellRight.Phrase = new Phrase(item.DPPAmount.ToString(), _normalFont);
                table.AddCell(cellRight);
                cellRight.Phrase = new Phrase(item.VATAmount.ToString(), _normalFont);
                table.AddCell(cellRight);
                cellRight.Phrase = new Phrase(item.IncomeTaxAmount.ToString(), _normalFont);
                table.AddCell(cellRight);
                cellRight.Phrase = new Phrase(item.Total.ToString(), _normalFont);
                table.AddCell(cellRight);
            }

            document.Add(table);

            document.Add(new Paragraph("\n"));
        }

        private static void SetTableLocal(Document document, ReportDto report, int timezoneOffset)
        {
            var table = new PdfPTable(17)
            {
                WidthPercentage = 100,
                HorizontalAlignment = Element.ALIGN_LEFT
            };

            var cellCenter = new PdfPCell()
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_CENTER
            };

            var cellLeft = new PdfPCell()
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                VerticalAlignment = Element.ALIGN_CENTER
            };

            var cellRight = new PdfPCell()
            {
                HorizontalAlignment = Element.ALIGN_RIGHT,
                VerticalAlignment = Element.ALIGN_CENTER
            };

            cellCenter.Rowspan = 2;
            cellCenter.Phrase = new Phrase("Tanggal Bon", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Supplier", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Keterangan", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("No. Surat Jalan", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("No. BP Besar", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("No. BP Kecil", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("No. Invoice", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("No. Faktur Pajak", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("No. NI", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Kategori Pembelian", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Kategori Pembukuan", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Kuantitas", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Mata Uang", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Rowspan = 1;
            cellCenter.Colspan = 3;
            cellCenter.Phrase = new Phrase("Pembelian", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Rowspan = 2;
            cellCenter.Colspan = 1;
            cellCenter.Phrase = new Phrase("Total (IDR)", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Rowspan = 1;
            cellCenter.Colspan = 1;
            cellCenter.Phrase = new Phrase("DPP", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("PPN", _subHeaderFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("PPh", _subHeaderFont);
            table.AddCell(cellCenter);

            foreach (var item in report.Data)
            {
                cellCenter.Rowspan = 1;
                cellCenter.Colspan = 1;
                cellCenter.Phrase = new Phrase(item.CustomsArrivalDate.AddHours(timezoneOffset).ToString("dd/MMMM/yyyy"), _normalFont);
                table.AddCell(cellCenter);
                cellLeft.Phrase = new Phrase(item.SupplierName, _normalFont);
                table.AddCell(cellLeft);
                cellLeft.Phrase = new Phrase(item.ProductName, _normalFont);
                table.AddCell(cellLeft);
                cellCenter.Phrase = new Phrase(item.GarmentDeliveryOrderNo, _normalFont);
                table.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase(item.BillNo, _normalFont);
                table.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase(item.PaymentBill, _normalFont);
                table.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase(item.InvoiceNo, _normalFont);
                table.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase(item.VATNo, _normalFont);
                table.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase(item.InternalNoteNo, _normalFont);
                table.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase(item.PurchasingCategoryName, _normalFont);
                table.AddCell(cellCenter);
                cellCenter.Phrase = new Phrase(item.AccountingCategoryName, _normalFont);
                table.AddCell(cellCenter);
                cellRight.Phrase = new Phrase(item.InternalNoteQuantity.ToString(), _normalFont);
                table.AddCell(cellRight);
                cellRight.Phrase = new Phrase(item.CurrencyCode, _normalFont);
                table.AddCell(cellRight);
                cellRight.Phrase = new Phrase(item.DPPAmount.ToString(), _normalFont);
                table.AddCell(cellRight);
                cellRight.Phrase = new Phrase(item.VATAmount.ToString(), _normalFont);
                table.AddCell(cellRight);
                cellRight.Phrase = new Phrase(item.IncomeTaxAmount.ToString(), _normalFont);
                table.AddCell(cellRight);
                cellRight.Phrase = new Phrase(item.Total.ToString(), _normalFont);
                table.AddCell(cellRight);
            }

            document.Add(table);

            document.Add(new Paragraph("\n"));
        }

        private static void SetTitle(Document document, DateTimeOffset startDate, DateTimeOffset endDate, bool isForeignCurrency, bool isImportSupplier, int timezoneOffset)
        {
            var title = "LAPORAN BUKU PEMBELIAN LOKAL";
            if (isForeignCurrency)
                title = "LAPORAN BUKU PEMBELIAN LOKAL VALAS";

            if (isImportSupplier)
                title = "LAPORAN BUKU PEMBELIAN IMPOR";

            var start = startDate.AddHours(timezoneOffset).ToString("dd/MMMM/yyyy");
            var end = endDate.AddHours(timezoneOffset).ToString("dd/MMMM/yyyy");

            var table = new PdfPTable(1)
            {
                WidthPercentage = 100,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            table.SetWidths(new float[] { 1f });

            var cellCenter = new PdfPCell()
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_CENTER
            };

            var cellLeft = new PdfPCell()
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_LEFT,
                VerticalAlignment = Element.ALIGN_CENTER
            };

            cellCenter.Phrase = new Phrase(title, _headerFont);
            table.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase($"Periode {start} sampai {end}", _headerFont);
            table.AddCell(cellCenter);

            document.Add(table);
            document.Add(new Paragraph("\n"));
        }
    }
}
