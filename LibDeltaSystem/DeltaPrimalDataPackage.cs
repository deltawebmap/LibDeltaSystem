using LibDeltaSystem.Db.ArkEntries;
using LibDeltaSystem.Entities.ArkEntries.Dinosaur;
using LibDeltaSystem.Entities.PrivateNet.Packages;
using LibDeltaSystem.Tools.InternalPrimalData;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LibDeltaSystem
{
    public class DeltaPrimalDataPackage
    {
        private DeltaConnection conn;
        private string[] mods;

        private static PrimalDataTypeBinder<DinosaurEntry> dino_cache;
        private static PrimalDataTypeBinder<ItemEntry> item_cache;

        public DeltaPrimalDataPackage(string[] mods, DeltaConnection conn)
        {
            this.conn = conn;

            if (dino_cache == null)
                dino_cache = new PrimalDataTypeBinder<DinosaurEntry>(conn.arkentries_dinos);
            if (item_cache == null)
                item_cache = new PrimalDataTypeBinder<ItemEntry>(conn.arkentries_items);
        }

        private string TrimClassname(string classname)
        {
            //Trim ending _C, if it's there
            if (classname.EndsWith("_C"))
                return classname.Substring(0, classname.Length - 2);
            return classname;
        }

        /// <summary>
        /// Returns a dino entry by it's classname
        /// </summary>
        /// <param name="classname"></param>
        /// <returns></returns>
        public async Task<DinosaurEntry> GetDinoEntryByClssnameAsnyc(string classname)
        {
            //Get real classname
            classname = TrimClassname(classname);

            //Get datas
            var cache = await dino_cache.GetDatasAsync();

            //Check if we have this item cached
            DbArkEntry<DinosaurEntry> entry = cache.Where(x => x.classname == classname).FirstOrDefault();
            if (entry != null)
                return entry.data;

            return null;
        }

        /// <summary>
        /// Returns a dino entry by it's display name
        /// </summary>
        /// <param name="classname"></param>
        /// <returns></returns>
        public async Task<DinosaurEntry> GetDinoEntryByNameAsnyc(string name)
        {
            //Get datas
            var cache = await dino_cache.GetDatasAsync();

            //Check if we have this item cached
            DbArkEntry<DinosaurEntry> entry = cache.Where(x => x.data.screen_name == name).FirstOrDefault();
            if (entry != null)
                return entry.data;

            return null;
        }

        /// <summary>
        /// Returns a dino entry by it's classname
        /// </summary>
        /// <param name="classname"></param>
        /// <returns></returns>
        [Obsolete("Please use GetDinoEntryByClassnameAsync instead.")]
        public DinosaurEntry GetDinoEntry(string classname)
        {
            return GetDinoEntryByClssnameAsnyc(classname).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Returns an item entry by it's classname
        /// </summary>
        /// <param name="classname"></param>
        /// <returns></returns>
        public async Task<ItemEntry> GetItemEntryByClssnameAsnyc(string classname)
        {
            //Get real classname
            classname = TrimClassname(classname);

            //Get datas
            var cache = await item_cache.GetDatasAsync();

            //Check if we have this item cached
            DbArkEntry<ItemEntry> entry = cache.Where(x => x.classname == classname).FirstOrDefault();
            if (entry != null)
                return entry.data;

            return null;
        }

        /// <summary>
        /// Returns an item entry by it's display name
        /// </summary>
        /// <param name="classname"></param>
        /// <returns></returns>
        public async Task<ItemEntry> GetItemEntryByNameAsnyc(string name)
        {
            //Get datas
            var cache = await item_cache.GetDatasAsync();

            //Check if we have this item cached
            DbArkEntry<ItemEntry> entry = cache.Where(x => x.data.name == name).FirstOrDefault();
            if (entry != null)
                return entry.data;

            return null;
        }

        /// <summary>
        /// Returns an item entry by it's classname
        /// </summary>
        /// <param name="classname"></param>
        /// <returns></returns>
        [Obsolete("Please use GetItemEntryByClassnameAsync instead.")]
        public ItemEntry GetItemEntry(string classname)
        {
            return GetItemEntryByClssnameAsnyc(classname).GetAwaiter().GetResult();
        }
    }
}
