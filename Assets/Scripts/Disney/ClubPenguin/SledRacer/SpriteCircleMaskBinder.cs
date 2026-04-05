using UnityEngine;

namespace Disney.ClubPenguin.SledRacer
{
	[ExecuteAlways]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(SpriteRenderer))]
	public class SpriteCircleMaskBinder : MonoBehaviour
	{
		private static readonly int MaskCenterId = Shader.PropertyToID("_MaskCenter");

		private static readonly int MaskRadiusId = Shader.PropertyToID("_MaskRadius");

		private MaterialPropertyBlock propertyBlock;

		private SpriteRenderer spriteRenderer;

		private void Awake()
		{
			CacheComponents();
			ApplyMaskData();
		}

		private void OnEnable()
		{
			CacheComponents();
			ApplyMaskData();
		}

		private void OnValidate()
		{
			CacheComponents();
			ApplyMaskData();
		}

		private void LateUpdate()
		{
			ApplyMaskData();
		}

		private void CacheComponents()
		{
			if (spriteRenderer == null)
			{
				spriteRenderer = GetComponent<SpriteRenderer>();
			}
			if (propertyBlock == null)
			{
				propertyBlock = new MaterialPropertyBlock();
			}
		}

		private void ApplyMaskData()
		{
			if (spriteRenderer == null)
			{
				return;
			}

			Bounds localBounds = spriteRenderer.localBounds;
			float radius = Mathf.Max(Mathf.Min(localBounds.extents.x, localBounds.extents.y), 0.0001f);

			// Push the current local bounds into the material property block so the
			// circle mask follows the sprite even when it comes from an atlas.
			spriteRenderer.GetPropertyBlock(propertyBlock);
			propertyBlock.SetVector(MaskCenterId, new Vector4(localBounds.center.x, localBounds.center.y, 0f, 0f));
			propertyBlock.SetFloat(MaskRadiusId, radius);
			spriteRenderer.SetPropertyBlock(propertyBlock);
		}
	}
}
