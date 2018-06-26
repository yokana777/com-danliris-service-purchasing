using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Helpers
{
    public static class QueryHelper<TModel>
        where TModel : IStandardEntity
    {
        public static IQueryable<TModel> ConfigureSearch(IQueryable<TModel> Query, List<string> SearchAttributes, string Keyword)
        {
            /* Search with Keyword */
            if (Keyword != null)
            {
                string SearchQuery = String.Empty;
                foreach (string Attribute in SearchAttributes)
                {
                    if (Attribute.Contains("."))
                    {
                        var Key = Attribute.Split(".");
                        SearchQuery = string.Concat(SearchQuery, Key[0], $".Any({Key[1]}.Contains(@0)) OR ");
                    }
                    else
                    {
                        SearchQuery = string.Concat(SearchQuery, Attribute, ".Contains(@0) OR ");
                    }
                }

                SearchQuery = SearchQuery.Remove(SearchQuery.Length - 4);

                Query = Query.Where(SearchQuery, Keyword);
            }
            return Query;
        }

        public static IQueryable<TModel> ConfigureFilter(IQueryable<TModel> Query, Dictionary<string, string> FilterDictionary)
        {
            if (FilterDictionary != null && !FilterDictionary.Count.Equals(0))
            {
                foreach (var f in FilterDictionary)
                {
                    string Key = f.Key;
                    object Value = f.Value;
                    string filterQuery = string.Concat(string.Empty, Key, " == @0");

                    Query = Query.Where(filterQuery, Value);
                }
            }
            return Query;
        }

        public static IQueryable<TModel> ConfigureOrder(IQueryable<TModel> Query, Dictionary<string, string> OrderDictionary)
        {
            /* Default Order */
            if (OrderDictionary.Count.Equals(0))
            {
                OrderDictionary.Add("LastModifiedUtc", "desc");

                Query = Query.OrderBy("LastModifiedUtc desc");
            }
            /* Custom Order */
            else
            {
                string Key = OrderDictionary.Keys.First();
                string OrderType = OrderDictionary[Key];

                Query = Query.OrderBy(string.Concat(Key.Replace(".", ""), " ", OrderType));
            }
            return Query;
        }
    }
}
