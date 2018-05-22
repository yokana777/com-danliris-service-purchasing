using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels
{
    public class UnitViewModel : BasicViewModel
    {
        public string _id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
    }
}
