using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Com.DanLiris.Service.Purchasing.Lib.Facades.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.Expedition;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.Expedition
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/expedition/pph-bank-expenditure-notes-report")]
    [Authorize]
    public class PPHBankExpenditureNoteReportController : Controller
    {
        private string ApiVersion = "1.0.0";
        private readonly IPPHBankExpenditureNoteReportFacade pphBankExpenditureNoteReportFacade;

        public PPHBankExpenditureNoteReportController(IPPHBankExpenditureNoteReportFacade pphBankExpenditureNoteReportFacade)
        {
            this.pphBankExpenditureNoteReportFacade = pphBankExpenditureNoteReportFacade;
        }

        [HttpGet]
        public ActionResult Get(int Size, int Page, string No, string UnitPaymentOrderNo, string InvoiceNo, string SupplierCode, DateTimeOffset? DateFrom, DateTimeOffset? DateTo)
        {
            int clientTimeZoneOffset = int.Parse(Request.Headers["x-timezone-offset"].First());

            ReadResponse<object> response = this.pphBankExpenditureNoteReportFacade.GetReport(Size, Page, No, UnitPaymentOrderNo, InvoiceNo, SupplierCode, DateFrom, DateTo, clientTimeZoneOffset);

            return Ok(new
            {
                apiVersion = ApiVersion,
                data = response.Data,
                info = new Dictionary<string, object>
                {
                    { "count", response.Data.Count },
                    { "total", response.TotalData },
                    { "order", response.Order },
                    { "page", Page },
                    { "size", Size }
                },
                message = General.OK_MESSAGE,
                statusCode = General.OK_STATUS_CODE
            });
        }
    }
}