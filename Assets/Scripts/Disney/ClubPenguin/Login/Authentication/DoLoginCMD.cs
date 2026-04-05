using Disney.ClubPenguin.CPModuleUtils;
using Disney.ClubPenguin.Service.MWS;
using Disney.ClubPenguin.Service.PDR;
using Disney.ClubPenguin.SledRacer;
using Disney.HTTP.Client;
using System;
using System.Collections;
using UnityEngine;

namespace Disney.ClubPenguin.Login.Authentication
{
	public class DoLoginCMD
	{
		private const int MIN_PASSWORD_LENGTH_LOGIN = 4;

		private string username;

		private string password;

		private bool savePassword;

		private string appId;

		private string appVersion;

		private IMWSClient mwsClient;

		private IPDRClient pdrClient;

		private MonoBehaviour timeoutCoRoutineBehaviour;

		private float requestTimeoutSec;

		private int maxPasswordLen;

		private int minPasswordLen;

		private int paperDollDimensions;

		private string paperDollLang;

		private bool loginRequestTimedOut;

		private bool loginRequestCompleted;

		private bool paperDollRequestTimedOut;

		private bool paperDollRequestCompleted;

		private SavedPlayerCollection savedPlayerCollection;

		private SavedPlayerData loggedInPlayerData;

		private IGetAuthTokenResponse successfulLoginResponse;

		public event Action LoginRequestSent;

		public event Action<IHTTPResponse> LoginFailed;

		public event Action<byte[]> PaperDollReceived;

		public event Action<string> PaperDollFailed;

		public event Action<IGetAuthTokenResponse, string, string> LoginSucceeded;

		public event Action<string, bool> InvalidInputSpecified;

		public DoLoginCMD(string username, string password, bool savePassword, string appId, string appVersion, IMWSClient mwsClient, IPDRClient pdrClient, MonoBehaviour timeoutCoRoutineBehaviour, float requestTimeoutSec = 30f, int minPasswordLen = 4, int maxPasswordLen = 32, int paperDollDimensions = 300, string paperDollLang = "en")
		{
			this.username = username;
			this.password = password;
			this.savePassword = savePassword;
			this.appId = appId;
			this.appVersion = appVersion;
			this.mwsClient = mwsClient;
			this.pdrClient = pdrClient;
			this.timeoutCoRoutineBehaviour = timeoutCoRoutineBehaviour;
			this.requestTimeoutSec = requestTimeoutSec;
			this.maxPasswordLen = maxPasswordLen;
			this.minPasswordLen = minPasswordLen;
			this.paperDollDimensions = paperDollDimensions;
			this.paperDollLang = paperDollLang;
		}

		public void Execute()
		{
			loginRequestTimedOut = false;
			loginRequestCompleted = false;
			username = InputFieldStringUtils.ToTitleCase(username);
			if (!HasInputErrors())
			{
				if (this.LoginRequestSent != null)
				{
					this.LoginRequestSent();
				}
				if (LocalPlayerAccountService.TryLogin(username, password, savePassword, out loggedInPlayerData))
				{
					successfulLoginResponse = LocalPlayerAccountService.CreateAuthResponse(loggedInPlayerData);
					if (this.LoginSucceeded != null)
					{
						this.LoginSucceeded(successfulLoginResponse, username, password);
					}
				}
				else if (this.LoginFailed != null)
				{
					this.LoginFailed(new HTTPErrorResponse(401, "Incorrect username or password"));
				}
			}
		}

		private IEnumerator CheckLoginRequestTimedOut()
		{
			yield return new WaitForSeconds(requestTimeoutSec);
			if (!loginRequestCompleted)
			{
				loginRequestTimedOut = true;
				UnityEngine.Debug.LogError("Login request timed out");
				if (this.LoginFailed != null)
				{
					HTTPErrorResponse errorResponse = new HTTPErrorResponse(500, "timed out");
					this.LoginFailed(errorResponse);
				}
			}
		}

		private void OnLoginResponseReceived(IGetAuthTokenResponse response)
		{
			if (loginRequestTimedOut)
			{
				return;
			}
			loginRequestCompleted = true;
			if (response.IsError)
			{
				if (this.LoginFailed != null)
				{
					this.LoginFailed(response);
				}
			}
			else
			{
				onLoginSuccess(response);
			}
		}

		private void onLoginSuccess(IGetAuthTokenResponse response)
		{
			successfulLoginResponse = response;
			savedPlayerCollection = new SavedPlayerCollection();
			if (savedPlayerCollection.ExistsOnDisk())
			{
				savedPlayerCollection.LoadFromDisk();
			}
			loggedInPlayerData = new SavedPlayerData();
			loggedInPlayerData.UserName = response.AuthData.Username;
			loggedInPlayerData.DisplayName = response.AuthData.DisplayName;
			loggedInPlayerData.Swid = response.AuthData.PlayerSwid;
			loggedInPlayerData.Password = ((!savePassword) ? string.Empty : password);
			paperDollRequestTimedOut = false;
			paperDollRequestCompleted = false;
			pdrClient.GetPaperDollImage(response.AuthData.PlayerSwid, paperDollDimensions, false, false, paperDollLang, OnPaperDollResponseReceived);
			timeoutCoRoutineBehaviour.StartCoroutine(CheckPaperDollRequestTimedOut());
		}

		private IEnumerator CheckPaperDollRequestTimedOut()
		{
			yield return new WaitForSeconds(requestTimeoutSec);
			if (!paperDollRequestCompleted)
			{
				paperDollRequestTimedOut = true;
				UnityEngine.Debug.LogError("Paper doll requeat timed out.");
				if (this.PaperDollFailed != null)
				{
					this.PaperDollFailed("timed out");
				}
				if (this.LoginSucceeded != null)
				{
					this.LoginSucceeded(successfulLoginResponse, username, password);
				}
			}
		}

		private void OnPaperDollResponseReceived(IGetPaperDollImageResponse response)
		{
			if (paperDollRequestTimedOut)
			{
				return;
			}
			paperDollRequestCompleted = true;
			if (response.IsError)
			{
				if (this.PaperDollFailed != null)
				{
					this.PaperDollFailed(response.StatusMessage);
				}
			}
			else
			{
				OnPaperDollSuccess(response.AvatarImageBytes);
			}
			if (this.LoginSucceeded != null)
			{
				this.LoginSucceeded(successfulLoginResponse, username, password);
			}
		}

		private void OnPaperDollSuccess(byte[] paperDollBytes)
		{
			loggedInPlayerData.PaperDollBytes = paperDollBytes;
			savedPlayerCollection.UpdateSavedPlayer(loggedInPlayerData);
			savedPlayerCollection.SaveToDisk();
			if (this.PaperDollReceived != null)
			{
				this.PaperDollReceived(paperDollBytes);
			}
		}

		private bool HasInputErrors()
		{
			string text = string.Empty;
			bool arg = false;
			if (username.Length == 0)
			{
				text = "3";
				arg = false;
			}
			else if (password.Length < minPasswordLen)
			{
				text = "1";
				arg = true;
			}
			else if (password.Length > maxPasswordLen)
			{
				text = "2";
				arg = true;
			}
			if (text != string.Empty)
			{
				if (this.InvalidInputSpecified != null)
				{
					this.InvalidInputSpecified(text, arg);
				}
				return true;
			}
			return false;
		}
	}
}
