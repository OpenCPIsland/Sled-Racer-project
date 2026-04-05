using System;
using System.Collections.Generic;
using DevonLocalization.Core;
using Disney.ClubPenguin.Service.MWS;
using Disney.ClubPenguin.Service.MWS.Domain;
using UnityEngine;

namespace Disney.ClubPenguin.SledRacer
{
    public class LeaderboardManager
    {
        private const string WEEKLY_LEADERBOARD_DURATION = "weekly";

        private const int WEEKLY_LEADERBOARD_HISTORIC_PERIOD = 1;

        private IMWSClient mwsClient;

        private LeaderBoardResponse cachedFriendsResponse;

        public LeaderboardManager()
        {
            mwsClient = Service.Get<IMWSClient>();
        }

        public void startGame(string gameType, Action<Game> callback)
        {
            if (!IsPlayerLoggedIn() || UseLocalLeaderboard())
            {
                callback(null);
                return;
            }

            mwsClient.StartGame(gameType, delegate (IStartGameResponse response)
            {
                callback(response.Game);
            });
        }

        public void saveGame(GameResult gameResult, Action<GameResult> callback)
        {
            if (!IsPlayerLoggedIn() || UseLocalLeaderboard())
            {
                callback(null);
                return;
            }

            mwsClient.SetGameResult(gameResult, delegate (ISetGameResultResponse response)
            {
                handleSaveGameResponse(callback, response.GameResult);
            });
        }

        private void handleSaveGameResponse(Action<GameResult> callback, GameResult result)
        {
            if (result != null)
            {
                long playerId = result.playerId;
                foreach (PlayerResult playerResult in result.playerResults)
                {
                    if (playerResult.PlayerId == playerId)
                    {
                        injectIntoCachedFriendsData(result.playerId, playerResult.Result);
                    }
                }
            }
            callback(result);
        }

        private void injectIntoCachedFriendsData(long playerId, string scoreStr)
        {
            int result = 0;
            int.TryParse(scoreStr, out result);
            if (cachedFriendsResponse == null || result == 0)
            {
                return;
            }
            int rank = 1;
            foreach (LeaderBoardHighScore player in cachedFriendsResponse.Players)
            {
                if (result < player.Score)
                {
                    rank = player.Rank + 1;
                }
                else if (result == player.Score)
                {
                    rank = player.Rank;
                }
                if (player.PlayerId == playerId)
                {
                    if (player.Score < result)
                    {
                        player.Score = result;
                        player.Rank = rank;
                        SortLeaderboard(cachedFriendsResponse, null);
                    }
                    break;
                }
            }
        }

        public void LoadMyAllTimeHighScore(string gameType, Action<int> SetHighScoreCallback)
        {
            if (!IsPlayerLoggedIn() || UseLocalLeaderboard())
            {
                SetHighScoreCallback(HighScore.GetOfflineHighScoreFromPrefs());
                return;
            }

            mwsClient.GetMyHighScore(gameType, delegate (IMyHighScoreResponse score)
            {
                SetHighScoreCallback(score.Score);
            });
        }

        public virtual void LoadAllHighScores(string gameType, Action<LeaderBoardResponse> SetHighScoresCallback)
        {
            if (!IsPlayerLoggedIn() || UseLocalLeaderboard())
            {
                SetHighScoresCallback(BuildOfflineLeaderboard());
                return;
            }

            mwsClient.GetLeaderBoard(gameType, currentLanguage(), "weekly", false, delegate (IGetLeaderBoardResponse response)
            {
                SortLeaderboard(response.LeaderBoard, SetHighScoresCallback);
            });
        }

        public void LoadFriendHighScores(string gameType, Action<LeaderBoardResponse> SetHighScoresCallback)
        {
            if (!IsPlayerLoggedIn() || UseLocalLeaderboard())
            {
                LeaderBoardResponse offline = BuildOfflineLeaderboard();
                cachedFriendsResponse = offline;
                SetHighScoresCallback(offline);
                return;
            }

            mwsClient.GetLeaderBoard(gameType, currentLanguage(), "weekly", true, delegate (IGetLeaderBoardResponse response)
            {
                cachedFriendsResponse = response.LeaderBoard;
                SortLeaderboard(response.LeaderBoard, SetHighScoresCallback);
            });
        }

        public bool LoadCachedFriendHighScores(string gameType, Action<LeaderBoardResponse> SetHighScoresCallback)
        {
            if (cachedFriendsResponse != null)
            {
                SetHighScoresCallback(cachedFriendsResponse);
                return true;
            }
            LoadFriendHighScores(gameType, SetHighScoresCallback);
            return false;
        }

        public bool AreFriendHighScoresCached()
        {
            return cachedFriendsResponse != null;
        }

        public void ClearCachedFriendHighScores()
        {
            Debug.Log("ClearCachedFriendHighScores");
            cachedFriendsResponse = null;
        }

        public void GetRewardStatus(string gameType, Action<LeaderBoardRewardStatus> GetRewardStatusCallback)
        {
            if (!IsPlayerLoggedIn() || UseLocalLeaderboard())
            {
                SavedPlayerData player = LocalPlayerAccountService.GetPlayer(Service.Get<PlayerDataService>().PlayerData.Account.PlayerSwid);
                GetRewardStatusCallback(LocalPlayerAccountService.GetRewardStatus(player));
                return;
            }

            mwsClient.GetLeaderBoardRewardStatus(gameType, "weekly", 1, delegate (IGetLeaderBoardRewardStatus response)
            {
                GetRewardStatusCallback(response.RewardStatus);
            });
        }

        private LeaderBoardResponse BuildOfflineLeaderboard()
        {
            PlayerDataService pds = Service.Get<PlayerDataService>();
            PlayerData playerData = pds.PlayerData;

            if (UseLocalLeaderboard() || !pds.IsPlayerLoggedIn())
            {
                List<LeaderBoardHighScore> list = new List<LeaderBoardHighScore>();
                foreach (SavedPlayerData rankedPlayer in LocalPlayerAccountService.GetRankedPlayers())
                {
                    list.Add(BuildLocalPlayerLeaderboardEntry(rankedPlayer, playerData.Account.PlayerSwid));
                }
                list.Add(BuildGuestLeaderboardEntry(playerData));
                list.Sort((LeaderBoardHighScore x, LeaderBoardHighScore y) => (x.Score == y.Score) ? string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase) : (-1 * x.Score.CompareTo(y.Score)));
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].Rank = i + 1;
                }
                LeaderBoardResponse leaderBoardResponse = new LeaderBoardResponse();
                leaderBoardResponse.Players = list;
                leaderBoardResponse.Countdown = 0;
                return leaderBoardResponse;
            }

            int score = HighScore.GetOfflineHighScoreFromPrefs();

            LeaderBoardHighScore entry = new LeaderBoardHighScore();
            entry.Rank = 1;
            entry.Name = playerData.Account.Username;
            entry.PlayerId = (int)playerData.Account.PlayerId;
            entry.PlayerSWID = playerData.Account.PlayerSwid;
            entry.Score = score;
            entry.Colour = playerData.Account.Colour;
            entry.IsFriend = false;
            entry.HasRewardItem = playerData.hasTrophy;

            LeaderBoardResponse response = new LeaderBoardResponse();
            response.Players = new List<LeaderBoardHighScore> { entry };
            response.Countdown = 0;

            return response;
        }

        private static LeaderBoardHighScore BuildLocalPlayerLeaderboardEntry(SavedPlayerData rankedPlayer, string activePlayerSwid)
        {
            LeaderBoardHighScore leaderBoardHighScore = new LeaderBoardHighScore();
            leaderBoardHighScore.Name = rankedPlayer.UserName;
            leaderBoardHighScore.PlayerId = (int)rankedPlayer.PlayerId;
            leaderBoardHighScore.PlayerSWID = rankedPlayer.Swid;
            leaderBoardHighScore.Score = rankedPlayer.HighScore;
            leaderBoardHighScore.Colour = rankedPlayer.PenguinColor;
            leaderBoardHighScore.IsFriend = (rankedPlayer.Swid != activePlayerSwid);
            leaderBoardHighScore.HasRewardItem = rankedPlayer.HasGoldenHelmet;
            return leaderBoardHighScore;
        }

        private static LeaderBoardHighScore BuildGuestLeaderboardEntry(PlayerData playerData)
        {
            bool flag = playerData != null && playerData.Account != null && playerData.Account.PlayerSwid == OfflinePlayerData.OFFLINE_PLAYER_SWID;
            LeaderBoardHighScore leaderBoardHighScore = new LeaderBoardHighScore();
            leaderBoardHighScore.Name = flag ? playerData.Account.Username : Localizer.Instance.GetTokenTranslation("guest.player.username");
            leaderBoardHighScore.PlayerId = flag ? (int)playerData.Account.PlayerId : -1;
            leaderBoardHighScore.PlayerSWID = OfflinePlayerData.OFFLINE_PLAYER_SWID;
            leaderBoardHighScore.Score = HighScore.GetGuestHighScoreFromPrefs();
            leaderBoardHighScore.Colour = OfflinePlayerData.OFFLINE_PLAYER_COLOR_INDEX;
            leaderBoardHighScore.IsFriend = !flag;
            leaderBoardHighScore.HasRewardItem = flag && playerData.hasTrophy;
            return leaderBoardHighScore;
        }

        private bool IsPlayerLoggedIn()
        {
            try
            {
                return Service.Get<PlayerDataService>().IsPlayerLoggedIn();
            }
            catch
            {
                return false;
            }
        }

        private void SortLeaderboard(LeaderBoardResponse leaderboard, Action<LeaderBoardResponse> SetHighScoresCallback)
        {
            leaderboard.Players.Sort((LeaderBoardHighScore x, LeaderBoardHighScore y) => (x.Rank == y.Rank) ? (-1 * x.Score.CompareTo(y.Score)) : x.Rank.CompareTo(y.Rank));
            if (SetHighScoresCallback != null)
            {
                SetHighScoresCallback(leaderboard);
            }
        }

        private string currentLanguage()
        {
            return LocalizationLanguage.GetLanguageString(Localizer.Instance.Language);
        }

        private bool UseLocalLeaderboard()
        {
            try
            {
                return LocalPlayerAccountService.IsLocalAccount(Service.Get<PlayerDataService>().PlayerData.Account);
            }
            catch
            {
                return false;
            }
        }
    }
}
