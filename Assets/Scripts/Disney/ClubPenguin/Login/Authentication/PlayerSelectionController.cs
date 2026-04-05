using Disney.ClubPenguin.SledRacer;
using UnityEngine;
using UnityEngine.UI;

namespace Disney.ClubPenguin.Login.Authentication
{
	public class PlayerSelectionController : MonoBehaviour
	{
		public delegate void PlayerSelectedDelegate(SavedPlayerData savedPlayerData);

		public delegate void AddPlayerClickedDelegate();

		public PlayerSelectedDelegate PlayerSelected;

		public AddPlayerClickedDelegate AddPlayerClicked;

		public SavedPlayerCollection SavedPlayers;

		private SavedPlayerData _SavedPlayer;

		public AudioClip ForgetPlayerAudioClip;

		public AudioClip ConfirmForgetAudioClip;

		public AudioClip CancelForgetAudioClip;

		public AudioClip SelectPlayerAudioClip;

		public AudioClip AddPlayerAudioClip;

		public GameObject forgetPlayerView;

		public GameObject savedPlayerView;

		public GameObject addPlayerView;

		public Image savedPlayerImage;

		[HideInInspector]
		public AudioSource RootAudioSource;

		public void SetSavedPlayer(SavedPlayerData value)
		{
			_SavedPlayer = value;
			Text componentInChildren = savedPlayerView.GetComponentInChildren<Text>();
			componentInChildren.text = _SavedPlayer.UserName;
			if (_SavedPlayer.PaperDollBytes != null && _SavedPlayer.PaperDollBytes.Length > 0)
			{
				Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, false);
				texture2D.LoadImage(_SavedPlayer.PaperDollBytes);
				Sprite sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0f, 0f));
				savedPlayerImage.sprite = sprite;
			}
			else
			{
				savedPlayerImage.sprite = AvatarUtil.GetLargeAvatar(_SavedPlayer.PenguinColor);
			}
			savedPlayerImage.enabled = true;
			ShowSavedPlayerView();
		}

		public void ShowAddNewView()
		{
			forgetPlayerView.SetActive(value: false);
			savedPlayerView.SetActive(value: false);
			addPlayerView.SetActive(value: true);
		}

		public void ShowSavedPlayerView()
		{
			forgetPlayerView.SetActive(value: false);
			savedPlayerView.SetActive(value: true);
			addPlayerView.SetActive(value: false);
		}

		public void ShowForgetPlayerView()
		{
			RootAudioSource.PlayOneShot(ForgetPlayerAudioClip);
			forgetPlayerView.SetActive(value: true);
			savedPlayerView.SetActive(value: false);
			addPlayerView.SetActive(value: false);
		}

		public void ConfirmForget()
		{
			RootAudioSource.PlayOneShot(ConfirmForgetAudioClip);
			SavedPlayers.RemoveSavedPlayer(_SavedPlayer);
			SavedPlayers.SaveToDisk();
			ShowAddNewView();
			RectTransform component = base.transform.parent.GetComponent<RectTransform>();
			GetComponent<RectTransform>().SetParent(component, worldPositionStays: false);
		}

		public void CancelForget()
		{
			RootAudioSource.PlayOneShot(CancelForgetAudioClip);
			ShowSavedPlayerView();
		}

		public void SelectPlayer()
		{
			RootAudioSource.PlayOneShot(SelectPlayerAudioClip);
			if (PlayerSelected != null)
			{
				PlayerSelected(_SavedPlayer);
			}
		}

		public void AddNewPlayer()
		{
			RootAudioSource.PlayOneShot(AddPlayerAudioClip);
			if (AddPlayerClicked != null)
			{
				AddPlayerClicked();
			}
		}
	}
}
