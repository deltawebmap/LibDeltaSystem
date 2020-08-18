using Firebase.Storage;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Tools
{
    public class UserContentTool
    {
        /// <summary>
        /// Uploads user content and returns a string for the URL
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task<string> UploadUserContent(DeltaConnection conn, Stream data)
        {
            //Get creds
            var credential = GoogleCredential.FromFile(conn.GetUserConfigPath(DeltaConnection.CONFIGNAME_FIREBASE));
            var client = StorageClient.Create(credential);

            //Generate a unique ID
            string id = SecureStringTool.GenerateSecureString(64);

            //Upload content
            var content = await client.UploadObjectAsync(conn.config.firebase_uc_bucket, id + ".png", "image/png", data);

            return $"https://firebasestorage.googleapis.com/v0/b/delta-web-map.appspot.com/o/{id}.png?alt=media";
        }

        /// <summary>
        /// Uploads an image after resizing it and returns the URL to it
        /// </summary>
        /// <param name="data"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static async Task<string> UploadUserContentResizeImage(DeltaConnection conn, Stream data, int width, int height)
        {
            //Read as image
            string url;
            using(MemoryStream ms = new MemoryStream())
            using(MemoryStream inputData = new MemoryStream())
            {
                //Read
                await data.CopyToAsync(inputData);

                //Rewind and create image
                inputData.Position = 0;
                using (Image<Rgba32> img = Image.Load<Rgba32>(inputData))
                {
                    //Resize
                    img.Mutate(x => x.Resize(width, height));

                    //Save
                    img.SaveAsPng(ms);
                }

                //Rewind and upload
                ms.Position = 0;
                url = await UploadUserContent(conn, ms);
            }
            return url;
        }
    }
}
