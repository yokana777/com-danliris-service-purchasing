﻿using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnpaidDispositionReport;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.PDFTemplates
{
    public static class UnpaidDispositionReportDetailPDFTemplate
    {
        private static readonly Font _headerFont = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 18);
        private static readonly Font _subHeaderFont = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 16);
        private static readonly Font _normalFont = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 9);
        private static readonly Font _smallFont = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 8);
        private static readonly Font _smallerFont = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 7);
        private static readonly Font _normalBoldFont = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 9);
        private static readonly Font _smallBoldFont = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 8);
        private static readonly Font _smallerBoldFont = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 7);
        private static readonly Font _smallerBoldWhiteFont = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 7, 0, BaseColor.White);

        public static MemoryStream Generate(UnpaidDispositionReportDetailViewModel viewModel, int timezoneOffset, DateTimeOffset? dateTo, bool isValas, bool isImport)
        {
            var date = (dateTo.HasValue ? dateTo.Value : DateTime.Now).ToUniversalTime();

            var document = new Document(PageSize.A4.Rotate(), 5, 5, 25, 25);
            var stream = new MemoryStream();
            PdfWriter.GetInstance(document, stream);
            document.Open();

            SetHeader(document, dateTo.GetValueOrDefault(), timezoneOffset, isValas, isImport);

            SetReportTable(document, viewModel, timezoneOffset, isValas, isImport);

            document.Close();
            byte[] byteInfo = stream.ToArray();
            stream.Write(byteInfo, 0, byteInfo.Length);
            stream.Position = 0;

            return stream;
        }

        private static PdfPCell GetCurrencySummaryTable(List<Summary> currencySummaries)
        {
            var table = new PdfPTable(2)
            {
                WidthPercentage = 100
            };

            var widths = new List<float>() { 1f, 2f };
            table.SetWidths(widths.ToArray());

            // set header
            var cellHeader = new PdfPCell()
            {
                BackgroundColor = new BaseColor(23, 50, 80),
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_CENTER
            };

            var cell = new PdfPCell()
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_CENTER
            };

            cellHeader.Phrase = new Phrase("Mata Uang", _smallerBoldWhiteFont);
            table.AddCell(cellHeader);

            cellHeader.Phrase = new Phrase("Total", _smallerBoldWhiteFont);
            table.AddCell(cellHeader);

            foreach (var currency in currencySummaries)
            {
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Phrase = new Phrase(currency.CurrencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Phrase = new Phrase(string.Format("{0:n}", currency.SubTotal), _smallerFont);
                table.AddCell(cell);

                //cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                //cell.Phrase = new Phrase(string.Format("{0:n}", currency.SubTotalCurrency), _smallerFont);
                //table.AddCell(cell);
            }

            return new PdfPCell(table) { Border = Rectangle.NO_BORDER };
        }

        //private static PdfPCell GetCategorySummaryTable(List<Summary> categorySummaries, decimal categorySummaryTotal)
        //{
        //    var table = new PdfPTable(2)
        //    {
        //        WidthPercentage = 100
        //    };

        //    var widths = new List<float>() { 1f, 2f };
        //    table.SetWidths(widths.ToArray());

        //    // set header
        //    var cell = new PdfPCell()
        //    {
        //        HorizontalAlignment = Element.ALIGN_CENTER,
        //        VerticalAlignment = Element.ALIGN_CENTER
        //    };

        //    cell.Phrase = new Phrase("Kategori", _smallerFont);
        //    table.AddCell(cell);

        //    cell.Phrase = new Phrase("Total (IDR)", _smallerFont);
        //    table.AddCell(cell);

        //    foreach (var categorySummary in categorySummaries)
        //    {
        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Phrase = new Phrase(categorySummary.Category, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", categorySummary.SubTotal), _smallerFont);
        //        table.AddCell(cell);
        //    }

        //    cell.Phrase = new Phrase("", _smallerFont);
        //    table.AddCell(cell);

        //    cell.Phrase = new Phrase(string.Format("{0:n}", categorySummaryTotal), _smallerFont);
        //    table.AddCell(cell);

        //    return new PdfPCell(table) { Border = Rectangle.NO_BORDER };
        //}

        private static PdfPCell GetUnitSummaryTable(List<Summary> unitSummaries)
        {
            var table = new PdfPTable(2)
            {
                WidthPercentage = 100
            };

            var widths = new List<float>() { 1f, 2f };
            table.SetWidths(widths.ToArray());

            // set header
            var cellHeader = new PdfPCell()
            {
                BackgroundColor = new BaseColor(23, 50, 80),
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_CENTER
            };

            var cell = new PdfPCell()
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_CENTER
            };

            var emptyCell = new PdfPCell()
            {
                Border = Rectangle.NO_BORDER
            };

            cellHeader.Phrase = new Phrase("Unit", _smallerBoldWhiteFont);
            table.AddCell(cellHeader);

            cellHeader.Phrase = new Phrase("Total (IDR)", _smallerBoldWhiteFont);
            table.AddCell(cellHeader);

            foreach (var unitSummary in unitSummaries)
            {
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Phrase = new Phrase(unitSummary.Name, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Phrase = new Phrase(string.Format("{0:n}", unitSummary.SubTotal), _smallerFont);
                table.AddCell(cell);
            }

            //cell.Phrase = new Phrase("", _smallerFont);
            //table.AddCell(cell);

            //cell.Phrase = new Phrase(string.Format("{0:n}", totalSummary), _smallerFont);
            //table.AddCell(cell);

            return new PdfPCell(table) { Border = Rectangle.NO_BORDER };
        }

        private static PdfPCell GetUnitSummaryValasTable(List<Summary> unitSummaries)
        {
            var table = new PdfPTable(3)
            {
                WidthPercentage = 100
            };

            var widths = new List<float>() { 2f, 1f, 2f };
            table.SetWidths(widths.ToArray());

            // set header
            var cellHeader = new PdfPCell()
            {
                BackgroundColor = new BaseColor(23, 50, 80),
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_CENTER
            };

            var cell = new PdfPCell()
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_CENTER
            };

            var emptyCell = new PdfPCell()
            {
                Border = Rectangle.NO_BORDER
            };

            cellHeader.Phrase = new Phrase("Unit", _smallerBoldWhiteFont);
            table.AddCell(cellHeader);

            cellHeader.Phrase = new Phrase("Currency", _smallerBoldWhiteFont);
            table.AddCell(cellHeader);

            cellHeader.Phrase = new Phrase("Total", _smallerBoldWhiteFont);
            table.AddCell(cellHeader);

            foreach (var unitSummary in unitSummaries)
            {
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Phrase = new Phrase(unitSummary.Name, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Phrase = new Phrase(unitSummary.CurrencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Phrase = new Phrase(string.Format("{0:n}", unitSummary.SubTotal), _smallerFont);
                table.AddCell(cell);
            }

            //cell.Phrase = new Phrase("", _smallerFont);
            //table.AddCell(cell);

            //cell.Phrase = new Phrase(string.Format("{0:n}", totalSummary), _smallerFont);
            //table.AddCell(cell);

            return new PdfPCell(table) { Border = Rectangle.NO_BORDER };
        }

        private static void SetReportTable(Document document, UnpaidDispositionReportDetailViewModel viewModel, int timezoneOffset, bool isValas, bool isImport)
        {
            var table = new PdfPTable(12)
            {
                WidthPercentage = 95
            };

            var widths = new List<float>();
            for (var i = 0; i < 12; i++)
            {
                if (i == 0 | i == 10)
                {
                    widths.Add(1f);
                    continue;
                }

                widths.Add(2f);
            }
            table.SetWidths(widths.ToArray());

            SetReportTableHeader(table);

            var grouppedByCategoryNames = viewModel.Reports.GroupBy(x => x.CategoryName).ToList();

            var cell = new PdfPCell()
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };

            var cellAlignRight = new PdfPCell()
            {
                HorizontalAlignment = Element.ALIGN_RIGHT,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };

            var cellColspan9 = new PdfPCell()
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Colspan = 9
            };


            int no = 1;
            foreach (var grouppedCategory in grouppedByCategoryNames)
            {
                var totalCurrencies = new Dictionary<string, decimal>();
                foreach (var data in grouppedCategory)
                {
                    cell.Phrase = new Phrase(no.ToString(), _smallerFont);
                    table.AddCell(cell);

                    cell.Phrase = new Phrase(data.DispositionDate.GetValueOrDefault().AddHours(timezoneOffset).ToString("dd-MM-yyyy"), _smallerFont);
                    table.AddCell(cell);

                    cell.Phrase = new Phrase(data.DispositionNo, _smallerFont);
                    table.AddCell(cell);

                    cell.Phrase = new Phrase(data.URNNo, _smallerFont);
                    table.AddCell(cell);

                    cell.Phrase = new Phrase(data.UPONo, _smallerFont);
                    table.AddCell(cell);

                    cell.Phrase = new Phrase(data.InvoiceNo, _smallerFont);
                    table.AddCell(cell);

                    cell.Phrase = new Phrase(data.SupplierName, _smallerFont);
                    table.AddCell(cell);

                    cell.Phrase = new Phrase(data.CategoryName, _smallerFont);
                    table.AddCell(cell);

                    cell.Phrase = new Phrase(data.AccountingUnitName, _smallerFont);
                    table.AddCell(cell);

                    cell.Phrase = new Phrase(data.PaymentDueDate.GetValueOrDefault().ToString("dd-MM-yyyy"), _smallerFont);
                    table.AddCell(cell);

                    cell.Phrase = new Phrase(data.CurrencyCode, _smallerFont);
                    table.AddCell(cell);

                    cellAlignRight.Phrase = new Phrase(string.Format("{0:n}", data.Total), _smallerFont);
                    table.AddCell(cellAlignRight);

                    // Currency summary
                    if (totalCurrencies.ContainsKey(data.CurrencyCode))
                    {
                        totalCurrencies[data.CurrencyCode] += data.Total;
                    }
                    else
                    {
                        totalCurrencies.Add(data.CurrencyCode, data.Total);
                    }

                    no++;
                }

                foreach(var totalCurrency in totalCurrencies)
                {
                    cellColspan9.Phrase = new Phrase();
                    table.AddCell(cellColspan9);

                    cell.Phrase = new Phrase("JUMLAH", _smallerBoldFont);
                    table.AddCell(cell);

                    cell.Phrase = new Phrase(totalCurrency.Key, _smallerBoldFont);
                    table.AddCell(cell);

                    cellAlignRight.Phrase = new Phrase(string.Format("{0:n}", totalCurrency.Value), _smallerBoldFont);
                    table.AddCell(cellAlignRight);
                }
            }

            document.Add(table);

            document.Add(new Paragraph("\n"));

            var summaryTable = new PdfPTable(4)
            {
                WidthPercentage = 95,

            };

            var widthSummaryTable = new List<float>() { 3f, 1f, 3f, 3f };
            summaryTable.SetWidths(widthSummaryTable.ToArray());

            if (isValas || isImport)
                summaryTable.AddCell(GetUnitSummaryValasTable(viewModel.UnitSummaries));
            else
                summaryTable.AddCell(GetUnitSummaryTable(viewModel.UnitSummaries));

            summaryTable.AddCell(new PdfPCell() { Border = Rectangle.NO_BORDER });
            summaryTable.AddCell(GetCurrencySummaryTable(viewModel.CurrencySummaries));
            summaryTable.AddCell(new PdfPCell() { Border = Rectangle.NO_BORDER });

            document.Add(summaryTable);
        }

        private static void SetReportTableHeader(PdfPTable table)
        {
            var cell = new PdfPCell()
            {
                BackgroundColor = new BaseColor(23, 50, 80),
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Rowspan = 2
            };

            cell.Phrase = new Phrase("No", _smallerBoldWhiteFont);
            table.AddCell(cell);

            cell.Phrase = new Phrase("Tgl Disposisi", _smallerBoldWhiteFont);
            table.AddCell(cell);

            cell.Phrase = new Phrase("No Disposisi", _smallerBoldWhiteFont);
            table.AddCell(cell);

            cell.Phrase = new Phrase("No SPB", _smallerBoldWhiteFont);
            table.AddCell(cell);

            cell.Phrase = new Phrase("No BP", _smallerBoldWhiteFont);
            table.AddCell(cell);

            cell.Phrase = new Phrase("No Invoice", _smallerBoldWhiteFont);
            table.AddCell(cell);

            cell.Phrase = new Phrase("Supplier", _smallerBoldWhiteFont);
            table.AddCell(cell);

            cell.Phrase = new Phrase("Kategori", _smallerBoldWhiteFont);
            table.AddCell(cell);

            cell.Phrase = new Phrase("Unit", _smallerBoldWhiteFont);
            table.AddCell(cell);

            cell.Phrase = new Phrase("Jatuh Tempo", _smallerBoldWhiteFont);
            table.AddCell(cell);

            cell.Phrase = new Phrase("Currency", _smallerBoldWhiteFont);
            table.AddCell(cell);

            cell.Phrase = new Phrase("Saldo", _smallerBoldWhiteFont);
            table.AddCell(cell);
        }

        private static void SetHeader(Document document, DateTimeOffset dateTo, int timezoneOffset, bool isValas, bool isImport)
        {
            var table = new PdfPTable(1)
            {
                WidthPercentage = 95
            };
            var cell = new PdfPCell()
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_LEFT,
                Phrase = new Phrase("PT DAN LIRIS", _headerFont)
            };
            table.AddCell(cell);

            var title = "LAPORAN DISPOSISI BELUM DIBAYAR LOKAL - DETAIL";

            if (isValas)
                title = "LAPORAN DISPOSISI BELUM DIBAYAR LOKAL VALAS - DETAIL";
            else if (isImport)
                title = "LAPORAN DISPOSISI BELUM DIBAYAR IMPORT - DETAIL";

            cell.Phrase = new Phrase(title, _headerFont);
            table.AddCell(cell);

            cell.Phrase = new Phrase($"Periode sampai {dateTo.AddHours(timezoneOffset):yyyy-dd-MM}", _subHeaderFont);
            table.AddCell(cell);

            cell.Phrase = new Phrase("", _headerFont);
            table.AddCell(cell);

            document.Add(table);
        }
    }
}