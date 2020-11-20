using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNote;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.PDFTemplates
{
    public static class DetailCreditBalanceReportPdfTemplate
    {
        private static readonly Font _headerFont = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 11);
        private static readonly Font _subHeaderFont = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 10);
        private static readonly Font _normalFont = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 9);
        private static readonly Font _smallFont = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 8);
        private static readonly Font _smallerFont = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 7);
        private static readonly Font _normalBoldFont = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 9);
        private static readonly Font _smallBoldFont = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 8);
        private static readonly Font _smallerBoldFont = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 7);
        private static readonly Font _smallerBoldWhiteFont = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 7, 0, BaseColor.White);

        public static MemoryStream Generate(DetailCreditBalanceReportViewModel viewModel, int timezoneOffset, DateTime? dateTo, bool isImport, bool isForeignCurrency)
        {
            //var d1 = dateFrom.GetValueOrDefault().ToUniversalTime();
            var d2 = (dateTo.HasValue ? dateTo.Value : DateTime.Now).ToUniversalTime();

            var document = new Document(PageSize.A4.Rotate(), 5, 5, 25, 25);
            var stream = new MemoryStream();
            PdfWriter.GetInstance(document, stream);
            document.Open();

            SetHeader(document, d2, timezoneOffset, isImport, isForeignCurrency);
            document.Add(new Paragraph("\n"));
            SetReportTable(document, viewModel, timezoneOffset);
            //document.Add(new Paragraph("\n"));
            //SetCategoryCurrencySummaryTable(document, viewModel.CategorySummaries, viewModel.CategorySummaryTotal, viewModel.CurrencySummaries);
            //SetFooter(document);

            document.Close();
            byte[] byteInfo = stream.ToArray();
            stream.Write(byteInfo, 0, byteInfo.Length);
            stream.Position = 0;

            return stream;
        }

        private static void SetHeader(Document document, DateTime dateTo, int timezoneOffset, bool isImport, bool isForeignCurrency)
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

            var sTitle = isImport ? "IMPOR" : isForeignCurrency ? "LOKAL VALAS" : "LOKAL";

            cell.Phrase = new Phrase($"LAPORAN SALDO HUTANG (DETAIL) {sTitle}", _headerFont);
            table.AddCell(cell);

            cell.Phrase = new Phrase($"Periode sampai {dateTo.AddHours(timezoneOffset):yyyy-dd-MM}", _subHeaderFont);
            table.AddCell(cell);

            document.Add(table);
        }

        private static void SetReportTableHeader(PdfPTable table)
        {
            var cell = new PdfPCell()
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                BackgroundColor = new BaseColor(23, 50, 80)
            };

            cell.Phrase = new Phrase("Tanggal SPB", _smallerBoldWhiteFont);
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

        private static void SetReportTable(Document document, DetailCreditBalanceReportViewModel viewModel, int timezoneOffset)
        {
            var table = new PdfPTable(10)
            {
                WidthPercentage = 95
            };
            table.SetWidths(new float[] { 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f });

            SetReportTableHeader(table);

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

            var cellAlignLeft = new PdfPCell()
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };

            var listReports = viewModel.Reports.OrderBy(order => order.AccountingUnitName).GroupBy(x => x.AccountingUnitName).ToList();
            var summaryUnit = new Dictionary<string, decimal>();

            foreach (var items in listReports)
            {
                //var accountingUnitName = cat.Select(x => x.AccountingUnitName).FirstOrDefault();
                //cellAlignLeft.Phrase = new Phrase(accountingUnitName, _smallerBoldFont);
                //cellAlignLeft.Colspan = 18;
                //table.AddCell(cellAlignLeft);

                var totalUnit = new Dictionary<string, decimal>();
                var totalCurrency = new Dictionary<string, Dictionary<string, decimal>>();
                decimal total = 0;

                foreach (var element in items)
                {
                    cell.Phrase = new Phrase(element.ReceiptDate.AddHours(timezoneOffset).ToString("yyyy-dd-MM"), _smallerFont);
                    table.AddCell(cell);

                    cell.Phrase = new Phrase(element.UPONo, _smallerFont);
                    table.AddCell(cell);

                    cell.Phrase = new Phrase(element.URNNo, _smallerFont);
                    table.AddCell(cell);

                    cell.Phrase = new Phrase(element.InvoiceNo, _smallerFont);
                    table.AddCell(cell);

                    cell.Phrase = new Phrase(element.SupplierName, _smallerFont);
                    table.AddCell(cell);

                    cell.Phrase = new Phrase(element.CategoryName, _smallerFont);
                    table.AddCell(cell);

                    cell.Phrase = new Phrase(element.AccountingUnitName, _smallerFont);
                    table.AddCell(cell);

                    cell.Phrase = new Phrase("", _smallerFont);
                    table.AddCell(cell);

                    cell.Phrase = new Phrase(element.CurrencyCode, _smallerFont);
                    table.AddCell(cell);

                    cellAlignRight.Phrase = new Phrase(string.Format("{0:n}", element.TotalSaldo), _smallerFont);
                    table.AddCell(cellAlignRight);

                    if (totalUnit.ContainsKey(element.AccountingUnitName))
                        totalUnit[element.AccountingUnitName] += element.TotalSaldo;
                    else
                        totalUnit.Add(element.AccountingUnitName, element.TotalSaldo);

                    if (totalCurrency.ContainsKey(element.CurrencyCode))
                    {

                        //totalCurrency[element.CurrencyCode]["DPP"] += element.TotalSaldo;
                        totalCurrency[element.CurrencyCode]["Total"] += element.TotalSaldo;
                    }
                    else
                    { 
                        totalCurrency.Add(element.CurrencyCode, new Dictionary<string, decimal>()
                        {
                            //{"DPP", element.TotalSaldo },
                            {"Total", element.TotalSaldo }
                        });
                    }

                    total += element.TotalSaldo;
                }

                if (totalCurrency.Count() > 0)
                    cellAlignRight.Phrase = new Phrase("JUMLAH", _smallerBoldFont);
                    cellAlignRight.Colspan = 8;
                    cellAlignRight.Rowspan = totalCurrency.Count();
                    table.AddCell(cellAlignRight);
                    cellAlignRight.Colspan = 1;
                    cellAlignRight.Rowspan = 1;
                    foreach (var v in totalCurrency)
                    {
                        cell.Phrase = new Phrase(v.Key, _smallerBoldFont);
                        table.AddCell(cell);

                        foreach (var x in v.Value)
                        {
                            cellAlignRight.Phrase = new Phrase(string.Format("{0:n}", x.Value), _smallerBoldFont);
                            table.AddCell(cellAlignRight);
                        }
                    }
                    //cellAlignRight.Phrase = new Phrase("", _smallerBoldFont);
                    //cellAlignRight.Colspan = 17;
                    //table.AddCell(cellAlignRight);
                    //cellAlignRight.Colspan = 1;

                    //cellAlignRight.Phrase = new Phrase(string.Format("{0:n}", total), _smallerBoldFont);
                    //table.AddCell(cellAlignRight);

                if (totalUnit.Count() > 0)
                    foreach (var v in totalUnit)
                    {
                        //cellAlignRight.Phrase = new Phrase(accountingUnitName, _smallerBoldFont);
                        //cellAlignRight.Colspan = 14;
                        //table.AddCell(cellAlignRight);
                        //cellAlignRight.Colspan = 1;

                    //    cell.Colspan = 1;
                    //    cell.Phrase = new Phrase(v.Key, _smallerBoldFont);
                    //    table.AddCell(cell);

                    //    cell.Phrase = new Phrase("", _smallerBoldFont);
                    //    table.AddCell(cell);

                    //    cell.Phrase = new Phrase("", _smallerBoldFont);
                    //    table.AddCell(cell);

                    //    cellAlignRight.Phrase = new Phrase(string.Format("{0:n}", v.Value), _smallerBoldFont);
                    //    table.AddCell(cellAlignRight);

                      if (summaryUnit.ContainsKey(v.Key))
                            summaryUnit[v.Key] += v.Value;
                        else
                            summaryUnit.Add(v.Key, v.Value);
                    }
            }

            document.Add(table);

            document.Add(new Paragraph("\n"));

            var summaryTable = new PdfPTable(3)
            {
                WidthPercentage = 95,

            };

            var widthSummaryTable = new List<float>() { 2f, 1f, 2f };
            summaryTable.SetWidths(widthSummaryTable.ToArray());

            //summaryTable.AddCell(GetCategorySummaryTable(viewModel.AccountingUnitSummaries, viewModel.AccountingUnitSummaryTotal));
            //summaryTable.AddCell(new PdfPCell() { Border = Rectangle.NO_BORDER });
            summaryTable.AddCell(GetUnitSummaryTable(summaryUnit));
            summaryTable.AddCell(new PdfPCell() { Border = Rectangle.NO_BORDER });
            summaryTable.AddCell(GetCurrencySummaryTable(viewModel.CurrencySummaries));

            document.Add(summaryTable);
        }

        //private static PdfPCell GetCategorySummaryTable(List<SummaryDCB> categorySummaries, decimal categorySummaryTotal)
        //{
        //    var table = new PdfPTable(2)
        //    {
        //        WidthPercentage = 100
        //    };

        //    var widths = new List<float>() { 2f, 3f };
        //    table.SetWidths(widths.ToArray());

        //    // set header
        //    var cell = new PdfPCell()
        //    {
        //        HorizontalAlignment = Element.ALIGN_CENTER,
        //        VerticalAlignment = Element.ALIGN_MIDDLE
        //    };

        //    cell.Phrase = new Phrase("Kategori", _smallerBoldFont);
        //    table.AddCell(cell);

        //    cell.Phrase = new Phrase("Total (IDR)", _smallerBoldFont);
        //    table.AddCell(cell);

        //    foreach (var accountingUnitSummary in categorySummaries)
        //    {
        //        cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //        cell.Phrase = new Phrase(accountingUnitSummary.AccountingUnitName, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", accountingUnitSummary.SubTotal), _smallerFont);
        //        table.AddCell(cell);
        //    }

        //    cell.Phrase = new Phrase("", _smallerFont);
        //    table.AddCell(cell);

        //    cell.Phrase = new Phrase(string.Format("{0:n}", categorySummaryTotal), _smallerFont);
        //    table.AddCell(cell);

        //    return new PdfPCell(table) { Border = Rectangle.NO_BORDER };
        //}

        private static PdfPCell GetUnitSummaryTable(Dictionary<string, decimal> unitSummaries)
        {
            var table = new PdfPTable(2)
            {
                WidthPercentage = 100
            };

            var widths = new List<float>() { 2f, 3f };
            table.SetWidths(widths.ToArray());

            // set header
            var cell = new PdfPCell()
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_CENTER
            };

            cell.Phrase = new Phrase("Unit", _smallerBoldFont);
            table.AddCell(cell);

            cell.Phrase = new Phrase("Total", _smallerBoldFont);
            table.AddCell(cell);

            decimal totalSummary = 0;
            foreach (var unitSummary in unitSummaries)
            {
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.Phrase = new Phrase(unitSummary.Key, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Phrase = new Phrase(string.Format("{0:n}", unitSummary.Value), _smallerFont);
                table.AddCell(cell);

                totalSummary += unitSummary.Value;
            }

            cell.Phrase = new Phrase("", _smallerFont);
            table.AddCell(cell);

            cell.Phrase = new Phrase(string.Format("{0:n}", totalSummary), _smallerFont);
            table.AddCell(cell);

            return new PdfPCell(table) { Border = Rectangle.NO_BORDER };
        }

        private static PdfPCell GetCurrencySummaryTable(List<SummaryDCB> currencySummaries)
        {
            var table = new PdfPTable(3)
            {
                WidthPercentage = 100
            };

            var widths = new List<float>() { 1f, 2f, 3f };
            table.SetWidths(widths.ToArray());

            // set header
            var cell = new PdfPCell()
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_CENTER
            };

            cell.Phrase = new Phrase("Mata Uang", _smallerBoldFont);
            table.AddCell(cell);

            cell.Phrase = new Phrase("Total", _smallerBoldFont);
            table.AddCell(cell);

            cell.Phrase = new Phrase("Total (IDR)", _smallerBoldFont);
            table.AddCell(cell);

            foreach (var currency in currencySummaries)
            {
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Phrase = new Phrase(currency.CurrencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Phrase = new Phrase(string.Format("{0:n}", currency.SubTotal), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Phrase = new Phrase(string.Format("{0:n}", currency.SubTotal), _smallerFont);
                table.AddCell(cell);
            }

            return new PdfPCell(table) { Border = Rectangle.NO_BORDER };
        }

        //private static void SetCategoryCurrencySummaryTable(Document document, List<SummaryDCB> categorySummaries, decimal categorySummaryTotal, List<SummaryDCB> currencySummaries)
        //{
        //    var table = new PdfPTable(3)
        //    {
        //        WidthPercentage = 95,

        //    };

        //    var widths = new List<float>() { 6f, 1f, 3f };
        //    table.SetWidths(widths.ToArray());

        //    table.AddCell(GetCategorySummaryTable(categorySummaries, categorySummaryTotal));
        //    table.AddCell(new PdfPCell() { Border = Rectangle.NO_BORDER });
        //    table.AddCell(GetCurrencySummaryTable(currencySummaries));

        //    document.Add(table);
        //}

        //private static void SetFooter(Document document)
        //{

        //}

    }
}
