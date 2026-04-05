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
			AllowAutoLogin = true;
			SwitchToOfflineData();
		}

		private void DispatchUpdateEvent()
		{
			UnityEngine.Debug.Log("[PlayerDataService] DISPATCH update");
			if (this.OnDataUpdate != null)
			{
				this.OnDataUpdate();
			}
		}

		public void SwitchToOfflineData()
		{
			PlayerData = new OfflinePlayerData();
		}

		public void setPlayerAccount(Account _acc, LeaderBoardRewardStatus rewardStatus)
		{
			UnityEngine.Debug.Log("[PlayerDataService] setPlayerAccount()");
			playerDataOnline();
			PlayerData.Account = _acc;
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
}
