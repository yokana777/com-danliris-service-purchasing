using Com.DanLiris.Service.Purchasing.Lib.Facades.DebtAndDispositionSummary;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.Reports
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/reports/debt-and-disposition-summaries")]
    [Authorize]
    public class DebtAndDispositionController : Controller
    {
        private readonly IDebtAndDispositionSummaryService _service;
        private readonly IdentityService _identityService;
        private const string ApiVersion = "1.0";

        public DebtAndDispositionController(IServiceProvider serviceProvider)
        {
            _service = serviceProvider.GetService<IDebtAndDispositionSummaryService>();
            _identityService = serviceProvider.GetService<IdentityService>();
        }

        private void VerifyUser()
        {
            _identityService.Username = User.Claims.ToArray().SingleOrDefault(p => p.Type.Equals("username")).Value;
            _identityService.Token = Request.Headers["Authorization"].FirstOrDefault().Replace("Bearer ", "");
            _identityService.TimezoneOffset = Convert.ToInt32(Request.Headers["x-timezone-offset"]);
        }

        [HttpGet]
        public IActionResult Get([FromQuery] int categoryId, [FromQuery] int unitId, [FromQuery] int divisionId, [FromQuery] DateTimeOffset? dueDate, [FromQuery] bool isImport, [FromQuery] bool isForeignCurrency)
        {

            try
            {
                if (!dueDate.HasValue)
                    dueDate = DateTimeOffset.Now;
                var result = _service.GetReport(categoryId, unitId, divisionId, dueDate.GetValueOrDefault(), isImport, isForeignCurrency);
                return Ok(new
                {
                    apiVersion = ApiVersion,
                    statusCode = General.OK_STATUS_CODE,
                    message = General.OK_MESSAGE,
                    data = result,
                    info = new Dictionary<string, object>
                    {
                        { "page", 1 },
                        { "size", 10 }
                    },
                });
            }
            catch (Exception e)
            {
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, e.Message + " " + e.StackTrace);
            }
        }

        [HttpGet("download-excel")]
        public IActionResult DownloadExcel([FromQuery] int categoryId, [FromQuery] int unitId, [FromQuery] int divisionId, [FromQuery] DateTimeOffset? dueDate, [FromQuery] bool isImport, [FromQuery] bool isForeignCurrency)
        {

            try
            {
                if (!dueDate.HasValue)
                    dueDate = DateTimeOffset.Now;
                var result = _service.GetReport(categoryId, unitId, divisionId, dueDate.GetValueOrDefault(), isImport, isForeignCurrency);
                return Ok(new
                {
                    apiVersion = ApiVersion,
                    statusCode = General.OK_STATUS_CODE,
                    message = General.OK_MESSAGE,
                    data = result,
                    info = new Dictionary<string, object>
                    {
                        { "page", 1 },
                        { "size", 10 }
                    },
                });
            }
            catch (Exception e)
            {
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, e.Message + " " + e.StackTrace);
            }
        }

        [HttpGet("download-pdf")]
        public IActionResult DownloadPdf([FromQuery] int categoryId, [FromQuery] int unitId, [FromQuery] int divisionId, [FromQuery] DateTimeOffset? dueDate, [FromQuery] bool isImport, [FromQuery] bool isForeignCurrency)
        {

            try
            {
                if (!dueDate.HasValue)
                    dueDate = DateTimeOffset.Now;
                var result = _service.GetReport(categoryId, unitId, divisionId, dueDate.GetValueOrDefault(), isImport, isForeignCurrency);
                return Ok(new
                {
                    apiVersion = ApiVersion,
                    statusCode = General.OK_STATUS_CODE,
                    message = General.OK_MESSAGE,
                    data = result,
                    info = new Dictionary<string, object>
                    {
                        { "page", 1 },
                        { "size", 10 }
                    },
                });
            }
            catch (Exception e)
            {
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, e.Message + " " + e.StackTrace);
            }
        }
    }
}
