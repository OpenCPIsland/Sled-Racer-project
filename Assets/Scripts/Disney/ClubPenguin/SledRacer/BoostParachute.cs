using UnityEngine;

namespace Disney.ClubPenguin.SledRacer
{
	public class BoostParachute : Boost, IBoost
	{
		public GameObject EffectPrefab;

		private float elevation;

		private Vector3 parachuteDrag;

		private Vector3 parachuteDeployVelocity;

		private GameObject effectInstance;

		private bool checkParachute;

		private bool hasExecuted;

		private Rigidbody playerRigidbody;

		private bool gravityDisabled;

		public BoostParachute(PlayerController _player, float _elevation, Vector3 _drag, Vector3 _velocityLimit)
			: base(_player)
		{
			myPhase = BoostType.Always;
			elevation = _elevation;
			parachuteDrag = _drag;
			parachuteDeployVelocity = _velocityLimit;
			playerRigidbody = _player.GetComponent<Rigidbody>();
			gravityDisabled = false;
			if (EffectPrefab != null)
			{
				effectInstance = (GameObject)Object.Instantiate(EffectPrefab);
				effectInstance.transform.parent = _player.transform;
				effectInstance.SetActive(value: false);
			}
		}

		public override void Execute()
		{
			DevTrace("BoostParachute Execute");
			active = false;
			hasExecuted = true;
			if (effectInstance != null)
			{
				effectInstance.SetActive(value: true);
			}
		}

		public override Vector3 FixedUpdate()
		{
			Vector3 result = player.AppliedForces;
			if (hasExecuted && player.currentLifeState != PlayerController.PlayerLifeState.Crashed)
			{
				if (player.currentMoveState != 0)
				{
					if (active)
					{
						result = Stall();
					}
					else
					{
						Vector3 velocity = playerRigidbody.linearVelocity;
						if (velocity.y > 0f && !checkParachute)
						{
							checkParachute = true;
						}
						else if (velocity.y < 0f && checkParachute)
						{
							checkParachute = false;
							active = AltitudeCheck();
							if (active)
							{
								DevTrace("BoostParachute DEPLOYED");
								Service.Get<IAudio>().SFX.Play(SFXEvent.SFX_Boost_Parachute);
								player.TriggerAnimation("RiderParachute");
								if (effectInstance != null)
								{
									effectInstance.SetActive(value: true);
								}
								velocity = Vector3.Scale(velocity, parachuteDeployVelocity);
								playerRigidbody.linearVelocity = velocity;
								gravityDisabled = true;
								playerRigidbody.useGravity = false;
							}
						}
					}
				}
				else if (active)
				{
					Service.Get<IAudio>().SFX.Stop(SFXEvent.SFX_Boost_Parachute);
					DevTrace("BoostParachute STOWED");
					playerRigidbody.useGravity = true;
					gravityDisabled = false;
					checkParachute = false;
					active = false;
					if (effectInstance != null)
					{
						effectInstance.SetActive(value: false);
					}
				}
				else
				{
					checkParachute = false;
				}
			}
			else if (active)
			{
				active = false;
				Service.Get<IAudio>().SFX.Stop(SFXEvent.SFX_Boost_Parachute);
			}
			return result;
		}

		private Vector3 Stall()
		{
			if (!gravityDisabled)
			{
				playerRigidbody.useGravity = false;
				gravityDisabled = true;
			}
			Vector3 appliedForces = player.AppliedForces;
			appliedForces = Vector3.Scale(appliedForces, parachuteDrag);
			return appliedForces;
		}

		private bool AltitudeCheck()
		{
			bool flag = false;
			return player.SurfaceRay.distance >= elevation;
		}

		public override void DrawGizmos()
		{
			if (hasExecuted && player != null)
			{
				Transform transform = player.transform;
				RaycastHit hitInfo = default(RaycastHit);
				if (Physics.Raycast(transform.transform.position + Vector3.up, Vector3.down, out hitInfo, 1000f, LayerMask.GetMask("SledTrack")))
				{
					Gizmos.color = Color.blue;
					Gizmos.DrawLine(transform.transform.position + Vector3.up, hitInfo.point);
					Gizmos.DrawSphere(hitInfo.point, 0.5f);
				}
				else
				{
					Gizmos.color = Color.red;
					Gizmos.DrawLine(transform.transform.position + Vector3.up, transform.transform.position + Vector3.up + Vector3.down * 1000f);
				}
			}
		}

		public override void Abort()
		{
		}
	}
}
