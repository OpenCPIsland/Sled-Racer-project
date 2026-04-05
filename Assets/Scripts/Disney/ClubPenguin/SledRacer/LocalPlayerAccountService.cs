using Disney.ClubPenguin.CPModuleUtils;
using Disney.ClubPenguin.Login;
using Disney.ClubPenguin.Service.MWS;
using Disney.ClubPenguin.Service.MWS.Domain;
using Disney.HTTP.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Disney.ClubPenguin.SledRacer
{
	public sealed class LocalAuthTokenResponse : IGetAuthTokenResponse
	{
		public AuthData AuthData
		{
			get;
			private set;
		}

		public ResponseError ResponseError => null;

		public int StatusCode => 200;

		public string StatusMessage => "OK";

		public string Text => string.Empty;

		public byte[] Bytes => null;

		public IHTTPHeaders Headers => null;

		public bool IsError => false;

		public LocalAuthTokenResponse(SavedPlayerData savedPlayer)
		{
			AuthData = new AuthData();
			AuthData.PlayerId = savedPlayer.PlayerId;
			AuthData.PlayerSwid = savedPlayer.Swid;
			AuthData.Username = savedPlayer.UserName;
			AuthData.DisplayName = savedPlayer.DisplayName;
			AuthData.AuthToken = "LOCAL-" + savedPlayer.Swid;
			AuthData.LastLogin = DateTime.UtcNow.ToString("o");
			AuthData.Member = false;
			AuthData.PendingActivation = false;
			AuthData.SaveMode = true;
			AuthData.AccountType = LocalPlayerAccountService.LocalAccountType;
			AuthData.DaysLeft = null;
		}
	}

	public static class LocalPlayerAccountService
	{
		public const string LocalAccountType = "LOCAL";

		private const string LocalSwidPrefix = "{local:";

		private static bool pendingGoldenHelmetUnlock;

		public static bool IsLocalAuthResponse(IGetAuthTokenResponse response)
		{
			return response != null && response.AuthData != null && response.AuthData.AccountType == LocalAccountType;
		}

		public static bool IsLocalAccount(Account account)
		{
			return account != null && !string.IsNullOrEmpty(account.PlayerSwid) && account.PlayerSwid.StartsWith(LocalSwidPrefix, StringComparison.Ordinal);
		}

		public static bool IsSupportedSavedPlayer(SavedPlayerData savedPlayer)
		{
			return savedPlayer != null && savedPlayer.IsLocalAccount && !string.IsNullOrEmpty(savedPlayer.UserName) && !string.IsNullOrEmpty(savedPlayer.Swid) && !string.IsNullOrEmpty(savedPlayer.StoredPassword);
		}

		public static void NormalizeCollection(SavedPlayerCollection collection)
		{
			if (collection == null)
			{
				return;
			}
			if (collection.SavedPlayers == null)
			{
				collection.SavedPlayers = new List<SavedPlayerData>();
				return;
			}
			List<SavedPlayerData> list = new List<SavedPlayerData>();
			foreach (SavedPlayerData savedPlayer in collection.SavedPlayers)
			{
				if (savedPlayer == null)
				{
					continue;
				}
				if (!savedPlayer.IsLocalAccount && !string.IsNullOrEmpty(savedPlayer.Swid) && savedPlayer.Swid.StartsWith(LocalSwidPrefix, StringComparison.Ordinal))
				{
					savedPlayer.IsLocalAccount = true;
				}
				if (!savedPlayer.IsLocalAccount)
				{
					continue;
				}
				if (string.IsNullOrEmpty(savedPlayer.DisplayName))
				{
					savedPlayer.DisplayName = savedPlayer.UserName;
				}
				if (savedPlayer.PlayerId <= 0)
				{
					savedPlayer.PlayerId = ParsePlayerId(savedPlayer.Swid);
				}
				if (string.IsNullOrEmpty(savedPlayer.StoredPassword))
				{
					continue;
				}
				if (savedPlayer.Password == null)
				{
					savedPlayer.Password = string.Empty;
				}
				if (savedPlayer.HighScore < 0)
				{
					savedPlayer.HighScore = 0;
				}
				list.Add(savedPlayer);
			}
			collection.SavedPlayers = list;
		}

		public static SavedPlayerCollection LoadCollection()
		{
			SavedPlayerCollection savedPlayerCollection = new SavedPlayerCollection();
			if (savedPlayerCollection.ExistsOnDisk())
			{
				try
				{
					savedPlayerCollection.LoadFromDisk();
				}
				catch (Exception ex)
				{
					Debug.LogWarning("[LocalPlayerAccountService] Failed to load local players: " + ex.Message);
				}
			}
			NormalizeCollection(savedPlayerCollection);
			return savedPlayerCollection;
		}

		public static void SaveCollection(SavedPlayerCollection collection)
		{
			NormalizeCollection(collection);
			collection.SaveToDisk();
		}

		public static bool TryCreateAccount(string username, string password, int penguinColor, out SavedPlayerData createdPlayer, out string errorCode)
		{
			createdPlayer = null;
			errorCode = string.Empty;
			string normalizedUsername = NormalizeUserName(username);
			SavedPlayerCollection collection = LoadCollection();
			if (collection.SavedPlayers.Count >= 6)
			{
				errorCode = "-32297";
				return false;
			}
			if (!HasValidUserNameCharacters(normalizedUsername))
			{
				errorCode = "-32283";
				return false;
			}
			if (collection.SavedPlayers.Any((SavedPlayerData player) => string.Equals(player.UserName, normalizedUsername, StringComparison.OrdinalIgnoreCase)))
			{
				errorCode = "409";
				return false;
			}
			long nextPlayerId = GetNextPlayerId(collection);
			createdPlayer = new SavedPlayerData();
			createdPlayer.UserName = normalizedUsername;
			createdPlayer.DisplayName = normalizedUsername;
			createdPlayer.Swid = CreateSwid(nextPlayerId);
			createdPlayer.PlayerId = nextPlayerId;
			createdPlayer.PenguinColor = penguinColor;
			createdPlayer.HighScore = 0;
			createdPlayer.IsLocalAccount = true;
			createdPlayer.HasGoldenHelmet = false;
			createdPlayer.StoredPassword = password;
			createdPlayer.Password = string.Empty;
			collection.UpdateSavedPlayer(createdPlayer);
			SaveCollection(collection);
			return true;
		}

		public static bool TryLogin(string username, string password, bool savePassword, out SavedPlayerData authenticatedPlayer)
		{
			authenticatedPlayer = null;
			string normalizedUsername = NormalizeUserName(username);
			SavedPlayerCollection collection = LoadCollection();
			SavedPlayerData savedPlayerData = collection.SavedPlayers.Find((SavedPlayerData player) => string.Equals(player.UserName, normalizedUsername, StringComparison.OrdinalIgnoreCase));
			if (savedPlayerData == null || !string.Equals(savedPlayerData.StoredPassword, password, StringComparison.Ordinal))
			{
				return false;
			}
			savedPlayerData.Password = ((!savePassword) ? string.Empty : password);
			collection.UpdateSavedPlayer(savedPlayerData);
			SaveCollection(collection);
			authenticatedPlayer = savedPlayerData;
			return true;
		}

		public static SavedPlayerData GetPlayer(string swid)
		{
			if (string.IsNullOrEmpty(swid))
			{
				return null;
			}
			SavedPlayerCollection collection = LoadCollection();
			return collection.SavedPlayers.Find((SavedPlayerData player) => player.Swid == swid);
		}

		public static Account BuildAccount(SavedPlayerData savedPlayer)
		{
			if (savedPlayer == null)
			{
				return null;
			}
			Account account = new Account();
			account.AccountType = LocalAccountType;
			account.Colour = savedPlayer.PenguinColor;
			account.Username = savedPlayer.UserName;
			account.PlayerId = savedPlayer.PlayerId;
			account.PlayerSwid = savedPlayer.Swid;
			account.Member = false;
			account.PendingActivation = false;
			account.SafeMode = true;
			account.Email = string.Empty;
			return account;
		}

		public static IGetAuthTokenResponse CreateAuthResponse(SavedPlayerData savedPlayer)
		{
			return new LocalAuthTokenResponse(savedPlayer);
		}

		public static LeaderBoardRewardStatus GetRewardStatus(SavedPlayerData savedPlayer)
		{
			LeaderBoardRewardStatus leaderBoardRewardStatus = new LeaderBoardRewardStatus();
			leaderBoardRewardStatus.Status = ((savedPlayer != null && savedPlayer.HasGoldenHelmet) ? LeaderBoardRewardStatus.RewardStatus.NOT_THE_LEADER_REWARD_OWNED : LeaderBoardRewardStatus.RewardStatus.NOT_THE_LEADER);
			leaderBoardRewardStatus.SecondsTillNextCheck = 0;
			return leaderBoardRewardStatus;
		}

		public static List<SavedPlayerData> GetRankedPlayers()
		{
			SavedPlayerCollection collection = LoadCollection();
			return collection.SavedPlayers.OrderByDescending((SavedPlayerData player) => player.HighScore).ThenBy((SavedPlayerData player) => player.UserName).ToList();
		}

		public static bool SetCurrentPlayerHighScore(int score)
		{
			Account currentAccount = GetCurrentAccount();
			if (!IsLocalAccount(currentAccount))
			{
				return false;
			}
			SavedPlayerCollection collection = LoadCollection();
			SavedPlayerData savedPlayerData = collection.SavedPlayers.Find((SavedPlayerData player) => player.Swid == currentAccount.PlayerSwid);
			if (savedPlayerData == null)
			{
				return false;
			}
			savedPlayerData.HighScore = Mathf.Max(score, 0);
			bool flag = false;
			if (savedPlayerData.HighScore > 0 && IsTopRankedPlayer(savedPlayerData, collection) && !savedPlayerData.HasGoldenHelmet)
			{
				savedPlayerData.HasGoldenHelmet = true;
				pendingGoldenHelmetUnlock = true;
				flag = true;
			}
			collection.UpdateSavedPlayer(savedPlayerData);
			SaveCollection(collection);
			TryApplyRuntimeRewardState(savedPlayerData, flag);
			return flag;
		}

		public static bool ConsumePendingGoldenHelmetUnlock()
		{
			bool flag = pendingGoldenHelmetUnlock;
			pendingGoldenHelmetUnlock = false;
			return flag;
		}

		private static void TryApplyRuntimeRewardState(SavedPlayerData savedPlayer, bool newlyUnlocked)
		{
			try
			{
				PlayerDataService playerDataService = Service.Get<PlayerDataService>();
				if (playerDataService == null || !IsLocalAccount(playerDataService.PlayerData.Account) || playerDataService.PlayerData.Account.PlayerSwid != savedPlayer.Swid)
				{
					return;
				}
				playerDataService.PlayerData.hasTrophy = savedPlayer.HasGoldenHelmet;
				playerDataService.PlayerData.RewardStatus = ((!newlyUnlocked) ? (savedPlayer.HasGoldenHelmet ? LeaderBoardRewardStatus.RewardStatus.NOT_THE_LEADER_REWARD_OWNED : LeaderBoardRewardStatus.RewardStatus.NOT_THE_LEADER) : LeaderBoardRewardStatus.RewardStatus.LEADER_REWARD_GRANTED);
			}
			catch
			{
			}
		}

		private static Account GetCurrentAccount()
		{
			try
			{
				PlayerDataService playerDataService = Service.Get<PlayerDataService>();
				if (playerDataService != null && playerDataService.PlayerData != null)
				{
					return playerDataService.PlayerData.Account;
				}
			}
			catch
			{
			}
			return null;
		}

		private static bool IsTopRankedPlayer(SavedPlayerData candidate, SavedPlayerCollection collection)
		{
			SavedPlayerData savedPlayerData = collection.SavedPlayers.OrderByDescending((SavedPlayerData player) => player.HighScore).ThenBy((SavedPlayerData player) => player.UserName).FirstOrDefault();
			return savedPlayerData != null && savedPlayerData.Swid == candidate.Swid;
		}

		private static long GetNextPlayerId(SavedPlayerCollection collection)
		{
			if (collection.SavedPlayers.Count == 0)
			{
				return 1L;
			}
			return collection.SavedPlayers.Max((SavedPlayerData player) => player.PlayerId) + 1;
		}

		private static long ParsePlayerId(string swid)
		{
			if (string.IsNullOrEmpty(swid) || !swid.StartsWith(LocalSwidPrefix, StringComparison.Ordinal) || !swid.EndsWith("}", StringComparison.Ordinal))
			{
				return 0L;
			}
			string s = swid.Substring(LocalSwidPrefix.Length, swid.Length - LocalSwidPrefix.Length - 1);
			long result = 0L;
			long.TryParse(s, out result);
			return result;
		}

		private static string CreateSwid(long playerId)
		{
			return LocalSwidPrefix + playerId + "}";
		}

		private static bool HasValidUserNameCharacters(string username)
		{
			for (int i = 0; i < username.Length; i++)
			{
				char c = username[i];
				if (!char.IsLetterOrDigit(c) && c != ' ')
				{
					return false;
				}
			}
			return true;
		}

		private static string NormalizeUserName(string username)
		{
			return InputFieldStringUtils.ToTitleCase((username ?? string.Empty).Trim());
		}
	}
}
