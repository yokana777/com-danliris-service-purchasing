﻿using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchasingBookReport;
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
    [Route("v{version:apiVersion}/garment-purchasing-book-reports")]
    [Authorize]
    public class GarmentPurchasingBookReportController : Controller
    {
        private readonly IGarmentPurchasingBookReportService _service;
        private readonly IdentityService _identityService;
        private string ApiVersion = "1.0.0";

        public GarmentPurchasingBookReportController(IServiceProvider serviceProvider)
        {
            _service = serviceProvider.GetService<IGarmentPurchasingBookReportService>();
            _identityService = serviceProvider.GetService<IdentityService>();

        }

        [HttpGet]
        public IActionResult GetReport([FromQuery] string billNo, [FromQuery] string paymentBill, [FromQuery] string category, [FromQuery] DateTimeOffset? startDate, [FromQuery] DateTimeOffset? endDate, [FromQuery] bool isForeignCurrency, [FromQuery] bool isImportSupplier)
        {
            try
            {
                startDate = startDate.HasValue ? startDate.GetValueOrDefault() : DateTimeOffset.MinValue;
                endDate = endDate.HasValue ? endDate.GetValueOrDefault() : DateTimeOffset.MaxValue;

                var result = _service.GetReport(billNo, paymentBill, category, startDate.GetValueOrDefault(), endDate.GetValueOrDefault(), isForeignCurrency, isImportSupplier);

                return Ok(new
                {
                    apiVersion = ApiVersion,
                    data = result,
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

        [HttpGet("bill-no")]
        public IActionResult GetBillNo([FromQuery] string keyword)
        {
            try
            {
                var result = _service.GetBillNos(keyword);

                return Ok(new
                {
                    apiVersion = ApiVersion,
                    data = result,
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

        [HttpGet("payment-bill")]
        public IActionResult GetPaymentBill([FromQuery] string keyword)
        {
            try
            {
                var result = _service.GetPaymentBills(keyword);

                return Ok(new
                {
                    apiVersion = ApiVersion,
                    data = result,
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

        [HttpGet("downloads/xls")]
        public async Task<IActionResult> GetXls([FromQuery] string billNo, [FromQuery] string paymentBill, [FromQuery] string category, [FromQuery] DateTimeOffset? startDate, [FromQuery] DateTimeOffset? endDate, [FromQuery] bool isForeignCurrency, [FromQuery] bool isImportSupplier)
        {
            try
            {
                startDate = startDate.HasValue ? startDate.GetValueOrDefault() : DateTimeOffset.MinValue;
                endDate = endDate.HasValue ? endDate.GetValueOrDefault() : DateTimeOffset.MaxValue;

                var filename = "Laporan Buku Pembelian Lokal";
                if (isForeignCurrency)
                    filename = "Laporan Buku Pembelian Lokal Valas";
                else if (!isForeignCurrency && isImportSupplier)
                    filename = "Laporan Buku Pembelian Impor";
                filename += ".xlsx";

                var result = await _service.GenerateExcel(billNo, paymentBill, category, startDate.GetValueOrDefault(), endDate.GetValueOrDefault(), isForeignCurrency, isImportSupplier,_identityService.TimezoneOffset);

                //return Ok(new
                //{
                //    apiVersion = ApiVersion,
                //    data = result,
                //    message = General.OK_MESSAGE,
                //    statusCode = General.OK_STATUS_CODE
                //});
                var bytes = result.ToArray();

                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
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