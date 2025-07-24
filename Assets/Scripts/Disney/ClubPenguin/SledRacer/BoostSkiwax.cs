using UnityEngine;

namespace Disney.ClubPenguin.SledRacer
{
	public class BoostSkiwax : Boost, IBoost
	{
		public GameObject EffectPrefab;

		private Material SkiwaxMaterial;

		private float VelocityBonus;

		private GameObject effectInstance;

		private bool hasExecuted;

		private bool unappliedBoost;

		public BoostSkiwax(PlayerController _player, float _VelocityBonus, Material _SkiwaxMaterial)
			: base(_player)
		{
			myPhase = BoostType.Always;
			VelocityBonus = _VelocityBonus;
			SkiwaxMaterial = _SkiwaxMaterial;
			if (effectInstance != null)
			{
				effectInstance = (GameObject)Object.Instantiate(EffectPrefab);
				effectInstance.transform.parent = _player.transform;
				effectInstance.SetActive(value: false);
			}
		}

		public override void Execute()
		{
			DevTrace("BoostSkiwax Execute");
			hasExecuted = true;
			SkinnedMeshRenderer componentInChildren = player.CurrentSled.GetComponentInChildren<SkinnedMeshRenderer>();
			if (SkiwaxMaterial != null)
			{
				componentInChildren.material = SkiwaxMaterial;
			}
			if (effectInstance != null)
			{
				effectInstance.SetActive(value: true);
			}
			player.OnPlayerCollidesWithIcePatch += OnPlayerStateChangedToBoosting;
			player.OnPlayerIcePatchBoostingComplete += OnPlayerIcePatchBoostingComplete;
		}

		public override Vector3 FixedUpdate()
		{
			Vector3 appliedForces = player.AppliedForces;
			Vector3 velocity = player.GetComponent<Rigidbody>().linearVelocity;
			if (unappliedBoost)
			{
				unappliedBoost = false;
				velocity.z += VelocityBonus;
				player.GetComponent<Rigidbody>().linearVelocity = velocity;
				DevTrace("Applying Skiwax velocity boost (+" + VelocityBonus + " = " + velocity + ")");
			}
			return appliedForces;
		}

		public void OnPlayerStateChangedToBoosting()
		{
			active = true;
			unappliedBoost = true;
			Service.Get<IAudio>().SFX.Play(SFXEvent.SFX_Boost_Skiwax);
		}

		public void OnPlayerIcePatchBoostingComplete()
		{
			active = false;
		}

		public override void Abort()
		{
		}

		public override void Destroy()
		{
			if (hasExecuted)
			{
				player.OnPlayerCollidesWithIcePatch -= OnPlayerStateChangedToBoosting;
				player.OnPlayerIcePatchBoostingComplete -= OnPlayerIcePatchBoostingComplete;
			}
		}
	}
}
