using Com.DanLiris.Service.Purchasing.Lib.Facades.BudgetCashflowService;
using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.BudgetCashflowWorstCaseModel
{
    public class BudgetCashflowWorstCaseItem : StandardEntity
    {
        public BudgetCashflowWorstCaseItem()
        {

        }

        public BudgetCashflowWorstCaseItem(BudgetCashflowCategoryLayoutOrder layoutOrder, int currencyId, double currencyNominal, double nominal, int budgetCashflowWorstCaseId)
        {
            LayoutOrder = layoutOrder;
            CurrencyId = currencyId;
            CurrencyNominal = currencyNominal;
            Nominal = nominal;
            BudgetCashflowWorstCaseId = budgetCashflowWorstCaseId;
        }

        public BudgetCashflowCategoryLayoutOrder LayoutOrder { get; private set; }
        public int CurrencyId { get; private set; }
        public double CurrencyNominal { get; private set; }
        public double Nominal { get; private set; }
        public int BudgetCashflowWorstCaseId { get; private set; }
    }
}
