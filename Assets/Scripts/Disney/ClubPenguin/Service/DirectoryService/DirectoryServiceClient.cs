using System;
using System.Collections.Generic;
using Disney.ClubPenguin.Service.DirectoryService.Domain;
using Disney.HTTP.Client;
using Disney.HTTP.Client.HTTPUnity;
using Newtonsoft.Json;
using strange.extensions.injector.api;
using UnityEngine;

namespace Disney.ClubPenguin.Service.DirectoryService
{
	[Implements(typeof(IDirectoryServiceClient), InjectionBindingScope.CROSS_CONTEXT)]
	public class DirectoryServiceClient : IDirectoryServiceClient
	{
		public class GetCompatibilityResponse : HTTPResponseDelegate, IHTTPResponse, IGetCompatibilityResponse
		{
			private CompatibilityStatus compatibilityStatus;

			public CompatibilityStatus CompatibilityStatus
			{
				get
				{
					return compatibilityStatus;
				}
			}

			public GetCompatibilityResponse(IHTTPResponse httpResponse)
				: base(httpResponse)
			{
				try
				{
					if (!httpResponse.IsError)
					{
						compatibilityStatus = JsonConvert.DeserializeObject<CompatibilityStatus>(httpResponse.Text);
					}
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}

		private static IDirectoryServiceClient instance;

		private IHTTPRequestFactory _httpRequestFactory;

		private Dictionary<string, string> _serviceUrls;

		private static string kLocalDirectoryServiceUrl;

		private static string kRoomsWorldName;

		private static string kDevDirectoryServiceUrl;

		private static string kStageDevDirectoryServiceUrl;

		private static string kSandboxDirectoryServiceUrl;

		private static string kIntDirectoryServiceUrl;

		private static string kQa1DirectoryServiceUrl;

		private static string kQa2DirectoryServiceUrl;

		private static string kPerfDirectoryServiceUrl;

		private static string kStageDirectoryServiceUrl;

		private static string kProdDirectoryServiceUrl;

		private CPEnvironment environment;

		private int lastVersionCheck;

		private int versionCheckInterval;

		public static IDirectoryServiceClient Instance
		{
			get
			{
				return instance;
			}
			set
			{
				instance = value;
			}
		}

		[Inject]
		public IHTTPRequestFactory httpRequestFactory
		{
			get
			{
				if (_httpRequestFactory == null)
				{
					return HTTPUnityRequestFactory.Instance;
				}
				return _httpRequestFactory;
			}
			set
			{
				_httpRequestFactory = value;
			}
		}

		public string DirectoryServiceUrl
		{
			get
			{
				return "https://api.PUT-YOUR-API-URL-HERE.COM/";
			}
		}

		public bool IsDev
		{
			get
			{
				if (environment != 0 && environment != CPEnvironment.DEV)
				{
					return environment == CPEnvironment.STAGEDEV;
				}
				return true;
			}
		}

		public bool IsInt
		{
			get
			{
				if (environment != CPEnvironment.SANDBOX)
				{
					return environment == CPEnvironment.INT;
				}
				return true;
			}
		}

		public bool IsQA
		{
			get
			{
				if (environment != CPEnvironment.QA1 && environment != CPEnvironment.QA2)
				{
					return environment == CPEnvironment.PERF;
				}
				return true;
			}
		}

		public bool IsStage
		{
			get
			{
				return environment == CPEnvironment.STAGE;
			}
		}

		public bool IsProd
		{
			get
			{
				return environment == CPEnvironment.PROD;
			}
		}

		public CPEnvironment Environment
		{
			get
			{
				return environment;
			}
			set
			{
				environment = value;
			}
		}

		public string EnvironmentName
		{
			get
			{
				if (IsDev)
				{
					return "Dev";
				}
				if (IsInt)
				{
					return "Int";
				}
				if (IsQA)
				{
					return "QA";
				}
				if (IsStage)
				{
					return "Stage";
				}
				if (IsProd)
				{
					return "Prod";
				}
				return "Unknown";
			}
		}

		public string MWSCellophanePrefix
		{
			get
			{
				switch (environment)
				{
				case CPEnvironment.LOCAL:
					return null;
				case CPEnvironment.DEV:
					return null;
				case CPEnvironment.STAGEDEV:
					return null;
				case CPEnvironment.SANDBOX:
					return null;
				case CPEnvironment.INT:
					return "/int1/clubpenguin/mobile/v2";
				case CPEnvironment.QA1:
					return "/qa1/clubpenguin/mobile/v2";
				case CPEnvironment.QA2:
					return "/qa2/clubpenguin/mobile/v2";
				case CPEnvironment.PERF:
					return "/perf1/clubpenguin/mobile/v2";
				case CPEnvironment.STAGE:
					return "/clubpenguin/mobile/v2";
				case CPEnvironment.PROD:
					return "/clubpenguin/mobile/v2";
				default:
					return null;
				}
			}
		}

		public string AppID { get; set; }

		public string AppVersion { get; set; }

		public int VersionCheckInterval
		{
			get
			{
				return versionCheckInterval;
			}
			set
			{
				versionCheckInterval = value;
			}
		}

		public event Action OnUpdateRecommended;

		public event Action OnUpdateRequired;

		static DirectoryServiceClient()
		{
			instance = new DirectoryServiceClient();
			kLocalDirectoryServiceUrl = "http://localhost:7080/clubpenguin-directory-service-1.0.7-SNAPSHOT";
			kRoomsWorldName = "CPNext_dev_branch-hildd004-wmd_test";
			kDevDirectoryServiceUrl = "http://dimgdev.general.disney.private/metaplace/trigger/invoke/" + kRoomsWorldName;
			kStageDevDirectoryServiceUrl = "http://dimg-platstaging-portal.general.disney.private/metaplace/trigger/invoke/";
			kSandboxDirectoryServiceUrl = "http://mwstomcat01.sandbox.clubpenguin.com:8080/clubpenguin-directory-service";
			kIntDirectoryServiceUrl = "http://n7vgi1clubpdir.general.disney.private";
			kQa1DirectoryServiceUrl = "http://n7vgq1clubpdir.general.disney.private";
			kQa2DirectoryServiceUrl = "http://n7vgq2clubpdir.general.disney.private";
			kPerfDirectoryServiceUrl = "http://n7vgl1clubpdir.general.disney.private";
			kStageDirectoryServiceUrl = "http://stage-directory.clubpenguin.com";
			kProdDirectoryServiceUrl = "http://directory.clubpenguin.com";
		}

		public void CheckVersion()
		{
			if (!CanCheckCompatibility())
			{
				Debug.Log("[DirectoryServiceClient] Skipping compatibility check because the directory service is not configured for this build.");
				return;
			}
			GetCompatibility(AppID, AppVersion, delegate(IGetCompatibilityResponse response)
			{
				if (response == null)
				{
					Debug.LogWarning("[DirectoryServiceClient] Compatibility check returned no response. Skipping forced update handling.");
				}
				else if (response.IsError)
				{
					Debug.LogError("Service API version check returned an error. Will assume the client is compatible with the API. Error: HTTP " + response.StatusCode + " - " + response.StatusMessage);
				}
				else if (response.CompatibilityStatus == null)
				{
					Debug.LogWarning("[DirectoryServiceClient] Compatibility check returned no compatibility status. Skipping forced update handling.");
				}
				else if (response.CompatibilityStatus.Status == 1)
				{
					this.OnUpdateRecommended?.Invoke();
				}
				else if (response.CompatibilityStatus.Status == 2)
				{
					this.OnUpdateRequired?.Invoke();
				}
			});
		}

		private bool CanCheckCompatibility()
		{
			if (string.IsNullOrEmpty(AppID) || string.IsNullOrEmpty(AppVersion))
			{
				return false;
			}
			string directoryServiceUrl = DirectoryServiceUrl;
			return !string.IsNullOrEmpty(directoryServiceUrl) && directoryServiceUrl.IndexOf("PUT-YOUR-API-URL-HERE", StringComparison.OrdinalIgnoreCase) < 0;
		}

		private static int CurrentUnixTimestamp()
		{
			return (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
		}

		private void PeriodicCheckVersion()
		{
			if (versionCheckInterval != 0 && CurrentUnixTimestamp() > lastVersionCheck + versionCheckInterval)
			{
				CheckVersion();
				lastVersionCheck = CurrentUnixTimestamp();
			}
		}

		private Dictionary<string, string> GetServiceURLs(Action<Dictionary<string, string>> successHandler = null, Action<IHTTPResponse> failureHandler = null)
		{
			if (_serviceUrls != null)
			{
				if (successHandler != null)
				{
					successHandler(_serviceUrls);
				}
				return _serviceUrls;
			}
			if (successHandler != null)
			{
				httpRequestFactory.CreateRequest("GET", DirectoryServiceUrl + "/services").ExecuteAsync(delegate(IHTTPResponse httpResponse)
				{
					if (httpResponse.IsError)
					{
						failureHandler(httpResponse);
					}
					else
					{
						_serviceUrls = JsonConvert.DeserializeObject<Dictionary<string, string>>(httpResponse.Text);
						successHandler(_serviceUrls);
					}
				});
				return null;
			}
			IHTTPResponse iHTTPResponse = httpRequestFactory.CreateRequest("GET", DirectoryServiceUrl + "/services").Execute();
			if (!iHTTPResponse.IsError)
			{
				_serviceUrls = JsonConvert.DeserializeObject<Dictionary<string, string>>(iHTTPResponse.Text);
				return _serviceUrls;
			}
			throw new Exception("GET /services request failed. HTTP " + iHTTPResponse.StatusCode + " - " + iHTTPResponse.StatusMessage);
		}

		public string GetServiceURL(string serviceName, Action<string> successHandler = null, Action<IHTTPResponse> failureHandler = null)
		{
			PeriodicCheckVersion();
			if (successHandler == null)
			{
				return GetServiceURLs()[serviceName];
			}
			GetServiceURLs(delegate(Dictionary<string, string> serviceUrls)
			{
				successHandler(serviceUrls[serviceName]);
			}, delegate(IHTTPResponse errorResponse)
			{
				failureHandler(errorResponse);
			});
			return null;
		}

		public IGetCompatibilityResponse GetCompatibility(string appId, string appVersion, Action<IGetCompatibilityResponse> responseHandler = null)
		{
			IHTTPRequest iHTTPRequest = httpRequestFactory.CreateRequest("GET", DirectoryServiceUrl + "/compatibility/" + appId + "?version=" + appVersion);
			if (responseHandler == null)
			{
				return new GetCompatibilityResponse(iHTTPRequest.Execute());
			}
			iHTTPRequest.ExecuteAsync(delegate(IHTTPResponse httpResponse)
			{
				responseHandler(new GetCompatibilityResponse(httpResponse));
			});
			return null;
		}
	}
}
