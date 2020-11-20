using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Com.DanLiris.Service.Purchasing.Lib.Facades.Report;
using Com.DanLiris.Service.Purchasing.Lib.PDFTemplates;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.Report
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/report/detail-credit-balance")]
    [Authorize]
    public class DetailCreditBalanceReportController : Controller
    {
        private string ApiVersion = "1.0.0";

        private readonly IDetailCreditBalanceReportFacade detailCreditBalanceReportFacade;

        public DetailCreditBalanceReportController(IDetailCreditBalanceReportFacade detailCreditBalanceReportFacade)
        {
            this.detailCreditBalanceReportFacade = detailCreditBalanceReportFacade;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int categoryId, int accountingUnitId, int divisionId, DateTime? dateTo, bool isImport, bool isForeignCurrency)
        {
            try
            {
                var data = await detailCreditBalanceReportFacade.GetReport(categoryId, accountingUnitId, divisionId, dateTo, isImport, isForeignCurrency);
                
                return Ok(new
                {
                    apiVersion = ApiVersion,
                    data = data,
                    info = new { total = data.Reports.Count },
                    message = General.OK_MESSAGE,
                    statusCode = General.OK_STATUS_CODE

                });
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }

        [HttpGet("pdf")]
        public async Task<IActionResult> GetPdf(int categoryId, int accountingUnitId, int divisionId, DateTime? dateTo, bool isImport, bool isForeignCurrency)
        {
            try
            {
                var clientTimeZoneOffset = int.Parse(Request.Headers["x-timezone-offset"].First());

                var data = await detailCreditBalanceReportFacade.GetReport(categoryId, accountingUnitId, divisionId, dateTo, isImport, isForeignCurrency);

                var stream = DetailCreditBalanceReportPdfTemplate.Generate(data, clientTimeZoneOffset, dateTo);

                var filename = isImport ? "Laporan Detail Saldo Hutang Usaha Impor" : isForeignCurrency ? "Laporan Detail Saldo Hutang Usaha Lokal Valas" : "Laporan Detail Saldo Hutang Usaha Lokal";
                filename += ".pdf";

                return new FileStreamResult(stream, "application/pdf")
                {
                    FileDownloadName = filename
                };
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }

        [HttpGet("xls")]
        public async Task<IActionResult> GetXls(int categoryId, int accountingUnitId, int divisionId, DateTime? dateTo, bool isImport, bool isForeignCurrency)
        {
            try
            {
                byte[] xlsInBytes;
                var xls = await detailCreditBalanceReportFacade.GenerateExcel(categoryId, accountingUnitId, divisionId, dateTo, isImport, isForeignCurrency);

                string filename = isImport ? "Laporan Detail Saldo Hutang Usaha Impor" : isForeignCurrency ? "Laporan Detail Saldo Hutang Usaha Lokal Valas" : "Laporan Detail Saldo Hutang Usaha Lokal";
                filename += ".xlsx";

                //if (dateFrom != null) filename += " " + ((DateTime)dateFrom).ToString("dd-MM-yyyy");
                if (dateTo != null) filename += "_" + ((DateTime)dateTo).ToString("dd-MM-yyyy");

                xlsInBytes = xls.ToArray();
                var file = File(xlsInBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
                return file;
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }
    }
}
