using Com.DanLiris.Service.Purchasing.Lib.Enums;
using Com.DanLiris.Service.Purchasing.Lib.Facades.DebtAndDispositionSummary;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.BudgetCashflowService.PdfGenerator
{
    public class BudgetCashflowDivisionPdf : IBudgetCashflowDivisionPdf
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

        private readonly IBudgetCashflowService _budgetCashflowService;
        private readonly IdentityService _identityService;
        private readonly List<DivisionDto> _divisions;
        private readonly List<CurrencyDto> _currencies;
        private readonly List<UnitDto> _units;
        private List<UnitDto> _selectedUnits;
        private List<DivisionDto> _selectedDivisions;

        public BudgetCashflowDivisionPdf(IServiceProvider serviceProvider)
        {
            _budgetCashflowService = serviceProvider.GetService<IBudgetCashflowService>();
            _identityService = serviceProvider.GetService<IdentityService>();

            var cache = serviceProvider.GetService<IDistributedCache>();

            var jsonDivisions = cache.GetString(MemoryCacheConstant.Divisions);
            _divisions = JsonConvert.DeserializeObject<List<DivisionDto>>(jsonDivisions, new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            });

            var jsonUnits = cache.GetString(MemoryCacheConstant.Units);
            _units = JsonConvert.DeserializeObject<List<UnitDto>>(jsonUnits, new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            });

            var jsonCurrencies = cache.GetString(MemoryCacheConstant.Currencies);
            _currencies = JsonConvert.DeserializeObject<List<CurrencyDto>>(jsonCurrencies, new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            });
        }

        private List<BudgetCashflowDivisionDto> GetDivisionBudgetCashflow(int divisionId, DateTimeOffset dueDate)
        {
            var result = new List<BudgetCashflowDivisionDto>();
            foreach (BudgetCashflowCategoryLayoutOrder layoutOrder in Enum.GetValues(typeof(BudgetCashflowCategoryLayoutOrder)))
            {
                result.Add(_budgetCashflowService.GetBudgetCashflowDivision(layoutOrder, divisionId, dueDate));
            }

            return result;
        }

        public MemoryStream Generate(int divisionId, DateTimeOffset dueDate)
        {
            var document = new Document(PageSize.A4.Rotate(), 20, 20, 20, 20);
            MemoryStream stream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, stream);
            document.Open();

            var budgetCashflowDivisions = GetDivisionBudgetCashflow(divisionId, dueDate);
            var rowData = budgetCashflowDivisions.SelectMany(element => element.Items).ToList();

            var unitIds = budgetCashflowDivisions.SelectMany(element => element.UnitIds).Distinct().Where(element => element > 0).ToList();

            var divisionIds = _units.Where(element => unitIds.Contains(element.Id)).Select(element => element.DivisionId).Distinct().ToList();

            var lastColumn = 3 + 1 + (divisionIds.Count * 2) + (unitIds.Count * 2) + 1;

            SetTitle(document, divisionId, dueDate, lastColumn);
            //SetUnitTable(document, unit, oaci.Count, oaciTotal.Count, oaco.Count, oacoTotal.Count, oadiff.Count, iaci.Count, iaciTotal.Count, iaco.Count, iacoTotal.Count, iadiff.Count, faci.Count, faciTotal.Count, faco.Count, facoTotal.Count, fadiff.Count, oaci, oaciTotal, oaco, oacoTotal, oadiff, iaci, iaciTotal, iaco, iacoTotal, iadiff, faci, faciTotal, faco, facoTotal, fadiff, worstCases);
           
            document.Close();
            byte[] byteInfo = stream.ToArray();
            stream.Write(byteInfo, 0, byteInfo.Length);
            stream.Position = 0;

            return stream;
        }

        private void SetTitle(Document document, int divisionId, DateTimeOffset dueDate, int lastColumn)
        {
            var company = "PT DAN LIRIS";
            var title = "LAPORAN BUDGET CASHFLOW";
            var divisionName = "SEMUA DIVISI"; 
            
            var division = _divisions.FirstOrDefault(element => element.Id == divisionId);
            if (division != null)
                divisionName = $"DIVISI: {division.Name}";

            var date = $"PERIODE S.D. {dueDate.AddHours(_identityService.TimezoneOffset):dd/MM/yyyy}";

            var table = new PdfPTable(1)
            {
                WidthPercentage = 100,
                HorizontalAlignment = Element.ALIGN_LEFT
            };

            var cell = new PdfPCell()
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_LEFT,
                Phrase = new Phrase(company, _headerFont),
            };
            table.AddCell(cell);

            cell.Phrase = new Phrase(title, _headerFont);
            table.AddCell(cell);

            cell.Phrase = new Phrase(divisionName, _headerFont);
            table.AddCell(cell);

            cell.Phrase = new Phrase(date, _headerFont);
            table.AddCell(cell);

            cell.Phrase = new Phrase("", _headerFont);
            table.AddCell(cell);

            document.Add(table);
        }

        //private void SetUnitTable(Document document, UnitDto unit, int oaciCount, int oaciTotalCount, int oacoCount, int oacoTotalCount, int oadiffCount, int iaciCount, int iaciTotalCount, int iacoCount, int iacoTotalCount, int iadiffCount, int faciCount, int faciTotalCount, int facoCount, int facoTotalCount, int fadiffCount, List<BudgetCashflowItemDto> oaci, List<BudgetCashflowItemDto> oaciTotal, List<BudgetCashflowItemDto> oaco, List<BudgetCashflowItemDto> oacoTotal, List<BudgetCashflowItemDto> oadiff, List<BudgetCashflowItemDto> iaci, List<BudgetCashflowItemDto> iaciTotal, List<BudgetCashflowItemDto> iaco, List<BudgetCashflowItemDto> iacoTotal, List<BudgetCashflowItemDto> iadiff, List<BudgetCashflowItemDto> faci, List<BudgetCashflowItemDto> faciTotal, List<BudgetCashflowItemDto> faco, List<BudgetCashflowItemDto> facoTotal, List<BudgetCashflowItemDto> fadiff, List<BudgetCashflowItemDto> worstCases)
        //{
        //    var table = new PdfPTable(13)
        //    {
        //        WidthPercentage = 100
        //    };
        //    table.SetWidths(new float[] { 2f, 2f, 1f, 1f, 15f, 5f, 10f, 10f, 10f, 5f, 10f, 10f, 10f });

        //    var cell = new PdfPCell()
        //    {
        //        HorizontalAlignment = Element.ALIGN_CENTER,
        //        VerticalAlignment = Element.ALIGN_MIDDLE
        //    };

        //    var unitName = "";
        //    if (unit != null)
        //        unitName = unit.Name;

        //    var operatingActivitiesCashInRowCount = 2 + oaciCount + oaciTotalCount;
        //    var operatingActivitiesCashOutRowCount = 6 + oacoCount + oacoTotalCount;
        //    var operatingActivitiesRowsCount = 2 + oaciCount + oaciTotalCount + 6 + oacoCount + oacoTotalCount + oadiffCount;
        //    var investingActivitiesCashInRowCount = 1 + iaciCount + iaciTotalCount;
        //    var investingActivitiesCashOutRowCount = 1 + iacoCount + iacoTotalCount;
        //    var investingActivitiesRowsCount = 1 + iaciCount + iaciTotalCount + 1 + iacoCount + iacoTotalCount + iadiffCount;
        //    var financingActivitiesCashInRowsCount = 2 + faciCount + faciTotalCount;
        //    var financingActivitiesCashOutRowsCount = 3 + facoCount + facoTotalCount;
        //    var financingActivitiesRowsCount = 2 + faciCount + faciTotalCount + 3 + facoCount + facoTotalCount + fadiffCount;

        //    cell.BackgroundColor = new BaseColor(197, 90, 17);
        //    cell.Colspan = 5;
        //    cell.Rowspan = 3;
        //    cell.Phrase = new Phrase("KETERANGAN", _normalBoldWhiteFont);
        //    table.AddCell(cell);

        //    cell.BackgroundColor = new BaseColor(146, 208, 80);
        //    cell.Colspan = 3;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase("BEST CASE", _normalBoldFont);
        //    table.AddCell(cell);

        //    cell.BackgroundColor = new BaseColor(146, 208, 80);
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase("ACTUAL", _normalBoldFont);
        //    table.AddCell(cell);

        //    cell.BackgroundColor = new BaseColor(255, 0, 0);
        //    cell.Colspan = 3;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase("WORST CASE", _normalBoldFont);
        //    table.AddCell(cell);

        //    cell.BackgroundColor = new BaseColor(255, 0, 0);
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase("ACTUAL", _normalBoldFont);
        //    table.AddCell(cell);

        //    cell.BackgroundColor = new BaseColor(23, 50, 80);
        //    cell.Colspan = 3;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(unitName, _normalBoldWhiteFont);
        //    table.AddCell(cell);

        //    cell.BackgroundColor = new BaseColor(23, 50, 80);
        //    cell.Colspan = 1;
        //    cell.Rowspan = 2;
        //    cell.Phrase = new Phrase("Rp. (000)", _normalBoldWhiteFont);
        //    table.AddCell(cell);

        //    cell.BackgroundColor = new BaseColor(23, 50, 80);
        //    cell.Colspan = 3;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(unitName, _normalBoldWhiteFont);
        //    table.AddCell(cell);

        //    cell.BackgroundColor = new BaseColor(23, 50, 80);
        //    cell.Colspan = 1;
        //    cell.Rowspan = 2;
        //    cell.Phrase = new Phrase("Rp. (000)", _normalBoldWhiteFont);
        //    table.AddCell(cell);

        //    cell.BackgroundColor = new BaseColor(23, 50, 80);
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase("MATA UANG", _normalBoldWhiteFont);
        //    table.AddCell(cell);

        //    cell.BackgroundColor = new BaseColor(23, 50, 80);
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase("NOMINAL VALAS", _normalBoldWhiteFont);
        //    table.AddCell(cell);

        //    cell.BackgroundColor = new BaseColor(23, 50, 80);
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase("NOMINAL IDR", _normalBoldWhiteFont);
        //    table.AddCell(cell);

        //    cell.BackgroundColor = new BaseColor(23, 50, 80);
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase("MATA UANG", _normalBoldWhiteFont);
        //    table.AddCell(cell);

        //    cell.BackgroundColor = new BaseColor(23, 50, 80);
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase("NOMINAL VALAS", _normalBoldWhiteFont);
        //    table.AddCell(cell);

        //    cell.BackgroundColor = new BaseColor(23, 50, 80);
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase("NOMINAL IDR", _normalBoldWhiteFont);
        //    table.AddCell(cell);

        //    cell.BackgroundColor = new BaseColor(255, 255, 255);
        //    cell.Rotation = 90;
        //    cell.Colspan = 1;
        //    cell.Rowspan = operatingActivitiesRowsCount;
        //    cell.Phrase = new Phrase("OPERATING ACTIVITIES", _smallBoldFont);
        //    table.AddCell(cell);

        //    cell.Colspan = 1;
        //    cell.Rowspan = operatingActivitiesCashInRowCount;
        //    cell.Phrase = new Phrase("CASH IN", _smallBoldFont);
        //    table.AddCell(cell);

        //    cell.Rotation = 0;
        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    cell.Colspan = 11;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase("Revenue", _smallerBoldFont);
        //    table.AddCell(cell);

        //    var isOthersSales = false;
            
        //    foreach (var item in oaci)
        //    {
        //        if (item.LayoutOrder == BudgetCashflowCategoryLayoutOrder.OthersSales && !isOthersSales)
        //        {
        //            isOthersSales = true;

        //            cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //            cell.Colspan = 11;
        //            cell.Rowspan = 1;
        //            cell.Phrase = new Phrase("Revenue from other operating", _smallerBoldFont);
        //            table.AddCell(cell);
        //        }

        //        var worstCase = worstCases.FirstOrDefault(element => element.CurrencyId == item.CurrencyId && element.LayoutOrder == item.LayoutOrder);
        //        if (worstCase == null)
        //            worstCase = new BudgetCashflowItemDto();

        //        var currencyCode = "";
        //        var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
        //        if (currency != null)
        //            currencyCode = currency.Code;

        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase("", _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //        cell.Colspan = 2;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(item.LayoutOrder.ToDescriptionString(), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.CurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.Nominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.ActualNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", worstCase.CurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", worstCase.Nominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", worstCase.ActualNominal), _smallerFont);
        //        table.AddCell(cell);
        //    }

        //    foreach (var item in oaciTotal)
        //    {
        //        var currencyCode = "";
        //        var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
        //        if (currency != null)
        //            currencyCode = currency.Code;

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 3;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase("Total", _smallerBoldFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.BestCaseCurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.BestCaseNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.BestCaseActualNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.CurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.Nominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.ActualNominal), _smallerFont);
        //        table.AddCell(cell);
        //    }

        //    cell.Rotation = 90;
        //    cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //    cell.Colspan = 1;
        //    cell.Rowspan = operatingActivitiesCashOutRowCount;
        //    cell.Phrase = new Phrase("CASH OUT", _smallBoldFont);
        //    table.AddCell(cell);

        //    cell.Rotation = 0;
        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    cell.Colspan = 11;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase("Cost of Good Sold", _smallerBoldFont);
        //    table.AddCell(cell);

        //    var isMarketingSalaryCost = false;
        //    var isStillMarketingSalaryCost = false;
        //    var isGeneralAdministrativeExternalOutcomeVATCalculation = false;
        //    var isGeneralAdministrativeSalaryCost = false;
        //    var isOthersOperationalCost = false;

        //    foreach (var item in oaco)
        //    {
        //        if (item.LayoutOrder == BudgetCashflowCategoryLayoutOrder.MarketingSalaryCost && !isMarketingSalaryCost)
        //        {
        //            isMarketingSalaryCost = true;

        //            cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //            cell.Colspan = 11;
        //            cell.Rowspan = 1;
        //            cell.Phrase = new Phrase("Marketing Expenses", _smallerBoldFont);
        //            table.AddCell(cell);
        //        }

        //        if (item.LayoutOrder == BudgetCashflowCategoryLayoutOrder.MarketingSalaryCost && !isStillMarketingSalaryCost)
        //        {
        //            isStillMarketingSalaryCost = true;

        //            cell.Colspan = 1;
        //            cell.Rowspan = 1;
        //            cell.Phrase = new Phrase("", _smallerBoldFont);
        //            table.AddCell(cell);

        //            cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //            cell.Colspan = 10;
        //            cell.Rowspan = 1;
        //            cell.Phrase = new Phrase("Biaya Penjualan", _smallerBoldFont);
        //            table.AddCell(cell);
        //        }

        //        if (item.LayoutOrder == BudgetCashflowCategoryLayoutOrder.GeneralAdministrativeExternalOutcomeVATCalculation && !isGeneralAdministrativeExternalOutcomeVATCalculation)
        //        {
        //            isGeneralAdministrativeExternalOutcomeVATCalculation = true;

        //            cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //            cell.Colspan = 11;
        //            cell.Rowspan = 1;
        //            cell.Phrase = new Phrase("General & Administrative Expenses", _smallerBoldFont);
        //            table.AddCell(cell);
        //        }

        //        if (item.LayoutOrder == BudgetCashflowCategoryLayoutOrder.GeneralAdministrativeSalaryCost && !isGeneralAdministrativeSalaryCost)
        //        {
        //            isGeneralAdministrativeSalaryCost = true;

        //            cell.Colspan = 1;
        //            cell.Rowspan = 1;
        //            cell.Phrase = new Phrase("", _smallerBoldFont);
        //            table.AddCell(cell);

        //            cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //            cell.Colspan = 10;
        //            cell.Rowspan = 1;
        //            cell.Phrase = new Phrase("Biaya umum dan administrasi", _smallerBoldFont);
        //            table.AddCell(cell);
        //        }

        //        if (item.LayoutOrder == BudgetCashflowCategoryLayoutOrder.OthersOperationalCost && !isOthersOperationalCost)
        //        {
        //            isOthersOperationalCost = true;

        //            cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //            cell.Colspan = 11;
        //            cell.Rowspan = 1;
        //            cell.Phrase = new Phrase("Other Operating Expenses", _smallerBoldFont);
        //            table.AddCell(cell);
        //        }

        //        var worstCase = worstCases.FirstOrDefault(element => element.CurrencyId == item.CurrencyId && element.LayoutOrder == item.LayoutOrder);
        //        if (worstCase == null)
        //            worstCase = new BudgetCashflowItemDto();

        //        var currencyCode = "";
        //        var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
        //        if (currency != null)
        //            currencyCode = currency.Code;

        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase("", _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //        cell.Colspan = 2;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(item.LayoutOrder.ToDescriptionString(), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.CurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.Nominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.ActualNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", worstCase.CurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", worstCase.Nominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", worstCase.ActualNominal), _smallerFont);
        //        table.AddCell(cell);
        //    }

        //    foreach (var item in oacoTotal)
        //    {
        //        var currencyCode = "";
        //        var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
        //        if (currency != null)
        //            currencyCode = currency.Code;

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 3;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase("Total", _smallerBoldFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.BestCaseCurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.BestCaseNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.BestCaseActualNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.CurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.Nominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.ActualNominal), _smallerFont);
        //        table.AddCell(cell);
        //    }

        //    foreach (var item in oadiff)
        //    {
        //        var currencyCode = "";
        //        var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
        //        if (currency != null)
        //            currencyCode = currency.Code;

        //        cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //        cell.Colspan = 4;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase("Surplus/Deficit-Cash from Operating Activities", _smallerBoldFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.BestCaseCurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.BestCaseNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.BestCaseActualNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.CurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.Nominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.ActualNominal), _smallerFont);
        //        table.AddCell(cell);
        //    }

        //    cell.Rotation = 90;
        //    cell.Colspan = 1;
        //    cell.Rowspan = investingActivitiesRowsCount;
        //    cell.Phrase = new Phrase("INVESTING ACTIVITIES", _smallBoldFont);
        //    table.AddCell(cell);

        //    cell.Colspan = 1;
        //    cell.Rowspan = investingActivitiesCashInRowCount;
        //    cell.Phrase = new Phrase("CASH IN", _smallBoldFont);
        //    table.AddCell(cell);

        //    cell.Rotation = 0;
        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    cell.Colspan = 11;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(" ", _smallerFont);
        //    table.AddCell(cell);

        //    foreach (var item in iaci)
        //    {
        //        var worstCase = worstCases.FirstOrDefault(element => element.CurrencyId == item.CurrencyId && element.LayoutOrder == item.LayoutOrder);
        //        if (worstCase == null)
        //            worstCase = new BudgetCashflowItemDto();

        //        var currencyCode = "";
        //        var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
        //        if (currency != null)
        //            currencyCode = currency.Code;

        //        cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //        cell.Colspan = 3;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(item.LayoutOrder.ToDescriptionString(), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.CurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.Nominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.ActualNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", worstCase.CurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", worstCase.Nominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", worstCase.ActualNominal), _smallerFont);
        //        table.AddCell(cell);
        //    }

        //    foreach (var item in iaciTotal)
        //    {
        //        var currencyCode = "";
        //        var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
        //        if (currency != null)
        //            currencyCode = currency.Code;

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 3;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase("Total", _smallerBoldFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.BestCaseCurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.BestCaseNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.BestCaseActualNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.CurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.Nominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.ActualNominal), _smallerFont);
        //        table.AddCell(cell);
        //    }

        //    cell.Rotation = 90;
        //    cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //    cell.Colspan = 1;
        //    cell.Rowspan = investingActivitiesCashOutRowCount;
        //    cell.Phrase = new Phrase("CASH OUT", _smallBoldFont);
        //    table.AddCell(cell);

        //    cell.Rotation = 0;
        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    cell.Colspan = 11;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase("Pembayaran pembelian asset tetap :", _smallerBoldFont);
        //    table.AddCell(cell);

        //    foreach (var item in iaco)
        //    {
        //        var worstCase = worstCases.FirstOrDefault(element => element.CurrencyId == item.CurrencyId && element.LayoutOrder == item.LayoutOrder);
        //        if (worstCase == null)
        //            worstCase = new BudgetCashflowItemDto();

        //        var currencyCode = "";
        //        var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
        //        if (currency != null)
        //            currencyCode = currency.Code;

        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase("", _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //        cell.Colspan = 2;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(item.LayoutOrder.ToDescriptionString(), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.CurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.Nominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.ActualNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", worstCase.CurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", worstCase.Nominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", worstCase.ActualNominal), _smallerFont);
        //        table.AddCell(cell);
        //    }
            
        //    foreach (var item in iacoTotal)
        //    {
        //        var currencyCode = "";
        //        var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
        //        if (currency != null)
        //            currencyCode = currency.Code;

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 3;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase("Total", _smallerBoldFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.BestCaseCurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.BestCaseNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.BestCaseActualNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.CurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.Nominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.ActualNominal), _smallerFont);
        //        table.AddCell(cell);
        //    }
            
        //    foreach (var item in iadiff)
        //    {
        //        var currencyCode = "";
        //        var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
        //        if (currency != null)
        //            currencyCode = currency.Code;

        //        cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //        cell.Colspan = 4;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase("Surplus/Deficit-Cash from Investing Activities", _smallerBoldFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.BestCaseCurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.BestCaseNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.BestCaseActualNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.CurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.Nominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.ActualNominal), _smallerFont);
        //        table.AddCell(cell);
        //    }

        //    cell.Rotation = 90;
        //    cell.Colspan = 1;
        //    cell.Rowspan = financingActivitiesRowsCount;
        //    cell.Phrase = new Phrase("FINANCING ACTIVITIES", _smallBoldFont);
        //    table.AddCell(cell);

        //    cell.Colspan = 1;
        //    cell.Rowspan = financingActivitiesCashInRowsCount;
        //    cell.Phrase = new Phrase("CASH IN", _smallBoldFont);
        //    table.AddCell(cell);

        //    cell.Rotation = 0;
        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    cell.Colspan = 11;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(" ", _smallerFont);
        //    table.AddCell(cell);

        //    var isCashInAffiliates = false;
            
        //    foreach (var item in faci)
        //    {
        //        if (item.LayoutOrder == BudgetCashflowCategoryLayoutOrder.CashInAffiliates && !isCashInAffiliates)
        //        {
        //            isCashInAffiliates = true;

        //            cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //            cell.Colspan = 11;
        //            cell.Rowspan = 1;
        //            cell.Phrase = new Phrase("Others :", _smallerBoldFont);
        //            table.AddCell(cell);
        //        }

        //        var worstCase = worstCases.FirstOrDefault(element => element.CurrencyId == item.CurrencyId && element.LayoutOrder == item.LayoutOrder);
        //        if (worstCase == null)
        //            worstCase = new BudgetCashflowItemDto();

        //        var currencyCode = "";
        //        var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
        //        if (currency != null)
        //            currencyCode = currency.Code;

        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase("", _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //        cell.Colspan = 2;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(item.LayoutOrder.ToDescriptionString(), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.CurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.Nominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.ActualNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", worstCase.CurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", worstCase.Nominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", worstCase.ActualNominal), _smallerFont);
        //        table.AddCell(cell);
        //    }
            
        //    foreach (var item in faciTotal)
        //    {
        //        var currencyCode = "";
        //        var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
        //        if (currency != null)
        //            currencyCode = currency.Code;

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 3;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase("Total", _smallerBoldFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.BestCaseCurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.BestCaseNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.BestCaseActualNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.CurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.Nominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.ActualNominal), _smallerFont);
        //        table.AddCell(cell);
        //    }

        //    cell.Rotation = 90;
        //    cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //    cell.Colspan = 1;
        //    cell.Rowspan = financingActivitiesCashOutRowsCount;
        //    cell.Phrase = new Phrase("CASH OUT", _smallBoldFont);
        //    table.AddCell(cell);

        //    cell.Rotation = 0;
        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    cell.Colspan = 11;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase("Loan Installment and Interest expense", _smallerBoldFont);
        //    table.AddCell(cell);

        //    var isCashOutBankInterest = false;
        //    var isOthersWritten = false;

        //    foreach (var item in faco)
        //    {
        //        if (item.LayoutOrder == BudgetCashflowCategoryLayoutOrder.CashOutBankInterest && !isCashOutBankInterest)
        //        {
        //            isCashOutBankInterest = true;

        //            cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //            cell.Colspan = 11;
        //            cell.Rowspan = 1;
        //            cell.Phrase = new Phrase("Bank Expenses", _smallerBoldFont);
        //            table.AddCell(cell);
        //        }

        //        if (item.LayoutOrder == BudgetCashflowCategoryLayoutOrder.CashOutBankAdministrationFee && !isOthersWritten)
        //        {
        //            isOthersWritten = true;

        //            cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //            cell.Colspan = 11;
        //            cell.Rowspan = 1;
        //            cell.Phrase = new Phrase("Others :", _smallerBoldFont);
        //            table.AddCell(cell);
        //        }

        //        var worstCase = worstCases.FirstOrDefault(element => element.CurrencyId == item.CurrencyId && element.LayoutOrder == item.LayoutOrder);
        //        if (worstCase == null)
        //            worstCase = new BudgetCashflowItemDto();

        //        var currencyCode = "";
        //        var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
        //        if (currency != null)
        //            currencyCode = currency.Code;

        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase("", _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //        cell.Colspan = 2;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(item.LayoutOrder.ToDescriptionString(), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.CurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.Nominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.ActualNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", worstCase.CurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", worstCase.Nominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", worstCase.ActualNominal), _smallerFont);
        //        table.AddCell(cell);
        //    }
            
        //    foreach (var item in facoTotal)
        //    {
        //        var currencyCode = "";
        //        var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
        //        if (currency != null)
        //            currencyCode = currency.Code;

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 3;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase("Total", _smallerBoldFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.BestCaseCurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.BestCaseNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.BestCaseActualNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.CurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.Nominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.ActualNominal), _smallerFont);
        //        table.AddCell(cell);
        //    }

        //    foreach (var item in fadiff)
        //    {
        //        var currencyCode = "";
        //        var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
        //        if (currency != null)
        //            currencyCode = currency.Code;

        //        cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //        cell.Colspan = 4;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase("Surplus/Deficit-Cash from Financing Activities", _smallerBoldFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.BestCaseCurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.BestCaseNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.BestCaseActualNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(currencyCode, _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.CurrencyNominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.Nominal), _smallerFont);
        //        table.AddCell(cell);

        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        cell.Colspan = 1;
        //        cell.Rowspan = 1;
        //        cell.Phrase = new Phrase(string.Format("{0:n}", item.ActualNominal), _smallerFont);
        //        table.AddCell(cell);
        //    }

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 5;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase("BEGINNING BALANCE", _smallerBoldFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(" ", _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(" ", _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 5;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase("CASH SURPLUS/DEFICIT", _smallerBoldFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(" ", _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(" ", _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 5;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase("ENDING BALANCE", _smallerBoldFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(" ", _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(" ", _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    cell.Colspan = 2;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(" ", _smallerBoldFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    cell.Colspan = 3;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase("Kenyataan", _smallerBoldFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(" ", _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(" ", _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    cell.Colspan = 2;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(" ", _smallerBoldFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    cell.Colspan = 3;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase("Selisih", _smallerBoldFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(" ", _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(" ", _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    cell.Colspan = 2;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(" ", _smallerBoldFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    cell.Colspan = 3;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase("Rate", _smallerBoldFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //    cell.Colspan = 8;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(" ", _smallerBoldFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 5;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase("TOTAL SURPLUS (DEFISIT) EQUIVALENT", _smallerBoldFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(" ", _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(" ", _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    cell.Colspan = 1;
        //    cell.Rowspan = 1;
        //    cell.Phrase = new Phrase(string.Format("{0:n}", 0), _smallerFont);
        //    table.AddCell(cell);

        //    document.Add(table);
        //}

    }
}
