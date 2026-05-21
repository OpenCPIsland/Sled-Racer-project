using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Disney.ClubPenguin.Security;
using Disney.ClubPenguin.Service.DirectoryService;
using Disney.HTTP.Client;
using Disney.Utility;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Disney.ClubPenguin.Service.MWS
{
    public class MWSRequest
    {
        private readonly MWSClient mwsClient;

        private readonly string httpMethod;

        private readonly string endpointPath;

        private NameValueCollection parameters;

        private readonly object body;

        private readonly JsonConverter[] converters;

        private static readonly Dictionary<CPEnvironment, string> environmentRequestTokenOverrideKeyMap = new Dictionary<CPEnvironment, string>
        {
            {
                CPEnvironment.LOCAL,
                "MICKEY123"
            },
            {
                CPEnvironment.SANDBOX,
                "MICKEY123"
            },
            {
                CPEnvironment.DEV,
                "MICKEY123"
            },
            {
                CPEnvironment.INT,
                "MICKEY123"
            },
            {
                CPEnvironment.QA1,
                "p8RRIyF9X82jlWxTxCtAsYo5uWwWDoVnmpRYJVEM"
            },
            {
                CPEnvironment.QA2,
                "p8RRIyF9X82jlWxTxCtAsYo5uWwWDoVnmpRYJVEM"
            },
            {
                CPEnvironment.PERF,
                "p8RRIyF9X82jlWxTxCtAsYo5uWwWDoVnmpRYJVEM"
            }
        };

        public IHTTPRequestFactory httpRequestFactory { get; set; }

        public IDirectoryServiceClient directoryServiceClient { get; set; }

        public MWSRequest(MWSClient mwsClient, string httpMethod, string endpointPath, NameValueCollection parameters = null, object body = null, JsonConverter[] converters = null)
        {
            this.mwsClient = mwsClient;
            httpRequestFactory = mwsClient.httpRequestFactory;
            directoryServiceClient = mwsClient.directoryServiceClient;
            this.httpMethod = httpMethod;
            this.endpointPath = endpointPath;
            this.parameters = parameters;
            this.body = body;
            this.converters = converters;
        }

        private void AddRequestTokenOverride()
        {
            if (parameters == null)
            {
                parameters = new NameValueCollection();
            }
            try
            {
                string value = environmentRequestTokenOverrideKeyMap[directoryServiceClient.Environment];
                parameters.Set("override", value);
            }
            catch (KeyNotFoundException)
            {
                Debug.LogWarning(string.Concat("No request token override key found for ", directoryServiceClient.Environment, ". Request may fail with HTTP 401"));
            }
        }

        public IHTTPResponse Send(Action<IHTTPResponse> responseHandler = null)
        {
            if (responseHandler == null)
            {
                string serviceURL = directoryServiceClient.GetServiceURL("cp-mobile-services");
                IHTTPRequest iHTTPRequest = createRequest(httpMethod, serviceURL, endpointPath, parameters, body);
                addAuthorization(iHTTPRequest);
                addRequestToken(iHTTPRequest);
                return iHTTPRequest.Execute();
            }
            directoryServiceClient.GetServiceURL("cp-mobile-services", delegate (string serviceUrl)
            {
                IHTTPRequest iHTTPRequest2 = createRequest(httpMethod, serviceUrl, endpointPath, parameters, body);
                addAuthorization(iHTTPRequest2);
                addRequestToken(iHTTPRequest2);
                iHTTPRequest2.ExecuteAsync(responseHandler);
            }, delegate (IHTTPResponse errorResponse)
            {
                responseHandler(errorResponse);
            });
            return null;
        }

        private void addAuthorization(IHTTPRequest httpRequest)
        {
            string text = null;
            if (mwsClient.Username != null && mwsClient.Password != null)
            {
                text = mwsClient.Username + ":" + mwsClient.Password;
            }
            if (mwsClient.AuthToken != null)
            {
                text = mwsClient.AuthToken + ":";
            }
            if (text != null)
            {
                string text2 = Base64.Encode(text);
                httpRequest.Headers.Add("Authorization", "Basic " + text2 + ", FD " + mwsClient.CellophaneToken);
            }
            else
            {
                httpRequest.Headers.Add("Authorization", "FD " + mwsClient.CellophaneToken);
            }
        }

        private void addRequestToken(IHTTPRequest httpRequest)
        {
            long num = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            string s = null;
            if ((httpMethod.Equals("POST") || httpMethod.Equals("PUT")) && httpRequest.Body != null)
            {
                s = Encoding.UTF8.GetString(httpRequest.Body);
            }
            string text = httpRequest.Uri.PathAndQuery;
            string mWSCellophanePrefix = directoryServiceClient.MWSCellophanePrefix;
            if (mWSCellophanePrefix != null && mWSCellophanePrefix.Length > 0)
            {
                text = text.Replace(mWSCellophanePrefix, string.Empty);
            }
            httpRequest.Headers.Add("X-Request-Timestamp", Convert.ToString(num));
            // httpRequest.Headers.Add(XRT3.GetRequestTokenHeader(), XRT3.Get(XRT3.Transmute1(text), XRT3.Transmute2(s), XRT3.Transmute3(Convert.ToString(num))));
        }

        private IHTTPRequest createRequest(string method, string baseURL, string endpointPath, NameValueCollection parameters = null, object body = null)
        {
            string uri = baseURL + "/" + endpointPath + toQueryString(parameters);
            IHTTPRequest iHTTPRequest;
            if (body == null)
            {
                iHTTPRequest = httpRequestFactory.CreateRequest(method, uri);
            }
            else if (body is byte[])
            {
                iHTTPRequest = httpRequestFactory.CreateRequest(method, uri, body as byte[]);
            }
            else
            {
                byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body, converters));
                iHTTPRequest = httpRequestFactory.CreateRequest(method, uri, bytes);
                iHTTPRequest.Headers.Add("Content-Type", "application/json");
            }
            return iHTTPRequest;
        }

        private string toQueryString(NameValueCollection nvc)
        {
            if (nvc == null || nvc.Count == 0)
            {
                return string.Empty;
            }
            List<string> list = new List<string>();
            string[] allKeys = nvc.AllKeys;
            foreach (string text in allKeys)
            {
                string[] values = nvc.GetValues(text);
                foreach (string s in values)
                {
                    list.Add(string.Format("{0}={1}", UnityWebRequest.EscapeURL(text), UnityWebRequest.EscapeURL(s)));
                }
            }
            return "?" + string.Join("&", list.ToArray());
        }
    }
}