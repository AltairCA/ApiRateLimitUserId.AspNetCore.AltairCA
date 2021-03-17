using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using APIRateLimiterUserId.AspNetCore.AltairCA.Interface;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace APIRateLimiterUserId.AspNetCore.AltairCA.BackupProvider.MongoDbBackupProvider
{
    public class MongoDbBackupProviderOptions
    {
        public string MongoConnectionString { get; set; }
        public string MongoDbName { get; set; }
        public string MongoCollectionName { get; set; }
        public TimeSpan BackgroundServiceInterval { get; set; } = TimeSpan.FromMinutes(5);
    }
    internal class UserIdMongoDbStructure
    {
        [BsonId]
        public string Id { get; set; }
        public string Value { get; set; }
        public TimeSpan? TimeSpan { get; set; }
        
    }
    internal class MongoDbBackupProvider:IAPIRateLimiterUserIdBackupStorageProvider,IHostedService, IDisposable
    {
        
        internal MongoClient _client { get; set; }
        internal IMongoCollection<UserIdMongoDbStructure> _collection { get; set; }

        private Dictionary<string, (object value,TimeSpan? span)> OnQueueInsert =
            new Dictionary<string, (object value,TimeSpan? span)>();
        private Timer _timer;
        private readonly ILogger<MongoDbBackupProvider> _logger;
        private static bool IsBackGroundTaskRunning = false;
        private readonly TimeSpan serviceTimeSpan;
        public MongoDbBackupProvider(MongoDbBackupProviderOptions options,ILogger<MongoDbBackupProvider> logger)
        {
            serviceTimeSpan = options.BackgroundServiceInterval;
            _logger = logger;
            _client = new MongoClient(options.MongoConnectionString);
            var database = _client.GetDatabase(options.MongoDbName);
            _collection = database.GetCollection<UserIdMongoDbStructure>(options.MongoCollectionName);
        }
        public async Task<T> GetAsync<T>(string key)
        {
            var res = await _collection.FindAsync(x=> x.Id == key);
            var dbRecord = await res.FirstOrDefaultAsync();
            if (dbRecord == null || dbRecord.Value == null)
                return default(T);
            return JsonConvert.DeserializeObject<T>(dbRecord.Value);
        }

        public async Task SetAsync(string key, object obj, TimeSpan span)
        {
            lock (OnQueueInsert)
            {
                var found = OnQueueInsert.ContainsKey(key);
                if(found)
                    OnQueueInsert.Remove(key);
                OnQueueInsert.Add(key,(obj,span));
            }
            
        }

        public async Task SetAsync(string key, object obj)
        {
            lock (OnQueueInsert)
            {
                var found = OnQueueInsert.ContainsKey(key);
                if(found)
                    OnQueueInsert.Remove(key);
                OnQueueInsert.Add(key,(obj,null));
            }
            var res = await _collection.FindAsync(x=> x.Id == key);
            var dbRecord = await res.FirstOrDefaultAsync();
            if (dbRecord != null)
            {
                dbRecord.Value = JsonConvert.SerializeObject(obj);
                
                await _collection.ReplaceOneAsync(x => x.Id == key, dbRecord);
            }
            else
            {
                dbRecord = new UserIdMongoDbStructure();
                dbRecord.Id = key;
                dbRecord.Value = JsonConvert.SerializeObject(obj);
                await _collection.InsertOneAsync(dbRecord);
            }
        }

        public async Task RemoveAsync(string key)
        {
            await _collection.DeleteOneAsync(x => x.Id == key);
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed hosted service backup provider running.");

            _timer = new Timer(async (object state)=> DoWork(state), null, TimeSpan.Zero, 
                serviceTimeSpan);

            return Task.CompletedTask;
        }
        private async Task DoWork(object state)
        {
            if (IsBackGroundTaskRunning)
            {
                return;
            }

            IsBackGroundTaskRunning = true;
            _logger.LogInformation(
                "Timed hosted service backup provider is working.");
            try
            {
                foreach (KeyValuePair<string,(object value, TimeSpan? span)> tuple in OnQueueInsert)
                {
                    var res = await _collection.FindAsync(x=> x.Id == tuple.Key);
                    var dbRecord = await res.FirstOrDefaultAsync();
                    if (dbRecord != null)
                    {
                        dbRecord.Value = JsonConvert.SerializeObject(tuple.Value.value);
                        dbRecord.TimeSpan = tuple.Value.span;
                        await _collection.ReplaceOneAsync(x => x.Id == tuple.Key, dbRecord);
                    }
                    else
                    {
                        dbRecord = new UserIdMongoDbStructure();
                        dbRecord.Id = tuple.Key;
                        dbRecord.Value = JsonConvert.SerializeObject(tuple.Value);
                        dbRecord.TimeSpan = tuple.Value.span;
                        await _collection.InsertOneAsync(dbRecord);
                    }
                }
                lock (OnQueueInsert)
                {
                    OnQueueInsert.Clear();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e,"Error in hosted service backup provider");
            }
            IsBackGroundTaskRunning = false;

        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed hosted service backup provider is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}