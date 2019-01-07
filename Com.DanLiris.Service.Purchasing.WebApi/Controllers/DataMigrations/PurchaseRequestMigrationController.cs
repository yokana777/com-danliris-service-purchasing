using Com.DanLiris.Service.Purchasing.Data.Migration.Lib.MigrationServices;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.DataMigrations
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/migration/purchase-requests")]
    public class PurchaseRequestMigrationController : Controller
    {
        private readonly IPurchaseRequestMigrationService _service;

        public PurchaseRequestMigrationController(IPurchaseRequestMigrationService service)
        {
            _service = service;
        }

        [HttpGet("{startingNumber}/{numberOfBatch}")]
        public async Task<IActionResult> Get([FromRoute] int startingNumber, [FromRoute] int numberOfBatch)
        {
            try
            {
                var result = await _service.RunAsync(startingNumber, numberOfBatch);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
