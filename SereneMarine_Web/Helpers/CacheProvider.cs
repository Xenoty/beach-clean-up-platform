using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;

        public CacheProvider(IMemoryCache memoryCache, IConfiguration configuration)
        {
            _cache = memoryCache;
            _configuration = configuration;
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

                double absExpiration = _configuration.GetValue<double>("CacheSettings:AbsoluteExpirationInMinutes");
                double slidingExpiration = _configuration.GetValue<double>("CacheSettings:SlidingExpirationInMinutes");
                long size = _configuration.GetValue<long>("CacheSettings:Size");

                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddMinutes(absExpiration),
                    SlidingExpiration = TimeSpan.FromMinutes(slidingExpiration),
                    Size = size,
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