using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.InternalPurchaseOrderViewModel;
using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Models.InternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Lib.Facades.InternalPO;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.Moonlay.NetCore.Lib.Service;

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.InternalPurchaseOrderController
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/internal-purchase-orders")]
    public class InternalPurchaseOrderController : Controller
    {
        private string ApiVersion = "1.0.0";
        private readonly IMapper _mapper;
        private readonly InternalPurchaseOrderFacade _facade;
        private readonly IdentityService identityService;
        public InternalPurchaseOrderController(IMapper mapper, InternalPurchaseOrderFacade facade, IdentityService identityService)
        {
            _mapper = mapper;
            _facade = facade;
            this.identityService = identityService;
        }

        [HttpGet]
        public IActionResult Get(int page = 1, int size = 25, string order = "{}", string keyword = null, string filter = "{}")
        {
            //Tuple<List<object>, int, Dictionary<string, string>> Data = _facade.Read(page, size, order, keyword, filter);
            var Data = _facade.Read(page, size, order, keyword, filter);

            var newData = _mapper.Map<List<InternalPurchaseOrderViewModel>>(Data.Item1);
            List<object> listData = new List<object>();
            listData.AddRange(
                newData.AsQueryable().Select(s => new
                {
                    s._id,
                    s.poNo,
                    s.isoNo,
                    s.prId,
                    s.prNo,
                    s.prDate,
                    s.expectedDeliveryDate,
                    s.budget,
                    s.division,
                    s.unit,
                    s.category,
                    s.remark,
                    s.status,
                    s.isPosted,
                }).ToList()
            );

            return Ok(new
            {
                apiVersion = ApiVersion,
                statusCode = General.OK_STATUS_CODE,
                message = General.OK_MESSAGE,
                data = Data.Item1,
                info = new Dictionary<string, object>
                {
                    { "count", Data.Item1.Count },
                    { "total", Data.Item2 },
                    { "order", Data.Item3 },
                    { "page", page },
                    { "size", size }
                },
            });
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            /* TODO API Result */

            /* Dibawah ini hanya dummy */

            //return Ok(_mapper.Map<PurchaseRequestViewModel>(_facade.ReadById(id)));
            return Ok(new
            {
                apiVersion = ApiVersion,
                statusCode = General.OK_STATUS_CODE,
                message = General.OK_MESSAGE,
                data = _mapper.Map<InternalPurchaseOrderItemViewModel>(_facade.ReadById(id)),
            });
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]InternalPurchaseOrderViewModel vm)
        {
            identityService.Token = Request.Headers["Authorization"].First().Replace("Bearer ", "");
            identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;
            InternalPurchaseOrder m = _mapper.Map<InternalPurchaseOrder>(vm);
            ValidateService validateService = (ValidateService)_facade.serviceProvider.GetService(typeof(ValidateService));

            try
            {                
                validateService.Validate(vm);

                int result = await _facade.Create(m, identityService.Username);

                if (result.Equals(0))
                {
                    return StatusCode(500);
                }
                else
                {
                    return Ok();
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

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute]int id, [FromBody]InternalPurchaseOrderViewModel vm)
        {
            identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;

            InternalPurchaseOrder m = _mapper.Map<InternalPurchaseOrder>(vm);

            ValidateService validateService = (ValidateService)_facade.serviceProvider.GetService(typeof(ValidateService));

            try
            {
                validateService.Validate(vm);

                int result = await _facade.Update(id, m, identityService.Username);

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

        [HttpDelete("{id}")]
        public IActionResult Delete([FromRoute]int id)
        {
            identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;

            try
            {
                _facade.Delete(id, identityService.Username);

                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE);
            }
        }
    }
}
