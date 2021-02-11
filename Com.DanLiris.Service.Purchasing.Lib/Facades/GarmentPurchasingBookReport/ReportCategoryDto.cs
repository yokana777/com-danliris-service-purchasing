namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchasingBookReport
{
    public class ReportCategoryDto
    {
        public ReportCategoryDto(int categoryId, string categoryName, double amount)
        {
            CategoryId = categoryId;
            CategoryName = categoryName;
            Amount = amount;
        }

        public int CategoryId { get; private set; }
        public string CategoryName { get; private set; }
        public double Amount { get; private set; }
    }
}