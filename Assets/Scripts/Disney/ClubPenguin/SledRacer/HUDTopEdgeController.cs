using Disney.ClubPenguin.CPModuleUtils;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Disney.ClubPenguin.SledRacer
{
	public class HUDTopEdgeController : BaseMenuController
	{
		public Image PlayerImage;

		public TMP_Text CurrentScore;

		public TMP_Text HighScore;

		public TMP_Text PlayerName;

		public Button PauseButton;

		private SledRacerGameManager gameManager;

		private PlayerDataService playerDataService;

		protected override void VStart()
		{
			gameManager = GameManager.GetInstanceAs<SledRacerGameManager>();
			playerDataService = Service.Get<PlayerDataService>();
			UnityEngine.Debug.Log("[HUDTopEdgeController] SUBSCRIBE To PlayerDataService");
			playerDataService.Subscribe(OnPlayerDataUpdate);
			playerDataService.PlayerData.HighScore.BindToScore(SetHighScore);
			HardwareBackButtonDispatcher.SetTargetClickHandler(PauseButton);
			HeadphoneUnplugDetection.OnHeadphoneUnplugged += PauseClicked;
		}

		protected override void Init()
		{
		}

		public override void InitAnimations()
		{
			UIManager uIManager = Service.Get<UIManager>();
			if (uIManager.GetPreviousUIScene() == uIManager.PauseMenuPanel)
			{
				SetLoadingAnimationTrigger("Return");
			}
			else
			{
				base.InitAnimations();
			}
		}

		private void OnDestroy()
		{
			playerDataService.PlayerData.HighScore.UnBindFromScore(SetHighScore);
			playerDataService.UnSubscribe(OnPlayerDataUpdate);
			HeadphoneUnplugDetection.OnHeadphoneUnplugged -= PauseClicked;
			OnPointerUp();
		}

		private void OnApplicationFocus(bool focus)
		{
			if (!focus && !Service.Get<LoadingPanelController>().Loading)
			{
				//PauseClicked();
			}
		}

		public void PauseClicked()
		{
			if (!Service.Get<UIManager>().IsLoadingPanel)
			{
				Service.Get<EventDataService>().SendUIEvent(this, new UIEvent(UIEvent.uiGameEvent.RequestPause));
				PauseButton.enabled = false;
			}
		}

		private void OnPlayerDataUpdate()
		{
			UnityEngine.Debug.Log("[HUDTopEdgeController] Binding To HighScore");
			PlayerName.text = playerDataService.PlayerData.Account.Username;
			PlayerImage.sprite = AvatarUtil.GetSmallAvatar(playerDataService.PlayerData.Account.Colour);
		}

		private void SetHighScore(int? score)
		{
			if (!score.HasValue)
			{
				HighScore.text = "...";
			}
			else
			{
				HighScore.text = Convert.ToString(score);
			}
		}

		private void Update()
		{
			CurrentScore.text = gameManager.getCurrentScore().ToString();
		}

		public void OnPointerDown()
		{
			UnityEngine.Debug.Log("OnPointerDown");
			GameManager.GetInstanceAs<SledRacerGameManager>().playerScript.controlsEnabled = false;
		}

		public void OnPointerUp()
		{
			UnityEngine.Debug.Log("OnPointerUp");
			SledRacerGameManager instanceAs = GameManager.GetInstanceAs<SledRacerGameManager>();
			if (instanceAs != null && instanceAs.playerScript != null)
			{
				instanceAs.playerScript.controlsEnabled = true;
			}
		}
	}
}
