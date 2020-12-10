using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.BudgetCashflowService
{
    public interface IBudgetCashflowService
    {
        List<BudgetCashflowItemDto> GetBudgetCashflowWorstCase(DateTimeOffset dueDate, int unitId);
        List<BudgetCashflowItemDto> GetBudgetCashflowUnit(BudgetCashflowCategoryLayoutOrder layoutOrder, int unitId, DateTimeOffset dueDate);
        BudgetCashflowDivisionDto GetBudgetCashflowDivision(BudgetCashflowCategoryLayoutOrder layoutOrder, int divisionId, DateTimeOffset dueDate);
        Task<int> UpsertWorstCaseBudgetCashflowUnit(WorstCaseBudgetCashflowFormDto form);
        Task<int> UpdateWorstCaseBudgetCashflowUnit(int year, int month, int unitId, WorstCaseBudgetCashflowFormDto form);
    }
}
