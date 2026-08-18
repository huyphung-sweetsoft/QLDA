using SweetSoft.QLDA.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace SweetSoft.QLDA.Core.WebRequest
{
    public class ApiClientBase
    {
        private readonly string _baseUrl;
        private readonly Dictionary<string, string> _defaultHeaders;

        public ApiClientBase(string baseUrl, string authToken = null, string contentType = "application/json")
        {
            _baseUrl = baseUrl.TrimEnd('/');

            _defaultHeaders = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(authToken))
            {
                _defaultHeaders["Authorization"] = $"Bearer {authToken}";
            }

            if (!string.IsNullOrEmpty(contentType))
            {
                _defaultHeaders["Content-Type"] = contentType;
            }
        }

        protected string BuildUrl(string relativePath)
        {
            return $"{_baseUrl}/{relativePath.TrimStart('/')}";
        }

        protected Dictionary<string, string> MergeHeaders(Dictionary<string, string> customHeaders)
        {
            var headers = new Dictionary<string, string>(_defaultHeaders);
            if (customHeaders != null)
            {
                foreach (var item in customHeaders)
                    headers[item.Key] = item.Value;
            }
            return headers;
        }

        public Task<string> GetAsync(string relativePath, Dictionary<string, string> headers = null)
        {
            return WebRequestHelpers.GetRequestJson(BuildUrl(relativePath), MergeHeaders(headers));
        }

        public Task<string> PostAsync(string relativePath, string jsonBody, Dictionary<string, string> headers = null, bool followRedirects = true)
        {
            return WebRequestHelpers.PostRequestJson(BuildUrl(relativePath), jsonBody, MergeHeaders(headers), followRedirects);
        }

        public Task<string> PutAsync(string relativePath, string jsonBody, Dictionary<string, string> headers = null)
        {
            return WebRequestHelpers.PutRequestJson(BuildUrl(relativePath), jsonBody, MergeHeaders(headers));
        }
        public Task<string> PatchAsync(string relativePath, string jsonBody, Dictionary<string, string> headers = null)
        {
            return WebRequestHelpers.PatchRequestJson(BuildUrl(relativePath), jsonBody, MergeHeaders(headers));
        }
        public Task<string> DeleteAsync(string relativePath, Dictionary<string, string> headers = null)
        {
            return WebRequestHelpers.DeleteRequestJson(BuildUrl(relativePath), MergeHeaders(headers));
        }
    }

}
