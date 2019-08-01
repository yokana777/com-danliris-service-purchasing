using Com.DanLiris.Service.Purchasing.Lib.Utilities.CacheManager;
using FluentScheduler;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.WebApi.SchedulerJobs
{
    public class MasterRegistry : Registry
    {
        public MasterRegistry(IServiceProvider serviceProvider)
        {
            var coreService = serviceProvider.GetService<ICoreData>();
            Schedule(() =>
            {
                coreService.SetCategoryCOA();
                coreService.SetDivisionCOA();
                coreService.SetUnitCOA();
            }).ToRunNow().AndEvery(12).Hours();
            
        }
    }
}
