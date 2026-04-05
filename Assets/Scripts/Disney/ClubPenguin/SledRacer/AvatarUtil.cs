using System.Collections.Generic;
using UnityEngine;

namespace Disney.ClubPenguin.SledRacer
{
	public static class AvatarUtil
	{
		private const string GOLD_HAT_BLEND_SHADER_PATH = "Shaders/RiderGoldHatBlend";

		private static readonly Dictionary<int, Material> goldHatMaterialCache = new Dictionary<int, Material>();

		private static Shader goldHatBlendShader;

		public static int[] COLOR_TO_RESOURCE_MAP = new int[17]
		{
			1,
			1,
			2,
			3,
			4,
			5,
			6,
			7,
			8,
			9,
			10,
			11,
			12,
			13,
			1,
			14,
			15
		};

		public static int GetResourceId(int colorId)
		{
			int result = 1;
			if (colorId >= 0 && colorId < COLOR_TO_RESOURCE_MAP.Length)
			{
				result = COLOR_TO_RESOURCE_MAP[colorId];
			}
			return result;
		}

		public static Sprite GetLargeAvatar(int colorId)
		{
			return Resources.Load<Sprite>("AvatarSprites/Pengiun_" + GetResourceId(colorId));
		}

		public static Sprite GetSmallAvatar(int colorId)
		{
			return Resources.Load<Sprite>("AvatarSpritesSmall/Pengiun_" + GetResourceId(colorId));
		}

		public static Material GetRiderMaterial(int colorId)
		{
			return Resources.Load<Material>("RiderMaterials/RiderMaterial_" + GetResourceId(colorId));
		}

		public static Material GetGoldenHelmetRiderMaterial(int colorId, Material goldenHelmetMaterial = null)
		{
			int resourceId = GetResourceId(colorId);
			Material value;
			if (goldHatMaterialCache.TryGetValue(resourceId, out value))
			{
				return value;
			}
			Material riderMaterial = GetRiderMaterial(colorId);
			Material material2 = (goldenHelmetMaterial != null) ? goldenHelmetMaterial : Resources.Load<Material>("RiderMaterials/RiderMaterial_GoldHat");
			Shader goldenHelmetBlendShader = GetGoldenHelmetBlendShader();
			if (riderMaterial == null || material2 == null || goldenHelmetBlendShader == null)
			{
				Debug.LogWarning("Unable to load golden helmet rider material resources for color " + resourceId + ". Falling back to the base rider material.");
				return riderMaterial;
			}
			Material material = new Material(goldenHelmetBlendShader);
			material.name = "RiderMaterial_GoldHat_" + resourceId;
			if (riderMaterial.HasProperty("_Color"))
			{
				material.SetColor("_Color", riderMaterial.GetColor("_Color"));
			}
			material.SetTexture("_MainTex", riderMaterial.mainTexture);
			material.SetTextureScale("_MainTex", riderMaterial.GetTextureScale("_MainTex"));
			material.SetTextureOffset("_MainTex", riderMaterial.GetTextureOffset("_MainTex"));
			material.SetTexture("_GoldHatTex", material2.mainTexture);
			material.SetTextureScale("_GoldHatTex", material2.GetTextureScale("_MainTex"));
			material.SetTextureOffset("_GoldHatTex", material2.GetTextureOffset("_MainTex"));
			goldHatMaterialCache[resourceId] = material;
			return material;
		}

		private static Shader GetGoldenHelmetBlendShader()
		{
			if (goldHatBlendShader == null)
			{
				goldHatBlendShader = Resources.Load<Shader>(GOLD_HAT_BLEND_SHADER_PATH);
			}
			return goldHatBlendShader;
		}
	}
}
