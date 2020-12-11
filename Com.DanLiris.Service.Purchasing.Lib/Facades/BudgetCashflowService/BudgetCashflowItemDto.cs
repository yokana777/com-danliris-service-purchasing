using Com.DanLiris.Service.Purchasing.Lib.Facades.DebtAndDispositionSummary;
using Com.DanLiris.Service.Purchasing.Lib.Utilities;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.BudgetCashflowService
{
    public class BudgetCashflowItemDto
    {
        public BudgetCashflowItemDto(int id, int currencyId, double currencyNominal, double nominal, double actualNominal, BudgetCashflowCategoryLayoutOrder layoutOrder)
        {
            Id = id;
            CurrencyId = currencyId;
            CurrencyNominal = currencyNominal;
            Nominal = nominal;
            ActualNominal = actualNominal;
            LayoutOrder = layoutOrder;
            LayoutName = layoutOrder.ToDescriptionString();
        }

        public BudgetCashflowItemDto(string currencyIdString, string currencyCode, double currencyRate, double total, BudgetCashflowCategoryLayoutOrder layoutOrder)
        {
            int.TryParse(currencyIdString, out var currencyId);
            CurrencyId = currencyId;
            if (currencyCode != "IDR")
            {
                CurrencyNominal = total;
                ActualNominal = total * currencyRate;
            }
            else
            {
                Nominal = total;
                ActualNominal = total;
            }

            LayoutOrder = layoutOrder;
            LayoutName = layoutOrder.ToDescriptionString();
        }

        public int Id { get; private set; }
        public int CurrencyId { get; private set; }
        public double CurrencyNominal { get; private set; }
        public double ActualNominal { get; private set; }
        public double Nominal { get; private set; }
        public BudgetCashflowCategoryLayoutOrder LayoutOrder { get; private set; }
        public string LayoutName { get; private set; }
    }
}