using System;
using System.Collections.Generic;
using Com.DanLiris.Service.Purchasing.Lib.Services.LocalPurchasingBookReport;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.LocalPurchasingBookReport
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/import-purchasing-book-reports")]
    [Authorize]
    public class LocalPurchasingBookReportController : Controller
    {
        private string ApiVersion = "1.0.0";
        private LocalPurchasingBookReportService localPurchasingBookReportService { get; }

        public LocalPurchasingBookReportController(LocalPurchasingBookReportService localPurchasingBookReportService)
        {
            this.localPurchasingBookReportService = localPurchasingBookReportService;
        }

        [HttpGet]
        public IActionResult Get(string no, string unit, string category, DateTime? dateFrom, DateTime? dateTo)
        {
            int offset = Convert.ToInt32(Request.Headers["x-timezone-offset"]);
            string accept = Request.Headers["Accept"];

            try
            {
                var data = localPurchasingBookReportService.GetReport(no, unit, category, dateFrom, dateTo);
                //var data = importPurchasingBookReportService.GetReport();

                return Ok(new
                {
                    apiVersion = ApiVersion,
                    data = data.Item1,
                    info = new { total = data.Item2 }
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
    }
}