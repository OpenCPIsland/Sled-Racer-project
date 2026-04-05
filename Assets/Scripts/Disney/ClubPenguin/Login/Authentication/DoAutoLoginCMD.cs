using Disney.ClubPenguin.Login.BI;
using Disney.ClubPenguin.Service.MWS;
using Disney.ClubPenguin.Service.PDR;
using Disney.ClubPenguin.SledRacer;
using Disney.HTTP.Client;
using System;
using UnityEngine;

namespace Disney.ClubPenguin.Login.Authentication
{
	public class DoAutoLoginCMD
	{
		private string appId;

		private string appVersion;

		private IMWSClient mwsClient;

		private IPDRClient pdrClient;

		private ILoginBIUtils loginBiUtils;

		private MonoBehaviour timeoutCoRoutineBehaviour;

		private float requestTimeoutSec;

		public event Action LoginRequestSent;

		public event Action<IHTTPResponse> LoginFailed;

		public event Action<byte[]> PaperDollReceived;

		public event Action<IGetAuthTokenResponse, string, string> LoginSucceeded;

		public DoAutoLoginCMD(string appId, string appVersion, IMWSClient mwsClient, IPDRClient pdrClient, ILoginBIUtils loginBiUtils, MonoBehaviour timeoutCoRoutineBehaviour, float requestTimeoutSec = 30f)
		{
			this.appId = appId;
			this.appVersion = appVersion;
			this.mwsClient = mwsClient;
			this.pdrClient = pdrClient;
			this.loginBiUtils = loginBiUtils;
			this.timeoutCoRoutineBehaviour = timeoutCoRoutineBehaviour;
			this.requestTimeoutSec = requestTimeoutSec;
		}

		public void Execute()
		{
			SavedPlayerCollection savedPlayerCollection = new SavedPlayerCollection();
			if (savedPlayerCollection.ExistsOnDisk())
			{
				savedPlayerCollection.LoadFromDisk();
				SavedPlayerData savedPlayerData = GetPreferredPlayer(savedPlayerCollection);
				string autoLoginPassword = GetAutoLoginPassword(savedPlayerData);
				if (savedPlayerData != null && !string.IsNullOrEmpty(autoLoginPassword))
				{
					DoLoginCMD doLoginCMD = new DoLoginCMD(savedPlayerData.UserName, autoLoginPassword, true, appId, appVersion, mwsClient, pdrClient, timeoutCoRoutineBehaviour, requestTimeoutSec);
					doLoginCMD.LoginSucceeded += OnLoginSucceeded;
					doLoginCMD.LoginFailed += OnLoginFailed;
					doLoginCMD.InvalidInputSpecified += OnLoginFailed;
					doLoginCMD.LoginRequestSent += OnLoginRequestSent;
					doLoginCMD.PaperDollReceived += OnPaperDollReceived;
					doLoginCMD.Execute();
					return;
				}
			}
			OnLoginFailed(null);
		}

		private SavedPlayerData GetPreferredPlayer(SavedPlayerCollection savedPlayerCollection)
		{
			string preferredAutoLoginPlayerSwid = SessionStatePrefs.GetPreferredAutoLoginPlayerSwid();
			if (!string.IsNullOrEmpty(preferredAutoLoginPlayerSwid))
			{
				return savedPlayerCollection.SavedPlayers.Find((SavedPlayerData player) => player.Swid == preferredAutoLoginPlayerSwid);
			}
			return savedPlayerCollection.GetMostRecentlyLoggedInPlayer();
		}

		private static string GetAutoLoginPassword(SavedPlayerData savedPlayerData)
		{
			if (savedPlayerData == null)
			{
				return string.Empty;
			}
			if (!string.IsNullOrEmpty(savedPlayerData.Password))
			{
				return savedPlayerData.Password;
			}
			if (savedPlayerData.IsLocalAccount && !string.IsNullOrEmpty(savedPlayerData.StoredPassword))
			{
				return savedPlayerData.StoredPassword;
			}
			return string.Empty;
		}

		public void OnLoginSucceeded(IGetAuthTokenResponse response, string username, string password)
		{
			loginBiUtils.SendPlayerInfo(response.AuthData.PlayerId, response.AuthData.Username);
			if (this.LoginSucceeded != null)
			{
				this.LoginSucceeded(response, username, password);
			}
		}

		public void OnLoginFailed(IHTTPResponse response)
		{
			if (this.LoginFailed != null)
			{
				this.LoginFailed(response);
			}
		}

		private void OnLoginFailed(string arg1, bool arg2)
		{
			if (this.LoginFailed != null)
			{
				this.LoginFailed(null);
			}
		}

		public void OnLoginRequestSent()
		{
			if (this.LoginRequestSent != null)
			{
				this.LoginRequestSent();
			}
		}

		public void OnPaperDollReceived(byte[] paperDollBytes)
		{
			if (this.PaperDollReceived != null)
			{
				this.PaperDollReceived(paperDollBytes);
			}
		}
	}
}
