using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using IMS2;
using IMS2.Repository;
using IMS2.Repository.Interfaces;
using IMS2.Repository.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add the database context using the connection string from appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMvc();

// Register the repository
builder.Services.AddScoped<IUserCreation, UserCreation>();
builder.Services.AddScoped<IUserScreenRights, UserScreenRights>();
builder.Services.AddScoped<ILogin, Login>();
builder.Services.AddScoped<ISettings, Settings>();
builder.Services.AddScoped<IItemMaster, ItemMaster>();
builder.Services.AddScoped<IItemMFIMapper, ItemMFIMapper>();
builder.Services.AddScoped<IPurchaseOrder, PurchaseOrder>();
builder.Services.AddScoped<ISalesReturnReport, SalesReturnReport>();
builder.Services.AddScoped<IUserMFIMapper, UserMFIMapper>();
builder.Services.AddScoped<IWareHouseStockTransfer, WareHouseStockTransfer>();
builder.Services.AddScoped<IBranch, Branch>();
builder.Services.AddScoped<IImportSalesOrder, ImportSalesOrder>();
builder.Services.AddScoped<IImportSpecialTransaction, ImportSpecialTransaction>();
builder.Services.AddScoped<IUpdateOrderStatus, UpdateOrderStatus>();
builder.Services.AddScoped<IUpdateInvoiceNo, UpdateInvoiceNo>();
builder.Services.AddScoped<IUpdateTransitFields, UpdateTransitFields>();
builder.Services.AddScoped<IBookedOrdersToSO, BookedOrdersToSO>();
builder.Services.AddScoped<IBulkUpdateIMEINo, BulkUpdateIMEINo>();
builder.Services.AddScoped<IBranchStockTransferAfterInvoice, BranchStockTransferAfterInvoice>();
builder.Services.AddScoped<IImportGRN, ImportGRN>();
builder.Services.AddScoped<IUploadDummyOrderReassignment, UploadDummyOrderReassignment>();
builder.Services.AddScoped<IUploadSalesReturn, UploadSalesReturn>();
builder.Services.AddScoped<IChangeInvoiceNo, ChangeInvoiceNo>();
builder.Services.AddScoped<IDeleteSalesOrder, DeleteSalesOrder>();
builder.Services.AddScoped<IBranchStock, BranchStock>();
builder.Services.AddScoped<ISalesReport, SalesReport>();
builder.Services.AddScoped<IBranchComplaintHO, BranchComplaintHO>();
builder.Services.AddScoped<IBranchStockReceivedHO, BranchStockReceivedHO>();
builder.Services.AddScoped<IGITReport, GITReport>();
builder.Services.AddScoped<IDisbursementReport, DisbursementReport>();
builder.Services.AddScoped<IPODVerify, PODVerify>();
builder.Services.AddScoped<IStateRegionCashSaleReport, StateRegionCashSaleReport>();
builder.Services.AddScoped<IPrintBranchSaleInvoice, PrintBranchSaleInvoice>();

//builder.Services.AddAuthentication(opt =>
//{
//    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(opt =>
//{   // for development only
//    opt.RequireHttpsMetadata = false;
//    opt.SaveToken = true;
//    opt.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuerSigningKey = true,
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"])),
//        ValidateIssuer = true,
//        ValidIssuer = builder.Configuration["Jwt:Issuer"],
//        ValidateAudience = true,
//        ValidAudience = builder.Configuration["Jwt:Audience"]
//    };
//});

var builders = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.Cookie.Name = "IMS2WebsiteCookie";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
            options.SlidingExpiration = true;
            options.LoginPath = "/Authentication/Index";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.ReturnUrlParameter = "returnUrl";
        });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Authentication}/{action=Index}/{id?}");

app.Run();
