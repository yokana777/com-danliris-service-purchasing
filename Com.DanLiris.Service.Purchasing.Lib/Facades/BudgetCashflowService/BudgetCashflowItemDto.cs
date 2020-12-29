using Com.DanLiris.Service.Purchasing.Lib.Facades.DebtAndDispositionSummary;
using Com.DanLiris.Service.Purchasing.Lib.Utilities;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.BudgetCashflowService
{
    public class BudgetCashflowItemDto
    {
        public BudgetCashflowItemDto()
        {

        }

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

        public BudgetCashflowItemDto(int currencyId, double currencyNominal, double nominal, double actualNominal, double bestCaseCurrencyNominal, double bestCaseNominal, double bestCaseActualNominal, BudgetCashflowCategoryLayoutOrder layoutOrder)
        {
            CurrencyId = currencyId;
            CurrencyNominal = currencyNominal;
            Nominal = nominal;
            ActualNominal = actualNominal;
            BestCaseCurrencyNominal = bestCaseCurrencyNominal;
            BestCaseNominal = bestCaseNominal;
            BestCaseActualNominal = bestCaseActualNominal;
            LayoutOrder = layoutOrder;
        }

        public int Id { get; set; }
        public int CurrencyId { get; set; }
        public double CurrencyNominal { get; set; }
        public double ActualNominal { get; set; }
        public double BestCaseCurrencyNominal { get; set; }
        public double BestCaseNominal { get; set; }
        public double BestCaseActualNominal { get; set; }
        public double Nominal { get; set; }
        public BudgetCashflowCategoryLayoutOrder LayoutOrder { get; set; }
        public string LayoutName { get; set; }
    }
}