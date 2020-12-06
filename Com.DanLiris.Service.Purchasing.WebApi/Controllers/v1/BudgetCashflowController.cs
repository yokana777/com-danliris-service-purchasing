using Com.DanLiris.Service.Purchasing.Lib.Facades.BudgetCashflowService;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Com.Moonlay.NetCore.Lib.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/budget-cashflows")]
    [Authorize]
    public class BudgetCashflowController : Controller
    {
        private readonly IBudgetCashflowService _service;
        private readonly IdentityService _identityService;
        private readonly IValidateService _validateService;
        private const string ApiVersion = "1.0";

        public BudgetCashflowController(IServiceProvider serviceProvider)
        {
            _service = serviceProvider.GetService<IBudgetCashflowService>();
            _identityService = serviceProvider.GetService<IdentityService>();
            _validateService = serviceProvider.GetService<IValidateService>();
        }

        private void VerifyUser()
        {
            _identityService.Username = User.Claims.ToArray().SingleOrDefault(p => p.Type.Equals("username")).Value;
            _identityService.Token = Request.Headers["Authorization"].FirstOrDefault().Replace("Bearer ", "");
            _identityService.TimezoneOffset = Convert.ToInt32(Request.Headers["x-timezone-offset"]);
        }

        [HttpGet("best-case")]
        public IActionResult GetBudgetCashflowBestCase([FromQuery] BudgetCashflowCategoryLayoutOrder layoutOrder, [FromQuery] int unitId, [FromQuery] DateTimeOffset dueDate)
        {

            try
            {
                VerifyUser();
                var result = _service.GetBudgetCashflowUnit(layoutOrder, unitId, dueDate);
                return Ok(new
                {
                    apiVersion = ApiVersion,
                    statusCode = General.OK_STATUS_CODE,
                    message = General.OK_MESSAGE,
                    data = result
                });
            }
            catch (Exception e)
            {
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, e.Message + " " + e.StackTrace);
            }
        }

        [HttpGet("worst-case")]
        public IActionResult GetBudgetCashflowWorstCase([FromQuery] int unitId, [FromQuery] DateTimeOffset dueDate)
        {

            try
            {
                VerifyUser();
                var result = _service.GetBudgetCashflowWorstCase(dueDate, unitId);
                return Ok(new
                {
                    apiVersion = ApiVersion,
                    statusCode = General.OK_STATUS_CODE,
                    message = General.OK_MESSAGE,
                    data = result
                });
            }
            catch (Exception e)
            {
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, e.Message + " " + e.StackTrace);
            }
        }

        [HttpPut("worst-case")]
        public async Task<IActionResult> Put([FromBody] WorstCaseBudgetCashflowFormDto form)
        {
            try
            {
                _validateService.Validate(form);

                var result = await _service.UpsertWorstCaseBudgetCashflowUnit(form);

                var response = new ResultFormatter(ApiVersion, General.CREATED_STATUS_CODE, General.OK_MESSAGE).Ok();
                return NoContent();
            }
            catch (ServiceValidationExeption e)
            {
                var response = new ResultFormatter(ApiVersion, General.BAD_REQUEST_STATUS_CODE, General.BAD_REQUEST_MESSAGE).Fail(e);
                return BadRequest(response);
            }
            catch (Exception e)
            {
                var response = new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message).Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, response);
            }
        }
    }
}
