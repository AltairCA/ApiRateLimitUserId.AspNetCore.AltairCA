using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace APIRateLimiterUserId.AspNetCore.AltairCA.Interface
{
    public interface APIRateLimiterUserIdHttpService
    {
        /// <summary>
        /// Clear the current logged in users limit
        /// </summary>
        /// <returns></returns>
        Task ClearLimit();
        /// <summary>
        /// Clear the current logged in users limit by group key
        /// </summary>
        /// <param name="groupKey"></param>
        /// <returns></returns>
        Task ClearLimit([Required(AllowEmptyStrings = false)] string groupKey);
    }
}