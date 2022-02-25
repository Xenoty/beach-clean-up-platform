using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using SereneMarine_Web.Models;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SereneMarine_Web.Helpers
{
    public interface ICacheProvider
    {
        Task <ApiStatisticsModel> GetCachedResponse();
    }

    public class CacheProvider : ICacheProvider
    {
        private static readonly SemaphoreSlim GetApiStatsSemaphore = new SemaphoreSlim(1, 1);
        private readonly IMemoryCache _cache;

        public CacheProvider(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }

        public async Task <ApiStatisticsModel> GetCachedResponse()
        {
            try
            {
                return await GetCachedResponse(CacheKeys.Events, GetApiStatsSemaphore);
            }
            catch
            {
                throw;
            }
        }

        private async Task<ApiStatisticsModel> GetCachedResponse(string cacheKey, SemaphoreSlim semaphoreSlim)
        {
            bool isAvaiable = _cache.TryGetValue(cacheKey, out ApiStatisticsModel apiStatistics);
            if (isAvaiable) return apiStatistics;
            try
            {
                await semaphoreSlim.WaitAsync();
                isAvaiable = _cache.TryGetValue(cacheKey, out apiStatistics);
                if (isAvaiable) return apiStatistics;

                //Get data from api
                string url = SD.ApiStatsPath;
                HttpClient client = new HttpClient();
                HttpResponseMessage response = null;

                response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    // TODO: Can't really log error message here?
                    return apiStatistics;
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                apiStatistics = JsonConvert.DeserializeObject<ApiStatisticsModel>(jsonString);

                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddMinutes(1),
                    SlidingExpiration = TimeSpan.FromMinutes(0.5),
                    Size = 1024,
                };
                _cache.Set(cacheKey, apiStatistics, cacheEntryOptions);
            }
            catch
            {
                throw;
            }
            finally
            {
                semaphoreSlim.Release();
            }
            return apiStatistics;
        }
    }
}