using Disney.ClubPenguin.Service.MWS.Domain;
using System;
using UnityEngine;

namespace Disney.ClubPenguin.SledRacer
{
	public sealed class PlayerDataService : IDataService
	{
		private const string REWARD_CHECK_KEY = "nextRewardCheckTime:";

		private const long NEVER_CHECK_REWARD = -1L;

		public PlayerData PlayerData
		{
			get;
			private set;
		}

		public Account LoadedAccount
		{
			get;
			set;
		}

		public bool LoadingAccount
		{
			get;
			set;
		}

		public LeaderBoardRewardStatus LoadedRewardStatus
		{
			get;
			set;
		}

		public bool LoadingRewardStatus
		{
			get;
			set;
		}

		public bool AllowAutoLogin
		{
			get;
			set;
		}

		public event Action OnDataUpdate;

		public PlayerDataService()
		{
			UnityEngine.Debug.Log("[PlayerDataService] constructor");
			LoadingAccount = false;
			LoadingRewardStatus = false;
			AllowAutoLogin = SessionStatePrefs.ShouldAttemptAutoLogin();
			SwitchToOfflineData(persistSession: false);
		}

		private void DispatchUpdateEvent()
		{
			UnityEngine.Debug.Log("[PlayerDataService] DISPATCH update");
			if (this.OnDataUpdate != null)
			{
				this.OnDataUpdate();
			}
		}

		public void SwitchToOfflineData(bool persistSession = true)
		{
			PlayerData = new OfflinePlayerData();
			if (persistSession)
			{
				SessionStatePrefs.SaveGuestSession();
			}
		}

		public void setPlayerAccount(Account _acc, LeaderBoardRewardStatus rewardStatus)
		{
			UnityEngine.Debug.Log("[PlayerDataService] setPlayerAccount()");
			playerDataOnline();
			PlayerData.Account = _acc;
			SessionStatePrefs.SaveLoggedInSession(_acc);
			PlayerData.RewardStatus = rewardStatus.Status;
			PlayerData.hasTrophy = false;
			if (rewardStatus.Status == LeaderBoardRewardStatus.RewardStatus.LEADER_REWARD_OWNED || rewardStatus.Status == LeaderBoardRewardStatus.RewardStatus.LEADER_REWARD_GRANTED || rewardStatus.Status == LeaderBoardRewardStatus.RewardStatus.NOT_THE_LEADER_REWARD_OWNED)
			{
				PlayerData.hasTrophy = true;
				setNextRewardCheckTime(-1L);
			}
			else if (rewardStatus.SecondsTillNextCheck > 0)
			{
				setNextRewardCheckTime(DateTime.UtcNow.AddSeconds(rewardStatus.SecondsTillNextCheck).ToFileTimeUtc());
			}
			else if (rewardStatus.Status == LeaderBoardRewardStatus.RewardStatus.NOT_THE_LEADER)
			{
				clearNeverCheckFlag();
			}
			DispatchUpdateEvent();
		}

		public void setFinalScore(int _score)
		{
			PlayerData.LastScore = _score;
			int? score = PlayerData.HighScore.Score;
			if (score.HasValue && _score > score.Value)
			{
				PlayerData.HighScore.SetScore(_score);
			}
			DispatchUpdateEvent();
		}

		public bool IsPlayerLoggedIn()
		{
			return !(PlayerData is OfflinePlayerData);
		}

		private void playerDataOnline()
		{
			if (PlayerData.GetType() == typeof(OfflinePlayerData))
			{
				UnityEngine.Debug.Log("[PlayerDataService] Setting To Online PlayerData");
				PlayerData = new PlayerData();
			}
		}

		public void Subscribe(Action _method)
		{
			UnSubscribe(_method);
			this.OnDataUpdate = (Action)Delegate.Combine(this.OnDataUpdate, _method);
			_method();
		}

		public void UnSubscribe(Action _method)
		{
			this.OnDataUpdate = (Action)Delegate.Remove(this.OnDataUpdate, _method);
		}

		private void setNextRewardCheckTime(long secondsInTheFuture)
		{
			string key = "nextRewardCheckTime:" + PlayerData.Account.PlayerSwid;
			PlayerPrefs.SetString(key, secondsInTheFuture.ToString());
		}

		private void clearNeverCheckFlag()
		{
			string key = "nextRewardCheckTime:" + PlayerData.Account.PlayerSwid;
			if (PlayerPrefs.HasKey(key) && PlayerPrefs.GetString(key) == (-1L).ToString())
			{
				PlayerPrefs.DeleteKey(key);
			}
		}

		public bool RewardStatusCheckRequired(string playerSwid)
		{
			string key = "nextRewardCheckTime:" + playerSwid;
			if (!PlayerPrefs.HasKey(key))
			{
				return true;
			}
			long result = 0L;
			long.TryParse(PlayerPrefs.GetString(key), out result);
			if (result == -1)
			{
				return false;
			}
			DateTime t = DateTime.FromFileTimeUtc(result);
			return t < DateTime.UtcNow;
		}
	}

	internal static class SessionStatePrefs
	{
		private const string LAST_SESSION_MODE_KEY = "sledracer.lastSessionMode";

		private const string LAST_SESSION_ACCOUNT_SWID_KEY = "sledracer.lastSessionAccountSwid";

		private const int SESSION_MODE_GUEST = 0;

		private const int SESSION_MODE_LOGGED_IN = 1;

		public static bool ShouldAttemptAutoLogin()
		{
			if (!PlayerPrefs.HasKey(LAST_SESSION_MODE_KEY))
			{
				return true;
			}
			return PlayerPrefs.GetInt(LAST_SESSION_MODE_KEY, 0) == SESSION_MODE_LOGGED_IN;
		}

		public static void SaveGuestSession()
		{
			PlayerPrefs.SetInt(LAST_SESSION_MODE_KEY, SESSION_MODE_GUEST);
			PlayerPrefs.Save();
		}

		public static void SaveLoggedInSession(Account account)
		{
			if (account == null)
			{
				return;
			}
			PlayerPrefs.SetInt(LAST_SESSION_MODE_KEY, SESSION_MODE_LOGGED_IN);
			if (!string.IsNullOrEmpty(account.PlayerSwid))
			{
				PlayerPrefs.SetString(LAST_SESSION_ACCOUNT_SWID_KEY, account.PlayerSwid);
			}
			PlayerPrefs.Save();
		}

		public static string GetPreferredAutoLoginPlayerSwid()
		{
			if (!ShouldAttemptAutoLogin())
			{
				return string.Empty;
			}
			return PlayerPrefs.GetString(LAST_SESSION_ACCOUNT_SWID_KEY, string.Empty);
		}
	}
}
