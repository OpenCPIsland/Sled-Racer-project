using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Disney.ClubPenguin.CPModuleUtils
{
	[RequireComponent(typeof(InputField))]
	public class KeyboardFocusLock : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
	{
		private const float FOCUS_RELEASE_WAIT_TIME = 0.7f;

		private float waitBeforeReleaseSec;

		private bool hasFocus;

		private bool isFocusLocked;

		private GraphicRaycaster[] rayCasters;

		private bool[] previousRayCasterStates;

		public void OnSelect(BaseEventData eventData)
		{
			waitBeforeReleaseSec = 0f;
			hasFocus = true;
		}

		public void OnDeselect(BaseEventData eventData)
		{
			hasFocus = false;
		}

		private void Update()
		{
			if (hasFocus && !isFocusLocked && TouchScreenKeyboard.visible)
			{
				isFocusLocked = true;
				LockInputFocus();
			}
			else if (isFocusLocked && !TouchScreenKeyboard.visible)
			{
				waitBeforeReleaseSec = 0.7f;
				ForceReleaseOfInputFocus();
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

		private IEnumerator ReleaseInputFocusWait()
		{
			yield return new WaitForSeconds(waitBeforeReleaseSec);
			ReleaseInputFocus();
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

		public void ForceReleaseOfInputFocus(bool wait = true)
		{
			if (isFocusLocked)
			{
				isFocusLocked = false;
				if (wait)
				{
					StartCoroutine(ReleaseInputFocusWait());
				}
				else
				{
					ReleaseInputFocus();
				}
			}
		}
	}
}
