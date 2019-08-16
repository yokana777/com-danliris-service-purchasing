using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.Expedition
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/unit-payment-orders-expedition-report")]
    [Authorize]
    public class UnitPaymentOrderExpeditionReportController : Controller
    {
        private const string _apiVersion = "1.0";
        private readonly IUnitPaymentOrderExpeditionReportService _service;

        public UnitPaymentOrderExpeditionReportController(IUnitPaymentOrderExpeditionReportService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetReport([FromQuery] string no, [FromQuery] string supplierCode, [FromQuery] string divisionCode, [FromQuery] int status, [FromQuery] DateTimeOffset? dateFrom, [FromQuery] DateTimeOffset? dateTo, [FromQuery] string order = "{'Date': 'desc'}", [FromQuery] int page = 1, [FromQuery] int size = 25)
        {
            if (dateTo == null)
                dateTo = DateTimeOffset.UtcNow;

            if (dateFrom == null)
                dateFrom = dateTo.GetValueOrDefault().AddDays(-30);

            try
            {
                var result = await _service.GetReport(no, supplierCode, divisionCode, status, dateFrom.GetValueOrDefault(), dateTo.GetValueOrDefault(), order, page, size);

                return Ok(new
                {
                    apiVersion = _apiVersion,
                    data = result,
                    info = new { total = result.Count, page, size }
                });
            }
            catch(Exception e)
            {
                Dictionary<string, object> result =
                    new ResultFormatter(_apiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, result);
            }
        }
    }
}
