using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;

namespace SweetSoft.QLDA.Core.Utils
{
    public class AutocompleteObj
    {
        public List<AutocompleteItem> ListAutocompleteItem { get; set; }
        public long Total { get; set; }
    }
    public partial class AutocompleteItem
    {
        //Label will be displayed before selecting
        public string Label { get; set; }
        //Value to display in input after selection
        public string Value { get; set; }
        //Data is the selected value
        public string Data { get; set; }
        //OtherData is used to store other custom information, for example can contain the user's email information in json.
        public string OtherData { get; set; }
    }

    public class AutocompleteHelper
    {
        public static AutocompleteObj ConvertIDataReaderToAutocompleteObj(IDataReader dataReader, bool hasOtherData)
        {
            int total = 0;
            while (dataReader.Read())
                total = dataReader.GetInt32(0);
            List<AutocompleteItem> listAutocompleteItem = new List<AutocompleteItem>();
            if (dataReader.NextResult())
            {
                AutocompleteItem autocompleteItem;
                while (dataReader.Read())
                {
                    autocompleteItem = new AutocompleteItem();
                    autocompleteItem.Label = dataReader["Label"].ToString();
                    autocompleteItem.Value = dataReader["Value"].ToString();
                    autocompleteItem.Data = dataReader["Data"].ToString();
                    if (hasOtherData)
                        autocompleteItem.OtherData = dataReader["OtherData"].ToString();
                    listAutocompleteItem.Add(autocompleteItem);
                }
            }
            AutocompleteObj autocompleteObj = new AutocompleteObj();
            autocompleteObj.Total = total;
            autocompleteObj.ListAutocompleteItem = listAutocompleteItem;
            return autocompleteObj;
        }
        public static string SetValue(string data, string label)
        {
            AutocompleteItem autocompleteItem = new AutocompleteItem();
            autocompleteItem.Data = data;
            autocompleteItem.Label = label;
            return SetValue(autocompleteItem);
        }
        public static string SetValue(AutocompleteItem autocompleteItem)
        {
            if (autocompleteItem == null)
                return string.Empty;

            List<AutocompleteItem> autocompleteItems = new List<AutocompleteItem>();
            autocompleteItems.Add(autocompleteItem);
            return SetValues(autocompleteItems);
        }
        public static string SetValues(List<AutocompleteItem> autocompleteItems)
        {
            if (autocompleteItems == null)
                autocompleteItems = new List<AutocompleteItem>();
            foreach (AutocompleteItem item in autocompleteItems)
            {
                item.Label = string.Empty;
            }
            return JsonConvert.SerializeObject(autocompleteItems);
        }
        public static AutocompleteItem GetValue(string json)
        {
            try
            {
                List<AutocompleteItem> autocompleteItems = JsonConvert.DeserializeObject<List<AutocompleteItem>>(json);
                if (autocompleteItems == null || autocompleteItems.Count == 0)
                    return new AutocompleteItem();
                return autocompleteItems[0];
            }
            catch
            {
                return new AutocompleteItem();
            }
        }
        public static List<AutocompleteItem> GetValues(string json)
        {
            try
            {
                List<AutocompleteItem> autocompleteItems = JsonConvert.DeserializeObject<List<AutocompleteItem>>(json);
                if (autocompleteItems == null)
                    return new List<AutocompleteItem>();
                return autocompleteItems;
            }
            catch
            {
                return new List<AutocompleteItem>();
            }
        }
    }
}