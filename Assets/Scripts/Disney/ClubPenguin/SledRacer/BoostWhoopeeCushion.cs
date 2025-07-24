using System;
using UnityEngine;

namespace Disney.ClubPenguin.SledRacer
{
	public class BoostWhoopeeCushion : Boost, IBoost
	{
		private ConfigController config;

		public GameObject EffectPrefab;

		private float blastAngle;

		private float blastPower;

		private GameObject effectInstance;

		private Animator effectAnimator;

		private bool canUse = true;

		private float usedDisplayDelay;

		private GameObject camTarget;

		private Vector3 camOrigin;

		public BoostWhoopeeCushion(PlayerController _player, GameObject _prefab, float _blastAngle, float _blastPower)
			: base(_player)
		{
			myPhase = BoostType.Death;
			blastAngle = _blastAngle;
			blastPower = _blastPower;
			config = Service.Get<ConfigController>();
			usedDisplayDelay = config.BoostWhoopieCushionUsedDisplayTime;
			if (_prefab != null)
			{
				effectInstance = _player.CreatePlayerElement(_prefab);
				effectInstance.SetActive(value: false);
				effectAnimator = effectInstance.GetComponent<Animator>();
				camTarget = player.CameraTarget;
				camOrigin = camTarget.transform.position;
			}
		}

		public override void Trigger()
		{
			DevTrace("BoostWhoopieCushion Trigger");
			if (!canUse)
			{
				DevTrace("Already Used");
				if (effectInstance != null)
				{
					effectInstance.SetActive(value: false);
				}
				return;
			}
			active = true;
			if (canUse)
			{
				player.disableUserInput();
				CollisionObject.DispatchReset();
				effectInstance.SetActive(value: true);
				Service.Get<IAudio>().SFX.Play(SFXEvent.SFX_Boost_WhoopeeCushion);
				Service.Get<IAudio>().Music.Play(MusicTrack.WhoopeeCushion);
				player.TriggerAnimation("RiderWhoopee");
				canUse = false;
			}
		}

		public override void Complete()
		{
			Blastoff();
		}

		private void Blastoff()
		{
			player.TriggerAnimation("RiderWhoopeeLaunch");
			effectInstance.GetComponent<SphereCollider>().enabled = false;
			Vector3 a = new Vector3(0f, Mathf.Sin((float)Math.PI / 180f * blastAngle), Mathf.Cos((float)Math.PI / 180f * blastAngle));
			a.Normalize();
			a *= blastPower;
			Vector3 position = player.transform.position;
			position += Vector3.up * config.BoostWhoopieCushionAnimBounceHeight;
			player.transform.position = position;
			player.GetComponent<Rigidbody>().linearVelocity = a;
			player.AddBoost(a.z);
			player.enableUserInput();
		}

		public override Vector3 FixedUpdate()
		{
			if (active)
			{
				Vector3 position = camOrigin;
				float @float = player.RiderAnimator.GetFloat("AnimY");
				DevTrace("pos : " + position.ToString() + ";   aY : " + @float.ToString());
				position.y += @float;
				camTarget.transform.position = position;
			}
			return player.AppliedForces;
		}

		public override void Update()
		{
			if (active)
			{
				usedDisplayDelay -= Time.deltaTime;
				if (usedDisplayDelay <= 0f)
				{
					CollisionObject.DispatchPlayerCrashed(null);
					active = false;
					used = true;
				}
			}
		}

		public override void Abort()
		{
			if (active)
			{
				camTarget.transform.position = camOrigin;
				effectAnimator.CrossFade("Idle", 0f);
				effectAnimator.ResetTrigger("RiderWhoopee");
				effectInstance.SetActive(value: false);
				player.ResetAnimationTrigger("RiderWhoopee");
				UnityEngine.Object.Destroy(effectInstance);
			}
		}
	}
}
