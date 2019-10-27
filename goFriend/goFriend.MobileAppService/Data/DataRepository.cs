using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using goFriend.MobileAppService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NLog;

namespace goFriend.MobileAppService.Data
{
    public class DataRepository : IDataRepository, IDisposable
    {
        protected readonly DbContext DbContext;
        protected readonly IMemoryCache MemoryCache;
        protected readonly IOptions<AppSettingsModel> AppSettings;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DataRepository(DbContext dbContext, IMemoryCache memoryCache, IOptions<AppSettingsModel> appSettings)
        {
            DbContext = dbContext;
            MemoryCache = memoryCache;
            AppSettings = appSettings;
        }

        //public void EnableDebug(Action<string> action)
        //{
        //    DbContext.Database.Log = action;
        //}

        //public void DisableDebug()
        //{
        //    DbContext.Database.Log = null;
        //}

        public IEnumerable<T> GetAll<T>(bool useCache = false) where T : class
        {
            var cacheKey = $"{DbContext.GetType().Name}.GetAll.{typeof(T).FullName}";
            if (useCache)
            {
                var cacheValue = MemoryCache.Get(cacheKey) as IEnumerable<T>;
                if (cacheValue != null)
                {
                    return cacheValue;
                }
            }
            var result = DbContext.Set<T>();
            if (useCache)
            {
                //var cachePrefix = $"TableCacheTimeout.{typeof(T).Name}";
                var cacheTimeout = AppSettings.Value.CacheTableTimeout;
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug($"Cached new TableCache: {cacheKey}, cacheTimeout={cacheTimeout}");
                }
                //MemoryCache.Set(cacheKey, result, DateTimeOffset.Now.AddDays(86400));
                var cacheValue = result.ToList().AsQueryable().SetComparer(StringComparison.CurrentCultureIgnoreCase);//ToList here is very important, force executing SQL to retrieve data
                MemoryCache.Set(cacheKey, cacheValue, DateTimeOffset.Now.AddMinutes(cacheTimeout));
                return cacheValue;
            }
            return result;
        }

        public IEnumerable<T> GetMany<T>(Expression<Func<T, bool>> where, bool useCache = false) where T : class
        {
            return useCache ? GetAll<T>(true).AsQueryable().Where(@where) : DbContext.Set<T>().Where(@where);
        }

        public T Get<T>(Expression<Func<T, bool>> where, bool useCache = false) where T : class
        {
            return useCache ? GetAll<T>(true).AsQueryable().Where(@where).FirstOrDefault() : DbContext.Set<T>().Where(@where).FirstOrDefault();
        }

        public void Add<T>(T entity) where T : class
        {
            DbContext.Set<T>().Add(entity);
            DbContext.SaveChanges();
        }

        public void Delete<T>(T entity) where T : class
        {
            DbContext.Set<T>().Remove(entity);
            DbContext.SaveChanges();
        }

        public void Delete<T>(Expression<Func<T, bool>> where) where T : class
        {
            DbContext.Set<T>().RemoveRange(DbContext.Set<T>().Where(where));
            DbContext.SaveChanges();
        }

        public void Commit()
        {
            DbContext.SaveChanges();
        }

        public void Dispose()
        {
            DbContext?.Dispose();
        }
    }
}
