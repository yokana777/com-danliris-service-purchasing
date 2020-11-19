using Com.DanLiris.Service.Purchasing.Lib.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Com.DanLiris.Service.Purchasing.Lib.Facades.UnpaidDispositionReportFacades;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.UnpaidDispositionReport
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/unpaid-disposition-report/detail")]
    [Authorize]

    public class UnpaidDispositionReportDetailController : Controller
    {
        private readonly IUnpaidDispositionReportDetailFacade _service;
        private readonly IdentityService _identityService;
        private const string ApiVersion = "1.0";

        public UnpaidDispositionReportDetailController(IServiceProvider serviceProvider)
        {
            _service = serviceProvider.GetService<IUnpaidDispositionReportDetailFacade>();
            _identityService = serviceProvider.GetService<IdentityService>();
        }

        private void VerifyUser()
        {
            _identityService.Username = User.Claims.ToArray().SingleOrDefault(p => p.Type.Equals("username")).Value;
            _identityService.Token = Request.Headers["Authorization"].FirstOrDefault().Replace("Bearer ", "");
            _identityService.TimezoneOffset = Convert.ToInt32(Request.Headers["x-timezone-offset"]);
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int accountingUnitId, [FromQuery] int accountingCategoryId, [FromQuery] int divisionId, [FromQuery] DateTimeOffset? dateTo, [FromQuery] bool isImport, [FromQuery] bool isForeignCurrency)
        {
            try
            {
                VerifyUser();
                var data = await _service.GetReport(accountingUnitId, accountingCategoryId, divisionId, dateTo.GetValueOrDefault(), isImport, isForeignCurrency);

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

        [HttpGet("download-excel")]
        public async Task<IActionResult> DownloadExcelAsync([FromQuery] int categoryId, [FromQuery] int unitId, [FromQuery] int divisionId, [FromQuery] DateTimeOffset? dateTo, [FromQuery] bool isImport, [FromQuery] bool isForeignCurrency)
        {

            try
            {
                if (!dateTo.HasValue)
                    dateTo = DateTimeOffset.Now;

                byte[] xlsInBytes;
                var xls = await _service.GenerateExcel(categoryId, unitId, divisionId, dateTo.GetValueOrDefault(), isImport, isForeignCurrency);

                string filename = "Laporan Buku Pembelian Lokal";
                if(isForeignCurrency)
                    filename = "Laporan Buku Pembelian Lokal Valas";
                else if(isImport)
                    filename = "Laporan Buku Pembelian Import";
                //if (dateTo != null) filename += "_" + ((DateTime)dateTo).ToString("dd-MM-yyyy");
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

        //[HttpGet("download-pdf")]
        //public IActionResult DownloadPdf([FromQuery] int categoryId, [FromQuery] int unitId, [FromQuery] int divisionId, [FromQuery] DateTimeOffset? dueDate, [FromQuery] bool isImport, [FromQuery] bool isForeignCurrency)
        //{

        //    try
        //    {
        //        if (!dueDate.HasValue)
        //            dueDate = DateTimeOffset.Now;
        //        var result = _service.GetReport(categoryId, unitId, divisionId, dueDate.GetValueOrDefault(), isImport, isForeignCurrency);
        //        return Ok(new
        //        {
        //            apiVersion = ApiVersion,
        //            statusCode = General.OK_STATUS_CODE,
        //            message = General.OK_MESSAGE,
        //            data = result,
        //            info = new Dictionary<string, object>
        //            {
        //                { "page", 1 },
        //                { "size", 10 }
        //            },
        //        });
        //    }
        //    catch (Exception e)
        //    {
        //        return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, e.Message + " " + e.StackTrace);
        //    }
        //}
    }
}
