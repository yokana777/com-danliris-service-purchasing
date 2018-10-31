using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInternNoteViewModel;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Com.Moonlay.NetCore.Lib.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.GarmentInternNoteControllers
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/garment-intern-notes")]
    [Authorize]
    public class GarmentInternNoteController : Controller
    {
        private string ApiVersion = "1.0.0";
        public readonly IServiceProvider serviceProvider;
        private readonly IMapper mapper;
        private readonly IGarmentInternNoteFacade facade;
        private readonly IdentityService identityService;

        public GarmentInternNoteController(IServiceProvider serviceProvider, IMapper mapper, IGarmentInternNoteFacade facade)
        {
            this.serviceProvider = serviceProvider;
            this.mapper = mapper;
            this.facade = facade;
            this.identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));
        }

        [HttpGet("by-user")]
        public IActionResult GetByUser(int page = 1, int size = 25, string order = "{}", string keyword = null, string filter = "{}")
        {
            try
            {
                identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;

                string filterUser = string.Concat("'CreatedBy':'", identityService.Username, "'");
                if (filter == null || !(filter.Trim().StartsWith("{") && filter.Trim().EndsWith("}")) || filter.Replace(" ", "").Equals("{}"))
                {
                    filter = string.Concat("{", filterUser, "}");
                }
                else
                {
                    filter = filter.Replace("}", string.Concat(", ", filterUser, "}"));
                }

                return Get(page, size, order, keyword, filter);
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }

        [HttpGet]
        public IActionResult Get(int page = 1, int size = 25, string order = "{}", string keyword = null, string filter = "{}")
        {
            try
            {
                identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;

                var Data = facade.Read(page, size, order, keyword, filter);

                var viewModel = mapper.Map<List<GarmentInternNoteViewModel>>(Data.Item1);

                List<object> listData = new List<object>();
                listData.AddRange(
                    viewModel.AsQueryable().Select(s => new
                    {
                        s.Id,
                        s.inNo,
                        s.inDate,
                        supplier = new { s.supplier.Name },
                        items = s.items.Select(i => new { i.garmentInvoice, i.detail }),
                        s.CreatedBy,
                        s.LastModifiedUtc
                    }).ToList()
                );

                var info = new Dictionary<string, object>
                    {
                        { "count", listData.Count },
                        { "total", Data.Item2 },
                        { "order", Data.Item3 },
                        { "page", page },
                        { "size", size }
                    };

                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.OK_STATUS_CODE, General.OK_MESSAGE)
                    .Ok(listData, info);
                return Ok(Result);
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                var model = facade.ReadById(id);
                var viewModel = mapper.Map<GarmentInternNoteViewModel>(model);
                if (viewModel == null)
                {
                    throw new Exception("Invalid Id");
                }

                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.OK_STATUS_CODE, General.OK_MESSAGE)
                    .Ok(viewModel);
                return Ok(Result);
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
        public async Task<IActionResult> Post([FromBody]GarmentInternNoteViewModel ViewModel)
        {
            try
            {
                identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;

                IValidateService validateService = (IValidateService)serviceProvider.GetService(typeof(IValidateService));

                validateService.Validate(ViewModel);

                var model = mapper.Map<GarmentInternNote>(ViewModel);

                await facade.Create(model, identityService.Username);

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

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody]GarmentInternNoteViewModel ViewModel)
        {
            try
            {
                identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;

                IValidateService validateService = (IValidateService)serviceProvider.GetService(typeof(IValidateService));

                validateService.Validate(ViewModel);

                var model = mapper.Map<GarmentInternNote>(ViewModel);

                await facade.Update(id, model, identityService.Username);

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
