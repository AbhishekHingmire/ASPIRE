using DocumentFormat.OpenXml.Bibliography;
using IMS2.Models;
using IMS2.Repository.Services;
using Microsoft.EntityFrameworkCore;

namespace IMS2
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {
            this.Database.SetCommandTimeout(300);
        }

        public DbSet<Users> User { get; set; }
        public DbSet<RoleModel> Roles { get; set; }
        public DbSet<UserScreenRight> Screens { get; set; }
        public DbSet<UserScreen> ScreenRights { get; set; }
        public DbSet<UserDetails> UserDetails { get; set; }
        public DbSet<UserScreenRightsModel> UserCreeenRights { get; set; }
        public DbSet<UserScreenRightsViewModel> UserScreenRightsList { get; set; }
        public DbSet<UserCreationGetDetailsModel> UserCreationList { get; set; }
        public DbSet<UserScreenRights> UserScreenRight { get; set; }
        public DbSet<ScreenRightsModel> UserScreenRights { get; set; }
        public DbSet<USerMFIMapperIDs> UserMFI { get; set; }
        public DbSet<MFIList> Partner { get; set; }
        public DbSet<GetItemMasterModel> GetItemMasterModels { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<ItemMFIMapperModel> PartnerItems { get; set; }
        public DbSet<ItemCBO> ItemCBOs { get; set; }
        public DbSet<PartnerItems> PartnerItem { get; set; }
        public DbSet<ItemMasterModel> Item { get; set; }
        public DbSet<ItemViewModel> ItemMaster { get; set; }
        public DbSet<PurchaseOrderModel> PurchaseOrderList { get; set; }
        public DbSet<SalesReturnReportModel> SalesReturnReport { get; set; }
        public DbSet<UserMFIMapperModel> GetAllUserMFIList { get; set; }
        public DbSet<SettingsModel> Settings { get; set; }
        public DbSet<ScreenResult> ScreenResults { get; set; }
        public DbSet<WareHouseModel> WareHouse { get; set; }
        public DbSet<POTypes> POType { get; set; }
        public DbSet<POCompanys> POCompany { get; set; }
        public DbSet<Suppliers> Supplier { get; set; }
        public DbSet<WareHouseStock> WareHouseStock { get; set; }
        public DbSet<BranchModel> BranchList { get; set; }
        public DbSet<BranchTypeModel> BranchType { get; set; }
        public DbSet<ParentBranchModel> getBranchType { get; set; }
        public DbSet<TransactionModel> Transactions { get; set; }
        public DbSet<FailedRecord> FailedRecords { get; set; }
        public DbSet<BranchDetails> Branch { get; set; }
        public DbSet<BranchHierarchy> BranchHierarchy { get; set; }
        public DbSet<BranchStockModel> BranchStockModels { get; set; }
        public DbSet<StateRegionCashSaleReportModel> StateRegionCashSale { get; set; }
        public DbSet<BranchComplaintHOModel> BranchComplaintHO { get; set; }
        public DbSet<ERPOrders> ERPOrder { get; set; }
        public DbSet<Provinces> Province { get; set; }
        public DbSet<PODVerifyModel> PODDetails { get; set; }
        public DbSet<FileUrl> FileUrls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PODVerifyModel>().HasNoKey(); 
            base.OnModelCreating(modelBuilder);

            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<FileUrl>().HasNoKey();
        }

    }
}
