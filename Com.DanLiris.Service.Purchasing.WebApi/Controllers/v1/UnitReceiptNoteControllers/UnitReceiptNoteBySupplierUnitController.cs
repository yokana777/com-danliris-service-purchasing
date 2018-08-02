using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Facades.UnitReceiptNoteFacade;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.PDFTemplates;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNoteViewModel;
using Com.DanLiris.Service.Purchasing.WebApi.Helpers;
using Com.Moonlay.NetCore.Lib.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.UnitReceiptNoteControllers
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/unit-receipt-notes/by-supplier-unit")]
    [Authorize]
    public class UnitReceiptNoteBySupplierUnitController : Controller
    {
        private string ApiVersion = "1.0.0";
        private readonly IMapper _mapper;
        private readonly UnitReceiptNoteFacade _facade;
        private readonly IdentityService identityService;

        public UnitReceiptNoteBySupplierUnitController(IMapper mapper, UnitReceiptNoteFacade facade, IdentityService identityService)
        {
            _mapper = mapper;
            _facade = facade;
            this.identityService = identityService;
        }

        [HttpGet]
        public IActionResult GetBySupplierUnit(int page = 1, int size = 25, string order = "{}", string keyword = null, string filter = "{}")
        {
            var Data = _facade.ReadBySupplierUnit(page, size, order, keyword, filter);

            var newData = _mapper.Map<List<UnitReceiptNoteViewModel>>(Data.Item1);

            List<object> listData = new List<object>();
            listData.AddRange(
                newData.AsQueryable().Select(s => new
                {
                    s._id,
                    s.no,
                    s.date,
                    s.supplier,
                    s.doId,
                    s.doNo,
                    unit = new
                    {
                        division = new { s.unit.division.name },
                        s.unit.name
                    },
                    s.items
                }).ToList()
            );

            return Ok(new
            {
                apiVersion = ApiVersion,
                statusCode = General.OK_STATUS_CODE,
                message = General.OK_MESSAGE,
                data = listData,
                info = new Dictionary<string, object>
                {
                    { "count", listData.Count },
                    { "total", Data.Item2 },
                    { "order", Data.Item3 },
                    { "page", page },
                    { "size", size }
                },
            });
        }
    }
}
