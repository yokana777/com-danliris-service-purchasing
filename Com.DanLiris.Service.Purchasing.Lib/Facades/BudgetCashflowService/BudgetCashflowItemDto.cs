using Com.DanLiris.Service.Purchasing.Lib.Facades.DebtAndDispositionSummary;
using Com.DanLiris.Service.Purchasing.Lib.Utilities;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.BudgetCashflowService
{
    public class BudgetCashflowItemDto
    {
        public BudgetCashflowItemDto(int id, int currencyId, double currencyNominal, double nominal, BudgetCashflowCategoryLayoutOrder layoutOrder)
        {
            Id = id;
            CurrencyId = currencyId;
            CurrencyNominal = currencyNominal;
            Nominal = nominal;
            LayoutOrder = layoutOrder;
            LayoutName = layoutOrder.GetDisplayName();
        }

        public BudgetCashflowItemDto(string currencyIdString, string currencyCode, double currencyRate, double total, BudgetCashflowCategoryLayoutOrder layoutOrder)
        {
            int.TryParse(currencyIdString, out var currencyId);
            CurrencyId = currencyId;
            if (currencyCode != "IDR")
            {
                CurrencyNominal = total;
                Nominal = total * currencyRate;
            }
            else
            {
                Nominal = total;
            }

            LayoutOrder = layoutOrder;
        }

        public int Id { get; private set; }
        public int CurrencyId { get; private set; }
        public double CurrencyNominal { get; private set; }
        public double Nominal { get; private set; }
        public BudgetCashflowCategoryLayoutOrder LayoutOrder { get; private set; }
        public string LayoutName { get; private set; }
    }
}