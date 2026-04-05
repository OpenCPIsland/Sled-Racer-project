using Disney.ClubPenguin.SledRacer;
using UnityEngine;
using UnityEngine.UI;

namespace Disney.ClubPenguin.Login.Authentication
{
	public class ReturningLoginController : MonoBehaviour
	{
		public Image PaperDollImage;

		public Text PlayerNameText;

		private SavedPlayerData savedPlayer;

		public void SetSavedPlayer(SavedPlayerData savedPlayer)
		{
			this.savedPlayer = savedPlayer;
			PlayerNameText.text = savedPlayer.UserName;
			if (savedPlayer.PaperDollBytes != null && savedPlayer.PaperDollBytes.Length > 0)
			{
				Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, false);
				texture2D.LoadImage(savedPlayer.PaperDollBytes);
				Sprite sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0f, 0f));
				PaperDollImage.sprite = sprite;
			}
			else
			{
				PaperDollImage.sprite = AvatarUtil.GetLargeAvatar(savedPlayer.PenguinColor);
			}
			PaperDollImage.enabled = true;
		}

		public void DoReturningLogin()
		{
			LoginController component = GetComponent<LoginController>();
			component.PlayAudioAndLogin(savedPlayer.UserName, component.passwordInputField.text, component.savePasswordToggle.isOn);
		}
	}
}
