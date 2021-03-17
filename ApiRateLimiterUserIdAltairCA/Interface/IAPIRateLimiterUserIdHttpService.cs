using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace APIRateLimiterUserId.AspNetCore.AltairCA.Interface
{
    public interface IAPIRateLimiterUserIdHttpService
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
        Task ClearLimitGroup([Required(AllowEmptyStrings = false)] string groupKey);
        /// <summary>
        /// Clear Limit by giving userid and group key
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="groupKey"></param>
        /// <returns></returns>
        Task ClearLimitGroup(string userId, string groupKey);

        /// <summary>
        /// Set Limit on Group By Providing userId
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="group"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        Task SetLimitGroup(string userId, string group, long i);
        /// <summary>
        /// Get Remaining limit count on userid and group. if you haven't specifically set a limit it will throw a exception
        /// </summary>
        /// <exception cref="LimitNotDefinedForGroupException"></exception>
        /// <param name="userId"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        Task<long> GetRemainingLimitGroup(string userId, string group);
        /// <summary>
        /// Get Current Request Count for the group
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        Task<long> GetCurrentCountGroup(string userId, string group);
    }
}