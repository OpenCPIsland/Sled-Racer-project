using DevonLocalization.Core;
using UnityEngine;

namespace Disney.ClubPenguin.SledRacer
{
	public class OfflinePlayerData : PlayerData
	{
		private const string OFFLINE_PLAYER_USERNAME_TOKEN = "guest.player.username";

		public const string OFFLINE_PLAYER_SWID = "{-1}";

		public const int OFFLINE_PLAYER_COLOR_INDEX = 15;

		public OfflinePlayerData()
		{
			base.Account.Colour = 15;
			base.Account.Member = true;
			base.Account.Username = Localizer.Instance.GetTokenTranslation("guest.player.username");
			base.Account.PlayerSwid = "{-1}";
			base.Account.PlayerId = -1L;
			base.HighScore.SetScore(HighScore.GetOfflineHighScoreFromPrefs());
			UnityEngine.Debug.Log("[OfflinePlayerData] Loaded local score " + base.HighScore.Score);
		}
	}
}
