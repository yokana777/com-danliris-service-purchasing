using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSupplierBalanceDebtModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentSupplierBalanceDebtViewModel;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Com.Moonlay.NetCore.Lib.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.GarmentSupplierBalanceDebtControllers
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/balance-debt")]
    [Authorize]
    public class GarmentSupplierBalanceDebtController : Controller
    {
        private string ApiVersion = "1.0.0";
        private readonly IMapper mapper;
        private readonly IBalanceDebtFacade _facade;
        private readonly IServiceProvider serviceProvider;
        private readonly IdentityService identityService;
        //private readonly string ContentType = "application/vnd.openxmlformats";
        //private readonly string FileName = string.Concat("Error Log - ", typeof(GarmentSupplierBalanceDebt).Name, " ", DateTime.Now.ToString("dd MMM yyyy"), ".csv");
        public GarmentSupplierBalanceDebtController(IBalanceDebtFacade facade, IMapper mapper, IServiceProvider serviceProvider)
        {
            this._facade = facade;
            this.mapper = mapper;
            this.serviceProvider = serviceProvider;
            this.identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService)); ;
        }
        [HttpGet]
        public IActionResult Get(int page = 1, int size = 25, string order = "{}", string keyword = null, string filter = "{}")
        {
            try
            {
                identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;

                var Data = _facade.Read(page, size, order, keyword, filter);

                var viewModel = mapper.Map<List<GarmentSupplierBalanceDebtViewModel>>(Data.Item1);

                List<object> listData = new List<object>();
                listData.AddRange(
                    viewModel.AsQueryable().Select(s => new
                    {
                        s.Id,
                        s.codeRequirment,
                        currency = new { s.currency.Code },
                        supplier = new { s.supplier.Name, s.supplier.Code, s.supplier.Import },
                        s.dOCurrencyRate,
                        s.totalValas,
                        s.totalAmountIDR,
                        s.Year,
                        s.LastModifiedUtc,
                        s.LastModifiedBy

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
        //[HttpPost("upload")]
        //public IActionResult PostCSVFilec()
        //{
        //    try
        //    {
        //        if (Request.Form.Files.Count > 0)
        //        {
        //            identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;
        //            var UploadedFile = Request.Form.Files[0];
        //            StreamReader Reader = new StreamReader(UploadedFile.OpenReadStream());
        //            List<string> FileHeader = new List<string>(Reader.ReadLine().Split(","));
        //            var ValidHeader = _facade.CsvHeader.SequenceEqual(FileHeader, StringComparer.OrdinalIgnoreCase);

        //            if (ValidHeader)
        //            {
        //                Reader.DiscardBufferedData();
        //                Reader.BaseStream.Seek(0, SeekOrigin.Begin);
        //                Reader.BaseStream.Position = 0;
        //                CsvReader Csv = new CsvReader(Reader);
        //                Csv.Configuration.IgnoreQuotes = false;
        //                Csv.Configuration.Delimiter = ",";
        //                Csv.Configuration.RegisterClassMap<DebtMap>();
        //                Csv.Configuration.HeaderValidated = null;

        //                List<GarmentSupplierBalanceDebtViewModel> Data = Csv.GetRecords<GarmentSupplierBalanceDebtViewModel>().ToList();

        //                Tuple<bool, List<object>> Validated = _facade.UploadValidate(ref Data, Request.Form.ToList());

        //                Reader.Close();

        //                if (Validated.Item1) /* If Data Valid */
        //                {
        //                    List<GarmentSupplierBalanceDebt> data = mapper.Map<List<GarmentSupplierBalanceDebt>>(Data);

        //                    _facade.UploadData(data, identityService.Username);


        //                    Dictionary<string, object> Result =
        //                        new ResultFormatter(ApiVersion, General.CREATED_STATUS_CODE, General.OK_MESSAGE)
        //                        .Ok();
        //                    return Created(HttpContext.Request.Path, Result);

        //                }
        //                else
        //                {
        //                    using (MemoryStream memoryStream = new MemoryStream())
        //                    {
        //                        using (StreamWriter streamWriter = new StreamWriter(memoryStream))
        //                        using (CsvWriter csvWriter = new CsvWriter(streamWriter))
        //                        {
        //                            csvWriter.WriteRecords(Validated.Item2);
        //                        }

        //                        return File(memoryStream.ToArray(), ContentType, FileName);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                Dictionary<string, object> Result =
        //                   new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, "The header row of CSV file is not valid")
        //                   .Fail();

        //                return NotFound(Result);
        //            }
        //        }
        //        else
        //        {
        //            Dictionary<string, object> Result =
        //                new ResultFormatter(ApiVersion, General.BAD_REQUEST_STATUS_CODE, "File not found")
        //                    .Fail();
        //            return BadRequest(Result);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Dictionary<string, object> Result =
        //           new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
        //           .Fail();

        //        return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
        //    }
        //}
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]GarmentSupplierBalanceDebtViewModel ViewModel)
        {
            try
            {
                identityService.Username = User.Claims.Single(p => p.Type.Equals("username")).Value;

                IValidateService validateService = (IValidateService)serviceProvider.GetService(typeof(IValidateService));

                validateService.Validate(ViewModel);

                var model = mapper.Map<GarmentSupplierBalanceDebt>(ViewModel);

                await _facade.Create(model, identityService.Username);

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
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                var model = _facade.ReadById(id);
                var viewModel = mapper.Map<GarmentSupplierBalanceDebtViewModel>(model);
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
        [HttpGet("loader")]
        public IActionResult GetLoader(int page = 1, int size = 25, int year = 0, string order = "{}", string keyword = null, string filter = "{}", string select = "{}", string search = "[]")
        {
            try
            {
                int Year = year != 0 ? year : DateTime.Now.Year;
                var Data = _facade.ReadLoader(page, size, order, Year, keyword, filter, select, search);

                var info = new Dictionary<string, object>
                    {
                        { "count", Data.Data.Count },
                        { "total", Data.TotalData },
                        { "order", Data.Order },
                        { "page", page },
                        { "size", size }
                    };

                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.OK_STATUS_CODE, General.OK_MESSAGE)
                    .Ok(Data.Data, info);
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
    }
}
