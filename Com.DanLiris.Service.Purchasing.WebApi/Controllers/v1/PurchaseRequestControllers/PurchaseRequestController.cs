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

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.PurchaseRequestControllers
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/purchase-requests")]
    public class PurchaseRequestController : Controller
    {
        private readonly IMapper _mapper;
        private readonly PurchaseRequestFacade _facade;
        public PurchaseRequestController(IMapper mapper, PurchaseRequestFacade facade)
        {
            _mapper = mapper;
            _facade = facade;
        }

        [HttpGet]
        public IActionResult Get()
        {
            /* TODO API Result */

            /* Dibawah ini hanya dummy */

            return Ok(_mapper.Map<List<PurchaseRequestViewModel>>(_facade.Read()));
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            /* TODO API Result */

            /* Dibawah ini hanya dummy */

            return Ok(_mapper.Map<PurchaseRequestViewModel>(_facade.ReadById(id)));
        }

        [HttpPost]
        public IActionResult Post([FromBody]PurchaseRequestViewModel vm)
        {
            PurchaseRequest m = _mapper.Map<PurchaseRequest>(vm);

            int Result = _facade.Create(m);

            /* TODO API Result */

            /* Dibawah ini hanya dummy */

            if (Result.Equals(0))
            {
                return StatusCode(500);
            }
            else
            {
                return Ok();
            }
        }

        [HttpPost("post")]
        public IActionResult PRPost([FromBody]List<PurchaseRequestViewModel> ListPurchaseRequestViewModel)
        {
            try
            {
                _facade.PRPost(
                    ListPurchaseRequestViewModel.Select(vm => _mapper.Map<PurchaseRequest>(vm)).ToList(),
                    User.Claims.Single(p => p.Type.Equals("username")).Value
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
            try
            {
                _facade.PRUnpost(id, User.Claims.Single(p => p.Type.Equals("username")).Value);

                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE);
            }
        }

    }
}
