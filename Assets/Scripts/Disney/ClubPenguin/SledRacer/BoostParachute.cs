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

		public BoostParachute(PlayerController _player, float _elevation, Vector3 _drag, Vector3 _velocityLimit)
			: base(_player)
		{
			myPhase = BoostType.Always;
			elevation = _elevation;
			parachuteDrag = _drag;
			parachuteDeployVelocity = _velocityLimit;
			if (effectInstance != null)
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
			Vector3 velocity = player.GetComponent<Rigidbody>().linearVelocity;
			if (hasExecuted && player.currentLifeState != PlayerController.PlayerLifeState.Crashed)
			{
				if (player.currentMoveState != 0)
				{
					if (active)
					{
						DevTrace("BoostParachute GLIDING");
						result = Stall();
					}
					else
					{
						Vector3 velocity2 = player.GetComponent<Rigidbody>().linearVelocity;
						if (velocity2.y > 0f && !checkParachute)
						{
							DevTrace("BoostParachute GONNA CHECK '" + player.currentMoveState + "'");
							checkParachute = true;
						}
						else
						{
							Vector3 velocity3 = player.GetComponent<Rigidbody>().linearVelocity;
							if (velocity3.y < 0f && checkParachute)
							{
								DevTrace("BoostParachute CHECKING ");
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
									player.GetComponent<Rigidbody>().linearVelocity = velocity;
								}
							}
						}
					}
				}
				else if (active)
				{
					Service.Get<IAudio>().SFX.Stop(SFXEvent.SFX_Boost_Parachute);
					DevTrace("BoostParachute STOWED");
					player.GetComponent<Rigidbody>().useGravity = true;
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
			player.GetComponent<Rigidbody>().useGravity = false;
			Vector3 appliedForces = player.AppliedForces;
			DevTrace("Parachute Forces WERE " + appliedForces.ToString());
			appliedForces = Vector3.Scale(appliedForces, parachuteDrag);
			DevTrace("Parachute Forces ARE " + appliedForces.ToString());
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
