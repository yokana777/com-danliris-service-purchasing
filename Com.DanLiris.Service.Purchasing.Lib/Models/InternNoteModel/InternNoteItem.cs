using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.InternNoteModel
{
    public class InternNoteItem : StandardEntity<long>
    {
        public string INVNOId { get; set; }
        public string INVName { get; set; }
        public DateTimeOffset INVDate { get; set; }
        public double TotalAmount { get; set; }
        public virtual long INNo { get; set; }
        public virtual ICollection<InternNoteDetail> Details { get; set; }

        public virtual long GarmentINId { get; set; }
        [ForeignKey("INNo")]
        public virtual InternNote InternNote { get; set; }
    }
}
