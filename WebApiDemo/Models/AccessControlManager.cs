using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

namespace WebApiDemo.Models
{
    /// <summary>
    /// 存取控制管理員
    /// </summary>
    public static class AccessControlManager
    {
        static Dictionary<string, string[]> ApiKeys =
            ConfigurationManager.AppSettings.AllKeys
                .Where(o => o.StartsWith("apikey:"))
                .ToDictionary(
                    o => o.Split(':').Last(),
                    o => (ConfigurationManager.AppSettings[o] ?? string.Empty).Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).ToArray()
                );


        /// <summary>
        /// 檢查ApiKey及IP是否有效？有效時傳回ApiClient識別碼，否則傳回null
        /// </summary>
        /// <param name="apiKey">ApiKey</param>
        /// <param name="ipAddress">IP位址</param>
        /// <returns>ApiClient識別碼或null</returns>
        public static string GetApiClientId(string apiKey, string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(ipAddress))
                return null;

            //利用閒置五分鐘Cache機制減少反覆查詢
            //缺點是增刪修改ApiKey設定後可能要等五分鐘
            string cacheKey = $"{apiKey}\t{ipAddress}";
            if (MemoryCache.Default.Contains(cacheKey))
                return (string)MemoryCache.Default[cacheKey];

            //此處使用 apSetting 儲存 Api Key，亦可考量改存於資料庫
            string apiClientId =
                 (ApiKeys.ContainsKey(apiKey) && ApiKeys[apiKey].Contains(ipAddress)) ? apiKey : null;

            //加入Cache減少查詢次數
            MemoryCache.Default.Add(cacheKey, apiClientId, new CacheItemPolicy
            {
                AbsoluteExpiration = DateTime.Now.NextSlotStartTime(5, 10)
            });
            return apiClientId;
        }

        //REF: https://blog.darkthread.net/blog/better-abs-time-expire-cache/
        static Random rnd = new Random();
        /// <summary>
        /// 下個時間格隔起點
        /// </summary>
        /// <param name="time">現在時間或推算基準</param>
        /// <param name="slotMins">時間格大小(以分鐘表示)</param>
        /// <param name="randomDelaySecs">隨機延遲</param>
        /// <returns>下個時間格的起算時間</returns>
        public static DateTime NextSlotStartTime(this DateTime time, int slotMins, int randomDelaySecs = 30)
        {
            var slotSecs = slotMins * 60;
            var remainingSecs = slotSecs - ((time - time.Date).TotalSeconds % slotSecs);
            //加上 Delay
            if (randomDelaySecs > 0)
                remainingSecs += rnd.Next(randomDelaySecs);
            return time.AddSeconds(remainingSecs);
        }
    }
}