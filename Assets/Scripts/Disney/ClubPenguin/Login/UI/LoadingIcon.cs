using UnityEngine;
using UnityEngine.UI;

namespace Disney.ClubPenguin.Login.UI
{
	[RequireComponent(typeof(RectTransform))]
	public class LoadingIcon : MonoBehaviour
	{
		public RectTransform LoadingIconPrefab;

		private GraphicRaycaster[] rayCasters;

		private bool[] previousRayCasterStates;

		private RectTransform loadingIcon;

		private bool lockFocus;

		private void Awake()
		{
			loadingIcon = (UnityEngine.Object.Instantiate(LoadingIconPrefab) as RectTransform);
			loadingIcon.SetParent(GetComponent<RectTransform>(), worldPositionStays: false);
			loadingIcon.gameObject.SetActive(value: false);
		}

		public void Show(bool lockInputFocus)
		{
			lockFocus = lockInputFocus;
			loadingIcon.gameObject.SetActive(value: true);
			if (lockInputFocus)
			{
				LockInputFocus();
			}
		}

		public void Hide()
		{
			loadingIcon.gameObject.SetActive(value: false);
			if (lockFocus)
			{
				ReleaseInputFocus();
			}
		}

		private void LockInputFocus()
		{
			rayCasters = (UnityEngine.Object.FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None));
			previousRayCasterStates = new bool[rayCasters.Length];
			for (int i = 0; i < rayCasters.Length; i++)
			{
				GraphicRaycaster graphicRaycaster = rayCasters[i];
				previousRayCasterStates[i] = graphicRaycaster.enabled;
				graphicRaycaster.enabled = false;
			}
		}

		private void ReleaseInputFocus()
		{
			if (rayCasters != null)
			{
				for (int i = 0; i < rayCasters.Length; i++)
				{
					GraphicRaycaster graphicRaycaster = rayCasters[i];
					graphicRaycaster.enabled = previousRayCasterStates[i];
				}
				rayCasters = null;
				previousRayCasterStates = null;
			}
		}
	}
}
