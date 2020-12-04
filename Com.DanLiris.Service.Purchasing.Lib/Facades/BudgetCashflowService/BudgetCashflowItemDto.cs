using Com.DanLiris.Service.Purchasing.Lib.Facades.DebtAndDispositionSummary;

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

        public int Id { get; internal set; }
        public int CurrencyId { get; internal set; }
        public double CurrencyNominal { get; internal set; }
        public double Nominal { get; internal set; }
        public BudgetCashflowCategoryLayoutOrder LayoutOrder { get; internal set; }
    }
}