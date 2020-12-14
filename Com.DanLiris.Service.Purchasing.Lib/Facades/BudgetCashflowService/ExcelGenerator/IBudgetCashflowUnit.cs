using System;
using System.IO;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.BudgetCashflowService.ExcelGenerator
{
    public interface IBudgetCashflowUnit
    {
        MemoryStream Generate(int unitId, DateTimeOffset dueDate);
    }
}
