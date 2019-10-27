using LibDeltaSystem.Entities.PrivateNet.Packages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem
{
    /// <summary>
    /// Should be shared across the entire system.
    /// </summary>
    public class DeltaPrimalDataCache
    {
        private HttpClient client;
        private string index_path;

        private TimeCachedFile<PackageIndex> index;
        private Dictionary<string, HashCachedFile<DeltaPrimalDataPackage>> mods;

        public DeltaPrimalDataCache(string index_path = "https://packages.deltamap.net/index.json")
        {
            this.index_path = index_path;
            client = new HttpClient();
        }

        /// <summary>
        /// Gets the index file
        /// </summary>
        /// <returns></returns>
        public async Task<PackageIndex> GetIndex()
        {
            if (index == null)
                index = new TimeCachedFile<PackageIndex>();
            return await index.GetFile(client, index_path, async (HttpClient hc, string url) =>
            {
                string content = await hc.GetStringAsync(url);
                return JsonConvert.DeserializeObject<PackageIndex>(content);
            });
        }

        /// <summary>
        /// Gets a single mod
        /// </summary>
        /// <returns></returns>
        public async Task<DeltaPrimalDataPackage> GetMod(string modUrl, string id)
        {
            if (!mods.ContainsKey(id))
                mods.Add(id, new HashCachedFile<DeltaPrimalDataPackage>());
            return await mods[id].GetFile(client, modUrl, async (HttpClient hc, string url) =>
            {
                //Get stream content
                Stream content = await hc.GetStreamAsync(url);

                //Load package contents
                var package = await DeltaPrimalDataPackage.LoadFromZipStream(content);

                //Return response
                return package;
            });
        }

        /// <summary>
        /// Loads a package specific to the mods offered. Normal API for most requests.
        /// </summary>
        /// <param name="mods"></param>
        /// <returns></returns>
        public async Task<DeltaPrimalDataPackage> LoadFullPackage(string[] mods)
        {
            return await DeltaPrimalDataPackage.LoadPackage(this, mods);
        }

        delegate Task<T> GetCachedFile<T>(HttpClient hc, string url); 
        abstract class BaseCachedFile<T>
        {
            private Task<T> fetch;
            private T data;
            private bool ready;
            
            public async Task<T> GetFile(HttpClient hc, string url, GetCachedFile<T> process)
            {
                //Check if the task has started
                if (fetch == null)
                    fetch = process(hc, url);

                //Check if we have our object
                if(ready)
                    return data;

                //Wait for it to be completed
                if (!fetch.IsCompleted)
                    await fetch;

                //Handle completion state
                if (fetch.IsCompletedSuccessfully)
                {
                    //Deserialize and return
                    ready = true;
                    return data;
                } else
                {
                    //Did not complete successfully
                    fetch = null;
                    throw new Exception("Couldn't download data " + url + "!");
                }
            }
        }

        class TimeCachedFile<T> : BaseCachedFile<T>
        {
            public DateTime expire;
        }

        class HashCachedFile<T> : BaseCachedFile<T>
        {
            public string sha1;
        }
    }
}
