using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.WebFramework.ServiceTemplates
{
    /// <summary>
    /// This endpoint allows an authenticated user to select and item in the following form:
    /// *
    /// */[id]
    /// 
    /// To use this, create a service definition to both of those, with the arguemnt "SELECT_ITEM_ID"
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SelectItemUserAuthDeltaService<T> : UserAuthDeltaService
    {
        public SelectItemUserAuthDeltaService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public const string SELECT_ITEM_ARG = "SELECT_ITEM_ID";

        private bool itemSpecified;
        private T item;

        public override async Task OnRequest()
        {
            if (itemSpecified)
                await OnRequestToItem(item);
            else
                await OnRequestNoItem();
        }

        public override async Task<bool> SetArgs(Dictionary<string, string> args)
        {
            //Check if an item was specified
            if(args.ContainsKey(SELECT_ITEM_ARG))
            {
                itemSpecified = false;
            } else
            {
                //Find the item
                item = await GetItemByRequestedString(args[SELECT_ITEM_ARG]);
                itemSpecified = true;
                if(item == null)
                {
                    await WriteString("Item Not Found", "text/plain", 404);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets the item or returns null if it doesn't exist
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract Task<T> GetItemByRequestedString(string id);

        /// <summary>
        /// On request to an item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public abstract Task OnRequestToItem(T item);

        /// <summary>
        /// On a request to no item (when none was even specified)
        /// </summary>
        /// <returns></returns>
        public abstract Task OnRequestNoItem();
    }
}
