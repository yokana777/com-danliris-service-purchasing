using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitPaymentCorrectionNoteViewModel;
using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentCorrectionNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Lib.Facades.UnitPaymentCorrectionNoteFacade;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.Moonlay.NetCore.Lib.Service;
namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.UnitPaymentCorrectionNoteController
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/internal-purchase-orders")]
    public class UnitPaymentQuantityCorrectionNoteController : Controller
    {
        private string ApiVersion = "1.0.0";
        private readonly IMapper _mapper;
        private readonly UnitPaymentQuantityCorrectionNoteFacade _facade;
        private readonly IdentityService identityService;
        public UnitPaymentQuantityCorrectionNoteController(IMapper mapper, UnitPaymentQuantityCorrectionNoteFacade facade, IdentityService identityService)
        {
            _mapper = mapper;
            _facade = facade;
            this.identityService = identityService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]UnitPaymentCorrectionNoteViewModel vm)
        {
            identityService.Token = Request.Headers["Authorization"].First().Replace("Bearer ", "");
            identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;
            UnitPaymentCorrectionNote m = _mapper.Map<UnitPaymentCorrectionNote>(vm);
            ValidateService validateService = (ValidateService)_facade.serviceProvider.GetService(typeof(ValidateService));

            try
            {
                validateService.Validate(vm);
                int clientTimeZoneOffset = int.Parse(Request.Headers["x-timezone-offset"].First());
                int result = await _facade.Create(m, identityService.Username, vm, clientTimeZoneOffset);

                if (result.Equals(0))
                {
                    return StatusCode(500);
                }
                else
                {
                    return Created(Request.Path, result);
                }
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
