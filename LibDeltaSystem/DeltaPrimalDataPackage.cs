using LibDeltaSystem.Entities.ArkEntries.Dinosaur;
using LibDeltaSystem.Entities.PrivateNet.Packages;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem
{
    public class DeltaPrimalDataPackage
    {
        public List<DinosaurEntry> dino_entries;

        public DeltaPrimalDataPackage()
        {
            dino_entries = new List<DinosaurEntry>();
        }

        /// <summary>
        /// Loads all content from a ZIP file stream
        /// </summary>
        /// <param name="s">ZIP file to load</param>
        /// <returns></returns>
        public static async Task<DeltaPrimalDataPackage> LoadFromZipStream(Stream s)
        {
            //Create package
            DeltaPrimalDataPackage package = new DeltaPrimalDataPackage();
            
            //Open ZIP stream on this
            using (ZipArchive zip = new ZipArchive(s, ZipArchiveMode.Read))
            {
                package.dino_entries = await ZipHelper<List<DinosaurEntry>>(zip.GetEntry("dinos.bson"));
            }

            return package;
        }

        private static async Task<T> ZipHelper<T>(ZipArchiveEntry entry)
        {
            //Get entry stream
            T output;
            using (Stream s = entry.Open())
            using (BsonReader reader = new BsonReader(s))
            {
                JsonSerializer serializer = new JsonSerializer();
                reader.ReadRootValueAsArray = true; //Thanks, James! https://stackoverflow.com/questions/16910369/bson-array-deserialization-with-json-net
                output = serializer.Deserialize<T>(reader);
            }
            return output;
        }

        public void CopyTo(DeltaPrimalDataPackage destination)
        {
            //Copy and overwrite entries
            destination.dino_entries.AddRange(dino_entries);
        }

        /// <summary>
        /// Loads a package from the cache
        /// </summary>
        /// <param name="mods"></param>
        /// <returns></returns>
        public static async Task<DeltaPrimalDataPackage> LoadPackage(DeltaPrimalDataCache cache, string[] mods)
        {
            //Get package index
            PackageIndex index = await cache.GetIndex();

            //Create package to add to
            DeltaPrimalDataPackage package = new DeltaPrimalDataPackage();

            //Get base mod and copy it to the package
            {
                //Search for this in the package
                PackageIndexPatch pack = index.GetPackageById("base");

                //Get package data
                DeltaPrimalDataPackage payload = await cache.GetMod(pack.url, pack.id);

                //Copy this to our package
                payload.CopyTo(package);
            }

            //Search for mods, in order
            foreach(var m in mods)
            {
                //Search for this in the package
                PackageIndexPatch pack = index.GetPackageById(m);

                //Stop if it's not discovered
                if (pack == null)
                    continue;

                //Get package data
                DeltaPrimalDataPackage payload = await cache.GetMod(pack.url, pack.id);

                //Copy this to our package
                payload.CopyTo(package);
            }

            return package;
        }

        /// <summary>
        /// Returns a dino entry by it's classname
        /// </summary>
        /// <param name="classname"></param>
        /// <returns></returns>
        public DinosaurEntry GetDinoEntry(string classname)
        {
            //Trim ending _C, if it's there
            if (classname.EndsWith("_C"))
                classname = classname.Substring(0, classname.Length - 2);

            //Search
            for(var i = dino_entries.Count - 1; i>=0; i++)
            {
                if (dino_entries[i].classname == classname)
                    return dino_entries[i];
            }

            return null;
        }
    }
}
