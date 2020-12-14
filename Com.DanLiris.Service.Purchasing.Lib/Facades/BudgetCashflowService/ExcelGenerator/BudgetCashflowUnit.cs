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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.BudgetCashflowService.ExcelGenerator
{
    public class BudgetCashflowUnit : IBudgetCashflowUnit
    {
        private readonly IBudgetCashflowService _budgetCashflowService;
        private readonly IdentityService _identityService;
        private readonly List<BudgetingCategoryDto> _budgetingCategories;
        private readonly List<UnitDto> _units;

        public BudgetCashflowUnit(IServiceProvider serviceProvider)
        {
            _budgetCashflowService = serviceProvider.GetService<IBudgetCashflowService>();
            _identityService = serviceProvider.GetService<IdentityService>();

            var cache = serviceProvider.GetService<IDistributedCache>();

            var jsonUnits = cache.GetString(MemoryCacheConstant.Units);
            _units = JsonConvert.DeserializeObject<List<UnitDto>>(jsonUnits, new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            });
        }

        private List<BudgetCashflowItemDto> GetRowDataBestCase(int unitId, DateTimeOffset dueDate)
        {
            var result = new List<BudgetCashflowItemDto>();
            foreach (BudgetCashflowCategoryLayoutOrder layoutOrder in Enum.GetValues(typeof(BudgetCashflowCategoryLayoutOrder)))
            {
                result.AddRange(_budgetCashflowService.GetBudgetCashflowUnit(layoutOrder, unitId, dueDate));
            }

            return result.OrderBy(element => new { element.LayoutOrder, element.CurrencyId }).ToList();
        }

        private List<BudgetCashflowItemDto> GetRowDataWorstCase(int unitId, DateTimeOffset dueDate)
        {
            var result = new List<BudgetCashflowItemDto>();

            result.AddRange(_budgetCashflowService.GetBudgetCashflowWorstCase(dueDate, unitId));

            return result.OrderBy(element => new { element.LayoutOrder, element.CurrencyId }).ToList();
        }

        public MemoryStream Generate(int unitId, DateTimeOffset dueDate)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet 1");

                var unit = _units.FirstOrDefault(element => element.Id == unitId);

                SetTitle(worksheet, unit, dueDate);
                SetBestCaseWorstCaseMark(worksheet);
                SetTableHeader(worksheet, unit);

                worksheet.Cells["A5:K7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["A5:K7"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells[worksheet.Cells.Address].AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);

                return stream;
            }
        }

        private void SetTableHeader(ExcelWorksheet worksheet, UnitDto unit)
        {
            var unitName = "";
            if (unit != null)
                unitName = unit.Name;

            worksheet.Cells["A6"].Value = "";
            worksheet.Cells["A6:A7"].Merge = true;
            worksheet.Cells["A6:A7"].Style.Font.Size = 14;
            worksheet.Cells["A6:A7"].Style.Font.Bold = true;
            worksheet.Cells["A6:A7"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["A6:A7"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A6:A7"].Style.Fill.BackgroundColor.SetColor(Color.Orange);
            worksheet.Cells["B6"].Value = "KETERANGAN";
            worksheet.Cells["B6:C7"].Merge = true;
            worksheet.Cells["B6:C7"].Style.Font.Size = 14;
            worksheet.Cells["B6:C7"].Style.Font.Bold = true;
            worksheet.Cells["B6:C7"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["B6:C7"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["B6:C7"].Style.Fill.BackgroundColor.SetColor(Color.Orange);
            worksheet.Cells["D6"].Value = unitName;
            worksheet.Cells["D6:F6"].Merge = true;
            worksheet.Cells["D6:F6"].Style.Font.Size = 14;
            worksheet.Cells["D6:F6"].Style.Font.Bold = true;
            worksheet.Cells["D6:F6"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["D6:F6"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["D6:F6"].Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
            worksheet.Cells["G6"].Value = "RP (000)";
            worksheet.Cells["G6:G7"].Merge = true;
            worksheet.Cells["G6:G7"].Style.Font.Size = 14;
            worksheet.Cells["G6:G7"].Style.Font.Bold = true;
            worksheet.Cells["G6:G7"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["G6:G7"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["G6:G7"].Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
            worksheet.Cells["H6"].Value = unitName;
            worksheet.Cells["H6:J6"].Merge = true;
            worksheet.Cells["H6:J6"].Style.Font.Size = 14;
            worksheet.Cells["H6:J6"].Style.Font.Bold = true;
            worksheet.Cells["H6:J6"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["H6:J6"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["H6:J6"].Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
            worksheet.Cells["K6"].Value = "RP (000)";
            worksheet.Cells["K6:K7"].Merge = true;
            worksheet.Cells["K6:K7"].Style.Font.Size = 14;
            worksheet.Cells["K6:K7"].Style.Font.Bold = true;
            worksheet.Cells["K6:K7"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["K6:K7"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["K6:K7"].Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);

            worksheet.Cells["D7"].Value = "Mata Uang";
            worksheet.Cells["D7"].Style.Font.Size = 14;
            worksheet.Cells["D7"].Style.Font.Bold = true;
            worksheet.Cells["D7"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["D7"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["D7"].Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
            worksheet.Cells["E7"].Value = "Nominal Valas";
            worksheet.Cells["E7"].Style.Font.Size = 14;
            worksheet.Cells["E7"].Style.Font.Bold = true;
            worksheet.Cells["E7"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["E7"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["E7"].Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
            worksheet.Cells["F7"].Value = "Nominal IDR";
            worksheet.Cells["F7"].Style.Font.Size = 14;
            worksheet.Cells["F7"].Style.Font.Bold = true;
            worksheet.Cells["F7"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["F7"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["F7"].Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);

            worksheet.Cells["H7"].Value = "Mata Uang";
            worksheet.Cells["H7"].Style.Font.Size = 14;
            worksheet.Cells["H7"].Style.Font.Bold = true;
            worksheet.Cells["H7"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["H7"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["H7"].Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
            worksheet.Cells["I7"].Value = "Nominal Valas";
            worksheet.Cells["I7"].Style.Font.Size = 14;
            worksheet.Cells["I7"].Style.Font.Bold = true;
            worksheet.Cells["I7"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["I7"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["I7"].Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
            worksheet.Cells["J7"].Value = "Nominal IDR";
            worksheet.Cells["J7"].Style.Font.Size = 14;
            worksheet.Cells["J7"].Style.Font.Bold = true;
            worksheet.Cells["J7"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["J7"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["J7"].Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);

        }

        private void SetBestCaseWorstCaseMark(ExcelWorksheet worksheet)
        {
            worksheet.Cells["D5"].Value = "BEST CASE";
            worksheet.Cells["D5:F5"].Merge = true;
            worksheet.Cells["D5:F5"].Style.Font.Size = 14;
            worksheet.Cells["D5:F5"].Style.Font.Bold = true;
            worksheet.Cells["D5:F5"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["D5:F5"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["D5:F5"].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
            worksheet.Cells["G5"].Value = "ACTUAL";
            worksheet.Cells["G5"].Style.Font.Size = 14;
            worksheet.Cells["G5"].Style.Font.Bold = true;
            worksheet.Cells["G5"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["G5"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["G5"].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
            worksheet.Cells["H5"].Value = "WORST CASE";
            worksheet.Cells["H5:J5"].Merge = true;
            worksheet.Cells["H5:J5"].Style.Font.Size = 14;
            worksheet.Cells["H5:J5"].Style.Font.Bold = true;
            worksheet.Cells["H5:J5"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["H5:J5"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["H5:J5"].Style.Fill.BackgroundColor.SetColor(Color.Red);
            worksheet.Cells["K5"].Value = "ACTUAL";
            worksheet.Cells["K5"].Style.Font.Size = 14;
            worksheet.Cells["K5"].Style.Font.Bold = true;
            worksheet.Cells["K5"].Style.Font.Color.SetColor(Color.White);
            worksheet.Cells["K5"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["K5"].Style.Fill.BackgroundColor.SetColor(Color.Red);
        }

        private void SetTitle(ExcelWorksheet worksheet, UnitDto unit, DateTimeOffset dueDate)
        {
            var company = "PT DAN LIRIS";
            var title = "LAPORAN BUDGET CASH FLOW";
            var unitName = "UNIT: ";
            if (unit != null)
                unitName += unit.Name;

            var dueDateString = $"{dueDate:dd/MM/yy}";
            var date = $"JATUH TEMPO s.d. {dueDateString}";

            worksheet.Cells["A1"].Value = company;
            worksheet.Cells["A1:K1"].Merge = true;
            worksheet.Cells["A1:K1"].Style.Font.Size = 20;
            worksheet.Cells["A1:K1"].Style.Font.Bold = true;
            worksheet.Cells["A2"].Value = title;
            worksheet.Cells["A2:K2"].Merge = true;
            worksheet.Cells["A2:K2"].Style.Font.Size = 20;
            worksheet.Cells["A2:K2"].Style.Font.Bold = true;
            worksheet.Cells["A3"].Value = unitName;
            worksheet.Cells["A3:K3"].Merge = true;
            worksheet.Cells["A3:K3"].Style.Font.Size = 20;
            worksheet.Cells["A3:K3"].Style.Font.Bold = true;
            worksheet.Cells["A4"].Value = date;
            worksheet.Cells["A4:K4"].Merge = true;
            worksheet.Cells["A4:K4"].Style.Font.Size = 20;
            worksheet.Cells["A4:K4"].Style.Font.Bold = true;
        }
    }
}
