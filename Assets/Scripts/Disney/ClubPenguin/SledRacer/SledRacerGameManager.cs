using Disney.ClubPenguin.ForcedUpdate;
using Disney.ClubPenguin.Login.Authentication;
using Disney.ClubPenguin.Service.DirectoryService;
using Disney.ClubPenguin.Service.MWS;
using Disney.ClubPenguin.Service.MWS.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Disney.ClubPenguin.SledRacer
{
	public class SledRacerGameManager : GameManager
	{
		public enum GameState
		{
			Loading,
			MainMenu,
			GameTutorial,
			GamePlay,
			Store
		}

		public const string VERSION = "1.3";

		public const string SLED_RACER_GAME_TYPE = "SledRacer";

		public const string IOS_BUNDLE_ID = "com.disney.clubpenguinsledracer";

		public const string APP_STORE_URL = "https://play.google.com/store/apps/details?id=com.disney.clubpenguinsledracer_goo";

		public const string TUTORIAL_PREFS_KEY = "TutorialComplete:";

		public const string GameScene = "SledGame";

		public const string LAUNCH_COUNT_KEY = "LaunchCount";

		public const string MUTED_MUSIC_PREF_KEY = "Audio.All.Music.Mute";

		public const bool ShowParentGate = false;

		public GameState CurrentGameState;

		public GameObject TrackManager;

		public GameObject MultiplayerManager;

		public GameObject NetworkManager;

		public GameObject PlayerManager;

		public GameObject GameController;

		public InputBehaviour inputBehaviour;

		public UIManager UIManager;

		public LoadingPanelController LoadingPanel;

		private ConfigController Config;

		private TrackManager trackScript;

		public PlayerController playerScript;

		private LeaderboardManager leaderboard;

		private IMWSClient mwsClient;

		private Coroutine unpauseGameRoutine;

		private Game activeGame;

		private IGameEventLogger gameLogger;

		public bool CanBounceOnObstacles;

		public bool TutorialMode;

		public bool MonitorExternalMusic = true;

		private bool waitingForInitLoad;

		private bool waitingForGameLoad;

		private bool showIntroVideo;

		public new static SledRacerGameManager Instance => GameManager.GetInstanceAs<SledRacerGameManager>();

		public bool Paused
		{
			get;
			private set;
		}

		public bool showVideo
		{
			get
			{
				return showIntroVideo;
			}
			set
			{
				showIntroVideo = value;
			}
		}

		public static event EventHandler OnGameInitFinished;

		public void ChangeGameState(GameState _targetState)
		{
			CurrentGameState = _targetState;
		}

		private void Start()
		{
			waitingForInitLoad = true;
			Config = Service.Get<ConfigController>();
			StartServices();
			ChangeGameState(GameState.Loading);
			int introFrequency = Service.Get<ConfigController>().IntroFrequency;
			int @int = PlayerPrefs.GetInt("LaunchCount", 0);
			if (musicOBJ.isMusicPlaying() || PlayerPrefs.GetInt("Audio.All.Music.Mute", 0) == 1)
			{
				showIntroVideo = false;
			}
			else
			{
				showIntroVideo = (@int % introFrequency == 0);
				PlayerPrefs.SetInt("LaunchCount", @int + 1);
				PlayerPrefs.Save();
			}
			inputBehaviour = GetComponent<InputBehaviour>();
			StartUI();
			StartCoroutine(LoadGame());
			ForcedUpdateManager.Instance.AlertOnUpdateRecommended = true;
			applicationForeground();
			if (musicOBJ.isMusicPlaying())
			{
				Service.Get<IAudio>().Mute();
			}
			setScreenAutoRotation();
			StartCoroutine(monitorExternalMusic());
		}

		private void StartServices()
		{
			Service.Set(MWSClient.Instance);
			Service.Set(DirectoryServiceClient.Instance);
			Service.Set(new PlayerDataService());
			Service.Set(new EventDataService());
			Service.Set(new BoostManager());
			Service.Set(new LeaderboardManager());
			Service.Set((IAudio)new Audio());
			Service.Set(LoadingPanel);
			Service.Set(UIManager);
			Service.Set((IBILogging)new BILogging());
			Service.Set(new BoostPurchaseManager());
			Service.Set(new NotificationService());
			ForcedUpdateManager.Instance.Init(UIManager.OverlayContainer, "SledRacer", "1.3", "https://play.google.com/store/apps/details?id=com.disney.clubpenguinsledracer_goo", 0);
			ForcedUpdateManager.Instance.DirectoryServiceClient = Service.Get<IDirectoryServiceClient>();
			UIManager.SetLoadingPanel(LoadingPanel);
			mwsClient = Service.Get<IMWSClient>();
			leaderboard = Service.Get<LeaderboardManager>();
		}

		private void OnApplicationFocus(bool focus)
		{
			UnityEngine.Debug.Log("OnApplicationFocus: " + focus);
			IAudio audio = Service.Get<IAudio>();
			if (audio != null)
			{
				if (musicOBJ.isMusicPlaying())
				{
					audio.Mute();
				}
				else if (focus)
				{
					if (audio.IsMuted())
					{
						audio.UnMute();
					}
				}
				else if (!TouchScreenKeyboard.visible)
				{
					audio.Mute();
				}
			}
			if (focus)
			{
				setScreenAutoRotation();
			}
		}

		private void setScreenAutoRotation()
		{
			bool autorotateToLandscapeRight = Screen.autorotateToLandscapeLeft = AutoRotation.AllowAutorotation();
			Screen.autorotateToLandscapeRight = autorotateToLandscapeRight;
		}

		private void OnApplicationPause(bool pause)
		{
			UnityEngine.Debug.Log("OnApplicationPause: " + pause);
			if (!pause)
			{
				ForcedUpdateManager.Instance.AlertOnUpdateRecommended = false;
				applicationForeground();
			}
			else
			{
				applicationBackground();
			}
		}

		private void OnApplicationQuit()
		{
			UnityEngine.Debug.Log("OnApplicationQuit");
			applicationBackground();
		}

		private void applicationForeground()
		{
			UnityEngine.Debug.Log("applicationForeground");
			Service.Get<NotificationService>().ClearNotifications();
			PlayerDataService playerDataService = Service.Get<PlayerDataService>();
			ForcedUpdateManager.Instance.PlayerId = ((!playerDataService.IsPlayerLoggedIn()) ? null : new long?(playerDataService.PlayerData.Account.PlayerId));
			ForcedUpdateManager.Instance.CheckVersion();
		}

		private void applicationBackground()
		{
			UnityEngine.Debug.Log("applicationBackground");
			Service.Get<NotificationService>().SetNotifications();
		}

		private IEnumerator monitorExternalMusic()
		{
			IAudio audio = Service.Get<IAudio>();
			while (true)
			{
				yield return new WaitForSeconds(3f);
				if (MonitorExternalMusic)
				{
					if (musicOBJ.isMusicPlaying() && !audio.IsMuted())
					{
						audio.Mute();
					}
					else if (!musicOBJ.isMusicPlaying() && audio.IsMuted())
					{
						audio.UnMute();
					}
				}
			}
		}

		private IEnumerator LoadGame()
		{
			DevTrace("LOADING GAME SCENE");
			Service.Get<LoadingPanelController>().AddLoadingComponent("initLoadGame");
			yield return SceneManager.LoadSceneAsync("SledGame", LoadSceneMode.Additive);
			DevTrace("GAME SCENE -- LOADED");
			GameController = GameObject.Find("/SledGameController");
			TrackManager = GameObject.Find("/SledGameController/TrackManager");
			PlayerManager = GameObject.Find("/SledGameController/MainPlayer");
			trackScript = TrackManager.GetComponent<TrackManager>();
			Service.Set(trackScript);
			playerScript = PlayerManager.GetComponent<PlayerController>();
			playerScript.inputBehaviour = inputBehaviour;
			Service.Set(playerScript);
			AddGameEventListeners();
			Camera.main.GetComponent<AudioListener>().enabled = false;
			GameObject.FindWithTag("PlayerCamera").GetComponent<AudioListener>().enabled = true;
			Service.Get<LoadingPanelController>().RemoveLoadingComponent("initLoadGame");
		}

		private void StartUI()
		{
			UIManager.init();
			AddUIEventListeners();
			UIManager.ShowMainMenu();
			ChangeGameState(GameState.MainMenu);
		}

		private void AddUIEventListeners()
		{
			Service.Get<EventDataService>().OnUIEvent += UIEventHandler;
		}

		private void AddGameEventListeners()
		{
			PlayerController.OnGameEvent += GameEventHandler;
		}

		private void UIEventHandler(object sender, UIEvent _e)
		{
			switch (_e.type)
			{
			case UIEvent.uiGameEvent.SelectBoosts:
			{
				string playerSwid = Service.Get<PlayerDataService>().PlayerData.Account.PlayerSwid;
				TutorialMode = (PlayerPrefs.GetInt("TutorialComplete:" + playerSwid, 0) == 0);
				if (TutorialMode)
				{
					changeStateToGamePlay();
				}
				else
				{
					UIManager.ShowBoostMenu();
				}
				break;
			}
			case UIEvent.uiGameEvent.Play:
				changeStateToGamePlay();
				break;
			case UIEvent.uiGameEvent.RequestPause:
				PauseGame();
				break;
			case UIEvent.uiGameEvent.RequestUnpause:
				UnpauseGame();
				break;
			case UIEvent.uiGameEvent.AccountRetrieved:
				DevTrace("Account Retrieved");
				Service.Get<BoostPurchaseManager>().OnLogin(Service.Get<PlayerDataService>().PlayerData.Account.Member);
				leaderboard.LoadMyAllTimeHighScore("SledRacer", Service.Get<PlayerDataService>().PlayerData.HighScore.SetScore);
				break;
			case UIEvent.uiGameEvent.Logout:
				new ForgetPlayerPasswordCMD(Service.Get<PlayerDataService>().PlayerData.Account.Username).Execute();
				mwsClient.ClearAuthToken();
				switchToOfflineUser();
				break;
			case UIEvent.uiGameEvent.SwitchingUser:
				switchToOfflineUser();
				break;
			case UIEvent.uiGameEvent.MainMenuRequest:
				UnpauseGame();
				playerScript.ChangeStateToEnd();
				break;
			case UIEvent.uiGameEvent.LoadingComplete:
				if (waitingForInitLoad && SledRacerGameManager.OnGameInitFinished != null)
				{
					SledRacerGameManager.OnGameInitFinished(this, null);
				}
				if (waitingForGameLoad)
				{
					playerScript.ResetStart();
					Service.Get<IAudio>().Ambience.Play(AmbienceTrack.GameAmbientGusting);
					Service.Get<IAudio>().Music.Play(MusicTrack.Gameplay);
					trackScript.LeadPlayer = PlayerManager.transform;
					if (Paused)
					{
						UnpauseGame();
					}
				}
				waitingForGameLoad = false;
				waitingForInitLoad = false;
				break;
			}
		}

		private void switchToOfflineUser()
		{
			Service.Get<PlayerDataService>().SwitchToOfflineData();
			leaderboard.ClearCachedFriendHighScores();
			Service.Get<BoostPurchaseManager>().OnLogout();
			Service.Get<BoostManager>().ClearEquipeBoosts();
		}

		private void GameEventHandler(object sender, GameEvent _e)
		{
			GameEvent.Type type = _e.type;
			if (type != GameEvent.Type.End)
			{
				return;
			}
			UnityEngine.Debug.Log("Heard A GameEvent.END " + _e.type);
			int currentScore = getCurrentScore();
			Service.Get<LoadingPanelController>().AddLoadingComponent("saveGame");
			if (activeGame != null)
			{
				Service.Get<PlayerDataService>().setFinalScore(currentScore);
				leaderboard.saveGame(createSaveGameRequest(currentScore), OnHighScoreSaved);
				return;
			}
			int? score = Service.Get<PlayerDataService>().PlayerData.HighScore.Score;
			if (score.HasValue && currentScore > score.Value)
			{
				HighScore.SaveOfflineHighScoreInPrefs(currentScore);
			}
			Service.Get<PlayerDataService>().setFinalScore(currentScore);
			OnHighScoreSaved(null);
		}

		private void OnHighScoreSaved(GameResult result)
		{
			PlayerData playerData = Service.Get<PlayerDataService>().PlayerData;
			playerData.LastGameCoinsEarned = 0L;
			playerData.TotalCoins = 0L;
			if (result != null)
			{
				try
				{
					playerData.LastGameCoinsEarned = (long)result.rewards["coinsEarned"];
					playerData.TotalCoins = (long)result.rewards["totalCoins"];
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogError("Unable to extract coins earned from server response: " + ex.Message);
				}
			}
			if (UIManager.IsLoadingPanel)
			{
				UIManager.CurrentLoader.OnFinished += delegate
				{
					UIManager.ShowEndGame();
				};
			}
			else
			{
				UIManager.ShowEndGame();
			}
			Service.Get<LoadingPanelController>().RemoveLoadingComponent("saveGame");
		}

		private void Update()
		{
#if ENABLE_INPUT_SYSTEM
			if (Keyboard.current != null && Keyboard.current.digit1Key.wasPressedThisFrame)
			{
				DevTrace("Pressed 1 - ");
			}
			if (Keyboard.current != null && Keyboard.current.digit2Key.wasPressedThisFrame)
			{
				DevTrace("Pressed 2 - ");
			}
#endif
		}

		private void changeStateToGamePlay()
		{
			Service.Get<LoadingPanelController>().AddLoadingComponent("loadGame");
			if (TutorialMode)
			{
				ChangeGameState(GameState.GameTutorial);
			}
			else
			{
				ChangeGameState(GameState.GamePlay);
			}
			if (Service.Get<PlayerDataService>().IsPlayerLoggedIn())
			{
				gameLogger = new GameEventLogger();
				loadState_GamePlay();
				leaderboard.startGame("SledRacer", delegate(Game response)
				{
					activeGame = response;
					if (activeGame != null)
					{
						Config.SetConfiguration(activeGame.GameConfig);
					}
					else
					{
						gameLogger = new NullGameEventLogger();
					}
					startState_GamePlay();
				});
			}
			else
			{
				activeGame = null;
				gameLogger = new NullGameEventLogger();
				loadState_GamePlay();
				startState_GamePlay();
			}
		}

		private int randomSeed;

		private void loadState_GamePlay()
		{
			trackScript.OnReady += SpawnPlayer;
			this.randomSeed = createRandomSeed ();
			UnityEngine.Random.InitState(randomSeed);
			UIManager.ShowGameHUD();
		}

		private void startState_GamePlay()
		{
			playerScript.ChangeStateToEnd();
			trackScript.ResetStart();
			gameLogger.log(Disney.ClubPenguin.Service.MWS.Domain.Event.RANDOM, randomSeed.ToString());
			Service.Get<IBILogging>().StartGame(Service.Get<BoostManager>().EquipedBoosts);
		}

		private int createRandomSeed()
		{
			return DateTime.Now.Second;
		}

		private void SpawnPlayer()
		{
			trackScript.OnReady -= SpawnPlayer;
			playerScript.gameLogger = gameLogger;
			waitingForGameLoad = true;
			Service.Get<LoadingPanelController>().RemoveLoadingComponent("loadGame");
		}

		public void PauseGame()
		{
			if (!Paused)
			{
				gameLogger.log(Disney.ClubPenguin.Service.MWS.Domain.Event.PAUSE_START);
				Time.timeScale = 0f;
				Paused = true;
				if (unpauseGameRoutine != null)
				{
					StopCoroutine(unpauseGameRoutine);
				}
			}
		}

		public void UnpauseGame()
		{
			if (Paused)
			{
				gameLogger.log(Disney.ClubPenguin.Service.MWS.Domain.Event.PAUSE_END);
				Paused = false;
				unpauseGameRoutine = StartCoroutine(UnpauseGameRoutine());
			}
		}

		private IEnumerator UnpauseGameRoutine()
		{
			while (UIManager.GetCurrentUIScene() == UIManager.PauseMenuPanel)
			{
				yield return null;
			}
			while (Time.timeScale < 1f)
			{
				Time.timeScale += 0.1f;
				yield return null;
			}
			Time.timeScale = 1f;
			unpauseGameRoutine = null;
		}

		private GameResult createSaveGameRequest(int score)
		{
			PlayerResult playerResult = new PlayerResult();
			playerResult.PlayerId = Service.Get<PlayerDataService>().PlayerData.Account.PlayerId;
			playerResult.Result = score.ToString();
			GameResult gameResult = new GameResult();
			gameResult.gameId = activeGame.GameId;
			gameResult.gameType = "SledRacer";
			gameResult.playerId = Service.Get<PlayerDataService>().PlayerData.Account.PlayerId;
			gameResult.playerResults = new List<PlayerResult>(1);
			gameResult.playerResults.Add(playerResult);
			gameResult.achievements = new Dictionary<string, object>(1);
			gameResult.gameEvents = gameLogger.getGameLog();
			return gameResult;
		}

		public int getCurrentScore()
		{
			if (trackScript != null && CurrentGameState != GameState.GameTutorial)
			{
				return (int)trackScript.DistanceTravelled;
			}
			return 0;
		}

		public Vector3 getCurrentVelocity()
		{
			if (playerScript != null)
			{
				return playerScript.CurrentVelocity;
			}
			return Vector3.zero;
		}

		public Vector3 getCurrentForces()
		{
			if (playerScript != null)
			{
				return playerScript.AppliedForces;
			}
			return Vector3.zero;
		}

		private void DevTrace(string _msg)
		{
		}
	}
}