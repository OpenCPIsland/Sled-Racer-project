using Disney.ClubPenguin.Service.MWS.Domain;
using Disney.ObjectPool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ClubPenguin.SledRacer
{
	public class TrackManager : MonoBehaviour
	{
		public delegate void OnTracksReadyEventHandler();

		public Transform LeadPlayer;

		public Transform LastPlayer;

		public ObjectPoolMulti Pool;

		public GameObject activeTrackContainer;

		private POTrackSegment currTrackSegment;

		private Queue<POTrackSegment> ActiveTracks;

		private Queue<GameObject> TrackContainers;

		private Stack<LeaderBoardHighScore> friendScores;

		private float storedTravel;

		private Vector3 vDistanceDrawn = Vector3.zero;

		private Vector3 vRealDistanceDrawn = Vector3.zero;

		private Vector3 vLastEndpointPassed = Vector3.zero;

		private bool TracksReady;

		private bool tracksBeingChanged;

		private bool recordMarkerAdded;

		private bool enableMarkers = true;

		private ConfigController config;

		private bool TutorialMode => SledRacerGameManager.Instance.CurrentGameState == SledRacerGameManager.GameState.GameTutorial;

		public float DistanceTravelled
		{
			get
			{
				float result;
				if (activeTrackContainer == null)
				{
					result = 0f;
				}
				else
				{
					Vector3 position = LeadPlayer.position;
					float z = position.z;
					Vector3 position2 = activeTrackContainer.transform.position;
					result = (z - position2.z + storedTravel) / config.TrackUnitsForProgress;
				}
				return result;
			}
		}

		public event OnTracksReadyEventHandler OnReady;

		protected virtual void TracksAreReady()
		{
			DevTrace("TracksAreReady() - dispatch");
			TracksReady = true;
			if (this.OnReady != null)
			{
				this.OnReady();
			}
			Service.Get<BackgroundCameraMovement>().ResetPostion();
		}

		private void Awake()
		{
			base.enabled = false;
			config = Service.Get<ConfigController>();
		}

		private void Pause()
		{
		}

		private void Update()
		{
			if (TracksReady)
			{
				validateTerrain();
			}
		}

		public void ResetStart()
		{
			DevTrace("ResetStart()");
			base.gameObject.SetActive(value: true);
			TracksReady = false;
			base.enabled = false;
			if (TutorialMode)
			{
				enableMarkers = false;
			}
			else
			{
				enableMarkers = true;
			}
			InitializeGameVariables();
			prepareActiveTrackQueue();
			GetFriendScores();
		}

		public void ResetEnd()
		{
			prepareActiveTrackQueue();
			Pool.Shrink();
			base.gameObject.SetActive(value: false);
		}

		private void InitializeGameVariables()
		{
			DevTrace("InitializeGameVariables()");
			storedTravel = 0f;
			storedTravel -= config.TrackStartPosition.z;
			vDistanceDrawn = Vector3.zero;
			vRealDistanceDrawn = Vector3.zero;
			currTrackSegment = null;
		}

		private void prepareActiveTrackQueue()
		{
			DevTrace("prepareActiveTrackQueue()");
			if (ActiveTracks == null)
			{
				ActiveTracks = new Queue<POTrackSegment>();
			}
			else
			{
				while (ActiveTracks.Count > 0)
				{
					DevTrace("Removed an OLD track segment");
					tracksBeingChanged = true;
					StartCoroutine(removeTrackSegment());
				}
			}
			if (TrackContainers == null)
			{
				TrackContainers = new Queue<GameObject>();
				prepareTrackContainers();
			}
			else if (activeTrackContainer == null && TrackContainers.Count > 0)
			{
				activeTrackContainer = TrackContainers.Dequeue();
			}
		}

		private void prepareTrackContainers()
		{
			DevTrace("prepareTrackContainers()");
			TrackContainers.Enqueue(newTrackContainer("tracks1"));
			TrackContainers.Enqueue(newTrackContainer("tracks2"));
			activeTrackContainer = TrackContainers.Dequeue();
		}

		private GameObject newTrackContainer(string containerName)
		{
			GameObject gameObject = new GameObject();
			gameObject.name = containerName;
			gameObject.transform.parent = base.transform;
			return gameObject;
		}

		private void GetFriendScores()
		{
			friendScores = new Stack<LeaderBoardHighScore>();
			LeaderboardManager leaderboardManager = Service.Get<LeaderboardManager>();
			if (Service.Get<PlayerDataService>().IsPlayerLoggedIn())
			{
				if (!leaderboardManager.LoadCachedFriendHighScores("SledRacer", OnFriendScoresLoaded))
				{
					UnityEngine.Debug.Log("GetFriendScores() - Leaderboard was not cached. Fetching now...");
				}
				else
				{
					UnityEngine.Debug.Log("GetFriendScores() - Leaderboard was cached. No wait time.");
				}
			}
			else
			{
				OnFriendScoresLoaded(null);
			}
		}

		private void OnFriendScoresLoaded(LeaderBoardResponse obj)
		{
			UnityEngine.Debug.Log("OnFriendScoresLoaded");
			if (obj != null)
			{
				PlayerData playerData = Service.Get<PlayerDataService>().PlayerData;
				foreach (LeaderBoardHighScore player in obj.Players)
				{
					if (player.PlayerId != playerData.Account.PlayerId || player.Score != playerData.HighScore.Score)
					{
						friendScores.Push(player);
						UnityEngine.Debug.Log("Friend Score: " + player.Name + " - " + player.Score);
					}
				}
			}
			recordMarkerAdded = false;
			createInitialTrack();
			base.enabled = true;
			TracksAreReady();
			DevTrace("TrackManager is now Enabled");
		}

		private void createInitialTrack()
		{
			tracksBeingChanged = true;
			StartCoroutine(addTrackSegments(config.TrackViewDistance, _immediate: true, oneTrackPerFrame: false));
		}

		private void validateTerrain()
		{
			if (Pool == null || !Pool.Ready)
			{
				throw new MissingComponentException("Terrain Pool is null or not ready");
			}
			if (tracksBeingChanged)
			{
				return;
			}
			float num = config.TrackViewDistance;
			float num2 = 0f - config.TrackTailDistance;
			if (TutorialMode)
			{
				num /= 2f;
				storedTravel -= DistanceTravelled;
			}
			if (LeadPlayer != null)
			{
				float num3 = num;
				Vector3 position = LeadPlayer.position;
				num = num3 + position.z;
				if (LastPlayer != null)
				{
					float num4 = num2;
					Vector3 position2 = LastPlayer.position;
					num2 = num4 + position2.z;
				}
				else
				{
					float num5 = num2;
					Vector3 position3 = LeadPlayer.position;
					num2 = num5 + position3.z;
				}
			}
			Vector3 endPoint = currTrackSegment.EndPoint;
			Vector3 endPoint2 = ActiveTracks.Peek().EndPoint;
			if (endPoint.z < num)
			{
				tracksBeingChanged = true;
				StartCoroutine(addTrackSegments(num));
			}
			if (endPoint2.z < num2)
			{
				tracksBeingChanged = true;
				vLastEndpointPassed = endPoint2;
				StartCoroutine(removeTrackSegment());
			}
		}

		private IEnumerator addTrackSegments(float _drawDistance, bool _immediate = false, bool oneTrackPerFrame = true)
		{
			Vector3 position = activeTrackContainer.transform.position;
			if (position.z < 0f - config.TrackMaxDrawDistance)
			{
				DevTrace("SWAPPING track containers");
				GameObject trackswap = activeTrackContainer;
				activeTrackContainer = TrackContainers.Dequeue();
				float num = storedTravel;
				Vector3 position2 = trackswap.transform.position;
				storedTravel = num + position2.z;
				activeTrackContainer.GetComponent<Rigidbody>().linearVelocity = trackswap.GetComponent<Rigidbody>().linearVelocity;
				foreach (POTrackSegment track2 in ActiveTracks)
				{
					track2.ReparentTo(activeTrackContainer.transform);
				}
				trackswap.transform.position = Vector3.zero;
				trackswap.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
				TrackContainers.Enqueue(trackswap);
			}
			else
			{
				Vector3 position3 = LeadPlayer.position;
				if (position3.z > config.TrackMaxDrawDistance)
				{
					Vector3 vShiftAmount = vLastEndpointPassed;
					storedTravel += vLastEndpointPassed.z;
					Vector3 positionOffset = Vector3.Scale(vLastEndpointPassed, -Vector3.one);
					DevTrace("SHIFTING (" + ActiveTracks.Count + ") track segments to start by " + positionOffset.ToString());
					int tc = 0;
					string empty = string.Empty;
					string empty2 = string.Empty;
					string res = "\n";
					foreach (POTrackSegment track in ActiveTracks)
					{
						tc++;
						if (tc == 1)
						{
						}
						string prev = track.transform.position.ToString();
						track.ShiftTrack(positionOffset);
						string curr = track.transform.position.ToString();
						string text = res;
						res = text + "\t" + tc + ". from " + prev + " to " + curr + "\n";
					}
					DevTrace("SHIFTED " + tc + "/" + ActiveTracks.Count + " track segments" + res);
					LeadPlayer.position += positionOffset;
					vDistanceDrawn -= vShiftAmount;
					_drawDistance -= vShiftAmount.z;
				}
			}
			vDistanceDrawn += activeTrackContainer.transform.position;
			vRealDistanceDrawn += activeTrackContainer.transform.position;
			while (vDistanceDrawn.z < _drawDistance)
			{
				POTrackSegment prevTrackSegment = currTrackSegment;
				currTrackSegment = getNewSegment((!(prevTrackSegment == null)) ? RandomTrackPiece() : 0);
				Vector3 targetPosition = (!(prevTrackSegment == null)) ? prevTrackSegment.EndPoint : Vector3.zero;
				currTrackSegment.Spawn(activeTrackContainer.transform, targetPosition, Quaternion.identity, GetCurrentDifficulty(), _immediate);
				ActiveTracks.Enqueue(currTrackSegment);
				DevTrace(">> add TrackSegments() added a track");
				float prevDistanceDrawn = vRealDistanceDrawn.z;
				vDistanceDrawn += currTrackSegment.LocalEndPoint;
				vRealDistanceDrawn += currTrackSegment.LocalEndPoint;
				float scoreAtEndOfSegment = vRealDistanceDrawn.z / config.TrackUnitsForProgress;
				if (enableMarkers)
				{
					CheckAddMarkers(currTrackSegment, prevDistanceDrawn, scoreAtEndOfSegment);
				}
				if (oneTrackPerFrame)
				{
					yield return null;
				}
			}
			tracksBeingChanged = false;
		}

		private void CheckAddMarkers(POTrackSegment currTrackSegment, float prevDistanceDrawn, float scoreAtEndOfSegment)
		{
			PlayerData playerData = Service.Get<PlayerDataService>().PlayerData;
			int? score = playerData.HighScore.Score;
			if (!recordMarkerAdded && score.HasValue)
			{
				recordMarkerAdded = CheckAddMarker(HighScoreMarkerType.LocalAllTime, score.Value, playerData.Account.Username, playerData.Account.Colour, prevDistanceDrawn, scoreAtEndOfSegment, isTopFriend: false);
			}
			if (friendScores == null)
			{
				return;
			}
			while (friendScores.Count > 0)
			{
				LeaderBoardHighScore leaderBoardHighScore = friendScores.Peek();
				HighScoreMarkerType type;
				if (leaderBoardHighScore.PlayerId == playerData.Account.PlayerId)
				{
					if (score.HasValue && leaderBoardHighScore.Score == score.Value)
					{
						continue;
					}
					type = HighScoreMarkerType.LocalWeekly;
				}
				else
				{
					type = HighScoreMarkerType.Friend;
				}
				if (CheckAddMarker(type, leaderBoardHighScore.Score, leaderBoardHighScore.Name, leaderBoardHighScore.Colour, prevDistanceDrawn, scoreAtEndOfSegment, friendScores.Count == 1))
				{
					friendScores.Pop();
					continue;
				}
				break;
			}
		}

		private bool CheckAddMarker(HighScoreMarkerType type, int score, string nameLabel, int colorID, float prevDistanceDrawn, float scoreAtEndOfSegment, bool isTopFriend)
		{
			if ((float)score < scoreAtEndOfSegment)
			{
				if ((float)score > config.ScoreMarkerMinimum)
				{
					float localZ = (float)score * config.TrackUnitsForProgress - prevDistanceDrawn;
					currTrackSegment.SpawnHighScoreMarker(type, localZ, nameLabel, colorID, isTopFriend);
				}
				return true;
			}
			return false;
		}

		private POTrackSegment getNewSegment(int _variant)
		{
			IPoolable poolable = Pool.CheckOut(_variant);
			return (POTrackSegment)poolable;
		}

		private IEnumerator removeTrackSegment()
		{
			ActiveTracks.Dequeue().Despawn();
			tracksBeingChanged = false;
			yield return false;
		}

		private void DevTrace(string _msg)
		{
		}

		private int RandomTrackPiece()
		{
			int num = 0;
			if (TutorialMode)
			{
				return 0;
			}
			return Mathf.FloorToInt(UnityEngine.Random.value * (float)Pool.Pools.Count);
		}

		private int GetCurrentDifficulty()
		{
			int result = 0;
			if (!TutorialMode)
			{
				int num = Mathf.CeilToInt(DistanceTravelled / config.TrackDistanceBeforeDifficultyIncrease);
				DevTrace("Difficulty is " + num);
				result = UnityEngine.Random.Range(1, Mathf.Min(num, currTrackSegment.TrackDecorations.Length));
			}
			return result;
		}
	}
}
