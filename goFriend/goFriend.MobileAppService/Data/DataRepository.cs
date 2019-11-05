using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using goFriend.MobileAppService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NLog;

namespace goFriend.MobileAppService.Data
{
    public class DataRepository : IDataRepository, IDisposable
    {
        private readonly DbContext _dbContext;
        private readonly ICacheService _cacheService;
        private readonly IOptions<AppSettingsModel> _appSettings;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DataRepository(DbContext dbContext, ICacheService cacheService, IOptions<AppSettingsModel> appSettings)
        {
            _dbContext = dbContext;
            _cacheService = cacheService;
            _appSettings = appSettings;
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
            var cacheKey = $"{_dbContext.GetType().Name}.GetAll.{typeof(T).Name}.";
            if (useCache)
            {
                var cacheValue = _cacheService.Get(cacheKey) as IEnumerable<T>;
                if (cacheValue != null)
                {
                    return cacheValue;
                }
            }
            var result = _dbContext.Set<T>();
            if (useCache)
            {
                //var cachePrefix = $"TableCacheTimeout.{typeof(T).Name}";
                var cacheTimeout = _appSettings.Value.CacheTableTimeout; 
                Logger.Debug($"Caching new TableCache: {cacheKey}, cacheTimeout={cacheTimeout}");
                var cacheValue = result.ToList().AsQueryable().SetComparer(StringComparison.CurrentCultureIgnoreCase);//ToList here is very important, force executing SQL to retrieve data
                _cacheService.Set(cacheKey, cacheValue, DateTimeOffset.Now.AddMinutes(cacheTimeout));
                return cacheValue;
            }
            return result;
        }

        public IEnumerable<T> GetMany<T>(Expression<Func<T, bool>> where, bool useCache = false) where T : class
        {
            return useCache ? GetAll<T>(true).AsQueryable().Where(@where) : _dbContext.Set<T>().Where(@where);
        }

        public T Get<T>(Expression<Func<T, bool>> where, bool useCache = false) where T : class
        {
            return useCache ? GetAll<T>(true).AsQueryable().Where(@where).FirstOrDefault() : _dbContext.Set<T>().Where(@where).FirstOrDefault();
        }

        public void Add<T>(T entity) where T : class
        {
            _dbContext.Set<T>().Add(entity);
            _dbContext.SaveChanges();
        }

        public void Delete<T>(T entity) where T : class
        {
            _dbContext.Set<T>().Remove(entity);
            _dbContext.SaveChanges();
        }

        public void Delete<T>(Expression<Func<T, bool>> where) where T : class
        {
            _dbContext.Set<T>().RemoveRange(_dbContext.Set<T>().Where(where));
            _dbContext.SaveChanges();
        }

        public void Commit()
        {
            _dbContext.SaveChanges();
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}
