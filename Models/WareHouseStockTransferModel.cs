using System.ComponentModel.DataAnnotations;

namespace IMS2.Models
{
    public class WareHouseStockTransferModel
    {
        public long FromWareHouseID { get; set; }
        public long ItemID { get; set; }
        public long ToWareHouseID { get; set; }
        public int Stock { get; set; }
        public string IMEINos { get; set; }
        public string Remarks { get; set; }
        public long AdminUserID { get; set; }
        public string InvoiceNo { get; set; }
        public string InvoiceFilePath { get; set; }
    }

    public class WareHouseModel
    {
        public long ID { get; set; }
        public string? Code { get; set; }
        public long UserID { get; set; }
    }

    public class ItemListModel
    {
        public long ID { get; set; }
        public string? Name { get; set; }
    }

    public class WareHouseStock
    {
        [Key]
        public long WareHouseID { get; set; }
        public long ItemID { get; set; }
        public long Qty { get; set; }
    }

    public class StockRequest
    {
        public long WareHouseID { get; set; }
        public long ItemID { get; set; }
    }
}
