using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.DailyBankTransaction;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.DailyBankTransaction;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Com.Moonlay.NetCore.Lib.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.DailyBankTransaction
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/daily-bank-transactions")]
    [Authorize]
    public class DailyBankTransactionControllers : Controller
    {
        private string ApiVersion = "1.0.0";
        public readonly IServiceProvider serviceProvider;
        private readonly IDailyBankTransactionFacade DailyBankTransactionFacade;
        private readonly IdentityService identityService;
        private readonly IMapper mapper;

        public DailyBankTransactionControllers(IServiceProvider serviceProvider, IDailyBankTransactionFacade DailyBankTransactionFacade, IMapper mapper)
        {
            this.serviceProvider = serviceProvider;
            this.DailyBankTransactionFacade = DailyBankTransactionFacade;
            this.mapper = mapper;
            identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));
        }

        [HttpGet]
        public IActionResult Get(int page = 1, int size = 25, string order = "{}", string keyword = null, string filter = "{}")
        {
            ReadResponse Response = DailyBankTransactionFacade.Read(page, size, order, keyword, filter);

            return Ok(new
            {
                apiVersion = "1.0.0",
                data = Response.Data,
                info = new Dictionary<string, object>
                {
                    { "count", Response.Data.Count },
                    { "total", Response.TotalData },
                    { "order", Response.Order },
                    { "page", page },
                    { "size", size }
                },
                message = General.OK_MESSAGE,
                statusCode = General.OK_STATUS_CODE
            });
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetById([FromRoute] int Id)
        {
            try
            {
                var model = await DailyBankTransactionFacade.ReadById(Id);
                DailyBankTransactionViewModel viewModel = mapper.Map<DailyBankTransactionViewModel>(model);

                if (model == null)
                {
                    Dictionary<string, object> Result =
                        new ResultFormatter(ApiVersion, General.NOT_FOUND_STATUS_CODE, General.NOT_FOUND_MESSAGE)
                        .Fail();
                    return NotFound(Result);
                }

                return Ok(new
                {
                    apiVersion = ApiVersion,
                    data = viewModel,
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

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] DailyBankTransactionViewModel viewModel)
        {
            identityService.Token = Request.Headers["Authorization"].First().Replace("Bearer ", "");
            identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;

            IValidateService validateService = (IValidateService)serviceProvider.GetService(typeof(IValidateService));

            try
            {
                validateService.Validate(viewModel);

                DailyBankTransactionModel model = mapper.Map<DailyBankTransactionModel>(viewModel);

                int result = await DailyBankTransactionFacade.Create(model, identityService.Username);

                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.CREATED_STATUS_CODE, General.OK_MESSAGE)
                    .Ok();
                return Created(String.Concat(Request.Path, "/", 0), Result);
            }
            catch (ServiceValidationExeption e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.BAD_REQUEST_STATUS_CODE, General.BAD_REQUEST_MESSAGE)
                    .Fail(e);
                return BadRequest(Result);
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }

        [HttpGet("mutation/report")]
        public IActionResult GetReport(string bankId, DateTimeOffset? dateFrom, DateTimeOffset? dateTo)
        {
            ReadResponse Result = DailyBankTransactionFacade.GetReport(bankId, dateFrom, dateTo);

            return Ok(new
            {
                apiVersion = "1.0.0",
                data = Result.Data,
                message = General.OK_MESSAGE,
                statusCode = General.OK_STATUS_CODE
            });
        }

        [HttpGet("mutation/report/download")]
        public IActionResult GetReportXls(string bankId, DateTimeOffset? dateFrom, DateTimeOffset? dateTo)
        {
            try
            {
                byte[] xlsInBytes;
                int offset = Convert.ToInt32(Request.Headers["x-timezone-offset"]);

                var xls = DailyBankTransactionFacade.GenerateExcel(bankId, dateFrom, dateTo, offset);

                string filename = String.Format("Mutasi Bank Harian - {0}.xlsx", DateTime.UtcNow.ToString("ddMMyyyy"));

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
