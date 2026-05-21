using UnityEngine;
using UnityEngine.UI;

namespace Disney.ClubPenguin.CPModuleUtils
{
	[RequireComponent(typeof(RectTransform))]
	public class LoadingOverlay : MonoBehaviour
	{
		public RectTransform LoadingOverlayPrefab;

		public string StatusTextComponentName = string.Empty;

		private GraphicRaycaster[] rayCasters;

		private bool[] previousRayCasterStates;

		private RectTransform loadingOverlay;

		private bool lockFocus;

		private Text statusTextComponent;

		private string statusText;

		private void Awake()
		{
			loadingOverlay = (UnityEngine.Object.Instantiate(LoadingOverlayPrefab) as RectTransform);
			loadingOverlay.SetParent(GetComponent<RectTransform>(), worldPositionStays: false);
			loadingOverlay.gameObject.SetActive(value: false);
		}

		private void Start()
		{
			if (!(StatusTextComponentName != string.Empty))
			{
				return;
			}
			Text[] componentsInChildren = loadingOverlay.GetComponentsInChildren<Text>();
			Text[] array = componentsInChildren;
			foreach (Text text in array)
			{
				if (text.name == StatusTextComponentName)
				{
					statusTextComponent = text;
					break;
				}
			}
			if (statusTextComponent == null)
			{
				UnityEngine.Debug.LogWarning("LoadingOverlayPrefab does not have a Text component child.");
				return;
			}
			UnityEngine.Debug.Log("Not DEV");
			statusTextComponent.text = string.Empty;
		}

		public void SetStatusText(string statusText)
		{
		}

		public void Show(bool lockInputFocus)
		{
			lockFocus = lockInputFocus;
			loadingOverlay.gameObject.SetActive(value: true);
			if (lockInputFocus)
			{
				LockInputFocus();
			}
		}

		public void Hide()
		{
			loadingOverlay.gameObject.SetActive(value: false);
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
