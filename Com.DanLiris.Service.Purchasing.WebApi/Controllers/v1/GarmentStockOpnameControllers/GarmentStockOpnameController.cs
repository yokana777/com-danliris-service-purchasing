using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentStockOpnameFacades;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.GarmentStockOpnameControllers
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/garment-stock-opnames")]
    [Authorize]
    public class GarmentStockOpnameController : Controller
    {
        private readonly IGarmentStockOpnameFacade _facade;
        private readonly IdentityService _identityService;
        private const string ApiVersion = "1.0";

        public GarmentStockOpnameController(IGarmentStockOpnameFacade service, IServiceProvider serviceProvider)
        {
            _facade = service;
            _identityService = serviceProvider.GetService<IdentityService>();
        }

        private void VerifyUser()
        {
            _identityService.Username = User.Claims.ToArray().SingleOrDefault(p => p.Type.Equals("username")).Value;
            _identityService.Token = Request.Headers["Authorization"].FirstOrDefault().Replace("Bearer ", "");
            _identityService.TimezoneOffset = Convert.ToInt32(Request.Headers["x-timezone-offset"]);
        }

        [HttpGet]
        public IActionResult Get(int page = 1, int size = 25, string order = "{}", string keyword = null, string filter = "{}", string select = null, string search = "[]")
        {
            try
            {
                VerifyUser();

                var readResponse = _facade.Read(page, size, order, keyword, filter);

                var info = new Dictionary<string, object>
                {
                    { "count", readResponse.Data.Count },
                    { "total", readResponse.TotalData },
                    { "order", readResponse.Order },
                    { "page", page },
                    { "size", size }
                };

                Dictionary<string, object> Result = new ResultFormatter(ApiVersion, General.OK_STATUS_CODE, General.OK_MESSAGE)
                    .Ok(readResponse.Data, info);
                return Ok(Result);
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result = new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                VerifyUser();

                var readData = _facade.ReadById(id);

                Dictionary<string, object> Result = new ResultFormatter(ApiVersion, General.OK_STATUS_CODE, General.OK_MESSAGE)
                    .Ok(readData);
                return Ok(Result);
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result = new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }

        [HttpGet("download")]
        public IActionResult DownloadFile(DateTimeOffset? date, string unit, string storage, string storageName)
        {
            try
            {
                VerifyUser();

                if (date == null || unit == null || storage == null)
                {
                    throw new Exception("Semua filter harus diisi");
                }

                var stream = _facade.Download(date.Value, unit, storage, storageName);
                stream.Position = 0;

                FileStreamResult fileStreamResult = new FileStreamResult(stream, "application/excel");
                fileStreamResult.FileDownloadName = $"Garment Stock Opname {unit} {storageName} {date.Value.ToString("dd MMMM yyyy")}.xlsx";

                return fileStreamResult;
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result = new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail(e.Message);
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile()
        {
            try
            {
                VerifyUser();

                if (Request.Form.Files.Count > 0)
                {
                    var UploadedFile = Request.Form.Files[0];

                    await _facade.Upload(UploadedFile.OpenReadStream());

                    Dictionary<string, object> Result = new ResultFormatter(ApiVersion, General.CREATED_STATUS_CODE, General.OK_MESSAGE)
                        .Ok();
                    return Created(HttpContext.Request.Path, Result);
                }
                else
                {
                    throw new Exception("No Uploaded Files");
                }
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result = new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail(e.Message);
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }
    }
}
