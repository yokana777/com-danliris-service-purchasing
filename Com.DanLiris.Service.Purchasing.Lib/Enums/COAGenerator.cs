namespace Com.DanLiris.Service.Purchasing.Lib.Enums
{
    public static class COAGenerator
    {
        public const string HUTANG_USAHA_LOKAL = "3010";
        public const string HUTANG_USAHA_IMPOR = "3020";

        public const string HUTANG_USAHA_OPERASIONAL = "01";
        public const string HUTANG_USAHA_INVESTASI = "02";

        public const string HUTANG_USAHA_DIVISI_SPINNING = "1";
        public const string HUTANG_USAHA_DIVISI_WEAVING = "2";
        public const string HUTANG_USAHA_DIVISI_FINISHINGPRINTING = "3";
        public const string HUTANG_USAHA_DIVISI_GARMENT = "5";

        public static string GetDebtCOA(bool isImport, string division)
        {
            var result = "";

            if (isImport)
                result += HUTANG_USAHA_IMPOR + "." + HUTANG_USAHA_OPERASIONAL;
            else
                result += HUTANG_USAHA_LOKAL + "." + HUTANG_USAHA_OPERASIONAL;

            switch (division.ToUpper().Replace(" ", ""))
            {
                case "SPINNING":
                    result += "." + HUTANG_USAHA_DIVISI_SPINNING + ".00";
                    break;
                case "WEAVING":
                    result += "." + HUTANG_USAHA_DIVISI_WEAVING + ".00";
                    break;
                case "FINISHING&PRINTING":
                    result += "." + HUTANG_USAHA_DIVISI_FINISHINGPRINTING + ".00";
                    break;
                case "GARMENT":
                    result += "." + HUTANG_USAHA_DIVISI_GARMENT + ".00";
                    break;
            }

            return result;
        }
    }
}
