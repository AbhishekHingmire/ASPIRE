using IMS2.Helpers;
using IMS2.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Drawing;
using IMS2.Models;
using QRCoder;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using Rotativa.AspNetCore;

namespace IMS2.Controllers
{
    public class PrintBranchSaleInvoiceController : Controller
    {
        private readonly IPrintBranchSaleInvoice _printBranchSaleInvoiceRepository;
        private readonly IImportSalesOrder _importSalesOrderRepository;
        private readonly ISettings _settingRepository;

        public PrintBranchSaleInvoiceController(
            IImportSalesOrder importSalesOrderRepository,
            ISettings settingsRepository,
            IPrintBranchSaleInvoice printBranchSaleInvoiceRepository)
        {
            _importSalesOrderRepository = importSalesOrderRepository;
            _settingRepository = settingsRepository;
            _printBranchSaleInvoiceRepository = printBranchSaleInvoiceRepository;
        }

        public async Task<IActionResult> Index()
        {
            var name = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            long userId = UserHelper.GetUserId(User);

            var (branchTypeID, branchID) = await UserHelper.GetUserBranchDetailsAsync(name, _importSalesOrderRepository);

            var mfiList = await UserHelper.GetMFIListAsync(userId, _settingRepository);

            ViewBag.MFICBO = mfiList;
            ViewBag.Name = name;
            ViewBag.BranchType = branchTypeID;

            return View();
        }

        public IActionResult SetBranchSalePrintInvoice(string invoiceNo)
        {
            try
            {
                var partnerIdCookie = Request.Cookies["SelectedMFI"];
                if (string.IsNullOrEmpty(partnerIdCookie) || !long.TryParse(partnerIdCookie, out long partnerId))
                {
                    return BadRequest("Invalid PartnerID cookie.");
                }

                var result = PrintBranchSaleInvoiceByInvoiceNo(partnerId, invoiceNo);
                string Result = result.ToString() ??"";

                var pdfResult = new ViewAsPdf(Result, new { PartnerID = partnerId, InvNo = invoiceNo })
                {
                    FileName = $"Invoice_{invoiceNo}.pdf",
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                    PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                    PageMargins = { Left = 1, Right = 1 }
                };

                return pdfResult;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error.");
            }
        }

        public IActionResult PrintBranchSaleInvoiceByInvoiceNo(long PartnerID, string InvNo)
        {
            try
            {
                var partnerIdCookie = Request.Cookies["SelectedMFI"];
                if (string.IsNullOrEmpty(partnerIdCookie) || !long.TryParse(partnerIdCookie, out long partnerId))
                {
                    return BadRequest("Invalid PartnerID cookie.");
                }

                var sb = new System.Text.StringBuilder();
                sb.Append("EXEC api_Admin_GetOrderDetailsForBranchSaleInvoice");
                sb.Append(string.Format(" @PartnerID = {0}, @InvoiceNo='{1}'", partnerId, InvNo));
                var ldts = _importSalesOrderRepository.ExecSQL(sb.ToString());

                // Check if ldts has at least one DataTable and that it contains rows
                if ((ldts == null || ldts.Count == 0) ||
                    (ldts[0].Rows.Count == 0 && (ldts.Count < 2 || ldts[1].Rows.Count == 0)))
                {
                    return NotFound("No invoice data found.");
                }

                // Initialize invoice object
                var Inv = new InvoiceByOrderNo();

                // Check if the first DataTable contains any rows before accessing
                if (ldts[0].Rows.Count > 0)
                {
                    Inv.InvoiceNo = ldts[0].Rows[0][0].ToString() ?? "";
                    Inv.InvoiceDate = ldts[0].Rows[0][1].ToString() ?? "";
                    Inv.OrderNo = ldts[0].Rows[0][20].ToString() ?? "";
                    Inv.BillToAddress = ldts[0].Rows[0][2].ToString() ?? "";
                    Inv.ShipToAddress = ldts[0].Rows[0][3].ToString() ?? "";
                    Inv.StateName = ldts[0].Rows[0][4].ToString() ?? "";

                    List<ItemListByOrderNo> itemDetails = new List<ItemListByOrderNo>();
                    for (var i = 0; i < ldts[0].Rows.Count; i++)
                    {
                        itemDetails.Add(new ItemListByOrderNo
                        {
                            ItemName = ldts[0].Rows[i][5].ToString() ?? "",
                            ItemHSNCode = ldts[0].Rows[i][6].ToString() ?? "",
                            ItemMRP = Convert.ToDecimal(ldts[0].Rows[i][7]),
                            ItemPrice = Convert.ToDecimal(ldts[0].Rows[i][8]),
                            GSTPercent = Convert.ToDecimal(ldts[0].Rows[i][9]),
                            GSTAmount = Convert.ToDecimal(ldts[0].Rows[i][10]),
                            ItemRate = Convert.ToDecimal(ldts[0].Rows[i][11]),
                            ItemQty = Convert.ToInt32(ldts[0].Rows[i][12]),
                            ItemAmount = Convert.ToDecimal(ldts[0].Rows[i][13])
                        });
                    }
                    Inv.ItemDetails = itemDetails;

                    // Additional fields that depend on ldts[0]
                    Inv.OrderDate = ldts[0].Rows[0][21].ToString();
                    Inv.BillFromAddress = ldts[0].Rows[0][22].ToString() ?? "";
                    Inv.BillFromGSTNo = ldts[0].Rows[0][23].ToString() ?? "";
                    Inv.Channel = ldts[0].Rows[0][24].ToString() ?? "";
                    Inv.ClientID = ldts[0].Rows[0][25].ToString() ?? "";
                    Inv.LoanAppNo = ldts[0].Rows[0][26].ToString() ?? "";
                    Inv.StateCode = ldts[0].Rows[0][27].ToString() ?? "";
                    Inv.IMEINo = ldts[0].Rows[0][28].ToString() ?? "";
                }

                // Validate the second DataTable
                if (ldts.Count > 1 && ldts[1].Rows.Count > 0)
                {
                    Inv.TotalCGST = !Convert.IsDBNull(ldts[1].Rows[0][1]) ? Convert.ToDecimal(ldts[1].Rows[0][1]) : 0;
                    Inv.TotalSGST = !Convert.IsDBNull(ldts[1].Rows[0][2]) ? Convert.ToDecimal(ldts[1].Rows[0][2]) : 0;
                    Inv.TotalIGST = !Convert.IsDBNull(ldts[1].Rows[0][3]) ? Convert.ToDecimal(ldts[1].Rows[0][3]) : 0;
                    Inv.TotalAmount = !Convert.IsDBNull(ldts[1].Rows[0][0]) ? Convert.ToDecimal(ldts[1].Rows[0][0]) : 0;
                    Inv.AmountInWords = !Convert.IsDBNull(ldts[1].Rows[0][4]) ? ldts[1].Rows[0][4].ToString() : "";
                }

                // Generate QR code
                string qrcode = $"upi://pay?pa=aspireinnovative@icici&pn=aspireinnovative&tn={Inv.InvoiceNo}&am={Decimal.Round(Inv.TotalAmount)}&cu=INR";
                using (MemoryStream ms = new MemoryStream())
                {
                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeGenerator.QRCode qrCode = qrGenerator.CreateQrCode(qrcode, QRCodeGenerator.ECCLevel.Q);
                    using (Bitmap bitMap = qrCode.GetGraphic(20))
                    {
                        bitMap.Save(ms, ImageFormat.Png);
                        Inv.QRCode = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
                    }
                }

                return View(Inv);
            }
            catch (Exception ex)
            {
                // Log the exception (ex) if needed
                return StatusCode(500, "Internal server error.");
            }
        }


    }
}
