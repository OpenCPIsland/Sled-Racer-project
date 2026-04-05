using Disney.ClubPenguin.CPModuleUtils;
using UnityEngine;
using UnityEngine.UI;

namespace Disney.ClubPenguin.SledRacer
{
	public class SettingsMenuController : BaseMenuController
	{
		public Button BackButton;

		public Button LogOutButton;

		public Button LoginButton;

		public Button ManageAccountButton;

		public Toggle MusicToggle;

		public Toggle SoundToggle;

		public Text VersionText;

		public Scrollbar ScrollBar;

		private IAudio audioService;

		private EventDataService eventDataService;

		private PlayerDataService playerDataService;

		private DefaultMessageDialog defaultMessageDialog;

		private bool ignoreToggleEvents;

		protected override void VStart()
		{
			audioService = Service.Get<IAudio>();
			eventDataService = Service.Get<EventDataService>();
			playerDataService = Service.Get<PlayerDataService>();
			ignoreToggleEvents = true;
			InitAudioToggles();
			ignoreToggleEvents = false;
			VersionText.text = "1.3";
			HardwareBackButtonDispatcher.SetTargetClickHandler(BackButton);
			eventDataService.OnUIEvent += UIEventHandler;
			SetLoginButtonStates(playerDataService.IsPlayerLoggedIn());
			ScrollBar.value = 1f;
		}

		private void InitAudioToggles()
		{
			MusicToggle.isOn = audioService.Music.IsMuted();
			SoundToggle.isOn = audioService.SFX.IsMuted();
		}

		protected override void Init()
		{
		}

		private void OnDestroy()
		{
			eventDataService.OnUIEvent -= UIEventHandler;
		}

		private void UIEventHandler(object sender, UIEvent _e)
		{
			UIEvent.uiGameEvent type = _e.type;
			if (type == UIEvent.uiGameEvent.LoginSuccess)
			{
				eventDataService.SendUIEvent(this, new UIEvent(UIEvent.uiGameEvent.RequestEnablePanelInput));
				SetLoginButtonStates(loggedIn: true);
			}
		}

		private void SetLoginButtonStates(bool loggedIn)
		{
			LogOutButton.gameObject.SetActive(loggedIn);
			LoginButton.gameObject.SetActive(!loggedIn);
			ManageAccountButton.interactable = loggedIn;
		}

		public void OnBackButton()
		{
			eventDataService.SendUIEvent(this, new UIEvent(UIEvent.uiGameEvent.MainMenuRequest));
		}

		public void OnMusicToggleChanged()
		{
			if (!ignoreToggleEvents)
			{
				IMusic music = audioService.Music;
				if (MusicToggle.isOn)
				{
					Service.Get<IBILogging>().ButtonPressed(BIButton.MUTE_MUSIC, BIScreen.SETTINGS, BIScreen.SETTINGS);
					music.Mute();
				}
				else
				{
					Service.Get<IBILogging>().ButtonPressed(BIButton.UNMUTE_MUSIC, BIScreen.SETTINGS, BIScreen.SETTINGS);
					music.UnMute();
				}
			}
		}

		public void OnSoundToggleChanged()
		{
			if (!ignoreToggleEvents)
			{
				ISFX sFX = audioService.SFX;
				if (SoundToggle.isOn)
				{
					Service.Get<IBILogging>().ButtonPressed(BIButton.MUTE_SOUND, BIScreen.SETTINGS, BIScreen.SETTINGS);
					sFX.Mute();
				}
				else
				{
					Service.Get<IBILogging>().ButtonPressed(BIButton.UNMUTE_SOUND, BIScreen.SETTINGS, BIScreen.SETTINGS);
					sFX.UnMute();
				}
			}
		}

		public void OnSwitchPenguins()
		{
			eventDataService.SendUIEvent(this, new UIEvent(UIEvent.uiGameEvent.LoginRequest));
		}

		public void OnRestorePurchases()
		{
			eventDataService.SendUIEvent(this, new UIEvent(UIEvent.uiGameEvent.RestorePurchaseRequest));
		}

		public void OnResetOfflinePlayer()
		{
			Service.Get<UIManager>().ShowConfirmationDialog("settings.resetscore.confirm", OnResetConfirmClosed);
		}

		private void OnResetConfirmClosed(bool confirmed)
		{
			if (confirmed)
			{
				HighScore.SaveOfflineHighScoreInPrefs(0);
				Service.Get<PlayerDataService>().PlayerData.HighScore.SetScore(0);
				PlayerPrefs.SetInt("LaunchCount", 0);
				PlayerPrefs.SetInt("TutorialComplete:" + Service.Get<PlayerDataService>().PlayerData.Account.PlayerSwid, 0);
				if (!Service.Get<PlayerDataService>().IsPlayerLoggedIn())
				{
					Service.Get<PlayerDataService>().SwitchToOfflineData();
				}
				audioService.SFX.UnMute();
				audioService.Music.UnMute();
				InitAudioToggles();
			}
		}

		public void OnLogOut()
		{
			SetLoginButtonStates(loggedIn: false);
			eventDataService.SendUIEvent(this, new UIEvent(UIEvent.uiGameEvent.Logout));
		}

		public void OnLicenseCredits()
		{
			if (!(defaultMessageDialog == null))
			{
				return;
			}
			GameObject gameObject = Resources.Load<GameObject>("Prefabs/DefaultMessageDialogBox");
			if (gameObject == null)
			{
				UnityEngine.Debug.LogError("Failed to load DefaultMessageDialogBox prefab");
				return;
			}
			defaultMessageDialog = (Object.Instantiate(gameObject) as GameObject).GetComponent<DefaultMessageDialog>();
			if (defaultMessageDialog == null)
			{
				UnityEngine.Debug.LogError("Failed to instantiate DefaultMessageDialogBox prefab");
				return;
			}
			defaultMessageDialog.ParentContainer = GetComponent<RectTransform>();
			defaultMessageDialog.Closed += OnLicenseCreditsClosed;
			defaultMessageDialog.ShowStatusTextFromToken("SettingsMenuCanvas.FooterPanel.Panel.LicenseCredits.Title", "SettingsMenuCanvas.FooterPanel.Panel.LicenseCredits.Body");
		}

		private void OnLicenseCreditsClosed()
		{
			defaultMessageDialog.Closed -= OnLicenseCreditsClosed;
			UnityEngine.Object.Destroy(defaultMessageDialog.gameObject);
			defaultMessageDialog = null;
		}
	}
}
