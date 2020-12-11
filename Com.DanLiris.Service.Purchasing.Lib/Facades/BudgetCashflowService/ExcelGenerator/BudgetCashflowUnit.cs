using Com.DanLiris.Service.Purchasing.Lib.Enums;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.BudgetCashflowService.ExcelGenerator
{
    public class BudgetCashflowUnit : IBudgetCashflowUnit
    {
        private readonly IBudgetCashflowService _budgetCashflowService;
        private readonly IdentityService _identityService;
        private readonly List<BudgetingCategoryDto> _budgetingCategories;

        public BudgetCashflowUnit(IServiceProvider serviceProvider)
        {
            _budgetCashflowService = serviceProvider.GetService<IBudgetCashflowService>();
            _identityService = serviceProvider.GetService<IdentityService>();

            var cache = serviceProvider.GetService<IDistributedCache>();
        }
        public MemoryStream Generate(int unitId, DateTimeOffset dueDate)
        {
            throw new NotImplementedException();
        }
    }
}
