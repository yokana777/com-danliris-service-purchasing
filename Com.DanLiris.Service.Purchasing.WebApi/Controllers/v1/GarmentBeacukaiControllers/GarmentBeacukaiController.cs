using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.GarmentBeacukaiControllers
{
	[Produces("application/json")]
	[ApiVersion("1.0")]
	[Route("v{version:apiVersion}/garment-beacukai")]
	[Authorize]
	public class GarmentBeacukaiController : Controller
	{
		private string ApiVersion = "1.0.0";
		public readonly IServiceProvider serviceProvider;
		private readonly IMapper mapper;
		private readonly IGarmentBeacukaiFacade facade;
		private readonly IdentityService identityService;

		public GarmentBeacukaiController(IServiceProvider serviceProvider, IMapper mapper, IGarmentBeacukaiFacade facade)
		{
			this.serviceProvider = serviceProvider;
			this.mapper = mapper;
			this.facade = facade;
			this.identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));
		}

	}
}