using LibDeltaSystem.Db.ArkEntries;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Tools.InternalPrimalData
{
    public class PrimalDataTypeBinder<T>
    {
        private List<DbArkEntry<T>> _data;
        private Task _computeTask;
        private IMongoCollection<DbArkEntry<T>> _collection;

        public bool isReady;
        public bool isComputing;

        public PrimalDataTypeBinder(IMongoCollection<DbArkEntry<T>> collection)
        {
            _collection = collection;
        }

        public async Task<List<DbArkEntry<T>>> GetDatasAsync()
        {
            if (isReady)
                return _data; //We're good to go already

            //We're still downloading.
            if(!isComputing)
            {
                //We haven't started downloading yet. Start that process
                isComputing = true;
                _computeTask = InternalDownloadData();
            }
            await _computeTask; //We've already started, wait on it
            return _data; //We've finished. Return the data
        }

        private async Task InternalDownloadData()
        {
            var filterBuilder = Builders<DbArkEntry<T>>.Filter;
            isComputing = true;
            isReady = false;

            var results = await _collection.FindAsync(filterBuilder.Empty);
            _data = new List<DbArkEntry<T>>();
            while(await results.MoveNextAsync())
            {
                _data.AddRange(results.Current);
            }

            isComputing = false;
            isReady = true;
        }
    }
}
