using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.WebApi.Helpers
{
    public class BasePost<TViewModel, TFacade> : Controller
        where TFacade : ICreateable
        where TViewModel : BaseViewModel
    {
        private readonly TFacade facade;
        private readonly string apiVersion;
        private readonly ValidateService validateService;
        private readonly string requestPath;

        public BasePost(TFacade facade, string apiVersion, ValidateService validateService, string requestPath)
        {
            this.facade = facade;
            this.apiVersion = apiVersion;
            this.validateService = validateService;
            this.requestPath = requestPath;
        }

        public async Task<ActionResult> Post(TViewModel viewModel)
        {
            try
            {
                this.validateService.Validate(viewModel);

                dynamic model = viewModel.ToModel();
                int id;

                try
                {
                    id = model.Id;
                }
                catch (RuntimeBinderException)
                {
                    id = 0;
                }

                await facade.Create(model);

                Dictionary<string, object> Result =
                    new ResultFormatter(apiVersion, General.CREATED_STATUS_CODE, General.OK_MESSAGE)
                    .Ok();
                return Created(String.Concat(this.requestPath, "/", id), Result);
            }
            catch (ServiceValidationExeption e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(apiVersion, General.BAD_REQUEST_STATUS_CODE, General.BAD_REQUEST_MESSAGE)
                    .Fail(e);
                return BadRequest(Result);
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(apiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }
    }
}
