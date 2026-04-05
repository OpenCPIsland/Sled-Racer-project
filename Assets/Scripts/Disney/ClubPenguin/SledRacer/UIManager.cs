using Disney.ClubPenguin.CPModuleUtils;
using Disney.ClubPenguin.Login.Authentication;
using Disney.ClubPenguin.Login.BI;
using Disney.ClubPenguin.Service.DirectoryService;
using Disney.ClubPenguin.Service.MWS;
using Disney.ClubPenguin.Service.MWS.Domain;
using Disney.ClubPenguin.Service.PDR;
using Disney.ClubPenguin.WebPageViewer;
using Disney.HTTP.Client;
using Disney.MobileNetwork;
using InAppPurchases;
using InAppPurchases.Restore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Disney.ClubPenguin.SledRacer
{
	public class UIManager : MonoBehaviour
	{
		public const string DEV_TARGET_ENVIRONMENT = "TargetEnvironment";

		private const string boostLoadingComponent = "boost_purchases_loading";

		private const string loadingIconResourcePath = "Prefabs/UIElements/LoadingIcon";

		private const string restoreFailedToken = "purchase.restore.found.nothing";

		private const string restoreSuccessToken = "purchase.restore.found.item";

		public string HUDPanel;

		public string MainMenuPanel;

		public string PauseMenuPanel;

		public string EndGamePanel;

		public string SettingsPanel;

		public string LeaderBoardPanel;

		public string BoostMenuPanel;

		public string StoreMenuPanel;

		public string AboutMembershipPanel;

		public GameObject OverlayContainer;

		public RestorePurchasesController RestorePurchasesPrefab;

		public ParentGatePanelController ParentPermissionPrefab;

		public DisplayNativeWebPage NativeWebPagePrefab;

		public MessageDialogOverlay MessageDialogOverlayPrefab;

		public ReferralStoreHelper referralStoreHelper;

		private GameObject currentPanel;

		private string currentPanelScene;

		private string targetPanelScene;

		private string previousPanelScene;

		private LoginContext loginContext;

		private ParentGatePanelController parentGatePanelController;

		private RestorePurchasesController restorePurchasesController;

		private Action postLoginAction;

		private LoadingPanelController loadingPanel;

		private MessageDialogOverlay messageDialogOverlay;

		private ConfirmationDialog confirmationDialog;

		private GameObject loadingIconObject;

		private PlayerDataService playerDataService;

		private DisplayNativeWebPage nativeWebPageinstance;

		private GoldenGogglesDialog goldenGogglesDialog;

		public PanelLoader CurrentLoader
		{
			get;
			private set;
		}

		public bool IsLoadingPanel
		{
			get
			{
				if (CurrentLoader == null)
				{
					return false;
				}
				return !CurrentLoader.IsFinished;
			}
		}

		public event EventHandler OnParentGateSuccess;

		public event EventHandler OnParentGateClosed;

		private void LoadEnvironmentFromPrefs()
		{
			Service.Get<IDirectoryServiceClient>().Environment = (CPEnvironment)PlayerPrefs.GetInt("TargetEnvironment", 3);
		}

		public void init()
		{
			Service.Get<IDirectoryServiceClient>().Environment = CPEnvironment.PROD;
			Service.Get<EventDataService>().OnUIEvent += UIEventHandler;
			playerDataService = Service.Get<PlayerDataService>();
			previousPanelScene = null;
		}

		private void OnDestroy()
		{
			Service.Get<EventDataService>().OnUIEvent -= UIEventHandler;
		}

		private void OnApplicationFocus(bool focus)
		{
			if (!focus && IsLoadingPanel && CurrentLoader.PanelScene == HUDPanel)
			{
				CurrentLoader.OnFinished += delegate
				{
					if (!Service.Get<PlayerController>().GameEnded)
					{
						Service.Get<EventDataService>().SendUIEvent(this, new UIEvent(UIEvent.uiGameEvent.RequestPause));
					}
				};
			}
		}

		private void UIEventHandler(object sender, UIEvent _e)
		{
			switch (_e.type)
			{
			case UIEvent.uiGameEvent.AccountRetrieved:
			case UIEvent.uiGameEvent.LoginSuccess:
			case UIEvent.uiGameEvent.LoginFailed:
			case UIEvent.uiGameEvent.LoginCancelled:
			case UIEvent.uiGameEvent.Logout:
			case UIEvent.uiGameEvent.Back:
			case UIEvent.uiGameEvent.FriendHighScoresLoaded:
			case UIEvent.uiGameEvent.SelectBoosts:
			case UIEvent.uiGameEvent.BoostEquiped:
			case UIEvent.uiGameEvent.BoostUnequiped:
			case UIEvent.uiGameEvent.LoadingStarting:
			case UIEvent.uiGameEvent.LoadingComplete:
				break;
			case UIEvent.uiGameEvent.LoginRequest:
				ShowLogin();
				break;
			case UIEvent.uiGameEvent.IAPRequest:
				ShowParentGate(ShowStore, null);
				break;
			case UIEvent.uiGameEvent.AboutMembershipRequest:
				ShowParentGate(OpenAboutMembership, null);
				break;
			case UIEvent.uiGameEvent.SettingsRequest:
				ShowSettings();
				break;
			case UIEvent.uiGameEvent.MainMenuRequest:
				Service.Get<IAudio>().Music.Volume = 1f;
				ShowMainMenu();
				break;
			case UIEvent.uiGameEvent.RequestPause:
				ShowPause();
				break;
			case UIEvent.uiGameEvent.RequestUnpause:
				ShowGameHUD();
				break;
			case UIEvent.uiGameEvent.LeaderboardRequest:
				ShowLeaderboard();
				break;
			case UIEvent.uiGameEvent.DisneyAffiliateRequest:
				ShowParentGate(OpenDisneyAffiliateStore, null);
				break;
			case UIEvent.uiGameEvent.AffiliateRequest:
				if (_e.data is AppIconsController.TrackedApp)
				{
					AppIconsController.TrackedApp trackedApp = (AppIconsController.TrackedApp)_e.data;
					ShowParentGate(delegate
					{
						OpenExternalApp(trackedApp);
					}, string.Empty);
				}
				break;
			case UIEvent.uiGameEvent.WebViewRequest:
			{
				string text2 = _e.data as string;
				if (!string.IsNullOrEmpty(text2))
				{
					StartWebViewer(text2);
				}
				break;
			}
			case UIEvent.uiGameEvent.OpenExternalURL:
			{
				string text = _e.data as string;
				if (!string.IsNullOrEmpty(text))
				{
					ShowParentGate(OpenExternalURL, text);
				}
				break;
			}
			case UIEvent.uiGameEvent.RequestDisablePanelInput:
				EnablePanelInput(enabled: false);
				break;
			case UIEvent.uiGameEvent.RequestEnablePanelInput:
				EnablePanelInput(enabled: true);
				break;
			case UIEvent.uiGameEvent.RestorePurchaseRequest:
				RestorePurchases();
				break;
			}
		}

		public string GetCurrentUIScene()
		{
			return currentPanelScene;
		}

		public string GetTargetUIScene()
		{
			return targetPanelScene;
		}

		public string GetPreviousUIScene()
		{
			return previousPanelScene;
		}

		public GameObject GetCurrentPanel()
		{
			return currentPanel;
		}

		public void SetLoadingPanel(LoadingPanelController loadingPanel)
		{
			this.loadingPanel = loadingPanel;
		}

		public void ShowPreviousUIScene()
		{
			showPanel(previousPanelScene);
		}

		public void ShowMainMenu()
		{
			ShowMainMenu(null);
		}

		public void ShowMainMenu(Action<GameObject> loadedCallback)
		{
			Service.Get<IAudio>().Ambience.Volume = 1f;
			showPanel(MainMenuPanel, loadedCallback);
		}

		public void ShowGameHUD()
		{
			ShowGameHUD(null);
		}

		public void ShowGameHUD(Action<GameObject> loadedCallback)
		{
			if (currentPanelScene != HUDPanel)
			{
				showPanel(HUDPanel, loadedCallback);
			}
		}

		public void ShowPause()
		{
			ShowPause(null);
		}

		public void ShowPause(Action<GameObject> loadedCallback)
		{
			if (currentPanelScene == HUDPanel)
			{
				showPanel(PauseMenuPanel, loadedCallback);
			}
		}

		public void ShowLeaderboard()
		{
			Service.Get<IAudio>().Ambience.Volume = 0.5f;
			Service.Get<IAudio>().Music.Play(MusicTrack.Leaderboard);
			showPanel(LeaderBoardPanel);
		}

		public void ShowBoostMenu()
		{
			BoostPurchaseManager boostPurchaseManager = Service.Get<BoostPurchaseManager>();
			if (boostPurchaseManager.IsLoading)
			{
				loadingPanel.AddLoadingComponent("boost_purchases_loading");
				boostPurchaseManager.OnLoadComplete += OnBoostPurchasesLoaded;
			}
			showPanel(BoostMenuPanel);
		}

		private void OnBoostPurchasesLoaded(object sender, EventArgs e)
		{
			Service.Get<BoostPurchaseManager>().OnLoadComplete -= OnBoostPurchasesLoaded;
			loadingPanel.RemoveLoadingComponent("boost_purchases_loading");
		}

		public void ShowEndGame()
		{
			ShowEndGame(null);
		}

		public void ShowEndGame(Action<GameObject> loadedCallback)
		{
			Service.Get<IAudio>().Music.Play(MusicTrack.Leaderboard);
			Service.Get<IAudio>().Ambience.Play(AmbienceTrack.UIAmbientGusting);
			showPanel(EndGamePanel, loadedCallback);
		}

		public void ShowSettings()
		{
			ShowSettings(null);
		}

		public void ShowSettings(Action<GameObject> loadedCallback)
		{
			showPanel(SettingsPanel, loadedCallback);
		}

		public void ShowLogin(Action successAction = null)
		{
			postLoginAction = successAction;
			SetupLogin(addToCanvas: true);
			loginContext.DetermineAndShowLoginState();
		}

		public void ShowStore(string url)
		{
			Service.Get<IAudio>().Ambience.Volume = 0.5f;
			Service.Get<IAudio>().Music.Play(MusicTrack.Storefront);
			showPanel(StoreMenuPanel);
		}

		public void ShowLoadingIcon()
		{
			GameObject original = Resources.Load<GameObject>("Prefabs/UIElements/LoadingIcon");
			loadingIconObject = (UnityEngine.Object.Instantiate(original) as GameObject);
			loadingIconObject.GetComponent<RectTransform>().SetParent(OverlayContainer.GetComponent<RectTransform>(), worldPositionStays: false);
			EnablePanelInput(enabled: false);
		}

		public void HideLoadingIcon()
		{
			if (loadingIconObject != null)
			{
				GameObjectUtil.CleanupImageReferences(loadingIconObject);
				UnityEngine.Object.Destroy(loadingIconObject);
				loadingIconObject = null;
			}
			EnablePanelInput(enabled: true);
		}

		public void RestorePurchases()
		{
			restorePurchasesController = (UnityEngine.Object.Instantiate(RestorePurchasesPrefab) as RestorePurchasesController);
			ShowLoadingIcon();
			restorePurchasesController.RestorePurchases(getGooglePlayToken());
			RestorePurchasesController obj = restorePurchasesController;
			obj.RestoreCompleted = (RestorePurchasesController.RestorePurchasesCompletedDelegate)Delegate.Combine(obj.RestoreCompleted, new RestorePurchasesController.RestorePurchasesCompletedDelegate(RestoreCompleted));
			RestorePurchasesController obj2 = restorePurchasesController;
			obj2.RestoreFailed = (RestorePurchasesController.RestorePurchasesFailedDelegate)Delegate.Combine(obj2.RestoreFailed, new RestorePurchasesController.RestorePurchasesFailedDelegate(RestoreFailed));
		}

		private void RestoreFailed(string errorToken, int errorNumber)
		{
			HideLoadingIcon();
			ShowDialog("purchase.restore.found.nothing");
			CleanUpRestore();
		}

		private void RestoreCompleted(SavedStorePurchasesCollection savedStoreCollection)
		{
			HideLoadingIcon();
			ShowDialog("purchase.restore.found.item");
			IList<string> list = new List<string>();
			foreach (SavedStorePurchaseData purchasedItemsDatum in savedStoreCollection.PurchasedItemsData)
			{
				list.Add(purchasedItemsDatum.sku);
			}
			Service.Get<BoostPurchaseManager>().OnRestore(list);
			CleanUpRestore();
		}

		private void CleanUpRestore()
		{
			RestorePurchasesController obj = restorePurchasesController;
			obj.RestoreCompleted = (RestorePurchasesController.RestorePurchasesCompletedDelegate)Delegate.Remove(obj.RestoreCompleted, new RestorePurchasesController.RestorePurchasesCompletedDelegate(RestoreCompleted));
			RestorePurchasesController obj2 = restorePurchasesController;
			obj2.RestoreFailed = (RestorePurchasesController.RestorePurchasesFailedDelegate)Delegate.Remove(obj2.RestoreFailed, new RestorePurchasesController.RestorePurchasesFailedDelegate(RestoreFailed));
			GameObjectUtil.CleanupImageReferences(restorePurchasesController.gameObject);
			UnityEngine.Object.Destroy(restorePurchasesController.gameObject);
		}

		public string getGooglePlayToken()
		{
			return "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAqK1nPVlanbMpEX28Fxt6PM2uwq4NxUrzTK95rvqlsH178ZoV9+lzeyiDn81CUOUrSCcvQ+mEqHf7yg4T/aMv94FQYpjCt5L0xTXmAzdYUOz4Kqgww42Vx+9BvJmGTkuQogUx0Imqgn34JbYfsUocrq1+iWxMnn89DmKSqIYluOR1Q+SKA0HvuOyFAvMB3OyFQ/AgV3HzxL/aUgRbi6NiD1BDxZAVnS71pF0+k/EBTPLGeGrv2KcrFMwRz5qvSPWButkW+doR2IQnjAU8vArT6hwDwLV23ggtEuu7tzXSxSoXh77fwFyrilNPWl5q1hknNEwZCbTEuunJ4CMX3plz0QIDAQAB";
		}

		public void TryAutoLogin()
		{
			EnablePanelInput(enabled: false);
			LoginBIUtils loginBIUtils = new LoginBIUtils();
			loginBIUtils.ContextName = "Sled Rascer BI Context/Login";
			DoAutoLoginCMD doAutoLoginCMD = new DoAutoLoginCMD("SledRacer", "1.3", MWSClient.Instance, PDRClient.Instance, loginBIUtils, this, 5f);
			doAutoLoginCMD.LoginSucceeded += delegate(IGetAuthTokenResponse r, string u, string p)
			{
				StartCoroutine(OnLoginSuccess(r, u, p));
			};
			doAutoLoginCMD.LoginFailed += OnAutoLoginFailed;
			doAutoLoginCMD.Execute();
		}

		private void SetupLogin(bool addToCanvas)
		{
			GameObject original = Resources.Load<GameObject>("Prefabs/LoginContext");
			GameObject gameObject = UnityEngine.Object.Instantiate(original) as GameObject;
			loginContext = gameObject.GetComponent<LoginContext>();
			if (addToCanvas)
			{
				loginContext.GetComponent<RectTransform>().SetParent(OverlayContainer.GetComponent<RectTransform>(), worldPositionStays: false);
			}
			LoginContext obj = loginContext;
			obj.LoginSucceeded = (LoginContext.LoginSucceededDelegate)Delegate.Combine(obj.LoginSucceeded, (LoginContext.LoginSucceededDelegate)delegate(IGetAuthTokenResponse r, string u, string p)
			{
				StartCoroutine(OnLoginSuccess(r, u, p));
			});
			LoginContext obj2 = loginContext;
			obj2.LoginClosed = (LoginContext.LoginClosedDelegate)Delegate.Combine(obj2.LoginClosed, new LoginContext.LoginClosedDelegate(OnLoginClose));
			loginContext.LoginFailed += OnLoginFailed;
			RectTransform[] array = new RectTransform[2];
			GameObject gameObject2 = Resources.Load<GameObject>("Prefabs/UIElements/SRMemberBenefit1");
			GameObject gameObject3 = Resources.Load<GameObject>("Prefabs/UIElements/SRMemberBenefit2");
			if (gameObject2 == null)
			{
				UnityEngine.Debug.LogError("Failed to load SRMemberBenefit1");
			}
			if (gameObject3 == null)
			{
				UnityEngine.Debug.LogError("Failed to load SRMemberBenefit2");
			}
			array[0] = gameObject2.GetComponent<RectTransform>();
			array[1] = gameObject3.GetComponent<RectTransform>();
			if (array != null && array.Length > 0)
			{
				loginContext.MemberBenefitViewPrefabs = array;
			}
			loginContext.AppID = "SledRacer";
			loginContext.AppVersion = "1.3";
			EnablePanelInput(enabled: false);
		}

		private IEnumerator OnLoginSuccess(IGetAuthTokenResponse response, string username, string password)
		{
			DevTrace("OnLoginSuccess");
			if (postLoginAction != null)
			{
				postLoginAction();
				postLoginAction = null;
			}
			bool flag = playerDataService.IsPlayerLoggedIn();
			playerDataService.LoadingAccount = true;
			playerDataService.LoadingRewardStatus = true;
			if (LocalPlayerAccountService.IsLocalAuthResponse(response))
			{
				yield return null;
				if (flag)
				{
					Service.Get<EventDataService>().SendUIEvent(this, new UIEvent(UIEvent.uiGameEvent.SwitchingUser));
				}
				Service.Get<EventDataService>().SendUIEvent(this, new UIEvent(UIEvent.uiGameEvent.LoginSuccess));
				SavedPlayerData player = LocalPlayerAccountService.GetPlayer(response.AuthData.PlayerSwid);
				playerDataService.LoadingAccount = false;
				playerDataService.LoadingRewardStatus = false;
				playerDataService.LoadedAccount = LocalPlayerAccountService.BuildAccount(player);
				playerDataService.LoadedRewardStatus = LocalPlayerAccountService.GetRewardStatus(player);
				OnAccountAndRewardStatusLoaded();
				Service.Get<LeaderboardManager>().LoadCachedFriendHighScores("SledRacer", OnFriendHighScoresLoaded);
				CleanupLoginContext();
				yield break;
			}
			Service.Get<IMWSClient>().GetAccount(OnGetAccount);
			loadRewardStatus(response.AuthData.PlayerSwid);
			Service.Get<LeaderboardManager>().LoadCachedFriendHighScores("SledRacer", OnFriendHighScoresLoaded);
			yield return null;
			if (flag)
			{
				Service.Get<EventDataService>().SendUIEvent(this, new UIEvent(UIEvent.uiGameEvent.SwitchingUser));
			}
			Service.Get<EventDataService>().SendUIEvent(this, new UIEvent(UIEvent.uiGameEvent.LoginSuccess));
			CleanupLoginContext();
		}

		private void loadRewardStatus(string playerSwid)
		{
			if (playerDataService.RewardStatusCheckRequired(playerSwid))
			{
				Service.Get<LeaderboardManager>().GetRewardStatus("SledRacer", OnGetRewardStatus);
			}
			else
			{
				Service.Get<IMWSClient>().GetPlayerCardData(OnGetPlayerCardData);
			}
		}

		private void OnGetAccount(IGetAccountResponse obj)
		{
			UnityEngine.Debug.Log("OnGetAccount");
			playerDataService.LoadingAccount = false;
			playerDataService.LoadedAccount = obj.Account;
			CheckAccountAndRewardStatusLoaded();
		}

		private void OnGetPlayerCardData(IGetPlayerCardDataResponse playerCardData)
		{
			playerDataService.LoadingRewardStatus = false;
			ConfigController configController = Service.Get<ConfigController>();
			LeaderBoardRewardStatus leaderBoardRewardStatus = new LeaderBoardRewardStatus();
			if (playerCardData.PlayerCardData.Items[configController.RewardItemType].Contains(configController.RewardItemId))
			{
				leaderBoardRewardStatus.Status = LeaderBoardRewardStatus.RewardStatus.NOT_THE_LEADER_REWARD_OWNED;
			}
			else
			{
				leaderBoardRewardStatus.Status = LeaderBoardRewardStatus.RewardStatus.NOT_THE_LEADER;
			}
			playerDataService.LoadedRewardStatus = leaderBoardRewardStatus;
			CheckAccountAndRewardStatusLoaded();
		}

		private void OnGetRewardStatus(LeaderBoardRewardStatus rewardStatus)
		{
			UnityEngine.Debug.Log("OnGetRewardStatus: " + rewardStatus.ToString());
			playerDataService.LoadingRewardStatus = false;
			playerDataService.LoadedRewardStatus = rewardStatus;
			CheckAccountAndRewardStatusLoaded();
		}

		private void CheckAccountAndRewardStatusLoaded()
		{
			if (!playerDataService.LoadingAccount && !playerDataService.LoadingRewardStatus)
			{
				OnAccountAndRewardStatusLoaded();
			}
		}

		private void OnAccountAndRewardStatusLoaded()
		{
			UnityEngine.Debug.Log("OnAccountAndRewardStatusLoaded");
			EnablePanelInput(enabled: true);
			playerDataService.setPlayerAccount(playerDataService.LoadedAccount, playerDataService.LoadedRewardStatus);
			playerDataService.LoadedAccount = null;
			Service.Get<EventDataService>().SendUIEvent(this, new UIEvent(UIEvent.uiGameEvent.AccountRetrieved));
		}

		private void OnFriendHighScoresLoaded(LeaderBoardResponse response)
		{
			Service.Get<EventDataService>().SendUIEvent(this, new UIEvent(UIEvent.uiGameEvent.FriendHighScoresLoaded));
		}

		private void OnLoginClose()
		{
			DevTrace("OnLoginClose");
			postLoginAction = null;
			Service.Get<EventDataService>().SendUIEvent(this, new UIEvent(UIEvent.uiGameEvent.LoginCancelled));
			EnablePanelInput(enabled: true);
			CleanupLoginContext();
		}

		private void OnAutoLoginFailed(IHTTPResponse _response)
		{
			DevTrace("OnAutoLoginFailed");
			postLoginAction = null;
			Service.Get<EventDataService>().SendUIEvent(this, new UIEvent(UIEvent.uiGameEvent.LoginFailed));
			EnablePanelInput(enabled: true);
		}

		private void OnLoginFailed(IHTTPResponse obj)
		{
			DevTrace("OnLoginFailed");
			postLoginAction = null;
			Service.Get<EventDataService>().SendUIEvent(this, new UIEvent(UIEvent.uiGameEvent.LoginFailed));
		}

		public void EnablePanelInput(bool enabled)
		{
			StartCoroutine(delayedEnablePanelInput(enabled));
		}

		private IEnumerator delayedEnablePanelInput(bool enabled)
		{
			UnityEngine.Debug.Log("EnablePanelInput " + enabled);
			if (currentPanel != null)
			{
				if (enabled)
				{
					yield return new WaitForEndOfFrame();
				}
				if (currentPanel != null && currentPanel.GetComponent<GraphicRaycaster>() != null)
				{
					currentPanel.GetComponent<GraphicRaycaster>().enabled = enabled;
				}
			}
		}

		private void SetupParentGate()
		{
			EnablePanelInput(enabled: false);
			parentGatePanelController = (UnityEngine.Object.Instantiate(ParentPermissionPrefab) as ParentGatePanelController);
			RectTransform component = parentGatePanelController.GetComponent<RectTransform>();
			component.SetParent(OverlayContainer.GetComponent<RectTransform>(), worldPositionStays: false);
			ParentGatePanelController obj = parentGatePanelController;
			obj.OnComplete = (ParentGatePanelController.OnCompleteDelegate)Delegate.Combine(obj.OnComplete, new ParentGatePanelController.OnCompleteDelegate(CleanupParentGate));
		}

		public void ShowParentGate(Action<string> onSuccess, string arg, bool force = false)
		{
			if (false || force)
			{
				SetupParentGate();
				parentGatePanelController.ShowParentPermisson(delegate(string str)
				{
					if (this.OnParentGateSuccess != null)
					{
						this.OnParentGateSuccess(this, null);
					}
					onSuccess(str);
				}, arg);
			}
			else
			{
				StartCoroutine(triggerParentGateSuccess(onSuccess, arg));
			}
		}

		private IEnumerator triggerParentGateSuccess(Action<string> onSuccess, string arg)
		{
			yield return new WaitForEndOfFrame();
			if (this.OnParentGateSuccess != null)
			{
				this.OnParentGateSuccess(this, null);
			}
			onSuccess(arg);
		}

		private void CleanupParentGate()
		{
			EnablePanelInput(enabled: true);
			ParentGatePanelController obj = parentGatePanelController;
			obj.OnComplete = (ParentGatePanelController.OnCompleteDelegate)Delegate.Remove(obj.OnComplete, new ParentGatePanelController.OnCompleteDelegate(CleanupParentGate));
			UnityEngine.Object.Destroy(parentGatePanelController.gameObject);
			if (this.OnParentGateClosed != null)
			{
				this.OnParentGateClosed(this, null);
			}
		}

		private void StartWebViewer(string url)
		{
			if (!(nativeWebPageinstance != null))
			{
				EnablePanelInput(enabled: false);
				nativeWebPageinstance = (UnityEngine.Object.Instantiate(NativeWebPagePrefab) as DisplayNativeWebPage);
				HardwareBackButtonDispatcher.SetTargetClickHandler(nativeWebPageinstance.GetComponentInChildren<Button>());
				RectTransform component = nativeWebPageinstance.GetComponent<RectTransform>();
				component.SetParent(OverlayContainer.GetComponent<RectTransform>(), worldPositionStays: false);
				DisplayNativeWebPage displayNativeWebPage = nativeWebPageinstance;
				displayNativeWebPage.Closed = (DisplayNativeWebPage.ClosedDelegate)Delegate.Combine(displayNativeWebPage.Closed, new DisplayNativeWebPage.ClosedDelegate(OnWebPageClosed));
				nativeWebPageinstance.DisplayPage(url);
			}
		}

		private void OnWebPageClosed()
		{
			DisplayNativeWebPage displayNativeWebPage = nativeWebPageinstance;
			displayNativeWebPage.Closed = (DisplayNativeWebPage.ClosedDelegate)Delegate.Remove(displayNativeWebPage.Closed, new DisplayNativeWebPage.ClosedDelegate(OnWebPageClosed));
			UnityEngine.Object.Destroy(nativeWebPageinstance.gameObject);
			EnablePanelInput(enabled: true);
		}

		public void OpenExternalApp(AppIconsController.TrackedApp trackedApp)
		{
			Service.Get<IBILogging>().OpenExternalApp(trackedApp.id);
			AppIconsController.AppConfig appConfig = AppIconsController.AppIcons[trackedApp.id];
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				string trackingUrl = appConfig.GetAppleAppStoreTrackingURL(trackedApp.location);
				new OpenAppOnDeviceCMD(appConfig.AppleAppBundleId, appConfig.AppleAppStoreURL, delegate(OpenAppOnDeviceCMD.APP_OPEN_OUTCOME outcome)
				{
					if (outcome == OpenAppOnDeviceCMD.APP_OPEN_OUTCOME.URL)
					{
						StartCoroutine(trackOpenExternalApp(trackingUrl));
					}
				}).Execute();
			}
			else if (Application.platform == RuntimePlatform.Android)
			{
				string trackingUrl2 = appConfig.GetAndroidAppStoreTrackingURL(trackedApp.location);
				new OpenAppOnDeviceCMD(appConfig.AndroidAppBundleId, appConfig.AndroidAppStoreURL, delegate(OpenAppOnDeviceCMD.APP_OPEN_OUTCOME outcome)
				{
					if (outcome == OpenAppOnDeviceCMD.APP_OPEN_OUTCOME.URL)
					{
						StartCoroutine(trackOpenExternalApp(trackingUrl2));
					}
				}).Execute();
			}
			else
			{
				StartCoroutine(trackOpenExternalApp(appConfig.GetAndroidAppStoreTrackingURL(trackedApp.location)));
				OpenExternalURL(appConfig.AndroidAppStoreURL);
			}
		}

		private IEnumerator trackOpenExternalApp(string url)
		{
			yield return new WWW(url);
		}

		private void OpenExternalURL(string url)
		{
			UnityEngine.Debug.Log("Opening External Url: " + url);
			Application.OpenURL(url);
		}

		private void OpenDisneyAffiliateStore(string url)
		{
			SledRacerGameManager.Instance.MonitorExternalMusic = false;
			Service.Get<IAudio>().Mute();
			ReferralStoreManager.StoreHideEvent += OnStoreClosed;
			referralStoreHelper.Show();
		}

		private void OnStoreClosed(string message)
		{
			ReferralStoreManager.StoreHideEvent -= OnStoreClosed;
			SledRacerGameManager.Instance.MonitorExternalMusic = true;
			if (!musicOBJ.isMusicPlaying())
			{
				Service.Get<IAudio>().UnMute();
			}
		}

		private void OpenAboutMembership(string url)
		{
			showPanel(AboutMembershipPanel);
		}

		public void ShowDialog(string messageToken, MessageDialogOverlay.ClosedDelegate onClose = null)
		{
			messageDialogOverlay = (UnityEngine.Object.Instantiate(MessageDialogOverlayPrefab) as MessageDialogOverlay);
			messageDialogOverlay.ParentContainer = OverlayContainer.GetComponent<RectTransform>();
			if (onClose != null)
			{
				MessageDialogOverlay obj = messageDialogOverlay;
				obj.Closed = (MessageDialogOverlay.ClosedDelegate)Delegate.Combine(obj.Closed, onClose);
			}
			MessageDialogOverlay obj2 = messageDialogOverlay;
			obj2.Closed = (MessageDialogOverlay.ClosedDelegate)Delegate.Combine(obj2.Closed, new MessageDialogOverlay.ClosedDelegate(CleanUpDialog));
			messageDialogOverlay.ShowStatusTextFromToken(messageToken);
		}

		private void CleanUpDialog()
		{
			messageDialogOverlay.Closed = null;
			UnityEngine.Object.Destroy(messageDialogOverlay.gameObject);
		}

		public void ShowConfirmationDialog(string messageToken, Action<bool> closedAction)
		{
			if (confirmationDialog != null)
			{
				UnityEngine.Debug.LogError("Failed to show ConfirmationDialog for token '" + messageToken + "' because the dialog is already open");
				closedAction(obj: false);
			}
			else if (InstantiatePrefabFromResources("Prefabs/ConfirmationDialog", out confirmationDialog))
			{
				confirmationDialog.ParentContainer = OverlayContainer.GetComponent<RectTransform>();
				confirmationDialog.Closed += closedAction;
				confirmationDialog.Closed += CleanupConfirmationDialog;
				confirmationDialog.ShowStatusTextFromToken(messageToken);
			}
			else
			{
				closedAction(obj: false);
			}
		}

		private void CleanupConfirmationDialog(bool confirmed)
		{
			confirmationDialog.Closed -= CleanupConfirmationDialog;
			UnityEngine.Object.Destroy(confirmationDialog.gameObject);
			confirmationDialog = null;
		}

		private void CleanupLoginContext()
		{
			if (loginContext != null)
			{
				GameObjectUtil.CleanupImageReferences(loginContext.gameObject);
				UnityEngine.Object.Destroy(loginContext);
				loginContext = null;
			}
		}

		public void ShowGoldenGogglesDialog()
		{
			if (goldenGogglesDialog != null)
			{
				UnityEngine.Debug.LogError("Failed to show GoldenGogglesDialog because the dialog is already open");
				return;
			}
			Service.Get<IBILogging>().RewardItemGranted(1996, "Clothing", "Golden Goggles");
			InstantiatePrefabFromResources("Prefabs/GoldenGogglesDialog", out goldenGogglesDialog, OverlayContainer);
		}

		private bool InstantiatePrefabFromResources<TController>(string prefabPath, out TController controller, GameObject parent = null) where TController : Component
		{
			controller = (TController)null;
			GameObject gameObject = Resources.Load<GameObject>(prefabPath);
			if (gameObject == null)
			{
				UnityEngine.Debug.LogError("Failed to load prefab: " + prefabPath);
				return false;
			}
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject) as GameObject;
			if (gameObject2 == null)
			{
				UnityEngine.Debug.LogError("Failed to instantiate prefab: " + prefabPath);
				return false;
			}
			controller = gameObject2.GetComponent<TController>();
			if ((UnityEngine.Object)controller == (UnityEngine.Object)null)
			{
				UnityEngine.Debug.LogError("Failed to get prefab controller: " + prefabPath + " (" + typeof(TController).FullName + ")");
				if (gameObject2 != null)
				{
					UnityEngine.Object.Destroy(gameObject2);
				}
				return false;
			}
			if (parent != null)
			{
				controller.GetComponent<RectTransform>().SetParent(parent.GetComponent<RectTransform>(), worldPositionStays: false);
			}
			return true;
		}

		public void showPanel(string scene, Action<GameObject> loadedCallback = null)
		{
			UnityEngine.Debug.Log($"showPanel({scene}, {loadedCallback})");
			if (IsLoadingPanel)
			{
				UnityEngine.Debug.LogError("A panel is already loading. Please wait for the current panel to finished before loading a new panel.");
				return;
			}
			if (currentPanelScene == scene)
			{
				UnityEngine.Debug.LogError(scene + " is already loaded.");
				return;
			}
			UnloadCurrentPanel();
			bool showLoadingScreen = true;
			if ((currentPanelScene == HUDPanel && scene == PauseMenuPanel) || (currentPanelScene == PauseMenuPanel && scene == HUDPanel))
			{
				showLoadingScreen = false;
			}
			targetPanelScene = scene;
			CurrentLoader = new PanelLoader(scene, this, showLoadingScreen, loadedCallback);
			CurrentLoader.OnFinished += OnPanelLoaded;
			CurrentLoader.Start();
		}

		public void UnloadCurrentPanel()
		{
			UnityEngine.Debug.Log("UnloadCurrentPanel: " + currentPanel + " (" + currentPanelScene + ")");
			if (currentPanel != null)
			{
				CleanupLoginContext();
				GameObjectUtil.CleanupImageReferences(currentPanel);
				UnityEngine.Object.Destroy(currentPanel);
				currentPanel = null;
				StartCoroutine(DelayedUnload());
			}
		}

		private void OnPanelLoaded(GameObject panel)
		{
			if (panel == null)
			{
				UnityEngine.Debug.LogError("Error loading panel '" + CurrentLoader.PanelScene + "'. panel was null in callback");
				return;
			}
			currentPanel = panel;
			previousPanelScene = currentPanelScene;
			currentPanelScene = CurrentLoader.PanelScene;
			CurrentLoader = null;
		}

		private IEnumerator DelayedUnload()
		{
			yield return new WaitForEndOfFrame();
			Resources.UnloadUnusedAssets();
			GC.Collect();
		}

		private void DevTrace(string _msg)
		{
			UnityEngine.Debug.Log("[" + MethodBase.GetCurrentMethod().ReflectedType.Name + "] " + _msg);
		}

		public void Reset()
		{
			UnloadCurrentPanel();
			currentPanelScene = null;
			previousPanelScene = null;
			targetPanelScene = null;
		}
	}
}
