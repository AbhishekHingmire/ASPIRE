using System.ComponentModel;

namespace IMS2.Helpers
{
    public enum EnumScreenNames
    {
        PO,
        SO,
        Branch,
        Item,
        ItemMFIMapper,
        Setting,
        ImportSalesOrder,
        ImportSpecialSO,
        UpdateOrderStatus,
        UpdateInvoiceNo,
        Updatetransitfields,
        BookedOrdertoSO,
        BulkUploadIMEINo,
        BranchStockTransferAfterInvoice,
        ImportGRN,
        UploadDummyOrderReassignment,
        BranchStock,
        SalesReport,
        BranchComplaintHO,
        PODVerify,
        GITReport,
        DisbursementReport,
        StateRegionCashSaleReport
    }

    public enum EnumScreenRights
    {
        Read = 1,
        Create = 2,
        Update = 3,
        Delete = 4
    }

    public enum Roles
    {
        Branch,
        Region,
        HO, 
        State
    }
}
