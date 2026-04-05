using System;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ClubPenguin.SledRacer
{
	public class SledRacerGraphicsService
	{
		private enum GraphicsTierLevel
		{
			Low,
			Medium,
			High
		}

		private sealed class GraphicsPreset
		{
			public readonly string QualitySettingName;

			public readonly GraphicsTierLevel TierLevel;

			public GraphicsPreset(string qualitySettingName, GraphicsTierLevel tierLevel)
			{
				QualitySettingName = qualitySettingName;
				TierLevel = tierLevel;
			}
		}

		private const string DEFAULT_QUALITY_TIER = "Standalone_High";

		private const string DEFAULT_WEB_QUALITY_TIER = "WebGL_High";

		private const string FALLBACK_QUALITY_TIER = "Default";

		private readonly ConfigController config;

		private readonly Dictionary<string, GraphicsPreset> presetByConditionalTier = new Dictionary<string, GraphicsPreset>(StringComparer.OrdinalIgnoreCase)
		{
			{
				"Standalone_Low",
				new GraphicsPreset("Low", GraphicsTierLevel.Low)
			},
			{
				"Standalone_Medium",
				new GraphicsPreset("Medium", GraphicsTierLevel.Medium)
			},
			{
				"Standalone_High",
				new GraphicsPreset("High", GraphicsTierLevel.High)
			},
			{
				"WebGL_Low",
				new GraphicsPreset("Low", GraphicsTierLevel.Low)
			},
			{
				"WebGL_Medium",
				new GraphicsPreset("Medium", GraphicsTierLevel.Medium)
			},
			{
				"WebGL_High",
				new GraphicsPreset("High", GraphicsTierLevel.High)
			},
			{
				"Default",
				new GraphicsPreset("Low", GraphicsTierLevel.Low)
			}
		};

		public SledRacerGraphicsService(ConfigController config)
		{
			this.config = config;
		}

		public void Init()
		{
			Apply();
		}

		public void Apply()
		{
			if (!IsSupportedGraphicsPlatform())
			{
				return;
			}
			string configuredTier = GetConfiguredTier();
			GraphicsPreset graphicsPreset = GetPreset(configuredTier);
			int qualityLevel = FindQualityLevel(graphicsPreset.QualitySettingName);
			if (qualityLevel >= 0 && QualitySettings.GetQualityLevel() != qualityLevel)
			{
				QualitySettings.SetQualityLevel(qualityLevel, true);
			}
			QualitySettings.antiAliasing = GetAntiAliasingSamples(graphicsPreset.TierLevel);
			ApplyFramePacing();
			UnityEngine.Debug.Log(string.Format("Applied {0} graphics tier '{1}' -> quality='{2}', msaa={3}x, vSync={4}, targetFrameRate={5}",
				IsWebGLPlatform() ? "web" : "desktop",
				configuredTier,
				graphicsPreset.QualitySettingName,
				QualitySettings.antiAliasing,
				QualitySettings.vSyncCount,
				Application.targetFrameRate));
		}

		private string GetConfiguredTier()
		{
			if (IsWebGLPlatform())
			{
				if (!string.IsNullOrEmpty(config.WebGraphicsQualityConditionalTier))
				{
					return config.WebGraphicsQualityConditionalTier;
				}
				return DEFAULT_WEB_QUALITY_TIER;
			}
			if (!string.IsNullOrEmpty(config.DesktopGraphicsQualityConditionalTier))
			{
				return config.DesktopGraphicsQualityConditionalTier;
			}
			return DEFAULT_QUALITY_TIER;
		}

		private GraphicsPreset GetPreset(string tierName)
		{
			GraphicsPreset graphicsPreset;
			if (!presetByConditionalTier.TryGetValue(tierName, out graphicsPreset))
			{
				graphicsPreset = presetByConditionalTier[FALLBACK_QUALITY_TIER];
			}
			return graphicsPreset;
		}

		private int GetAntiAliasingSamples(GraphicsTierLevel tierLevel)
		{
			bool isWebGLPlatform = IsWebGLPlatform();
			switch (tierLevel)
			{
			case GraphicsTierLevel.Medium:
				return NormalizeAntiAliasingSamples(isWebGLPlatform ? config.WebGraphicsAntiAliasSamplesMedium : config.DesktopGraphicsAntiAliasSamplesMedium);
			case GraphicsTierLevel.High:
				return NormalizeAntiAliasingSamples(isWebGLPlatform ? config.WebGraphicsAntiAliasSamplesHigh : config.DesktopGraphicsAntiAliasSamplesHigh);
			default:
				return 0;
			}
		}

		private void ApplyFramePacing()
		{
			if (IsWebGLPlatform())
			{
				QualitySettings.vSyncCount = Mathf.Clamp(config.WebGraphicsVSyncCount, 0, 4);
				Application.targetFrameRate = -1;
				return;
			}
			QualitySettings.vSyncCount = Mathf.Clamp(config.DesktopGraphicsVSyncCount, 0, 4);
			Application.targetFrameRate = -1;
		}

		private int NormalizeAntiAliasingSamples(int samples)
		{
			if (samples <= 0)
			{
				return 0;
			}
			if (samples <= 2)
			{
				return 2;
			}
			if (samples <= 4)
			{
				return 4;
			}
			return 8;
		}

		private int FindQualityLevel(string qualityName)
		{
			string[] names = QualitySettings.names;
			for (int i = 0; i < names.Length; i++)
			{
				if (names[i].Equals(qualityName, StringComparison.OrdinalIgnoreCase))
				{
					return i;
				}
			}
			return -1;
		}

		private bool IsSupportedGraphicsPlatform()
		{
			if (Application.isMobilePlatform)
			{
				return false;
			}
			return IsWebGLPlatform() || SystemInfo.deviceType == DeviceType.Desktop;
		}

		private bool IsWebGLPlatform()
		{
			return Application.platform == RuntimePlatform.WebGLPlayer;
		}
	}
}
