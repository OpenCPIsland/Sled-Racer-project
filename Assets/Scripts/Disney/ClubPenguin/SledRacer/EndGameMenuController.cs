using DevonLocalization.Core;
using Disney.ClubPenguin.CPModuleUtils;
using Disney.ClubPenguin.Service.MWS.Domain;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Disney.ClubPenguin.SledRacer
{
	public class EndGameMenuController : BaseMenuController
	{
		private class TabView
		{
			public Toggle toggle;

			public Text text;

			public Color activeTextColor;

			public Color inactiveTextColor;

			public TabView(Toggle toggle, Text text, Color activeTextColor, Color inactiveTextColor)
			{
				this.toggle = toggle;
				this.text = text;
				this.activeTextColor = activeTextColor;
				this.inactiveTextColor = inactiveTextColor;
				this.toggle.onValueChanged.AddListener(OnToggleChanged);
				OnToggleChanged(toggle.isOn);
			}

			private void OnToggleChanged(bool isActive)
			{
				text.color = ((!isActive) ? inactiveTextColor : activeTextColor);
			}
		}

		private const int SECONDS_IN_A_DAY = 86400;

		private const string COUNTDOWN_DAYS_LEFT_TOKEN = "EndGameMenuCanvas.EndGameContentPanel.MiddlePanel.GameObject.ScoreBoardTitles.DaysLeftText";

		private const string COUNTDOWN_HOURS_LEFT_TOKEN = "EndGameMenuCanvas.EndGameContentPanel.MiddlePanel.GameObject.ScoreBoardTitles.HoursLeftText";

		private const string COUNTDOWN_ENDING_SOON_TOKEN = "EndGameMenuCanvas.EndGameContentPanel.MiddlePanel.GameObject.ScoreBoardTitles.EndingSoonText";

		public GameObject HighScoreRowPrefab;

		public GameObject NoFriendsPrefab;

		public GameObject NotLoggedInPrefab;

		public GameObject NotInternetPrefab;

		public GameObject GoldenAwardPrefab;

		public GameObject ScoreListLayout;

		public Text ScoreText;

		public Text HighScoreText;

		public Text CoinsText;

		public Text TotalCoinsText;

		public Text CountdownText;

		public Toggle AllToggle;

		public Toggle FriendsToggle;

		public Text AllText;

		public Text FriendsText;

		public GameObject LoadingIcon;

		public Button QuitButton;

		private TabView allTabView;

		private TabView friendsTabView;

		private TabView activeTabView;

		private GameObject NotLoggedInView;

		private LeaderboardManager leaderboard;

		private LeaderBoardResponse allScores;

		private LeaderBoardResponse friendScores;

		private bool scriptAttached;

		private int highlightedRowIndex;

		private GameObject[] currentRows;

		private ScrollRect scoreListScrollRect;

		private RectTransform window;

		private RectTransform content;

		private AudioGroupEndGame myAudioGroup;

		protected override void VStart()
		{
		}

		protected override void Init()
		{
			myAudioGroup = new AudioGroupEndGame();
			scriptAttached = true;
			Service.Get<EventDataService>().OnUIEvent += OnUIEvent;
			Color color = AllText.color;
			Color color2 = FriendsText.color;
			allTabView = new TabView(AllToggle, AllText, color, color2);
			friendsTabView = new TabView(FriendsToggle, FriendsText, color, color2);
			leaderboard = Service.Get<LeaderboardManager>();
			allTabView.toggle.onValueChanged.AddListener(OnAllToggled);
			friendsTabView.toggle.onValueChanged.AddListener(OnFriendsToggled);
			allScores = null;
			friendScores = null;
			scoreListScrollRect = ScoreListLayout.GetComponentInParent<ScrollRect>();
			window = scoreListScrollRect.GetComponentInParent<RectTransform>();
			content = ScoreListLayout.GetComponent<RectTransform>();
			if (!Service.Get<PlayerDataService>().IsPlayerLoggedIn())
			{
				friendsTabView.toggle.gameObject.SetActive(value: false);
			}
			UpdateScoreForRun();
			OnAllToggled(isActive: true);
			CountdownText.text = string.Empty;
			HardwareBackButtonDispatcher.SetTargetClickHandler(QuitButton);
		}

		private void OnDestroy()
		{
			if (myAudioGroup != null)
			{
				myAudioGroup.StopAll();
			}
			Service.Get<EventDataService>().OnUIEvent -= OnUIEvent;
			scriptAttached = false;
			if (allTabView != null)
			{
				allTabView.toggle.onValueChanged.RemoveListener(OnAllToggled);
			}
			if (friendsTabView != null)
			{
				friendsTabView.toggle.onValueChanged.RemoveListener(OnFriendsToggled);
			}
		}

		private void OnUIEvent(object sender, UIEvent e)
		{
			switch (e.type)
			{
			case UIEvent.uiGameEvent.LoginRequest:
				break;
			case UIEvent.uiGameEvent.LoginSuccess:
				friendsTabView.toggle.gameObject.SetActive(value: true);
				ClearList();
				LoadingIcon.SetActive(value: true);
				EnableTabs(enabled: false);
				if (NotLoggedInView != null)
				{
					UnityEngine.Object.Destroy(NotLoggedInView);
				}
				break;
			case UIEvent.uiGameEvent.AccountRetrieved:
				allScores = null;
				activeTabView = null;
				OnAllToggled(isActive: true);
				break;
			}
		}

		public void ExitClicked()
		{
			Service.Get<EventDataService>().SendUIEvent(this, new UIEvent(UIEvent.uiGameEvent.MainMenuRequest));
		}

		public void PlayClicked()
		{
			Service.Get<EventDataService>().SendUIEvent(this, new UIEvent(UIEvent.uiGameEvent.SelectBoosts));
		}

		private void EnableTabs(bool enabled)
		{
			allTabView.toggle.enabled = enabled;
			friendsTabView.toggle.enabled = enabled;
		}

		private void OnAllToggled(bool isActive)
		{
			UnityEngine.Debug.Log("OnAllToggled " + isActive);
			if (isActive && activeTabView != allTabView)
			{
				BIScreen screenFromScene = BILogButtonNavigation.GetScreenFromScene(Service.Get<UIManager>().GetCurrentUIScene());
				Service.Get<IBILogging>().ButtonPressed(BIButton.GLOBAL_LEADERBOARD_TAB, screenFromScene, screenFromScene);
				if (allScores == null)
				{
					LoadingIcon.SetActive(value: true);
					EnableTabs(enabled: false);
					leaderboard.LoadAllHighScores("SledRacer", OnAllScoreLoaded);
				}
				else
				{
					OnAllScoreLoaded(allScores);
				}
				activeTabView = allTabView;
			}
		}

		private void OnAllScoreLoaded(LeaderBoardResponse leaderboard)
		{
			if (scriptAttached)
			{
				allScores = leaderboard;
				ClearList();
				AddHighScoreRows(allScores.Players, isFriendsLeaderboard: false);
				SetCountdownTime(allScores.Countdown);
				LoadingIcon.SetActive(value: false);
				EnableTabs(enabled: true);
			}
		}

		private void OnFriendsToggled(bool isActive)
		{
			UnityEngine.Debug.Log("OnFriendsToggled " + isActive);
			if (isActive && activeTabView != friendsTabView)
			{
				BIScreen screenFromScene = BILogButtonNavigation.GetScreenFromScene(Service.Get<UIManager>().GetCurrentUIScene());
				Service.Get<IBILogging>().ButtonPressed(BIButton.FRIENDS_LEADERBOARD_TAB, screenFromScene, screenFromScene);
				if (friendScores == null)
				{
					LoadingIcon.SetActive(value: true);
					EnableTabs(enabled: false);
					leaderboard.LoadFriendHighScores("SledRacer", OnFriendScoresLoaded);
				}
				else
				{
					OnFriendScoresLoaded(friendScores);
				}
				activeTabView = friendsTabView;
			}
		}

		private void OnFriendScoresLoaded(LeaderBoardResponse leaderboard)
		{
			if (scriptAttached)
			{
				friendScores = leaderboard;
				ClearList();
				AddHighScoreRows(friendScores.Players, isFriendsLeaderboard: true);
				SetCountdownTime(allScores.Countdown);
				LoadingIcon.SetActive(value: false);
				EnableTabs(enabled: true);
			}
		}

		private void ClearList()
		{
			List<GameObject> list = new List<GameObject>();
			foreach (Transform item in ScoreListLayout.transform)
			{
				list.Add(item.gameObject);
			}
			list.ForEach(delegate(GameObject child)
			{
				UnityEngine.Object.Destroy(child);
			});
		}

		private void UpdateScoreForRun()
		{
			PlayerData playerData = Service.Get<PlayerDataService>().PlayerData;
			int? num = playerData.HighScore.Score;
			if (num.HasValue && playerData.LastScore > num.Value)
			{
				num = playerData.LastScore;
			}
			if (HighScoreText != null)
			{
				HighScoreText.text = num.ToString();
			}
			if (ScoreText != null)
			{
				ScoreText.text = playerData.LastScore.ToString();
			}
			if (CoinsText != null)
			{
				CoinsText.text = playerData.LastGameCoinsEarned.ToString();
			}
			if (TotalCoinsText != null)
			{
				TotalCoinsText.text = playerData.TotalCoins.ToString();
			}
		}

		private void SetCountdownTime(int secondsLeft)
		{
			int num = secondsLeft / 86400;
			if (num > 1)
			{
				CountdownText.text = Localizer.Instance.GetTokenTranslation("EndGameMenuCanvas.EndGameContentPanel.MiddlePanel.GameObject.ScoreBoardTitles.DaysLeftText").Replace("%0%", num.ToString());
				return;
			}
			int num2 = secondsLeft / 3600;
			if (num2 > 1)
			{
				CountdownText.text = Localizer.Instance.GetTokenTranslation("EndGameMenuCanvas.EndGameContentPanel.MiddlePanel.GameObject.ScoreBoardTitles.HoursLeftText").Replace("%0%", num2.ToString());
			}
			else
			{
				CountdownText.text = Localizer.Instance.GetTokenTranslation("EndGameMenuCanvas.EndGameContentPanel.MiddlePanel.GameObject.ScoreBoardTitles.EndingSoonText");
			}
		}

		private void AddHighScoreRows(List<LeaderBoardHighScore> scores, bool isFriendsLeaderboard)
		{
			GameObject gameObject = null;
			PlayerDataService playerDataService = Service.Get<PlayerDataService>();
			PlayerData playerData = playerDataService.PlayerData;
			bool flag = false;
			ScoreListLayout.GetComponent<ContentSizeFitter>().enabled = true;
			if (scores.Count == 0)
			{
				GameObject gameObject2 = Object.Instantiate(NotInternetPrefab) as GameObject;
				if (gameObject2 != null)
				{
					gameObject2.transform.SetParent(ScoreListLayout.transform);
					gameObject2.transform.localScale = Vector3.one;
					flag = true;
				}
			}
			if (isFriendsLeaderboard && scores.Count <= 1 && !flag)
			{
				GameObject gameObject4 = Object.Instantiate(NoFriendsPrefab) as GameObject;
				if (gameObject4 != null)
				{
					gameObject4.transform.SetParent(ScoreListLayout.transform);
					gameObject4.transform.localScale = Vector3.one;
				}
			}
			else
			{
				if (!playerData.hasTrophy && isFriendsLeaderboard && !flag)
				{
					GameObject gameObject5 = Object.Instantiate(GoldenAwardPrefab) as GameObject;
					if (gameObject5 != null)
					{
						gameObject5.transform.SetParent(ScoreListLayout.transform);
						gameObject5.transform.localScale = Vector3.one;
					}
				}
				highlightedRowIndex = 0;
				currentRows = new GameObject[scores.Count];
				for (int i = 0; i < scores.Count; i++)
				{
					LeaderBoardHighScore leaderBoardHighScore = scores[i];
					GameObject gameObject6 = AddHighScoreRow(leaderBoardHighScore);
					currentRows[i] = gameObject6;
					if (playerData.Account.PlayerSwid == leaderBoardHighScore.PlayerSWID)
					{
						gameObject = gameObject6;
						highlightedRowIndex = i;
					}
				}
			}
			if (gameObject != null)
			{
				HighlightRow(gameObject);
			}
			else
			{
				StartCoroutine(ScrollToTop());
			}
			StartCoroutine(disableContentSizeFilter());
		}

		private IEnumerator disableContentSizeFilter()
		{
			yield return null;
			ScoreListLayout.GetComponent<ContentSizeFitter>().enabled = false;
		}

		private GameObject AddHighScoreRow(LeaderBoardHighScore score)
		{
			GameObject gameObject = Object.Instantiate(HighScoreRowPrefab) as GameObject;
			if (gameObject != null)
			{
				gameObject.transform.SetParent(ScoreListLayout.transform);
				HighScoreRow component = gameObject.GetComponent<HighScoreRow>();
				component.RankText.text = score.Rank.ToString();
				component.PenguinName.text = score.Name;
				component.ScoreText.text = score.Score.ToString();
				component.PlayerIcon.sprite = AvatarUtil.GetSmallAvatar(score.Colour);
				component.FriendIcon.SetActive(score.IsFriend.HasValue && score.IsFriend.Value);
				component.transform.localScale = Vector3.one;
				component.TrophyIcon.SetActive(score.HasRewardItem.HasValue && score.HasRewardItem.Value);
			}
			return gameObject;
		}

		private void HighlightRow(GameObject rowObject)
		{
			StartCoroutine(CentreVericallyInParent());
			if (rowObject != null)
			{
				HighScoreRow component = rowObject.GetComponent<HighScoreRow>();
				component.CurrentPlayerRow.SetActive(value: true);
				component.OtherPlayerRow.SetActive(value: false);
				component.FriendIcon.SetActive(value: false);
			}
		}

		private IEnumerator ScrollToTop()
		{
			scoreListScrollRect.verticalNormalizedPosition = 1f;
			yield return null;
			scoreListScrollRect.verticalNormalizedPosition = 1f;
		}

		private IEnumerator CentreVericallyInParent()
		{
			yield return null;
			RectTransform target = currentRows[highlightedRowIndex].GetComponent<RectTransform>();
			float contentSize2 = content.rect.height;
			float windowSize = window.rect.height;
			float rowSize = target.rect.height + ScoreListLayout.GetComponent<VerticalLayoutGroup>().spacing;
			float visibleRows = windowSize / rowSize;
			float floatIndex = highlightedRowIndex;
			if (floatIndex < visibleRows / 2f)
			{
				scoreListScrollRect.verticalNormalizedPosition = 1f;
				yield break;
			}
			if (floatIndex > (float)(currentRows.Length - 1) - visibleRows / 2f)
			{
				scoreListScrollRect.verticalNormalizedPosition = 0f;
				yield break;
			}
			contentSize2 -= windowSize;
			float indexOffset = visibleRows / 2f;
			float finalIndex2 = floatIndex - indexOffset;
			finalIndex2 += 0.5f;
			float position = rowSize * finalIndex2 / contentSize2;
			scoreListScrollRect.verticalNormalizedPosition = 1f - position;
		}
	}
}
