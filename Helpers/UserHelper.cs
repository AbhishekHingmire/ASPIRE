using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace IMS2.Helpers
{
    public static class UserHelper
    {
        public static (string roleClaim, List<string> lstScreenRights) GetRoleAndScreenRights(ClaimsPrincipal user)
        {
            var claims = user.Claims;
            var roleClaim = claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
            var lstScreenRights = claims.Where(c => c.Type == "ScreenRights").Select(c => c.Value).ToList();
            return (roleClaim, lstScreenRights);
        }

        public static long GetUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == "ID")?.Value;
            if (long.TryParse(userIdClaim, out long userId))
            {
                return userId;
            }
            throw new InvalidOperationException("Invalid User ID claim.");
        }

        public static async Task<List<SelectListItem>> GetMFIListAsync(long userId, ISettings settingRepository)
        {
            var partners = await settingRepository.GetPartnersAsync(userId);
            return partners.Select(p => new SelectListItem
            {
                Text = p.Name,
                Value = p.ID.ToString()
            }).ToList();
        }

        public static EnumScreenRights? GetScreenAndRight(ClaimsPrincipal user, EnumScreenNames screenName)
        {
            var claims = user.Claims;
            var lstScreenRights = claims.Where(c => c.Type == "ScreenRights").Select(c => c.Value).ToList();
            var screenAndRight = lstScreenRights
                .FirstOrDefault(s => s.ToLower().Replace(" ", "").Contains(screenName.ToString().ToLower()));

            if (screenAndRight != null)
            {
                var screenRightRank = 1;
                var userScreenRight = screenAndRight.Split('|')[1].ToString();

                if (Enum.TryParse(userScreenRight, out EnumScreenRights screenRight))
                {
                    switch (screenRight)
                    {
                        case EnumScreenRights.Read:
                            screenRightRank = (int)EnumScreenRights.Read;
                            break;
                        case EnumScreenRights.Create:
                            screenRightRank = (int)EnumScreenRights.Create;
                            break;
                        case EnumScreenRights.Update:
                            screenRightRank = (int)EnumScreenRights.Update;
                            break;
                        case EnumScreenRights.Delete:
                            screenRightRank = (int)EnumScreenRights.Delete;
                            break;
                    }
                    return screenRight;
                }
            }

            return null;
        }

        public static async Task<(long BranchTypeID, long BranchID)> GetUserBranchDetailsAsync(string username, IImportSalesOrder importSalesOrderRepository)
        {
            var query = string.Format(
                "SELECT U.ID, Role, IsApproved, UserName, ISNULL(B.BranchTypeID, -1) BranchTypeID, ISNULL(B.PartnerID, -1) PartnerID, ISNULL(B.ID, -1) BranchID FROM [User] U WITH(NOLOCK) LEFT JOIN Branch B WITH(NOLOCK) ON B.UserID = U.ID WHERE Username = N'{0}'",
                username);

            var dtUser = importSalesOrderRepository.ExecSQL(query)[0];

            var branchTypeID = Convert.ToInt64(dtUser.Rows[0]["BranchTypeID"]);
            var branchID = Convert.ToInt64(dtUser.Rows[0]["BranchID"]);

            return (branchTypeID, branchID);
        }

        public static async Task<List<string>> GetScreensForUserAsync(long userId, ISettings settingRepository)
        {
            var screens = await settingRepository.GetScreensByUserId(userId);
            var screenNames = screens.Select(s => s.Name).ToList();
            return screenNames;
        }
    }

}

