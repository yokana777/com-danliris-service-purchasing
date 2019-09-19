using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Com.DanLiris.Service.Purchasing.Lib.Facades.Report;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.Report
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/report/import-purchasing-book-reports")]
    [Authorize]
    public class ImportPurchasingBookReportController : Controller
    {
        private string ApiVersion = "1.0.0";
        private readonly IImportPurchasingBookReportFacade importPurchasingBookReportFacade;

        public ImportPurchasingBookReportController(IImportPurchasingBookReportFacade importPurchasingBookReportFacade)
        {
            this.importPurchasingBookReportFacade = importPurchasingBookReportFacade;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string no, string unit, string category, DateTime? dateFrom, DateTime? dateTo)
        {
            try
            {
                var data = await importPurchasingBookReportFacade.GetReport(no, unit, category, dateFrom, dateTo);
                //var data = importPurchasingBookReportService.GetReport();

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

        [HttpGet("download")]
        public async Task<IActionResult> GetXls(string no, string unit, string category, DateTime? dateFrom, DateTime? dateTo)
        {
            try
            {
                byte[] xlsInBytes;

                var xls = await importPurchasingBookReportFacade.GenerateExcel(no, unit, category, dateFrom, dateTo);

                string filename = "Laporan Buku Pembelian Impor";

                if (dateFrom != null) filename += " " + ((DateTime)dateFrom).ToString("dd-MM-yyyy");

                if (dateTo != null) filename += "_" + ((DateTime)dateTo).ToString("dd-MM-yyyy");
                filename += ".xlsx";

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
