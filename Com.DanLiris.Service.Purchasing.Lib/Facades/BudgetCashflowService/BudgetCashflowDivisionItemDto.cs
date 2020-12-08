namespace Com.DanLiris.Service.Purchasing.Lib.Facades.BudgetCashflowService
{
    public class BudgetCashflowDivisionItemDto
    {
        public BudgetCashflowDivisionItemDto(string currencyIdString, string currencyCode, double currencyRate, string divisionIdString, string unitIdString, double total, BudgetCashflowCategoryLayoutOrder layoutOrder)
        {
            int.TryParse(currencyIdString, out var currencyId);
            CurrencyId = currencyId;

            int.TryParse(unitIdString, out var unitId);
            UnitId = unitId;

            int.TryParse(divisionIdString, out var divisionId);
            DivisionId = divisionId;

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
            LayoutName = layoutOrder.ToDescriptionString();
        }

        public int CurrencyId { get; private set; }
        public double CurrencyNominal { get; private set; }
        public double Nominal { get; private set; }
        public BudgetCashflowCategoryLayoutOrder LayoutOrder { get; private set; }
        public string LayoutName { get; private set; }
        public int UnitId { get; private set; }
        public int DivisionId { get; private set; }
    }
}