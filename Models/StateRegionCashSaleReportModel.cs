using System;
using System.ComponentModel.DataAnnotations;

namespace IMS2.Models
{
    public class StateRegionCashSaleReportModel
    {
        [Key]
        public string CashReciptNo { get; set; }
        public string CashSaleDate { get; set; }
        public string TotalAmount { get; set; }
        public string State { get; set; }
        public string Region { get; set; }
        public string Branch { get; set; }
        public string Item { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string AddressZip { get; set; }
        public string AadharNo { get; set; }
        public string EmployeeCode { get; set; }
        public string InvoiceDate { get; set; }
        public string InvoiceNo { get; set; }
        public string OrderStatus { get; set; }
        public DateTime DeliveryDate { get; set; }
        public DateTime DisbursementDate { get; set; }
    }
}
