using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Facades;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.UnitPaymentOrderControllers
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/unit-payment-orders")]
    [Authorize]
    public class UnitPaymentOrderController : Controller
    {
        private string ApiVersion = "1.0.0";
        private readonly IMapper mapper;
        private readonly UnitPaymentOrderFacade facade;
        private readonly IdentityService identityService;

        public UnitPaymentOrderController(IMapper mapper, UnitPaymentOrderFacade facade, IdentityService identityService)
        {
            this.mapper = mapper;
            this.facade = facade;
            this.identityService = identityService;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var data = facade.ReadById(id);
            var newData = mapper.Map<UnitPaymentOrder>(data);
            return Ok();
        }
    }
}
