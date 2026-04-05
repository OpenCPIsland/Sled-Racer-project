using Disney.ClubPenguin.Service.MWS;
using Disney.DMOAnalytics;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ClubPenguin.Login.BI
{
	public class LoginBIUtils : ILoginBIUtils
	{
		private const string EMPTY_PLAYERID = "NULL";

		private const string CONTEXT = "context";

		private const string ACTION = "action";

		private const string LOCATION = "location";

		private const string CREATED = "created";

		private const string PENGUIN = "penguin";

		private const string PLAYERID = "player_id";

		private const string PLAYERNAME = "player_name";

		private const string MESSAGE = "message";

		private const string SOFTCURRENCY = "soft_currency";

		private const string COINS = "coins:";

		private const string AGEGATE = "age_gate";

		private const string TYPE = "type";

		private const string PLAYERINFO = "player_info";

		private const string PAGEVIEW = "page_view";

		private long playerID;

		private string playerName;

		private static string[] PAGENAMES = new string[6]
		{
			"choose_penguin",
			"login_success",
			"login_failure",
			"create_penguin",
			"login_start_new",
			"login_start_return"
		};

		public string ContextName
		{
			get;
			set;
		}

		public static string GetPageNameString(BiPageNames pageName)
		{
			return PAGENAMES[(int)pageName];
		}

		public void SendPlayerInfo(long playerID, string playerName)
		{
			if (MWSClient.Instance.AuthToken == null)
			{
				UnityEngine.Debug.LogWarning("Skipping MWS player card lookup because no auth token is available. Logging BI player info without wallet data.");
				this.playerID = playerID;
				this.playerName = playerName;
				LogPlayerInfo(0L);
				return;
			}
			this.playerID = playerID;
			this.playerName = playerName;
			MWSClient.Instance.GetPlayerCardData(OnPlayerCardDataReceived);
		}

		private void OnPlayerCardDataReceived(IGetPlayerCardDataResponse response)
		{
			if (response.IsError)
			{
				UnityEngine.Debug.LogError("Unable to fetch player card data.");
				return;
			}
			LogPlayerInfo(response.PlayerCardData.Coins);
		}

		private void LogPlayerInfo(long coins)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("player_id", playerID);
			dictionary.Add("player_name", playerName);
			dictionary.Add("soft_currency", "{coins:" + coins.ToString() + "}");
			if (ContextName != null)
			{
				dictionary.Add("context", ContextName);
			}
			Disney.DMOAnalytics.DMOAnalytics.SharedAnalytics.LogAnalyticsEventWithContext("player_info", dictionary);
		}

		public void SendAccoundCreated(long playerID, string playerName)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("context", "penguin");
			dictionary.Add("action", "created");
			dictionary.Add("player_id", playerID);
			dictionary.Add("message", playerName);
			Disney.DMOAnalytics.DMOAnalytics.SharedAnalytics.LogGameAction(dictionary);
		}

		public void SendPageviewOnLogin(BiPageNames pageName, long passedPlayerID = 0)
		{
			string pageNameString = GetPageNameString(pageName);
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			if (passedPlayerID == 0L)
			{
				dictionary.Add("player_id", "NULL");
			}
			else
			{
				dictionary.Add("player_id", passedPlayerID);
			}
			dictionary.Add("location", pageNameString);
			if (ContextName != null)
			{
				dictionary.Add("context", ContextName);
			}
			Disney.DMOAnalytics.DMOAnalytics.SharedAnalytics.LogAnalyticsEventWithContext("page_view", dictionary);
		}

		public void SendAgeEnteredGameAction(int age, long passedPlayerID = 0)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("context", "age_gate");
			dictionary.Add("action", "age_gate");
			dictionary.Add("type", age.ToString());
			if (passedPlayerID == 0L)
			{
				dictionary.Add("player_id", "NULL");
			}
			else
			{
				dictionary.Add("player_id", playerID);
			}
			Disney.DMOAnalytics.DMOAnalytics.SharedAnalytics.LogGameAction(dictionary);
		}
	}
}
