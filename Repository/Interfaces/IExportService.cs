using IMS2.Models;
using OfficeOpenXml;

namespace IMS2.Repository.Interfaces
{
    public interface IExportService
    {
        void ExportTemplate(long ID, bool IsWithData, params object[] args);
        ExcelPackage GetExportTemplate(long ID, bool IsWithData, params object[] args);
        ImportSalesOrder GetEtlConfig(long ID);
        string GetExcelColumnName(int columnNumber);
        string GetAuthor(long ID);
        void DownloadExportTemplate(long ID, ExcelPackage epackage);

    }
}
