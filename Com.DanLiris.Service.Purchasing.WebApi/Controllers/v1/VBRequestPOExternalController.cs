using Com.DanLiris.Service.Purchasing.Lib.Facades.VBRequestPOExternal;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/vb-request-po-external")]
    [Authorize]
    public class VBRequestPOExternalController : Controller
    {
        private readonly IVBRequestPOExternalService _service;
        private const string ApiVersion = "1.0";

        public VBRequestPOExternalController(IVBRequestPOExternalService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] string keyword, [FromQuery] string division, [FromQuery] string currencyCode)
        {

            try
            {
                var result = _service.ReadPOExternal(keyword, division, currencyCode);
                return Ok(new
                {
                    apiVersion = ApiVersion,
                    statusCode = General.OK_STATUS_CODE,
                    message = General.OK_MESSAGE,
                    data = result,
                    info = new Dictionary<string, object>
                {
                    { "page", 1 },
                    { "size", 10 }
                },
                });
            }
            catch (Exception e)
            {
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, e.Message + " " + e.StackTrace);
            }
        }

        [HttpGet("spb")]
        public IActionResult GetSPB([FromQuery] string keyword, [FromQuery] string division, [FromQuery] string epoIds, [FromQuery] string currencyCode)
        {

            try
            {
                var epoIdList = JsonConvert.DeserializeObject<List<int>>(epoIds);
                var result = _service.ReadSPB(keyword, division, epoIdList, currencyCode);
                return Ok(new
                {
                    apiVersion = ApiVersion,
                    statusCode = General.OK_STATUS_CODE,
                    message = General.OK_MESSAGE,
                    data = result,
                    info = new Dictionary<string, object>
                {
                    { "page", 1 },
                    { "size", 10 }
                },
                });
            }
            catch (Exception e)
            {
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, e.Message + " " + e.StackTrace);
            }
        }

        [HttpPut("spb/{id}")]
        public IActionResult UpdateVBCreatedFlag([FromQuery] string division, [FromRoute] int id)
        {

            try
            {
                var result = _service.UpdateSPB(division, id);
                return Ok(new
                {
                    apiVersion = ApiVersion,
                    statusCode = General.OK_STATUS_CODE,
                    message = General.OK_MESSAGE,
                    data = result,
                    info = new Dictionary<string, object>
                {
                    { "page", 1 },
                    { "size", 10 }
                },
                });
            }
            catch (Exception e)
            {
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, e.Message + " " + e.StackTrace);
            }
        }
    }
}
