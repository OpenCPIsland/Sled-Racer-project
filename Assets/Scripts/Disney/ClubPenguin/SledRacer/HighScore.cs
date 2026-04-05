using System;
using System.Collections.Generic;
using Disney.ClubPenguin.Service.MWS.Domain;
using UnityEngine;

namespace Disney.ClubPenguin.SledRacer
{
	public class HighScore
	{
		private static string OFFLINE_HIGH_SCORE_PREF_KEY = "OfflineHighScore";

		private IList<Action<int?>> boundToScore = new List<Action<int?>>();

		public int? Score
		{
			get;
			private set;
		}

		public static int GetOfflineHighScoreFromPrefs()
		{
			string offlineHighScorePrefKey = GetOfflineHighScorePrefKey();
			if (PlayerPrefs.HasKey(offlineHighScorePrefKey))
			{
				return PlayerPrefs.GetInt(offlineHighScorePrefKey, 0);
			}
			try
			{
				Account account = Service.Get<PlayerDataService>().PlayerData.Account;
				if (account != null && LocalPlayerAccountService.IsLocalAccount(account))
				{
					SavedPlayerData player = LocalPlayerAccountService.GetPlayer(account.PlayerSwid);
					if (player != null)
					{
						return player.HighScore;
					}
				}
			}
			catch
			{
			}
			return 0;
		}

		public static void SaveOfflineHighScoreInPrefs(int score)
		{
			PlayerPrefs.SetInt(GetOfflineHighScorePrefKey(), score);
			PlayerPrefs.Save();
			LocalPlayerAccountService.SetCurrentPlayerHighScore(score);
		}

		public void SetScore(int score)
		{
			if (Score != score)
			{
				Score = score;
				foreach (Action<int?> item in boundToScore)
				{
					item(Score);
				}
			}
		}

		public void BindToScore(Action<int?> handleScoreChange)
		{
			boundToScore.Add(handleScoreChange);
			handleScoreChange(Score);
		}

		public void UnBindFromScore(Action<int?> handleScoreChange)
		{
			boundToScore.Remove(handleScoreChange);
		}

		private static string GetOfflineHighScorePrefKey()
		{
			try
			{
				Account account = Service.Get<PlayerDataService>().PlayerData.Account;
				if (account != null && LocalPlayerAccountService.IsLocalAccount(account))
				{
					return OFFLINE_HIGH_SCORE_PREF_KEY + ":" + account.PlayerSwid;
				}
			}
			catch
			{
			}
			return OFFLINE_HIGH_SCORE_PREF_KEY;
		}
	}
}
