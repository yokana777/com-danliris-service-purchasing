using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Facades.BankExpenditureNoteFacades;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.BankExpenditureNote;
using Com.DanLiris.Service.Purchasing.Lib.Models.BankExpenditureNoteModel;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Com.Moonlay.NetCore.Lib.Service;

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.BankExpenditureNote
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/bank-expenditure-notes")]
    [Authorize]
    public class BankExpenditureNoteController : Controller
    {
        private readonly string ApiVersion = "1.0.0";
        private readonly IMapper mapper;
        private readonly BankExpenditureNoteFacade facade;
        private readonly IdentityService identityService;

        public BankExpenditureNoteController(IMapper mapper, BankExpenditureNoteFacade facade, IdentityService identityService)
        {
            this.mapper = mapper;
            this.facade = facade;
            this.identityService = identityService;
        }


        [HttpGet]
        public ActionResult Get(int page = 1, int size = 25, string order = "{}", string keyword = null, string filter = "{}")
        {
            return new BaseGet<BankExpenditureNoteFacade>(facade)
                .Get(page, size, order, keyword, filter);
        }

        [HttpGet("{Id}")]
        public IActionResult GetById([FromRoute] int Id)
        {
            try
            {
                var model = facade.ReadById(Id);
                BankExpenditureNoteViewModel viewModel = mapper.Map<BankExpenditureNoteViewModel>(model);

                if (viewModel == null)
                {
                    Dictionary<string, object> Result =
                        new ResultFormatter(ApiVersion, General.NOT_FOUND_STATUS_CODE, General.NOT_FOUND_MESSAGE)
                        .Fail();
                    return NotFound(Result);
                }

                return Ok(new
                {
                    apiVersion = ApiVersion,
                    statusCode = General.OK_STATUS_CODE,
                    message = General.OK_MESSAGE,
                    data = viewModel,
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
        public async Task<IActionResult> Post([FromBody] BankExpenditureNoteViewModel viewModel)
        {
            identityService.Token = Request.Headers["Authorization"].First().Replace("Bearer ", "");
            identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;

            ValidateService validateService = (ValidateService)facade.serviceProvider.GetService(typeof(ValidateService));

            try
            {
                validateService.Validate(viewModel);

                BankExpenditureNoteModel model = mapper.Map<BankExpenditureNoteModel>(viewModel);

                int result = await facade.Create(model, identityService.Username);

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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute]int id)
        {
            identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;

            try
            {
                await facade.Delete(id, identityService.Username);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute]int id, [FromBody]BankExpenditureNoteViewModel vm)
        {
            identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;

            BankExpenditureNoteModel m = mapper.Map<BankExpenditureNoteModel>(vm);

            ValidateService validateService = (ValidateService)facade.serviceProvider.GetService(typeof(ValidateService));

            try
            {
                validateService.Validate(vm);

                int result = await facade.Update(id, m, identityService.Username);

                return NoContent();
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
    }
}
