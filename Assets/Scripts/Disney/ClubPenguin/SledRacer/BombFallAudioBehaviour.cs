using UnityEngine;

namespace Disney.ClubPenguin.SledRacer
{
	public class BombFallAudioBehaviour : MonoBehaviour
	{
		private PlayerController player;

		private float elevation;

		private bool checkBombfall;

		private bool hasExecuted;

		private bool activateSFX;

		private void Awake()
		{
			player = GetComponent<PlayerController>();
			elevation = Service.Get<ConfigController>().SfxBombfallAltitudeTrigger;
		}

		private void Update()
		{
			Vector3 velocity = player.GetComponent<Rigidbody>().linearVelocity;
			if (player.currentLifeState == PlayerController.PlayerLifeState.Crashed)
			{
				if (player.currentMoveState != 0)
				{
					Vector3 velocity2 = player.GetComponent<Rigidbody>().linearVelocity;
					if (velocity2.y > 0f && !checkBombfall)
					{
						checkBombfall = true;
						return;
					}
					Vector3 velocity3 = player.GetComponent<Rigidbody>().linearVelocity;
					if (velocity3.y < 0f && checkBombfall)
					{
						checkBombfall = false;
						activateSFX = AltitudeCheck();
						if (activateSFX)
						{
							Service.Get<IAudio>().SFX.Play(SFXEvent.SFX_Bombfall);
						}
					}
				}
				else if (activateSFX)
				{
					Service.Get<IAudio>().SFX.Play(SFXEvent.SFX_HardImpact);
					checkBombfall = false;
					activateSFX = false;
				}
				else
				{
					checkBombfall = false;
				}
			}
			else
			{
				activateSFX = false;
			}
		}

		private bool AltitudeCheck()
		{
			bool flag = false;
			return player.SurfaceRay.distance >= elevation;
		}
	}
}
