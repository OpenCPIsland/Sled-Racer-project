using UnityEngine;

namespace Disney.ClubPenguin.SledRacer
{
	public class BoostJetpack : Boost, IBoost
	{
		private float altitude;

		private float duration;

		private float warnTime;

		private float velocity;

		private float activeTime;

		private GameObject effectInstance;

		private Animator effectAnimator;

		private ConfigController config;

		private Vector3 flightAngles;

		private Quaternion normalsOffsetAngle;

		private Quaternion targetFlightAngle;

		public BoostJetpack(PlayerController _player, GameObject _prefab, float _time, float _speed, float _altitude)
			: base(_player)
		{
			config = Service.Get<ConfigController>();
			myPhase = BoostType.Start;
			duration = _time;
			velocity = _speed;
			altitude = _altitude;
			warnTime = duration - config.BoostJetpackWarningDuration;
			if (warnTime < 0f)
			{
				warnTime = 0f;
			}
			if (_prefab != null)
			{
				effectInstance = _player.CreatePlayerElement(_prefab);
				effectInstance.SetActive(value: false);
				effectAnimator = effectInstance.GetComponent<Animator>();
			}
		}

		public override void Execute()
		{
			DevTrace("BoostInvulnerable Execute");
			player.ChangeActionState(PlayerController.PlayerActionState.Boosting);
			active = true;
			ending = false;
			activeTime = 0f;
			player.TriggerAnimation("RiderJetpackON");
			Service.Get<IAudio>().SFX.Play(SFXEvent.SFX_Boost_Jetpack);
			if (effectInstance != null)
			{
				effectInstance.SetActive(value: true);
				effectAnimator.SetTrigger("RiderJetpackON");
			}
		}

		public override Vector3 FixedUpdate()
		{
			Vector3 appliedForces = player.AppliedForces;
			if (active && player.currentLifeState != PlayerController.PlayerLifeState.Crashed)
			{
				player.AddBoost(velocity);
			}
			return appliedForces;
		}

		public override void Update()
		{
			if (!active)
			{
				return;
			}
			activeTime += Time.deltaTime;
			DevTrace("Time Remaining: " + (duration - activeTime));
			Vector3 to = player.SurfaceRay.point + Vector3.up * altitude;
			AlignToterrain();
			player.transform.position = Vector3.Lerp(player.transform.position, to, config.BoostJetpackFollowSoftness * activeTime / (duration / 10f));
			if (!ending && activeTime > warnTime && activeTime < duration)
			{
				ending = true;
			}
			if (activeTime > duration && duration != 0f)
			{
				DevTrace("BoostJetpack Deactivate runTime=" + activeTime + ", Duration=" + duration);
				active = false;
				used = true;
				ending = false;
				activeTime = 0f;
				player.RiderAnimator.ResetTrigger("RiderBoost");
				player.TriggerAnimation("RiderJetpackOFF");
				Service.Get<IAudio>().SFX.Stop(SFXEvent.SFX_Boost_Jetpack);
				if (effectInstance != null)
				{
					effectAnimator.SetTrigger("RiderJetpackOFF");
				}
				player.ChangeActionState(PlayerController.PlayerActionState.Idle);
			}
			else if (player.currentLifeState == PlayerController.PlayerLifeState.Crashed)
			{
				Service.Get<IAudio>().SFX.Stop(SFXEvent.SFX_Boost_Jetpack);
				effectAnimator.SetTrigger("RiderCrash");
				active = false;
				used = true;
				ending = false;
				activeTime = 0f;
			}
		}

		public override void Abort()
		{
			DevTrace("Aborting Jetpack!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
			player.RiderAnimator.ResetTrigger("RiderBoost");
			player.ResetAnimationTrigger("RiderJetpackOFF");
			player.ResetAnimationTrigger("RiderJetpackON");
			if (effectInstance != null)
			{
				effectAnimator.ResetTrigger("RiderJetpackOFF");
				effectAnimator.ResetTrigger("RiderJetpackON");
			}
			used = true;
			activeTime = 0f;
			active = false;
			ending = false;
			UnityEngine.Object.Destroy(effectInstance);
		}

		private void AlignToterrain()
		{
			if (player.SurfaceRay.distance <= 100f)
			{
				flightAngles = player.SurfaceRay.normal;
				flightAngles = player.LsideRay.normal + player.RsideRay.normal + player.UpcomingRay.normal + player.TrailingRay.normal;
			}
			targetFlightAngle = Quaternion.FromToRotation(Vector3.up, flightAngles);
			normalsOffsetAngle = Quaternion.Lerp(normalsOffsetAngle, targetFlightAngle, 10f * Time.deltaTime);
			Quaternion rotation = player.PlayerScaleObject.transform.rotation;
			player.PlayerScaleObject.transform.LookAt(player.transform.position + player.GetComponent<Rigidbody>().linearVelocity);
			Quaternion rotation2 = player.PlayerScaleObject.transform.rotation;
			rotation2.z = normalsOffsetAngle.z;
			if (normalsOffsetAngle.x < 0f && player.currentLifeState != PlayerController.PlayerLifeState.Invincible)
			{
				rotation2.x = normalsOffsetAngle.x * 2f;
			}
			else
			{
				rotation2.x = normalsOffsetAngle.x;
			}
			player.PlayerScaleObject.transform.rotation = rotation;
			player.PlayerScaleObject.transform.rotation = Quaternion.Lerp(rotation, rotation2, 20f * Time.deltaTime);
		}

		public override void DrawGizmos()
		{
		}
	}
}
