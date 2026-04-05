using Newtonsoft.Json;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace Disney.ClubPenguin.SledRacer
{
	public class ConfigController : MonoBehaviour
	{
		private const string CONFIG_KEY = "sledracer.config";

		public bool UseServerSettings = true;

		public bool QuickDropMode;

		public float QuickDropStrength = 10f;

		public float BoostInvulnerableDuration = 10f;

		public float BoostInvulnerableShiledOffTime = 0.5f;

		public float BoostInvulnerableWarningDuration = 3f;

		public float BoostJetpackAltitude = 12f;

		public float BoostJetpackDuration = 20f;

		public float BoostJetpackSpeed = 135f;

		public float BoostJetpackFollowSoftness = 0.3f;

		public float BoostJetpackWarningDuration = 2f;

		public float BoostParachuteOpenAltiutude = 18f;

		public Vector3 BoostParachuteDrag = new Vector3(0.8f, 0.2f, 0.75f);

		public Vector3 BoostParachuteDeployVelocity = new Vector3(0.5f, 0.5f, 0.5f);

		public float BoostSkiwaxVelocityBonus = 35f;

		public float BoostWhoopieCushionAnimBounceHeight = 2.52f;

		public float BoostWhoopieCushionBlastAngle = 30f;

		public float BoostWhoopieCushionBlastPower = 230f;

		public float BoostWhoopieCushionUsedDisplayTime = 3f;

		public float BoostReviveUsedDisplayTime = 1.5f;

		public float BoostReviveHeight = 10f;

		public float PlayerPhysicsMass = 1.5f;

		public float PlayerMinimumForwardVelocity = 55f;

		public float PlayerSteerForce = 95f;

		public float PlayerForwardForce = 45f;

		public float PlayerDownwardForce = 42f;

		public float PlayerJumpStrength = 1700f;

		public float PlayerSurfaceMatchingDistance = 2f;

		public float CameraFOVSpeedMinimum = 30f;

		public float CameraDistance = 7.5f;

		public float CameraHeight = 5f;

		public float CameraRotationDamping = 10f;

		public float CameraHeightDamping = 10f;

		public float CameraZoomRatio = 0.6f;

		public float CameraDefaultFOV = 60f;

		public float CameraMaximumFOV = 100f;

		public float CameraMaxVelocityRange = 20f;

		public float CameraMaxOffsetValue = 5f;

		public Vector3 CameraSpringRatio = new Vector3(0f, -1f, 0f);

		public float IcePatchInvulnerabilityTime = 3f;

		public float IcePatchInvulnerabilityWarnTime = 0.5f;

		public float IcePatchAudioCutoffFrequency = 2200f;

		public float IcePatchBoostStrength = 150f;

		public float FluffyDistanceBeforePopup = 80f;

		public float AnvilSlideDuration = 1f;

		public float AnvilIdleTime = 0.5f;

		public Vector3 AnvilMotion = new Vector3(0f, 10f, 0f);

		public float BackgroundCameraStartY = 1.5f;

		public float BackgroundCameraTargetY = 0.15f;

		public float BackgroundCameraMinY = -0.75f;

		public float BackgroundCameraMaxY = 1.5f;

		public float BackgroundCameraSpeed = 0.22f;

		public float BackgroundCameraDistanceCheck = 0.1f;

		public float BackgroundCameraMovementDampening = 60f;

		public float BackgroundCameraMaxHorizontal = 3f;

		public float BackgroundCameraMinHorizontal = -3f;

		public Vector3 DefaultSledGroundProperties = new Vector3(2f, 1f, 1f);

		public Vector3 DefaultSledAerialProperties = new Vector3(1f, 1f, 1f);

		public float DefaultSledMaxVelocity = 240f;

		public float TrackMaxDrawDistance = 50000f;

		public float TrackViewDistance = 1000f;

		public float TrackTailDistance = 50f;

		public float TrackGravity = 20f;

		public float TrackUnitsForProgress = 5f;

		public float TrackDistanceBeforeDifficultyIncrease = 250f;

		public Vector3 TrackStartPosition = new Vector3(0f, 0f, 20f);

		public float TrackTweenTime = 0.5f;

		public float TrackTweenDistance = 30f;

		public float ScoreMarkerMinimum = 100f;

		public Vector3 FriendMarkerLaunchForce = new Vector3(0f, 250f, 0f);

		public float FriendMarkerLaunchDelaySeconds = 1.3f;

		public float FriendMarkerDestroyDelaySeconds = 3f;

		public float FriendMarkerSoundDelaySeconds;

		public float FriendMarkerDistanceFromPlayerFadePercent = 0.3f;

		public float FriendMarkerDistanceFromGround = 22f;

		public float GroundedDistance = 1.5f;

		public float ImpactGroundMinVelocity = -30f;

		public float ImpactGroundHardMinVelocity = 80f;

		public float SfxCatchAirDistanceFromGround = 15f;

		public float SfxAirTimeDistanceFromGround = 20f;

		public float SfxAirTimeDelaySeconds = 1.5f;

		public float SfxSledHangMinVelocity = 150f;

		public float SfxVelocityForFullGust = 200f;

		public float SfxAltitudeForFullGust = 10f;

		public float SfxBombfallAltitudeTrigger = 20f;

		public float SfxSledHangChance = 0.5f;

		public int MoPubDisplayFrequency = 5;

		public int NotificationNumberOfDelays = 6;

		public int NotificationIntervalDays = 7;

		public int NotificationDayOfWeek = 4;

		public int NotificationHourOfDay = 15;

		public int NotificationMinuteOfHour = 30;

		public int NotificationMinFirstNotificationDelayMinutes = 60;

		public int IntroFrequency = 5;

		public string DesktopGraphicsQualityConditionalTier = "Standalone_High";

		public int DesktopGraphicsAntiAliasSamplesMedium = 2;

		public int DesktopGraphicsAntiAliasSamplesHigh = 4;

		public int DesktopGraphicsVSyncCount = 1;

		public string WebGraphicsQualityConditionalTier = "WebGL_High";

		public int WebGraphicsAntiAliasSamplesMedium = 2;

		public int WebGraphicsAntiAliasSamplesHigh = 4;

		public int WebGraphicsVSyncCount = 1;

		public string RewardItemType = "HEAD";

		public int RewardItemId = 1996;

		private FieldInfo[] fields;

		private Hashtable ModifiedFieldsOrginalValue = new Hashtable();

		private void Awake()
		{
			Service.Set(this);
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			FieldInfo[] array = GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
			fields = new FieldInfo[array.Length - 1];
			int num = 0;
			FieldInfo[] array2 = array;
			foreach (FieldInfo fieldInfo in array2)
			{
				if (fieldInfo.Name != "UseServerSettings")
				{
					fields[num++] = fieldInfo;
				}
			}
			if (UseServerSettings)
			{
				Load(PlayerPrefs.GetString("sledracer.config"));
			}
		}

		public void SetConfiguration(Hashtable configOverrides)
		{
			if (UseServerSettings && Load(configOverrides))
			{
				PlayerPrefs.SetString("sledracer.config", ToJsonString(configOverrides));
			}
		}

		private string ToJsonString(Hashtable data)
		{
			if (data == null)
			{
				data = new Hashtable();
			}
			return JsonConvert.SerializeObject(data);
		}

		private bool Load(string data)
		{
			return Load(JsonConvert.DeserializeObject<Hashtable>(data));
		}

		private bool Load(Hashtable config)
		{
			bool result = false;
			if (config != null)
			{
				FieldInfo[] array = fields;
				foreach (FieldInfo fieldInfo in array)
				{
					if (config.ContainsKey(fieldInfo.Name) && !fieldInfo.GetValue(this).Equals(config[fieldInfo.Name]))
					{
						if (ModifiedFieldsOrginalValue.ContainsKey(fieldInfo.Name))
						{
							if (fieldInfo.GetValue(this) == ModifiedFieldsOrginalValue[fieldInfo.Name])
							{
								ModifiedFieldsOrginalValue.Remove(fieldInfo.Name);
							}
						}
						else
						{
							ModifiedFieldsOrginalValue.Add(fieldInfo.Name, fieldInfo.GetValue(this));
						}
						try
						{
							fieldInfo.SetValue(this, Convert.ChangeType(config[fieldInfo.Name], fieldInfo.FieldType));
							result = true;
						}
						catch (InvalidCastException)
						{
							UnityEngine.Debug.LogWarning("Invalid cast " + config[fieldInfo.Name].GetType() + " to " + fieldInfo.FieldType + " for value " + config[fieldInfo.Name] + " for the field " + fieldInfo.Name);
						}
						catch (ArgumentException)
						{
							UnityEngine.Debug.LogWarning("Invalid value " + config[fieldInfo.Name] + " for the field " + fieldInfo.Name);
						}
					}
					if (ModifiedFieldsOrginalValue.ContainsKey(fieldInfo.Name) && !config.ContainsKey(fieldInfo.Name))
					{
						fieldInfo.SetValue(this, ModifiedFieldsOrginalValue[fieldInfo.Name]);
						ModifiedFieldsOrginalValue.Remove(fieldInfo.Name);
					}
				}
			}
			return result;
		}
	}
}
