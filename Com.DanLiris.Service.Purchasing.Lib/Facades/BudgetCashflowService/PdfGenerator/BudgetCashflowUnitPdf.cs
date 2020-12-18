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
    public class BudgetCashflowUnitPdf : IBudgetCashflowUnitPdf
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

        private readonly IBudgetCashflowService _budgetCashflowService;
        private readonly IdentityService _identityService;
        private readonly List<UnitDto> _units;
        private readonly List<CurrencyDto> _currencies;

        public BudgetCashflowUnitPdf(IServiceProvider serviceProvider)
        {
            _budgetCashflowService = serviceProvider.GetService<IBudgetCashflowService>();
            _identityService = serviceProvider.GetService<IdentityService>();

            var cache = serviceProvider.GetService<IDistributedCache>();

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

        private List<BudgetCashflowItemDto> GetOperatingActivitiesCashIn(int unitId, DateTimeOffset dueDate)
        {
            var result = new List<BudgetCashflowItemDto>();
            foreach (BudgetCashflowCategoryLayoutOrder layoutOrder in Enum.GetValues(typeof(BudgetCashflowCategoryLayoutOrder)))
            {
                if (layoutOrder <= BudgetCashflowCategoryLayoutOrder.ExternalIncomeVATCalculation)
                    result.AddRange(_budgetCashflowService.GetBudgetCashflowUnit(layoutOrder, unitId, dueDate));
                else
                    break;
            }

            return result.OrderBy(element => element.LayoutOrder).ToList();
        }

        private List<BudgetCashflowItemDto> GetOperatingActivitiesCashInTotal(int unitId, DateTimeOffset dueDate)
        {
            var result = new List<BudgetCashflowItemDto>();
            result.AddRange(_budgetCashflowService.GetCashInOperatingActivitiesByUnit(unitId, dueDate));

            return result.OrderBy(element => element.CurrencyId).ToList();
        }

        private List<BudgetCashflowItemDto> GetOperatingActivitiesCashOut(int unitId, DateTimeOffset dueDate)
        {
            var result = new List<BudgetCashflowItemDto>();
            foreach (BudgetCashflowCategoryLayoutOrder layoutOrder in Enum.GetValues(typeof(BudgetCashflowCategoryLayoutOrder)))
            {
                if (layoutOrder < BudgetCashflowCategoryLayoutOrder.ImportedRawMaterial)
                    continue;
                else if (layoutOrder >= BudgetCashflowCategoryLayoutOrder.ImportedRawMaterial && layoutOrder <= BudgetCashflowCategoryLayoutOrder.OthersOperationalCost)
                    result.AddRange(_budgetCashflowService.GetBudgetCashflowUnit(layoutOrder, unitId, dueDate));
                else
                    break;
            }

            return result.OrderBy(element => element.LayoutOrder).ToList();
        }

        private List<BudgetCashflowItemDto> GetOperatingActivitiesCashOutTotal(int unitId, DateTimeOffset dueDate)
        {
            var result = new List<BudgetCashflowItemDto>();
            result.AddRange(_budgetCashflowService.GetCashOutOperatingActivitiesByUnit(unitId, dueDate));

            return result.OrderBy(element => element.CurrencyId).ToList();
        }

        private List<BudgetCashflowItemDto> GetOperatingActivitiesDifference(int unitId, DateTimeOffset dueDate)
        {
            var result = new List<BudgetCashflowItemDto>();
            result.AddRange(_budgetCashflowService.GetDiffOperatingActivitiesByUnit(unitId, dueDate));

            return result.OrderBy(element => element.CurrencyId).ToList();
        }

        private List<BudgetCashflowItemDto> GetInvestingActivitiesCashIn(int unitId, DateTimeOffset dueDate)
        {
            var result = new List<BudgetCashflowItemDto>();
            foreach (BudgetCashflowCategoryLayoutOrder layoutOrder in Enum.GetValues(typeof(BudgetCashflowCategoryLayoutOrder)))
            {
                if (layoutOrder == BudgetCashflowCategoryLayoutOrder.CashInDeposit)
                    result.AddRange(_budgetCashflowService.GetBudgetCashflowUnit(layoutOrder, unitId, dueDate));
                else if (layoutOrder == BudgetCashflowCategoryLayoutOrder.CashInOthers)
                    result.AddRange(_budgetCashflowService.GetBudgetCashflowUnit(layoutOrder, unitId, dueDate));
                else if (layoutOrder < BudgetCashflowCategoryLayoutOrder.CashInDeposit)
                    continue;
                else
                    break;
            }

            return result.OrderBy(element => element.LayoutOrder).ToList();
        }

        private List<BudgetCashflowItemDto> GetInvestingActivitiesCashInTotal(int unitId, DateTimeOffset dueDate)
        {
            var result = new List<BudgetCashflowItemDto>();
            result.AddRange(_budgetCashflowService.GetCashInInvestingActivitiesByUnit(unitId, dueDate));

            return result.OrderBy(element => element.LayoutOrder).ToList();
        }

        private List<BudgetCashflowItemDto> GetInvestingActivitiesCashOut(int unitId, DateTimeOffset dueDate)
        {
            var result = new List<BudgetCashflowItemDto>();
            foreach (BudgetCashflowCategoryLayoutOrder layoutOrder in Enum.GetValues(typeof(BudgetCashflowCategoryLayoutOrder)))
            {
                if (layoutOrder < BudgetCashflowCategoryLayoutOrder.MachineryPurchase)
                    continue;
                else if (layoutOrder >= BudgetCashflowCategoryLayoutOrder.MachineryPurchase && layoutOrder <= BudgetCashflowCategoryLayoutOrder.CashOutDeposit)
                    result.AddRange(_budgetCashflowService.GetBudgetCashflowUnit(layoutOrder, unitId, dueDate));
                else
                    break;
            }

            return result.OrderBy(element => element.LayoutOrder).ToList();
        }

        private List<BudgetCashflowItemDto> GetInvestingActivitiesCashOutTotal(int unitId, DateTimeOffset dueDate)
        {
            var result = new List<BudgetCashflowItemDto>();
            result.AddRange(_budgetCashflowService.GetCashOutInvestingActivitiesByUnit(unitId, dueDate));

            return result.OrderBy(element => element.LayoutOrder).ToList();
        }

        private List<BudgetCashflowItemDto> GetInvestingActivitiesDifference(int unitId, DateTimeOffset dueDate)
        {
            var result = new List<BudgetCashflowItemDto>();
            result.AddRange(_budgetCashflowService.GetDiffInvestingActivitiesByUnit(unitId, dueDate));

            return result.OrderBy(element => element.LayoutOrder).ToList();
        }

        private List<BudgetCashflowItemDto> GetFinancingActivitiesCashIn(int unitId, DateTimeOffset dueDate)
        {
            var result = new List<BudgetCashflowItemDto>();
            foreach (BudgetCashflowCategoryLayoutOrder layoutOrder in Enum.GetValues(typeof(BudgetCashflowCategoryLayoutOrder)))
            {
                if (layoutOrder < BudgetCashflowCategoryLayoutOrder.CashInLoanWithdrawal)
                    continue;
                else if (layoutOrder >= BudgetCashflowCategoryLayoutOrder.CashInLoanWithdrawal && layoutOrder <= BudgetCashflowCategoryLayoutOrder.CashInLoanWithdrawalOthers)
                    result.AddRange(_budgetCashflowService.GetBudgetCashflowUnit(layoutOrder, unitId, dueDate));
                else
                    break;
            }

            return result.OrderBy(element => element.LayoutOrder).ToList();
        }

        private List<BudgetCashflowItemDto> GetFinancingActivitiesCashInTotal(int unitId, DateTimeOffset dueDate)
        {
            var result = new List<BudgetCashflowItemDto>();
            result.AddRange(_budgetCashflowService.GetCashInFinancingActivitiesByUnit(unitId, dueDate));

            return result.OrderBy(element => element.LayoutOrder).ToList();
        }

        private List<BudgetCashflowItemDto> GetFinancingActivitiesCashOut(int unitId, DateTimeOffset dueDate)
        {
            var result = new List<BudgetCashflowItemDto>();
            foreach (BudgetCashflowCategoryLayoutOrder layoutOrder in Enum.GetValues(typeof(BudgetCashflowCategoryLayoutOrder)))
            {
                if (layoutOrder < BudgetCashflowCategoryLayoutOrder.CashOutInstallments)
                    continue;
                else if (layoutOrder >= BudgetCashflowCategoryLayoutOrder.CashOutInstallments && layoutOrder <= BudgetCashflowCategoryLayoutOrder.CashOutOthers)
                    result.AddRange(_budgetCashflowService.GetBudgetCashflowUnit(layoutOrder, unitId, dueDate));
                else
                    break;
            }

            return result.OrderBy(element => element.LayoutOrder).ToList();
        }

        private List<BudgetCashflowItemDto> GetFinancingActivitiesCashOutTotal(int unitId, DateTimeOffset dueDate)
        {
            var result = new List<BudgetCashflowItemDto>();
            result.AddRange(_budgetCashflowService.GetCashOutFinancingActivitiesByUnit(unitId, dueDate));

            return result.OrderBy(element => element.LayoutOrder).ToList();
        }

        private List<BudgetCashflowItemDto> GetFinancingActivitiesDifference(int unitId, DateTimeOffset dueDate)
        {
            var result = new List<BudgetCashflowItemDto>();
            result.AddRange(_budgetCashflowService.GetDiffFinancingActivitiesByUnit(unitId, dueDate));

            return result.OrderBy(element => element.LayoutOrder).ToList();
        }

        private List<BudgetCashflowItemDto> GetRowDataWorstCase(int unitId, DateTimeOffset dueDate)
        {
            var result = new List<BudgetCashflowItemDto>();
            result.AddRange(_budgetCashflowService.GetBudgetCashflowWorstCase(dueDate, unitId));

            return result.OrderBy(element => element.LayoutOrder).ToList();
        }

        public MemoryStream Generate(int unitId, DateTimeOffset dueDate)
        {
            var document = new Document(PageSize.A4.Rotate(), 20, 20, 20, 20);
            MemoryStream stream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, stream);
            document.Open();

            var unit = _units.FirstOrDefault(element => element.Id == unitId);

            SetTitle(document, unit, dueDate);
            //SetBestCaseWorstCaseMark(document);
            //SetTableHeader(document, unit);

            // oaci = operating activities cash in
            var oaci = GetOperatingActivitiesCashIn(unitId, dueDate);
            // oaciTotal = operating activities cash in total
            var oaciTotal = GetOperatingActivitiesCashInTotal(unitId, dueDate);
            // oaco = operating activities cash out
            var oaco = GetOperatingActivitiesCashOut(unitId, dueDate);
            // oacoTotal = operating activities cash out total
            var oacoTotal = GetOperatingActivitiesCashOutTotal(unitId, dueDate);
            // oadiff = opearting activities difference
            var oadiff = GetOperatingActivitiesDifference(unitId, dueDate);
            // iaci = investing activities cash in
            var iaci = GetInvestingActivitiesCashIn(unitId, dueDate);
            // iaciTotal = investing activities cash in total
            var iaciTotal = GetInvestingActivitiesCashInTotal(unitId, dueDate);
            // iaco = investing activities cash out
            var iaco = GetInvestingActivitiesCashOut(unitId, dueDate);
            // iacoTotal = investing activities cash out total
            var iacoTotal = GetInvestingActivitiesCashOutTotal(unitId, dueDate);
            // iadiff = investing activities difference
            var iadiff = GetInvestingActivitiesDifference(unitId, dueDate);
            // faci = financing activities cash in
            var faci = GetFinancingActivitiesCashIn(unitId, dueDate);
            // faciTotal = financing activities cash in total
            var faciTotal = GetFinancingActivitiesCashInTotal(unitId, dueDate);
            // faco = financing activities cash out
            var faco = GetFinancingActivitiesCashOut(unitId, dueDate);
            // fatotal = financing activities cash out total
            var facoTotal = GetFinancingActivitiesCashOutTotal(unitId, dueDate);
            // fadiff = financing activities difference
            var fadiff = GetFinancingActivitiesDifference(unitId, dueDate);

            var worstCases = GetRowDataWorstCase(unitId, dueDate);

            SetUnitTable(document, oaci.Count, oaciTotal.Count, oaco.Count, oacoTotal.Count, oadiff.Count, iaci.Count, iaciTotal.Count, iaco.Count, iacoTotal.Count, iadiff.Count, faci.Count, faciTotal.Count, faco.Count, facoTotal.Count, fadiff.Count, oaci, oaciTotal, oaco, oacoTotal, oadiff, iaci, iaciTotal, iaco, iacoTotal, iadiff, faci, faciTotal, faco, facoTotal, fadiff, worstCases);
            //SetLeftRemarkColumn(oaci.Count, oaciTotal.Count, oaco.Count, oacoTotal.Count, oadiff.Count, iaci.Count, iaciTotal.Count, iaco.Count, iacoTotal.Count, iadiff.Count, faci.Count, faciTotal.Count, faco.Count, facoTotal.Count, fadiff.Count, document);
            //SetData(oaci, oaciTotal, oaco, oacoTotal, oadiff, iaci, iaciTotal, iaco, iacoTotal, iadiff, faci, faciTotal, faco, facoTotal, fadiff, worstCases, document);

            document.Close();
            byte[] byteInfo = stream.ToArray();
            stream.Write(byteInfo, 0, byteInfo.Length);
            stream.Position = 0;

            return stream;
        }

        private void SetTitle(Document document, UnitDto unit, DateTimeOffset dueDate)
        {
            var company = "PT DAN LIRIS";
            var title = "LAPORAN BUDGET CASHFLOW";
            var unitName = "UNIT: ";
            if (unit != null)
                unitName += unit.Name;

            var dueDateString = $"{dueDate.AddHours(_identityService.TimezoneOffset):dd/MM/yyyy}";
            var date = $"PERIODE S.D. {dueDateString}";

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

            cell.Phrase = new Phrase(unitName, _headerFont);
            table.AddCell(cell);

            cell.Phrase = new Phrase(date, _headerFont);
            table.AddCell(cell);

            cell.Phrase = new Phrase("", _headerFont);
            table.AddCell(cell);

            document.Add(table);
        }

        private void SetUnitTable(Document document, int oaciCount, int oaciTotalCount, int oacoCount, int oacoTotalCount, int oadiffCount, int iaciCount, int iaciTotalCount, int iacoCount, int iacoTotalCount, int iadiffCount, int faciCount, int faciTotalCount, int facoCount, int facoTotalCount, int fadiffCount, List<BudgetCashflowItemDto> oaci, List<BudgetCashflowItemDto> oaciTotal, List<BudgetCashflowItemDto> oaco, List<BudgetCashflowItemDto> oacoTotal, List<BudgetCashflowItemDto> oadiff, List<BudgetCashflowItemDto> iaci, List<BudgetCashflowItemDto> iaciTotal, List<BudgetCashflowItemDto> iaco, List<BudgetCashflowItemDto> iacoTotal, List<BudgetCashflowItemDto> iadiff, List<BudgetCashflowItemDto> faci, List<BudgetCashflowItemDto> faciTotal, List<BudgetCashflowItemDto> faco, List<BudgetCashflowItemDto> facoTotal, List<BudgetCashflowItemDto> fadiff, List<BudgetCashflowItemDto> worstCases)
        {
            var table = new PdfPTable(13)
            {
                WidthPercentage = 100
            };
            table.SetWidths(new float[] { 2f, 2f, 1f, 1f, 15f, 5f, 10f, 10f, 10f, 5f, 10f, 10f, 10f });

            var cell = new PdfPCell()
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };

            var operatingActivitiesCashInRowCount = 2 + oaciCount + oaciTotalCount;
            var operatingActivitiesCashOutRowCount = 6 + oacoCount + oacoTotalCount;
            var operatingActivitiesRowsCount = 2 + oaciCount + oaciTotalCount + 6 + oacoCount + oacoTotalCount + oadiffCount;
            var investingActivitiesCashInRowCount = 1 + iaciCount + iaciTotalCount;
            var investingActivitiesCashOutRowCount = 1 + iacoCount + iacoTotalCount;
            var investingActivitiesRowsCount = 1 + iaciCount + iaciTotalCount + 1 + iacoCount + iacoTotalCount + iadiffCount;
            var financingActivitiesCashInRowsCount = 2 + faciCount + faciTotalCount;
            var financingActivitiesCashOutRowsCount = 3 + facoCount + facoTotalCount;
            var financingActivitiesRowsCount = 2 + faciCount + faciTotalCount + 3 + facoCount + facoTotalCount + fadiffCount;

            cell.Colspan = 5;
            cell.Rowspan = 3;
            cell.Phrase = new Phrase("KETERANGAN", _normalBoldFont);
            table.AddCell(cell);

            cell.Colspan = 3;
            cell.Rowspan = 1;
            cell.Phrase = new Phrase("BEST CASE", _normalBoldFont);
            table.AddCell(cell);

            cell.Colspan = 1;
            cell.Rowspan = 1;
            cell.Phrase = new Phrase("ACTUAL", _normalBoldFont);
            table.AddCell(cell);

            cell.Colspan = 3;
            cell.Rowspan = 1;
            cell.Phrase = new Phrase("WORST CASE", _normalBoldFont);
            table.AddCell(cell);

            cell.Colspan = 1;
            cell.Rowspan = 1;
            cell.Phrase = new Phrase("ACTUAL", _normalBoldFont);
            table.AddCell(cell);

            cell.Colspan = 3;
            cell.Rowspan = 1;
            cell.Phrase = new Phrase("UNIT", _normalBoldFont);
            table.AddCell(cell);

            cell.Colspan = 1;
            cell.Rowspan = 2;
            cell.Phrase = new Phrase("Rp. (000)", _normalBoldFont);
            table.AddCell(cell);

            cell.Colspan = 3;
            cell.Rowspan = 1;
            cell.Phrase = new Phrase("UNIT", _normalBoldFont);
            table.AddCell(cell);

            cell.Colspan = 1;
            cell.Rowspan = 2;
            cell.Phrase = new Phrase("Rp. (000)", _normalBoldFont);
            table.AddCell(cell);

            cell.Colspan = 1;
            cell.Rowspan = 1;
            cell.Phrase = new Phrase("MATA UANG", _normalBoldFont);
            table.AddCell(cell);

            cell.Colspan = 1;
            cell.Rowspan = 1;
            cell.Phrase = new Phrase("NOMINAL VALAS", _normalBoldFont);
            table.AddCell(cell);

            cell.Colspan = 1;
            cell.Rowspan = 1;
            cell.Phrase = new Phrase("NOMINAL IDR", _normalBoldFont);
            table.AddCell(cell);

            cell.Colspan = 1;
            cell.Rowspan = 1;
            cell.Phrase = new Phrase("MATA UANG", _normalBoldFont);
            table.AddCell(cell);

            cell.Colspan = 1;
            cell.Rowspan = 1;
            cell.Phrase = new Phrase("NOMINAL VALAS", _normalBoldFont);
            table.AddCell(cell);

            cell.Colspan = 1;
            cell.Rowspan = 1;
            cell.Phrase = new Phrase("NOMINAL IDR", _normalBoldFont);
            table.AddCell(cell);

            cell.Rotation = 90;
            cell.Colspan = 1;
            cell.Rowspan = operatingActivitiesRowsCount;
            cell.Phrase = new Phrase("OPERATING ACTIVITIES", _smallBoldFont);
            table.AddCell(cell);

            cell.Colspan = 1;
            cell.Rowspan = operatingActivitiesCashInRowCount;
            cell.Phrase = new Phrase("CASH IN", _smallBoldFont);
            table.AddCell(cell);

            cell.Rotation = 0;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            cell.Colspan = 11;
            cell.Rowspan = 1;
            cell.Phrase = new Phrase("Revenue", _smallerBoldFont);
            table.AddCell(cell);

            var isOthersSales = false;
            
            foreach (var item in oaci)
            {
                if (item.LayoutOrder == BudgetCashflowCategoryLayoutOrder.OthersSales && !isOthersSales)
                {
                    isOthersSales = true;

                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.Colspan = 11;
                    cell.Rowspan = 1;
                    cell.Phrase = new Phrase("Revenue from other operating", _smallerBoldFont);
                    table.AddCell(cell);
                }

                var worstCase = worstCases.FirstOrDefault(element => element.CurrencyId == item.CurrencyId && element.LayoutOrder == item.LayoutOrder);
                if (worstCase == null)
                    worstCase = new BudgetCashflowItemDto();

                var currencyCode = "";
                var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
                if (currency != null)
                    currencyCode = currency.Code;

                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase("", _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.Colspan = 2;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.LayoutOrder.ToDescriptionString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.CurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.Nominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.ActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(worstCase.CurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(worstCase.Nominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(worstCase.ActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);
            }

            foreach (var item in oaciTotal)
            {
                var currencyCode = "";
                var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
                if (currency != null)
                    currencyCode = currency.Code;

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 3;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase("Total", _smallerBoldFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.BestCaseCurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.BestCaseNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.BestCaseActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.CurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.Nominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.ActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);
            }

            cell.Rotation = 90;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Colspan = 1;
            cell.Rowspan = operatingActivitiesCashOutRowCount;
            cell.Phrase = new Phrase("CASH OUT", _smallBoldFont);
            table.AddCell(cell);

            cell.Rotation = 0;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            cell.Colspan = 11;
            cell.Rowspan = 1;
            cell.Phrase = new Phrase("Cost of Good Sold", _smallerBoldFont);
            table.AddCell(cell);

            var isMarketingSalaryCost = false;
            var isStillMarketingSalaryCost = false;
            var isGeneralAdministrativeExternalOutcomeVATCalculation = false;
            var isGeneralAdministrativeSalaryCost = false;
            var isOthersOperationalCost = false;

            foreach (var item in oaco)
            {
                if (item.LayoutOrder == BudgetCashflowCategoryLayoutOrder.MarketingSalaryCost && !isMarketingSalaryCost)
                {
                    isMarketingSalaryCost = true;

                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.Colspan = 11;
                    cell.Rowspan = 1;
                    cell.Phrase = new Phrase("Marketing Expenses", _smallerBoldFont);
                    table.AddCell(cell);
                }

                if (item.LayoutOrder == BudgetCashflowCategoryLayoutOrder.MarketingSalaryCost && !isStillMarketingSalaryCost)
                {
                    isStillMarketingSalaryCost = true;

                    cell.Colspan = 1;
                    cell.Rowspan = 1;
                    cell.Phrase = new Phrase("", _smallerBoldFont);
                    table.AddCell(cell);

                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.Colspan = 10;
                    cell.Rowspan = 1;
                    cell.Phrase = new Phrase("Biaya Penjualan", _smallerBoldFont);
                    table.AddCell(cell);
                }

                if (item.LayoutOrder == BudgetCashflowCategoryLayoutOrder.GeneralAdministrativeExternalOutcomeVATCalculation && !isGeneralAdministrativeExternalOutcomeVATCalculation)
                {
                    isGeneralAdministrativeExternalOutcomeVATCalculation = true;

                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.Colspan = 11;
                    cell.Rowspan = 1;
                    cell.Phrase = new Phrase("General & Administrative Expenses", _smallerBoldFont);
                    table.AddCell(cell);
                }

                if (item.LayoutOrder == BudgetCashflowCategoryLayoutOrder.GeneralAdministrativeSalaryCost && !isGeneralAdministrativeSalaryCost)
                {
                    isGeneralAdministrativeSalaryCost = true;

                    cell.Colspan = 1;
                    cell.Rowspan = 1;
                    cell.Phrase = new Phrase("", _smallerBoldFont);
                    table.AddCell(cell);

                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.Colspan = 10;
                    cell.Rowspan = 1;
                    cell.Phrase = new Phrase("Biaya umum dan administrasi", _smallerBoldFont);
                    table.AddCell(cell);
                }

                if (item.LayoutOrder == BudgetCashflowCategoryLayoutOrder.OthersOperationalCost && !isOthersOperationalCost)
                {
                    isOthersOperationalCost = true;

                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.Colspan = 11;
                    cell.Rowspan = 1;
                    cell.Phrase = new Phrase("Other Operating Expenses", _smallerBoldFont);
                    table.AddCell(cell);
                }

                var worstCase = worstCases.FirstOrDefault(element => element.CurrencyId == item.CurrencyId && element.LayoutOrder == item.LayoutOrder);
                if (worstCase == null)
                    worstCase = new BudgetCashflowItemDto();

                var currencyCode = "";
                var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
                if (currency != null)
                    currencyCode = currency.Code;

                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase("", _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.Colspan = 2;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.LayoutOrder.ToDescriptionString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.CurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.Nominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.ActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(worstCase.CurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(worstCase.Nominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(worstCase.ActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);
            }

            foreach (var item in oacoTotal)
            {
                var currencyCode = "";
                var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
                if (currency != null)
                    currencyCode = currency.Code;

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 3;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase("Total", _smallerBoldFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.BestCaseCurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.BestCaseNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.BestCaseActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.CurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.Nominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.ActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);
            }

            foreach (var item in oadiff)
            {
                var currencyCode = "";
                var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
                if (currency != null)
                    currencyCode = currency.Code;

                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.Colspan = 4;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase("Surplus/Deficit-Cash from Operating Activities", _smallerBoldFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.BestCaseCurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.BestCaseNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.BestCaseActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.CurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.Nominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.ActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);
            }

            cell.Rotation = 90;
            cell.Colspan = 1;
            cell.Rowspan = investingActivitiesRowsCount;
            cell.Phrase = new Phrase("INVESTING ACTIVITIES", _smallBoldFont);
            table.AddCell(cell);

            cell.Colspan = 1;
            cell.Rowspan = investingActivitiesCashInRowCount;
            cell.Phrase = new Phrase("CASH IN", _smallBoldFont);
            table.AddCell(cell);

            cell.Rotation = 0;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            cell.Colspan = 11;
            cell.Rowspan = 1;
            cell.Phrase = new Phrase(" ", _smallerFont);
            table.AddCell(cell);

            foreach (var item in iaci)
            {
                var worstCase = worstCases.FirstOrDefault(element => element.CurrencyId == item.CurrencyId && element.LayoutOrder == item.LayoutOrder);
                if (worstCase == null)
                    worstCase = new BudgetCashflowItemDto();

                var currencyCode = "";
                var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
                if (currency != null)
                    currencyCode = currency.Code;

                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.Colspan = 3;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.LayoutOrder.ToDescriptionString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.CurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.Nominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.ActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(worstCase.CurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(worstCase.Nominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(worstCase.ActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);
            }

            foreach (var item in iaciTotal)
            {
                var currencyCode = "";
                var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
                if (currency != null)
                    currencyCode = currency.Code;

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 3;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase("Total", _smallerBoldFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.BestCaseCurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.BestCaseNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.BestCaseActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.CurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.Nominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.ActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);
            }

            cell.Rotation = 90;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Colspan = 1;
            cell.Rowspan = investingActivitiesCashOutRowCount;
            cell.Phrase = new Phrase("CASH OUT", _smallBoldFont);
            table.AddCell(cell);

            cell.Rotation = 0;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            cell.Colspan = 11;
            cell.Rowspan = 1;
            cell.Phrase = new Phrase("Pembayaran pembelian asset tetap :", _smallerBoldFont);
            table.AddCell(cell);

            foreach (var item in iaco)
            {
                var worstCase = worstCases.FirstOrDefault(element => element.CurrencyId == item.CurrencyId && element.LayoutOrder == item.LayoutOrder);
                if (worstCase == null)
                    worstCase = new BudgetCashflowItemDto();

                var currencyCode = "";
                var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
                if (currency != null)
                    currencyCode = currency.Code;

                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase("", _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.Colspan = 2;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.LayoutOrder.ToDescriptionString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.CurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.Nominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.ActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(worstCase.CurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(worstCase.Nominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(worstCase.ActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);
            }
            
            foreach (var item in iacoTotal)
            {
                var currencyCode = "";
                var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
                if (currency != null)
                    currencyCode = currency.Code;

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 3;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase("Total", _smallerBoldFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.BestCaseCurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.BestCaseNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.BestCaseActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.CurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.Nominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.ActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);
            }
            
            foreach (var item in iadiff)
            {
                var currencyCode = "";
                var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
                if (currency != null)
                    currencyCode = currency.Code;

                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.Colspan = 4;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase("Surplus/Deficit-Cash from Investing Activities", _smallerBoldFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.BestCaseCurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.BestCaseNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.BestCaseActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.CurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.Nominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.ActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);
            }

            cell.Rotation = 90;
            cell.Colspan = 1;
            cell.Rowspan = financingActivitiesRowsCount;
            cell.Phrase = new Phrase("FINANCING ACTIVITIES", _smallBoldFont);
            table.AddCell(cell);

            cell.Colspan = 1;
            cell.Rowspan = financingActivitiesCashInRowsCount;
            cell.Phrase = new Phrase("CASH IN", _smallBoldFont);
            table.AddCell(cell);

            cell.Rotation = 0;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            cell.Colspan = 11;
            cell.Rowspan = 1;
            cell.Phrase = new Phrase(" ", _smallerFont);
            table.AddCell(cell);

            var isCashInAffiliates = false;
            
            foreach (var item in faci)
            {
                if (item.LayoutOrder == BudgetCashflowCategoryLayoutOrder.CashInAffiliates && !isCashInAffiliates)
                {
                    isCashInAffiliates = true;

                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.Colspan = 11;
                    cell.Rowspan = 1;
                    cell.Phrase = new Phrase("Others :", _smallerBoldFont);
                    table.AddCell(cell);
                }

                var worstCase = worstCases.FirstOrDefault(element => element.CurrencyId == item.CurrencyId && element.LayoutOrder == item.LayoutOrder);
                if (worstCase == null)
                    worstCase = new BudgetCashflowItemDto();

                var currencyCode = "";
                var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
                if (currency != null)
                    currencyCode = currency.Code;

                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase("", _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.Colspan = 2;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.LayoutOrder.ToDescriptionString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.CurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.Nominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.ActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(worstCase.CurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(worstCase.Nominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(worstCase.ActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);
            }
            
            foreach (var item in faciTotal)
            {
                var currencyCode = "";
                var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
                if (currency != null)
                    currencyCode = currency.Code;

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 3;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase("Total", _smallerBoldFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.BestCaseCurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.BestCaseNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.BestCaseActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.CurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.Nominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.ActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);
            }

            cell.Rotation = 90;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Colspan = 1;
            cell.Rowspan = financingActivitiesCashOutRowsCount;
            cell.Phrase = new Phrase("CASH OUT", _smallBoldFont);
            table.AddCell(cell);

            cell.Rotation = 0;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            cell.Colspan = 11;
            cell.Rowspan = 1;
            cell.Phrase = new Phrase("Loan Installment and Interest expense", _smallerBoldFont);
            table.AddCell(cell);

            var isCashOutBankInterest = false;
            var isOthersWritten = false;

            foreach (var item in faco)
            {
                if (item.LayoutOrder == BudgetCashflowCategoryLayoutOrder.CashOutBankInterest && !isCashOutBankInterest)
                {
                    isCashOutBankInterest = true;

                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.Colspan = 11;
                    cell.Rowspan = 1;
                    cell.Phrase = new Phrase("Bank Expenses", _smallerBoldFont);
                    table.AddCell(cell);
                }

                if (item.LayoutOrder == BudgetCashflowCategoryLayoutOrder.CashOutBankAdministrationFee && !isOthersWritten)
                {
                    isOthersWritten = true;

                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.Colspan = 11;
                    cell.Rowspan = 1;
                    cell.Phrase = new Phrase("Others :", _smallerBoldFont);
                    table.AddCell(cell);
                }

                var worstCase = worstCases.FirstOrDefault(element => element.CurrencyId == item.CurrencyId && element.LayoutOrder == item.LayoutOrder);
                if (worstCase == null)
                    worstCase = new BudgetCashflowItemDto();

                var currencyCode = "";
                var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
                if (currency != null)
                    currencyCode = currency.Code;

                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase("", _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.Colspan = 2;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.LayoutOrder.ToDescriptionString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.CurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.Nominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.ActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(worstCase.CurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(worstCase.Nominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(worstCase.ActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);
            }
            
            foreach (var item in facoTotal)
            {
                var currencyCode = "";
                var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
                if (currency != null)
                    currencyCode = currency.Code;

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 3;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase("Total", _smallerBoldFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.BestCaseCurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.BestCaseNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.BestCaseActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.CurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.Nominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.ActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);
            }

            foreach (var item in fadiff)
            {
                var currencyCode = "";
                var currency = _currencies.FirstOrDefault(element => element.Id == item.CurrencyId);
                if (currency != null)
                    currencyCode = currency.Code;

                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.Colspan = 4;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase("Surplus/Deficit-Cash from Financing Activities", _smallerBoldFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.BestCaseCurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.BestCaseNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.BestCaseActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(currencyCode, _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.CurrencyNominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.Nominal.ToString(), _smallerFont);
                table.AddCell(cell);

                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Rowspan = 1;
                cell.Phrase = new Phrase(item.ActualNominal.ToString(), _smallerFont);
                table.AddCell(cell);
            }

            document.Add(table);
        }

        private void SetData(List<BudgetCashflowItemDto> oaci, List<BudgetCashflowItemDto> oaciTotal, List<BudgetCashflowItemDto> oaco, List<BudgetCashflowItemDto> oacoTotal, List<BudgetCashflowItemDto> oadiff, List<BudgetCashflowItemDto> iaci, List<BudgetCashflowItemDto> iaciTotal, List<BudgetCashflowItemDto> iaco, List<BudgetCashflowItemDto> iacoTotal, List<BudgetCashflowItemDto> iadiff, List<BudgetCashflowItemDto> faci, List<BudgetCashflowItemDto> faciTotal, List<BudgetCashflowItemDto> faco, List<BudgetCashflowItemDto> facoTotal, List<BudgetCashflowItemDto> fadiff, List<BudgetCashflowItemDto> worstCases, Document document)
        {

        }

        private void SetLeftRemarkColumn(int oaciCount, int oaciTotalCount, int oacoCount, int oacoTotalCount, int oadiffCount, int iaciCount, int iaciTotalCount, int iacoCount, int iacoTotalCount, int iadiffCount, int faciCount, int faciTotalCount, int facoCount, int facoTotalCount, int fadiffCount, Document document)
        {
            var table = new PdfPTable(5)
            {
                WidthPercentage = 35
            };
            table.SetWidths(new float[] { 8f, 8f, 2f, 2f, 80f });

            // keterangan, mata uang, dll
            var headerCell = new PdfPCell()
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };

            // oa, ia, fa
            var sectionCell = new PdfPCell()
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Rotation = 90
            };

            // oaci, oaco, iaci, iaco, faci, faco
            var subSectionCell = new PdfPCell()
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Rotation = 90
            };

            // items
            var subSectionItemCell = new PdfPCell()
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };

            // total
            var subSectionTotalCell = new PdfPCell()
            {
                HorizontalAlignment = Element.ALIGN_RIGHT,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };

            // surplus/deficit
            var subSectionDiffCell = new PdfPCell()
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };

            int oaci = 10;
            int oaciTotal = 1;
            int oaco = 65;
            int oacoTotal = 1;
            int oaDiff = 1;
            int oa = oaci + oaciTotal + oaco + oacoTotal + oaDiff;
            int iaci = 2;
            int iaciTotal = 1;
            int iaco = 8;
            int iacoTotal = 1;
            int iaDiff = 1;
            int ia = iaci + iaciTotal + iaco + iacoTotal + iaDiff;
            int faci = 6;
            int faciTotal = 1;
            int faco = 9;
            int facoTotal = 1;
            int faDiff = 1;
            int fa = faci + faciTotal + faco + facoTotal + faDiff;

            sectionCell.Phrase = new Phrase("OPERATING ACTIVITIES", _smallBoldFont);
            sectionCell.Rowspan = oa;
            table.AddCell(sectionCell);

            subSectionCell.Phrase = new Phrase("CASH IN", _smallBoldFont);
            subSectionCell.Rowspan = oaci + oaciTotal;
            table.AddCell(subSectionCell);

            subSectionItemCell.Phrase = new Phrase("Revenue", _smallerBoldFont);
            subSectionItemCell.Colspan = 3;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Penjualan Export", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Penjualan Lokal", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Penjualan Tunai", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Penjualan Intern (Antar Divisi)", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Penjualan Intern (Antar Unit Satu Divisi)", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("PPN Masukan Intern (Perhitungan)", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Revenue from other operating", _smallerBoldFont);
            subSectionItemCell.Colspan = 3;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Penjualan Lain-lain", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("PPN Masukan Extern (Pembelian Lokal)", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionTotalCell.Phrase = new Phrase("Total", _smallerBoldFont);
            subSectionTotalCell.Colspan = 3;
            table.AddCell(subSectionTotalCell);

            subSectionCell.Phrase = new Phrase("CASH OUT", _smallBoldFont);
            subSectionCell.Rowspan = oaco + oacoTotal;
            table.AddCell(subSectionCell);

            subSectionItemCell.Phrase = new Phrase("Cost of Good Sold", _smallerBoldFont);
            subSectionItemCell.Colspan = 3;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Bahan Baku Import", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Bahan Baku Lokal", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Biaya Tenaga Kerja Langsung/U.Karyawan", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Bahan Pembantu", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("SubCount", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Embalage", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Listrik (PLN)", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Batu Bara", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("BBM & Pelumas", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Spare Part & Pemeliharaan Mesin", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("BTKL Staf Unit", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("THR/Bonus Karyawan & Staf Unit", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Gaji/Honor Konsultan & TKA", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Askes & Jamsostek Unit", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Pesangon/Pensiun Unit", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Pengolahan Limbah, ABT, dll", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Biaya Import (Inklaring, Demurage, dll)", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Pembelian Intern (Antar Divisi)", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Pembelian Intern (Antar Unit Satu Divisi)", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("PPN Keluaran Intern (Perhitungan)", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Marketing Expenses", _smallerBoldFont);
            subSectionItemCell.Colspan = 3;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerBoldFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Biaya Penjualan", _smallerBoldFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Beban Gaji Staf", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Upah Karyawan", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("jpk & jamsost staff& karyw", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("thr & bonus kary & staf", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("beban iklan, reklame, & pameran", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("beban perjalanan dinas", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("beban pengiriman/ongkos angkut", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("beban komisi penj lokal/exspP.", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("beban freight/emkl", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("biaya claim", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("biaya pengurusan doc", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("beban asuransi", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("beban penjualan lain2", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("General & Administrative Expenses", _smallerBoldFont);
            subSectionItemCell.Colspan = 3;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("PPN Keluaran Extern (Pejualan Lokal)", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Pajak (PPN, PPh,PBB, PNBP dll)", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerBoldFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Biaya umum dan administrasi", _smallerBoldFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("beban gaji staff kantor", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("upah karyw kantor", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("askes & jamsos kary& staf", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("beban gaji direksi/direktur", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Beban Pemeliharaan gedung", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("beban perjalanan dinas", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("beban pengiriman surat", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("beban alat tulis", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("beban air/abt", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("beban listrik", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("beban notaris & konsultan", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("beban training & pendidikan", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("beban perijinan & sertifikat", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("sumbangan", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("representatif, entertainment tamu dll", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("ass kend, gedung, dan mesin", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("beban URTP", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("pesangon staf & kary", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("thr & bonus kary & staf umum, dirktur &direksi", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("beban kendaraan", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("beban keamanan", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("beban lain-lain", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Telephone, Fax & Internet", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Other Operating Expenses", _smallerBoldFont);
            subSectionItemCell.Colspan = 3;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Biaya Lainya", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionTotalCell.Phrase = new Phrase("Total", _smallerBoldFont);
            subSectionTotalCell.Colspan = 3;
            table.AddCell(subSectionTotalCell);

            subSectionDiffCell.Phrase = new Phrase("Surplus/Deficit-Cash from Operating Activities", _smallerBoldFont);
            subSectionDiffCell.Colspan = 4;
            table.AddCell(subSectionDiffCell);

            sectionCell.Phrase = new Phrase("INVESTING ACTIVITIES", _smallBoldFont);
            sectionCell.Rowspan = ia;
            table.AddCell(sectionCell);

            subSectionCell.Phrase = new Phrase("CASH IN", _smallBoldFont);
            subSectionCell.Rowspan = iaci + iaciTotal;
            table.AddCell(subSectionCell);

            subSectionItemCell.Phrase = new Phrase("Deposito", _smallerFont);
            subSectionItemCell.Colspan = 3;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Lain-lain", _smallerFont);
            subSectionItemCell.Colspan = 3;
            table.AddCell(subSectionItemCell);

            subSectionTotalCell.Phrase = new Phrase("Total", _smallerBoldFont);
            subSectionTotalCell.Colspan = 3;
            table.AddCell(subSectionTotalCell);

            subSectionCell.Phrase = new Phrase("CASH OUT", _smallBoldFont);
            subSectionCell.Rowspan = iaco + iacoTotal;
            table.AddCell(subSectionCell);

            subSectionItemCell.Phrase = new Phrase("Pembayaran pembelian asset tetap :", _smallerBoldFont);
            subSectionItemCell.Colspan = 3;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Mesin", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Kendaraan", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Inventaris", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Alat Komputer", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Alat & Bahan Produksi", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Proyek", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Deposito", _smallerFont);
            subSectionItemCell.Colspan = 3;
            table.AddCell(subSectionItemCell);

            subSectionTotalCell.Phrase = new Phrase("Total", _smallerBoldFont);
            subSectionTotalCell.Colspan = 3;
            table.AddCell(subSectionTotalCell);

            subSectionDiffCell.Phrase = new Phrase("Surplus/Deficit-Cash from Investing Activities", _smallerBoldFont);
            subSectionDiffCell.Colspan = 4;
            table.AddCell(subSectionDiffCell);

            sectionCell.Phrase = new Phrase("FINANCING ACTIVITIES", _smallBoldFont);
            sectionCell.Rowspan = fa;
            table.AddCell(sectionCell);

            subSectionCell.Phrase = new Phrase("CASH IN", _smallBoldFont);
            subSectionCell.Rowspan = faci + faciTotal;
            table.AddCell(subSectionCell);

            subSectionItemCell.Phrase = new Phrase("Loan Withdrawal", _smallerFont);
            subSectionItemCell.Colspan = 3;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Others :", _smallerBoldFont);
            subSectionItemCell.Colspan = 3;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Afiliasi", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Jual Beli Valas", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Cadangan Perusahaan", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Lain-lain (Klaim ass)/tab thr/vb import/giro/dll", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionTotalCell.Phrase = new Phrase("Total", _smallerBoldFont);
            subSectionTotalCell.Colspan = 3;
            table.AddCell(subSectionTotalCell);

            subSectionCell.Phrase = new Phrase("CASH OUT", _smallBoldFont);
            subSectionCell.Rowspan = faco + facoTotal;
            table.AddCell(subSectionCell);

            subSectionItemCell.Phrase = new Phrase("Loan Installment and Interest expense", _smallerBoldFont);
            subSectionItemCell.Colspan = 3;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Angsuran Kredit", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Biaya Bunga Bank", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Bank Expenses", _smallerBoldFont);
            subSectionItemCell.Colspan = 3;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Biaya Adm Bank", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Others :", _smallerBoldFont);
            subSectionItemCell.Colspan = 3;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Afiliasi (Psr, Group)", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Jual Beli Valas", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("", _smallerFont);
            subSectionItemCell.Colspan = 1;
            table.AddCell(subSectionItemCell);

            subSectionItemCell.Phrase = new Phrase("Lain-lain /efrata/b mndri/md/Cad THR", _smallerFont);
            subSectionItemCell.Colspan = 2;
            table.AddCell(subSectionItemCell);

            subSectionTotalCell.Phrase = new Phrase("Total", _smallerBoldFont);
            subSectionTotalCell.Colspan = 3;
            table.AddCell(subSectionTotalCell);

            subSectionDiffCell.Phrase = new Phrase("Surplus/Deficit-Cash from Financing Activities", _smallerBoldFont);
            subSectionDiffCell.Colspan = 4;
            table.AddCell(subSectionDiffCell);

            document.Add(table);

        }

        private void SetTableHeader(Document document, UnitDto unit)
        {

        }

        private void SetBestCaseWorstCaseMark(Document document)
        {

        }

    }
}
