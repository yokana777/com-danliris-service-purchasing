using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.PurchaseRequestViewModel;
using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Models.PurchaseRequestModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Lib.Facades;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Services;

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.PurchaseRequestControllers
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/purchase-requests")]
    [Authorize]
    public class PurchaseRequestController : Controller
    {
        private readonly IMapper _mapper;
        private readonly PurchaseRequestFacade _facade;
        private readonly IdentityService identityService;

        public PurchaseRequestController(IMapper mapper, PurchaseRequestFacade facade, IdentityService identityService)
        {
            _mapper = mapper;
            _facade = facade;
            this.identityService = identityService;
        }

        //[HttpGet]
        //public IActionResult Get()
        //{
        //    /* TODO API Result */

        //    /* Dibawah ini hanya dummy */

        //    return Ok(_mapper.Map<List<PurchaseRequestViewModel>>(_facade.Read()));
        //}

        //[HttpGet("{id}")]
        //public IActionResult Get(int id)
        //{
        //    /* TODO API Result */

        //    /* Dibawah ini hanya dummy */

        //    return Ok(_mapper.Map<PurchaseRequestViewModel>(_facade.ReadById(id)));
        //}

        //[HttpPost]
        //public IActionResult Post([FromBody]PurchaseRequestViewModel vm)
        //{
        //    PurchaseRequest m = _mapper.Map<PurchaseRequest>(vm);

        //    int Result = _facade.Create(m);

        //    /* TODO API Result */

        //    /* Dibawah ini hanya dummy */

        //    if (Result.Equals(0))
        //    {
        //        return StatusCode(500);
        //    }
        //    else
        //    {
        //        return Ok();
        //    }
        //}

        [HttpPost("post")]
        public IActionResult PRPost([FromBody]List<PurchaseRequestViewModel> ListPurchaseRequestViewModel)
        {
            identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;
            try
            {
                _facade.PRPost(
                    ListPurchaseRequestViewModel.Select(vm => _mapper.Map<PurchaseRequest>(vm)).ToList(), identityService.Username
                );

                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE);
            }
        }

        [HttpPut("unpost/{id}")]
        public IActionResult PRUnpost([FromRoute]int id)
        {
            identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;

            try
            {
                _facade.PRUnpost(id, identityService.Username);

                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE);
            }
        }

    }
}
