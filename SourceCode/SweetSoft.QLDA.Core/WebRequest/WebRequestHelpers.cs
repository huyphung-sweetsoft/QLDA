//----------------------------------------PROGRAMER LOGS----------------------------------------
//Created by: Doan, 26 Nov 2024

using RestSharp;
using SweetSoft.QLDA.Core.SysManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
namespace SweetSoft.QLDA.Core.WebRequest
{
    public static class WebRequestHelpers
    {
        public static readonly Method Patch = Method.Patch;

        static WebRequestHelpers()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        private static void AddHeaders(RestRequest request, Dictionary<string, string> headers)
        {
            if (headers == null) return;
            foreach (var item in headers)
            {
                if (item.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                    continue;

                request.AddHeader(item.Key, item.Value);
            }
        }

        public static async Task<string> GetRequestJson(string url, Dictionary<string, string> headers = null)
        {
            try
            {
                url = url.TrimEnd('/');
                var options = new RestClientOptions(url)
                {
                    FollowRedirects = true
                };
                var client = new RestClient(options);

                var request = new RestRequest("", Method.Get);

                AddHeaders(request, headers);

                var response = await client.ExecuteAsync(request).ConfigureAwait(false);
                if (!response.IsSuccessful)
                {
                    SysLogger.LogError($"File upload failed: {response.StatusCode} \n {response.Content}");
                    throw new HttpRequestException($"Request failed with status {response.StatusCode}: {response.Content}");
                }

                return response.Content;
            }
            catch(Exception ex)
            {
                SysLogger.LogError($"File upload failed: {ex.Message}");
                return string.Empty;
            }
        }

        public static async Task<string> SendJsonRequestAsync(Method method, string url, string jsonPayload, 
            Dictionary<string, string> headers = null, bool followRedirects = true)
        {
            try
            {
                url = url.TrimEnd('/');
                var options = new RestClientOptions(url)
                {
                    FollowRedirects = followRedirects
                };
                var client = new RestClient(options);
                var request = new RestRequest("", method);

                AddHeaders(request, headers);

                request.AddJsonBody(jsonPayload);

                var response = await client.ExecuteAsync(request).ConfigureAwait(false);

                if (!response.IsSuccessful)
                {
                    SysLogger.LogError($"Request failed with status: {response.StatusCode} \n {response.Content}");
                    throw new HttpRequestException($"Request failed with status {response.StatusCode}: {response.Content}");
                }

                return response.IsSuccessful ? response.Content : string.Empty;
            }
            catch(Exception ex)
            {
                SysLogger.LogError($"Request failed with status: {ex.Message}");
                return string.Empty;
            }
        }

        public static Task<string> PostRequestJson(string url, string json, Dictionary<string, string> headers = null, bool followRedirects= true)
            => SendJsonRequestAsync(Method.Post, url, json, headers, followRedirects);
        public static Task<string> PutRequestJson(string url, string json, Dictionary<string, string> headers = null)
            => SendJsonRequestAsync(Method.Put, url, json, headers);
        public static Task<string> PatchRequestJson(string url, string json, Dictionary<string, string> headers = null)
            => SendJsonRequestAsync(Method.Patch, url, json, headers);

        public static async Task<string> PostRequestFile(string url, Dictionary<string, string> data, string fileName, string filePath, Dictionary<string, string> headers = null)
        {
            try
            {
                url = url.TrimEnd('/');
                var options = new RestClientOptions(url)
                {
                    FollowRedirects = true
                };
                var client = new RestClient(options);
                var request = new RestRequest("", Method.Post);

                AddHeaders(request, headers);

                // Thêm file
                request.AddFile("file", filePath, "application/octet-stream");

                // Thêm form fields
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        request.AddParameter(item.Key, item.Value);
                    }
                }

                var response = await client.ExecuteAsync(request).ConfigureAwait(false);

                if (!response.IsSuccessful)
                {
                    SysLogger.LogError($"File upload failed: {response.StatusCode} \n {response.Content}");
                    throw new HttpRequestException($"File upload failed: {response.StatusCode} - {response.Content}");
                }

                return response.Content;
            }
            catch (Exception ex)
            {
                SysLogger.LogError($"File upload failed: {ex.Message}");
                return string.Empty;
            }
        }
        public static async Task<string> DeleteRequestJson(string url, Dictionary<string, string> headers = null)
        {
            try
            {
                url = url.TrimEnd('/');
                var options = new RestClientOptions(url)
                {
                    FollowRedirects = true
                };
                var client = new RestClient(options);
                var request = new RestRequest("", Method.Delete);

                AddHeaders(request, headers);

                var response = await client.ExecuteAsync(request).ConfigureAwait(false);

                if (!response.IsSuccessful)
                {
                    SysLogger.LogError($"DELETE Request: {response.StatusCode} \n {response.Content}");
                    throw new HttpRequestException($"DELETE failed: {response.StatusCode} - {response.Content}");
                }

                return response.Content;
            }
            catch(Exception ex)
            {
                SysLogger.LogError($"DELETE Request: {ex.Message}");
                return string.Empty;
            }
        }

    }


}
