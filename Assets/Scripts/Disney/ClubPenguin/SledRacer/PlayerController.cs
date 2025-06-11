using System;
using System.Collections.Generic;
using Disney.ClubPenguin.Service.MWS.Domain;
using Disney.Utility;
using Fabric;
using UnityEngine;

namespace Disney.ClubPenguin.SledRacer
{
	public class PlayerController : MonoBehaviour
	{
		public enum PlayerMoveState
		{
			OnGround,
			InAir,
			Other
		}

		public enum PlayerActionState
		{
			Idle,
			Jumping,
			Landing,
			LandingHard,
			Boosting,
			Crashing
		}

		public enum PlayerLifeState
		{
			Alive,
			Invincible,
			Crashed,
			Done
		}

		private enum PlayerCarveTurn
		{
			None,
			Left,
			Right
		}

		public Material SkiwaxMaterial;

		public Material RiderGoldHatMaterial;

		private RaycastHit hitDownVector;

		private RaycastHit hitUpcomingVector;

		private RaycastHit hitTrailingVector;

		private RaycastHit hitLeftVector;

		private RaycastHit hitRightVector;

		public CameraBaseRig playerCameraMount;

		public CameraBaseRig playerCameraRig;

		private List<IBoost> EquippedBoosts = new List<IBoost>();

		private PlayerCarveTurn lastCarve;

		public PlayerMoveState currentMoveState;

		public PlayerActionState currentActionState;

		public PlayerLifeState currentLifeState;

		public PlayerToSurface MatchSurfaceScript;

		public Axis HorizontalAxis;

		public Vector3 StartPosition = new Vector3(0f, 0f, 0f);

		public GameObject PlayerCameraObject;

		public GameObject PlayerScaleObject;

		private Camera MainCamera;

		private Camera PlayerCamera;

		public GameObject CameraTarget;

		public AbstractInputBehaviour inputBehaviour;

		protected GameObject CurrentRider;

		public GameObject SelectedRider;

		public GameObject DefaultRider;

		public Animator RiderAnimator;

		public GameObject CurrentSled;

		protected SledDefinition CurrentSledDef;

		public GameObject SelectedSled;

		public GameObject DefaultSled;

		public GameObject SledCollider;

		protected Animator SledAnimator;

		public GameObject JetpackPrefab;

		public GameObject WhoopeeCushionPrefab;

		public GameObject InvulnerabilityObject;

		public float BounceHeight = 10f;

		public PlayerStateObject InvulnerabilityStateObject;

		public PlayerStateObject OnGroundStateObject;

		public PlayerStateObjectTutorial tutorialStateObject;

		private bool TutorialMode;

		protected Vector3 ControlVector = new Vector3(0f, 0f, 0f);

		private int GroundLayerID;

		private int IceLayerID;

		private int CollisionLayerID;

		public bool controlsEnabled = true;

		private bool turningRight;

		private bool turningLeft;

		public IGameEventLogger gameLogger;

		private Vector3 PrevForces = Vector3.zero;

		private PlayerDataService playerDataService;

		private ConfigController config;

		public bool playerTurning;

		private bool playedTurnSFX;

		private IAudio audioService;

		private bool canPlayCatchAirSfx = true;

		private bool canPlayAirTimeSfx = true;

		private bool canPlaySledHangSfx = true;

		private float timeInAir;

		private Vector2 WindMagnitude = Vector2.zero;

		private GroupComponent WindBuffetLoop;

		private BIGameObjectType? impactedObjectType;

		private bool userInputEnabled;

		private bool actionPressed;

		public RaycastHit SurfaceRay
		{
			get
			{
				return hitDownVector;
			}
		}

		public RaycastHit UpcomingRay
		{
			get
			{
				return hitUpcomingVector;
			}
		}

		public RaycastHit TrailingRay
		{
			get
			{
				return hitTrailingVector;
			}
		}

		public RaycastHit LsideRay
		{
			get
			{
				return hitLeftVector;
			}
		}

		public RaycastHit RsideRay
		{
			get
			{
				return hitRightVector;
			}
		}

		public bool GameEnded
		{
			get
			{
				return currentLifeState == PlayerLifeState.Done;
			}
		}

		public Vector3 AppliedForces
		{
			get
			{
				return PrevForces;
			}
		}

		public Vector3 CurrentVelocity
		{
			get
			{
				return base.GetComponent<Rigidbody>().GetPointVelocity(base.transform.position);
			}
		}

		public static event EventHandler<GameEvent> OnGameEvent;

		public event Action OnPlayerCollidesWithIcePatch;

		public event Action OnPlayerIcePatchBoostingComplete;

		private void Awake()
		{
			config = Service.Get<ConfigController>();
			audioService = Service.Get<IAudio>();
			base.enabled = false;
			SetupPlayer();
		}

		private void SetupPlayer()
		{
			playerDataService = Service.Get<PlayerDataService>();
			base.GetComponent<Rigidbody>().useGravity = false;
			base.GetComponent<Rigidbody>().mass = config.PlayerPhysicsMass;
			MainCamera = Camera.main;
			PlayerCamera = PlayerCameraObject.GetComponent<Camera>();
			GroundLayerID = LayerMask.NameToLayer("SledTrack");
			CollisionLayerID = LayerMask.NameToLayer("Colliders");
			IceLayerID = LayerMask.NameToLayer("Ice");
			AddStateObjectListeners();
		}

		private void EquipeBoosts()
		{
			BoostManager boostManager = Service.Get<BoostManager>();
			foreach (BoostManager.AvailableBoosts equipedBoost in boostManager.EquipedBoosts)
			{
				IBoost boost = null;
				switch (equipedBoost)
				{
				case BoostManager.AvailableBoosts.INVULNERABLE:
					boost = new BoostInvulnerable(this, InvulnerabilityObject, config.BoostInvulnerableDuration);
					break;
				case BoostManager.AvailableBoosts.JETPACK:
					boost = new BoostJetpack(this, JetpackPrefab, config.BoostJetpackDuration, config.BoostJetpackSpeed, config.BoostJetpackAltitude);
					break;
				case BoostManager.AvailableBoosts.PARACHUTE:
					boost = new BoostParachute(this, config.BoostParachuteOpenAltiutude, config.BoostParachuteDrag, config.BoostParachuteDeployVelocity);
					break;
				case BoostManager.AvailableBoosts.REVIVE:
					boost = new BoostRevive(this);
					break;
				case BoostManager.AvailableBoosts.SKIWAX:
					boost = new BoostSkiwax(this, config.BoostSkiwaxVelocityBonus, SkiwaxMaterial);
					break;
				case BoostManager.AvailableBoosts.WHOOPIECUSHION:
					boost = new BoostWhoopeeCushion(this, WhoopeeCushionPrefab, config.BoostWhoopieCushionBlastAngle, config.BoostWhoopieCushionBlastPower);
					break;
				}
				if (boost != null)
				{
					EquippedBoosts.Add(boost);
					boostManager.AddBoostObject(equipedBoost, boost);
				}
			}
		}

		private void AddStateObjectListeners()
		{
			InvulnerabilityStateObject.OnExitStateEvent += OnInvulnerabilityComplete;
		}

		public void ResetStart()
		{
			ChangeStateToStart();
		}

		public void SetSled(GameObject _sled)
		{
			SledDefinition component = _sled.GetComponent<SledDefinition>();
			if (CurrentSled != null)
			{
				UnityEngine.Object.Destroy(CurrentSled);
			}
			CurrentSled = CreatePlayerElement(_sled);
			CurrentSledDef = component;
			SledCollider.GetComponent<Collider>().material = component.SledMaterial;
			SledAnimator = CurrentSled.GetComponent<Animator>();
			ResetAnimationVariables(SledAnimator);
			CurrentSled.SetActive(true);
		}

		private void SetRider(GameObject _rider)
		{
			if (CurrentRider != null)
			{
				UnityEngine.Object.Destroy(CurrentRider);
			}
			CurrentRider = CreatePlayerElement(_rider);
			RiderAnimator = CurrentRider.GetComponent<Animator>();
			SkinnedMeshRenderer componentInChildren = CurrentRider.GetComponentInChildren<SkinnedMeshRenderer>();
			if (componentInChildren.material != null)
			{
			}
			Material[] materials = componentInChildren.materials;

// Set base rider material for color
Material material = AvatarUtil.GetRiderMaterial(playerDataService.PlayerData.Account.Colour);

// Apply same material to first two slots
materials[0] = material;
materials[1] = material;

// Always apply gold hat material to the third slot
materials[2] = RiderGoldHatMaterial;

// Assign updated materials back
componentInChildren.materials = materials;

// Reset animation and show rider
ResetAnimationVariables(RiderAnimator);
CurrentRider.SetActive(true);

		}

		public GameObject CreatePlayerElement(GameObject _prefab)
		{
			GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(_prefab, base.transform.position, base.transform.rotation);
			gameObject.transform.localScale = PlayerScaleObject.transform.localScale;
			gameObject.transform.position = PlayerScaleObject.transform.position;
			gameObject.transform.parent = PlayerScaleObject.transform;
			return gameObject;
		}

		private void ResetAnimationVariables(Animator _targetAnimator)
		{
			_targetAnimator.SetFloat("RiderLean", 0f);
			_targetAnimator.SetBool("RiderOnGround", true);
			_targetAnimator.SetFloat("RiderVelocityZ", 0f);
			_targetAnimator.ResetTrigger("RiderLanding");
			_targetAnimator.ResetTrigger("RiderJump");
			_targetAnimator.ResetTrigger("RiderCrash");
		}

		private void RemoveSled()
		{
			if (CurrentSled != null)
			{
				CurrentSled.SetActive(false);
			}
		}

		private void RemoveRider()
		{
			if (CurrentRider != null)
			{
				CurrentRider.SetActive(false);
			}
		}

		private void FixedUpdate()
		{
			if (currentLifeState == PlayerLifeState.Done)
			{
				return;
			}
			Vector3 vector = Vector3.zero;
			if (controlsEnabled)
			{
				vector = getInput();
			}
			PrevForces = Vector3.zero;
			Vector3 vector2 = ((currentMoveState != 0) ? config.DefaultSledAerialProperties : config.DefaultSledGroundProperties);
			float num = 1f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			float num5 = 0f;
			float num6 = 0f;
			num3 = config.PlayerDownwardForce;
			num4 = config.PlayerForwardForce;
			if (currentLifeState == PlayerLifeState.Alive || currentLifeState == PlayerLifeState.Invincible)
			{
				num2 = 0f;
				if (vector.y > 0f && currentMoveState == PlayerMoveState.OnGround)
				{
					actionPressed = true;
					ChangeStateToJump();
					num2 += config.PlayerJumpStrength;
					vector.y = 0f;
				}
				else if (vector.y <= 0f)
				{
					actionPressed = false;
					vector.y = 0f;
				}
				else if (config.QuickDropMode && !actionPressed)
				{
					num += config.QuickDropStrength;
				}
				RiderAnimator.SetFloat("RiderLean", vector.x);
				SledAnimator.SetFloat("RiderLean", vector.x);
				num2 *= vector2.y;
				num3 *= vector2.y;
				num4 *= vector2.z;
				num6 = config.PlayerSteerForce;
				num6 *= vector.x;
				num6 *= vector2.x;
				num6 += ((vector.x == 0f) ? 0f : (vector.x / Mathf.Abs(vector.x))) * base.GetComponent<Rigidbody>().velocity.z / 200f * config.PlayerSteerForce;
			}
			else if (currentLifeState == PlayerLifeState.Crashed)
			{
				num4 = 0f;
				num6 = config.PlayerSteerForce;
				num6 *= vector.x;
				float @float = RiderAnimator.GetFloat("BarrelValue");
				SledAnimator.SetFloat("BarrelValue", @float);
			}
			PrevForces += Vector3.up * num2;
			PrevForces += Vector3.down * num3;
			PrevForces += Vector3.forward * num4;
			PrevForces += Vector3.back * num5;
			PrevForces += Vector3.right * num6;
			Vector3 velocity = base.GetComponent<Rigidbody>().velocity;
			velocity.z = ((currentActionState != PlayerActionState.Boosting) ? Mathf.Min(config.DefaultSledMaxVelocity, base.GetComponent<Rigidbody>().velocity.z) : base.GetComponent<Rigidbody>().velocity.z);
			velocity.z = ((currentLifeState == PlayerLifeState.Crashed || currentLifeState == PlayerLifeState.Done) ? velocity.z : Mathf.Max(base.GetComponent<Rigidbody>().velocity.z, config.PlayerMinimumForwardVelocity));
			base.GetComponent<Rigidbody>().velocity = velocity;
			PrevForces = ResolveBoosts();
			PrevForces.y *= num;
			base.GetComponent<Rigidbody>().AddForce(PrevForces);
			if (base.GetComponent<Rigidbody>().velocity.magnitude >= config.SfxSledHangMinVelocity && canPlaySledHangSfx && currentLifeState != PlayerLifeState.Crashed)
			{
				canPlaySledHangSfx = false;
				if (UnityEngine.Random.value > config.SfxSledHangChance)
				{
					audioService.SFX.Play(SFXEvent.SFX_SledHang);
				}
			}
			else if (base.GetComponent<Rigidbody>().velocity.magnitude < config.SfxSledHangMinVelocity)
			{
				canPlaySledHangSfx = true;
			}
			RiderAnimator.SetFloat("RiderVelocityZ", base.GetComponent<Rigidbody>().velocity.z);
			SledAnimator.SetFloat("RiderVelocityZ", base.GetComponent<Rigidbody>().velocity.z);
			RiderAnimator.SetFloat("RiderVelocityY", base.GetComponent<Rigidbody>().velocity.y);
			SledAnimator.SetFloat("RiderVelocityY", base.GetComponent<Rigidbody>().velocity.y);
		}

		private void OnHardLandStart(AnimationEvent other)
		{
			audioService.SFX.Play(SFXEvent.SFX_LandHardSpin);
		}

		private void OnLandNormalStart(AnimationEvent other)
		{
		}

		private void OnTumbleEnd(AnimationEvent other)
		{
			disableUserInput();
			IBoost boost = null;
			foreach (IBoost equippedBoost in EquippedBoosts)
			{
				if (equippedBoost.GetType() == typeof(BoostRevive) && !equippedBoost.Used && !equippedBoost.Active)
				{
					boost = equippedBoost;
				}
				if (equippedBoost.GetType() == typeof(BoostWhoopeeCushion) && boost == null && !equippedBoost.Used && !equippedBoost.Active)
				{
					boost = equippedBoost;
				}
			}
			if (boost != null)
			{
				if (currentMoveState != 0)
				{
					ChangeMoveState(PlayerMoveState.OnGround);
				}
				boost.Trigger();
			}
			else
			{
				ChangeStateToDone();
			}
		}

		private void OnWhoopeeLaunch(AnimationEvent other)
		{
			foreach (IBoost equippedBoost in EquippedBoosts)
			{
				if (equippedBoost.GetType() == typeof(BoostWhoopeeCushion))
				{
					equippedBoost.Complete();
				}
			}
		}

		private Vector3 AbortBoosts()
		{
			foreach (IBoost equippedBoost in EquippedBoosts)
			{
				equippedBoost.Abort();
			}
			return PrevForces;
		}

		private Vector3 ResolveBoosts()
		{
			foreach (IBoost equippedBoost in EquippedBoosts)
			{
				PrevForces = equippedBoost.FixedUpdate();
			}
			return PrevForces;
		}

		public void disableUserInput()
		{
			userInputEnabled = false;
		}

		public void enableUserInput()
		{
			userInputEnabled = true;
		}

		public Vector3 getInput()
		{
			Vector3 zero = Vector3.zero;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			playerTurning = false;
			if (userInputEnabled)
			{
				if (TutorialMode)
				{
					flag = tutorialStateObject.jump();
					flag3 = tutorialStateObject.left();
					flag2 = tutorialStateObject.right();
				}
				else
				{
					flag = inputBehaviour.jump();
					flag3 = inputBehaviour.left();
					flag2 = inputBehaviour.right();
				}
			}
			if (flag)
			{
				zero.y = 1f;
			}
			if (turningRight && !flag2)
			{
				DispatchGameEvent(new GameEvent(GameEvent.Type.RightEnd));
				gameLogger.log(Disney.ClubPenguin.Service.MWS.Domain.Event.RIGHT_END);
				turningRight = false;
			}
			if (turningLeft && !flag3)
			{
				DispatchGameEvent(new GameEvent(GameEvent.Type.LeftEnd));
				gameLogger.log(Disney.ClubPenguin.Service.MWS.Domain.Event.LEFT_END);
				turningLeft = false;
			}
			if (flag2)
			{
				if (!turningRight)
				{
					DispatchGameEvent(new GameEvent(GameEvent.Type.RightStart));
					gameLogger.log(Disney.ClubPenguin.Service.MWS.Domain.Event.RIGHT_START);
					turningRight = true;
				}
				zero.x = HorizontalAxis.GetValue(1);
			}
			if (flag3)
			{
				if (!turningLeft)
				{
					DispatchGameEvent(new GameEvent(GameEvent.Type.LeftStart));
					gameLogger.log(Disney.ClubPenguin.Service.MWS.Domain.Event.LEFT_START);
					turningLeft = true;
				}
				zero.x = HorizontalAxis.GetValue(-1);
			}
			if ((flag3 && flag2) || (!flag3 && !flag2))
			{
				zero.x = HorizontalAxis.GetValue(0);
				playerTurning = false;
				lastCarve = PlayerCarveTurn.None;
				if (flag3 && flag2)
				{
					DispatchGameEvent(new GameEvent(GameEvent.Type.Both));
				}
			}
			else
			{
				playerTurning = true;
			}
			return zero;
		}

		private float OnGroundTest()
		{
			Transform transform = base.transform;
			Physics.Raycast(transform.transform.position + Vector3.up, Vector3.down + Vector3.forward * 2f, out hitUpcomingVector, 1000f, LayerMask.GetMask("SledTrack"));
			Physics.Raycast(transform.transform.position + Vector3.up, Vector3.down + Vector3.back * 2f, out hitTrailingVector, 1000f, LayerMask.GetMask("SledTrack"));
			Physics.Raycast(transform.transform.position + Vector3.up, Vector3.down + Vector3.left * 2f, out hitLeftVector, 1000f, LayerMask.GetMask("SledTrack"));
			Physics.Raycast(transform.transform.position + Vector3.up, Vector3.down + Vector3.right * 2f, out hitRightVector, 1000f, LayerMask.GetMask("SledTrack"));
			if (Physics.Raycast(transform.transform.position + Vector3.up, Vector3.down, out hitDownVector, 1000f, LayerMask.GetMask("SledTrack")) && hitDownVector.distance < config.GroundedDistance)
			{
				RiderAnimator.SetBool("RiderOnGround", true);
				SledAnimator.SetBool("RiderOnGround", true);
				if (currentMoveState == PlayerMoveState.InAir)
				{
					ResetAnimationTrigger("RiderLanding");
					ResetAnimationTrigger("RiderLandingHard");
					float num = Math.Abs(base.GetComponent<Rigidbody>().velocity.y);
					string anim = "RiderLanding";
					if (currentLifeState == PlayerLifeState.Crashed)
					{
						audioService.SFX.Play(SFXEvent.SFX_PlayerImpactSnow);
					}
					else
					{
						if (num > config.ImpactGroundHardMinVelocity)
						{
							anim = "RiderLandingHard";
							if (base.GetComponent<Rigidbody>().velocity.y <= 100f)
							{
								audioService.SFX.Play(SFXEvent.SFX_SledImpactSnow);
							}
						}
						else if (num > config.ImpactGroundMinVelocity)
						{
							audioService.SFX.Play(SFXEvent.SFX_SledImpactSnow);
						}
						TriggerAnimation(anim);
						ChangeActionState(PlayerActionState.Landing);
					}
					canPlayCatchAirSfx = true;
					canPlayAirTimeSfx = true;
					timeInAir = 0f;
					if (currentLifeState == PlayerLifeState.Alive)
					{
						audioService.SFX.Play(SFXEvent.SFX_SledOnGround);
					}
				}
				ChangeMoveState(PlayerMoveState.OnGround);
			}
			else
			{
				RiderAnimator.SetBool("RiderOnGround", false);
				SledAnimator.SetBool("RiderOnGround", false);
				if (hitDownVector.distance > config.SfxCatchAirDistanceFromGround && canPlayCatchAirSfx)
				{
					canPlayCatchAirSfx = false;
					audioService.SFX.Play(SFXEvent.SFX_CatchAir);
				}
				ChangeStateToInAir();
			}
			return hitDownVector.distance;
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(UpcomingRay.point, SurfaceRay.point + Vector3.up);
			Gizmos.DrawLine(TrailingRay.point, SurfaceRay.point + Vector3.up);
			Gizmos.color = Color.red;
			Gizmos.DrawLine(LsideRay.point, SurfaceRay.point + Vector3.up);
			Gizmos.DrawLine(RsideRay.point, SurfaceRay.point + Vector3.up);
			Gizmos.color = Color.green;
			Gizmos.DrawLine(SurfaceRay.point, SurfaceRay.point + Vector3.up);
			Gizmos.DrawCube(SurfaceRay.point + Vector3.up, Vector3.one);
			foreach (IBoost equippedBoost in EquippedBoosts)
			{
				equippedBoost.DrawGizmos();
			}
		}

		private void OnCollisionEnter(Collision other)
		{
			GameObject gameObject = other.collider.gameObject;
			if (gameObject.layer == IceLayerID)
			{
				if (currentActionState == PlayerActionState.Boosting)
				{
					return;
				}
				audioService.SFX.Play(SFXEvent.SFX_IcePatchBoost);
				if (currentLifeState != PlayerLifeState.Crashed)
				{
					InvulnerabilityStateObject.EnterState();
					if (this.OnPlayerCollidesWithIcePatch != null)
					{
						this.OnPlayerCollidesWithIcePatch();
					}
				}
				if (currentLifeState != PlayerLifeState.Done)
				{
					AddBoost(config.IcePatchBoostStrength, true);
				}
			}
			else if (gameObject.layer == CollisionLayerID && currentLifeState != PlayerLifeState.Invincible)
			{
				ChangeStateToCrash(gameObject);
				PlayCollisionSfx(gameObject);
			}
		}

		private void OnCollisionExit(Collision other)
		{
		}

		private void OnTriggerEnter(Collider other)
		{
			GameObject gameObject = other.gameObject;
			IBoost boost = EquippedBoosts.Find((IBoost b) => b.GetType() == typeof(BoostInvulnerable));
			if (gameObject.layer == CollisionLayerID)
			{
				if (currentLifeState != PlayerLifeState.Invincible)
				{
					if (boost != null && !boost.Used)
					{
						boost.Trigger();
						audioService.SFX.Play(SFXEvent.SFX_Boost_InvincitubeImpact);
					}
					else
					{
						ChangeStateToCrash(gameObject);
						PlayCollisionSfx(gameObject);
						CollisionObject.DispatchPlayerCrashed(gameObject);
					}
				}
				else if (boost != null && !boost.Used && boost.Active)
				{
					audioService.SFX.Play(SFXEvent.SFX_Boost_InvincitubeImpact);
				}
			}
			else if (gameObject.layer == LayerMask.NameToLayer("BoundaryLimits"))
			{
				ChangeStateToDone();
			}
		}

		private void PlayCollisionSfx(GameObject other)
		{
			string collisionObjectName = getCollisionObjectName(other);
			if (collisionObjectName.StartsWith("anvil"))
			{
				audioService.SFX.Play(SFXEvent.SFX_PlayerImpactAnvil);
			}
			else if (collisionObjectName.StartsWith("tree") || collisionObjectName.StartsWith("log"))
			{
				audioService.SFX.Play(SFXEvent.SFX_PlayerImpactWood);
			}
			else
			{
				audioService.SFX.Play(SFXEvent.SFX_PlayerImpactObstacle);
			}
		}

		private string getCollisionObjectName(GameObject collisionObject)
		{
			return collisionObject.transform.parent.gameObject.name.ToLower();
		}

		private BIGameObjectType getCollisionObjectType(GameObject collisionObject)
		{
			string collisionObjectName = getCollisionObjectName(collisionObject);
			if (collisionObjectName.StartsWith("tree"))
			{
				while (collisionObject.transform.parent != null)
				{
					collisionObject = collisionObject.transform.parent.gameObject;
					if (collisionObject.name.ToLower().Equals("cluster of three"))
					{
						return BIGameObjectType.TREE_CLUMPS_3;
					}
					if (collisionObject.name.ToLower().Equals("cluster of five"))
					{
						return BIGameObjectType.TREE_CLUMPS_5;
					}
				}
				return BIGameObjectType.TREE_CLUMPS_1;
			}
			if (collisionObjectName.StartsWith("rock"))
			{
				while (collisionObject.transform.parent != null)
				{
					collisionObject = collisionObject.transform.parent.gameObject;
					if (collisionObject.name.ToLower().Equals("rock pile"))
					{
						return BIGameObjectType.ROCK_PILE;
					}
				}
				return BIGameObjectType.ROCK;
			}
			if (collisionObjectName.StartsWith("anvil"))
			{
				return BIGameObjectType.ANVIL;
			}
			if (collisionObjectName.StartsWith("log"))
			{
				return BIGameObjectType.LOG;
			}
			if (collisionObjectName.StartsWith("fluffy"))
			{
				while (collisionObject.transform.parent != null)
				{
					collisionObject = collisionObject.transform.parent.gameObject;
					if (collisionObject.name.ToLower().Contains("giant fluffy"))
					{
						return BIGameObjectType.GIANT_FLUFFY;
					}
				}
				return BIGameObjectType.FLUFFY;
			}
			if (collisionObjectName.StartsWith("cactus"))
			{
				return BIGameObjectType.CACTUS;
			}
			return BIGameObjectType.OTHER;
		}

		private void OnTriggerExit(Collider other)
		{
			if (other.gameObject.layer != CollisionLayerID)
			{
			}
		}

		public void ChangeStateToEnd()
		{
			BoostManager boostManager = Service.Get<BoostManager>();
			AbortBoosts();
			foreach (IBoost equippedBoost in EquippedBoosts)
			{
				equippedBoost.Destroy();
			}
			EquippedBoosts.Clear();
			boostManager.ClearBoostObjects();
			base.transform.position = StartPosition;
			PlayerCamera.enabled = false;
			MainCamera.enabled = true;
			base.GetComponent<Rigidbody>().useGravity = false;
			base.GetComponent<Rigidbody>().isKinematic = true;
			base.enabled = false;
			RemoveSled();
			RemoveRider();
			ChangeActionState(PlayerActionState.Idle);
			ChangeLifeState(PlayerLifeState.Done);
			ChangeMoveState(PlayerMoveState.Other);
			InvulnerabilityStateObject.AbortState();
			OnGroundStateObject.AbortState();
			if ((bool)tutorialStateObject)
			{
				tutorialStateObject.AbortState();
			}
			MatchSurfaceScript.Reset();
		}

		private void ChangeStateToStart()
		{
			WindBuffetLoop = FabricManager.Instance.GetComponentByName("Audio_All_SFX_Race_WindBuffetLoop") as GroupComponent;
			audioService.SFX.Play(SFXEvent.SFX_WindBuffet);
			CollisionObject.DispatchReset();
			base.transform.position = StartPosition;
			MainCamera.enabled = false;
			PlayerCamera.enabled = true;
			base.GetComponent<Rigidbody>().useGravity = true;
			base.GetComponent<Rigidbody>().isKinematic = false;
			base.GetComponent<Rigidbody>().velocity = Vector3.zero;
			base.enabled = true;
			impactedObjectType = null;
			SelectedSled = ((!(SelectedSled == null)) ? SelectedSled : DefaultSled);
			SelectedRider = ((!(SelectedRider == null)) ? SelectedRider : DefaultRider);
			MatchSurfaceScript.Reset();
			SetSled(SelectedSled);
			SetRider(SelectedRider);
			ChangeActionState(PlayerActionState.Idle);
			ChangeLifeState(PlayerLifeState.Alive);
			ChangeMoveState(PlayerMoveState.Other);
			enableUserInput();
			audioService.SFX.Play(SFXEvent.UI_PlayGame);
			EquipeBoosts();
			playerCameraMount.remountMode = CameraBaseRig.CameraMountMode.Snap;
			playerCameraRig.changeBasePlate(0);
			TutorialMode = SledRacerGameManager.Instance.CurrentGameState == SledRacerGameManager.GameState.GameTutorial;
			if (TutorialMode)
			{
				tutorialStateObject = base.gameObject.AddComponent<PlayerStateObjectTutorial>();
				gameLogger.SuspendGameTimer();
				tutorialStateObject.OnExitStateEvent += OnTutorialComplete;
				tutorialStateObject.EnterState();
			}
			else
			{
				ActivateBoosts();
			}
		}

		private void ActivateBoosts()
		{
			foreach (IBoost equippedBoost in EquippedBoosts)
			{
				if (equippedBoost.BoostPhase == BoostType.Start || equippedBoost.BoostPhase == BoostType.Always)
				{
					equippedBoost.Execute();
				}
			}
		}

		public void ChangeStateToRevive()
		{
			CollisionObject.DispatchReset();
			base.GetComponent<Rigidbody>().useGravity = true;
			base.GetComponent<Rigidbody>().isKinematic = false;
			base.GetComponent<Rigidbody>().velocity = Vector3.zero;
			base.enabled = true;
			impactedObjectType = null;
			ResetAnimationVariables(RiderAnimator);
			ResetAnimationVariables(SledAnimator);
			TriggerAnimation("RiderRevive");
			playerCameraMount.remountMode = CameraBaseRig.CameraMountMode.Ease;
			playerCameraRig.changeBasePlate(0);
			enableUserInput();
			ChangeActionState(PlayerActionState.Idle);
			ChangeMoveState(PlayerMoveState.Other);
		}

		private void ChangeStateToJump()
		{
			gameLogger.log(Disney.ClubPenguin.Service.MWS.Domain.Event.JUMP);
			ChangeActionState(PlayerActionState.Jumping);
			audioService.SFX.Play(SFXEvent.SFX_PlayerJump);
			TriggerAnimation("RiderJump");
		}

		private void ChangeStateToInAir()
		{
			if (currentMoveState != PlayerMoveState.InAir)
			{
				audioService.SFX.Stop(SFXEvent.SFX_SledOnGround);
			}
			ChangeMoveState(PlayerMoveState.InAir);
			ChangeActionState(PlayerActionState.Idle);
		}

		private void ChangeStateToCrash(GameObject collisionObject)
		{
			BIGameObjectType? bIGameObjectType = impactedObjectType;
			if (!bIGameObjectType.HasValue)
			{
				impactedObjectType = getCollisionObjectType(collisionObject);
			}
			ChangeActionState(PlayerActionState.Crashing);
			ChangeLifeState(PlayerLifeState.Crashed);
			TriggerAnimation("RiderCrash");
			playerCameraMount.remountMode = CameraBaseRig.CameraMountMode.Ease;
			playerCameraRig.changeBasePlate(1);
			if (audioService.Music.CurrentTrack != MusicTrack.Wipeout)
			{
				audioService.Music.Play(MusicTrack.Wipeout);
			}
			audioService.SFX.Stop(SFXEvent.SFX_SledOnGround);
		}

		public void ChangeStateToDone()
		{
			gameLogger.log(Disney.ClubPenguin.Service.MWS.Domain.Event.END);
			ChangeActionState(PlayerActionState.Idle);
			ChangeLifeState(PlayerLifeState.Done);
			base.GetComponent<Rigidbody>().isKinematic = true;
			audioService.Music.Stop();
			audioService.SFX.Play(SFXEvent.SFX_PlayerFail);
			IBILogging iBILogging = Service.Get<IBILogging>();
			BIGameObjectType? bIGameObjectType = impactedObjectType;
			int killedBy;
			if (!bIGameObjectType.HasValue)
			{
				killedBy = 10;
			}
			else
			{
				BIGameObjectType? bIGameObjectType2 = impactedObjectType;
				killedBy = (int)bIGameObjectType2.Value;
			}
			iBILogging.EndGame((BIGameObjectType)killedBy, SledRacerGameManager.Instance.getCurrentScore());
			DispatchGameEvent(new GameEvent(GameEvent.Type.End));
		}

		public void ChangeActionState(PlayerActionState _targetState)
		{
			if (currentActionState != _targetState)
			{
				currentActionState = _targetState;
			}
		}

		public void ChangeMoveState(PlayerMoveState _targetState)
		{
			if (currentMoveState != _targetState)
			{
				currentMoveState = _targetState;
				if (_targetState == PlayerMoveState.OnGround)
				{
					audioService.SFX.Unpause(SFXEvent.SFX_PlayerRoll);
					OnGroundStateObject.EnterState();
				}
				else
				{
					audioService.SFX.Pause(SFXEvent.SFX_PlayerRoll);
					OnGroundStateObject.AbortState();
				}
			}
		}

		public void ChangeLifeState(PlayerLifeState _targetState)
		{
			if (currentLifeState != _targetState)
			{
				currentLifeState = _targetState;
			}
		}

		public void AddBoost(float _boostAmount = 200f, bool _invulnerable = false)
		{
			Vector3 velocity = base.GetComponent<Rigidbody>().velocity;
			velocity.z = _boostAmount;
			base.GetComponent<Rigidbody>().velocity = velocity;
			ChangeActionState(PlayerActionState.Boosting);
			TriggerAnimation("RiderBoost");
			if (currentLifeState == PlayerLifeState.Alive && _invulnerable)
			{
				ChangeLifeState(PlayerLifeState.Invincible);
			}
		}

		private void Reset()
		{
		}

		private void Pause()
		{
		}

		private void Update()
		{
			AdjustWindBuffetLoop();
			if (playerTurning && currentMoveState == PlayerMoveState.OnGround && currentLifeState != PlayerLifeState.Crashed && ((turningLeft && lastCarve != PlayerCarveTurn.Left) || (turningRight && lastCarve != PlayerCarveTurn.Right)))
			{
				audioService.SFX.Play(SFXEvent.SFX_SledTurn);
				lastCarve = (turningLeft ? PlayerCarveTurn.Left : PlayerCarveTurn.Right);
			}
			float num = OnGroundTest();
			if (currentMoveState == PlayerMoveState.InAir)
			{
				timeInAir += Time.deltaTime;
				if (timeInAir > config.SfxAirTimeDelaySeconds && canPlayAirTimeSfx && num > config.SfxAirTimeDistanceFromGround && currentLifeState != PlayerLifeState.Crashed)
				{
					canPlayAirTimeSfx = false;
					audioService.SFX.Play(SFXEvent.SFX_PlayerAirTime);
				}
			}
			foreach (IBoost equippedBoost in EquippedBoosts)
			{
				equippedBoost.Update();
			}
		}

		private void AdjustWindBuffetLoop()
		{
			WindMagnitude.x = base.GetComponent<Rigidbody>().velocity.magnitude / config.SfxVelocityForFullGust;
			WindMagnitude.y = (hitDownVector.distance - 1f) / config.SfxAltitudeForFullGust;
			WindBuffetLoop.Volume = WindMagnitude.magnitude;
		}

		private void OnInvulnerabilityComplete()
		{
			IBoost boost = EquippedBoosts.Find((IBoost b) => b.GetType() == typeof(BoostInvulnerable));
			if (currentLifeState == PlayerLifeState.Invincible && (boost == null || boost.Used || !boost.Active))
			{
				ChangeLifeState(PlayerLifeState.Alive);
			}
			ChangeActionState(PlayerActionState.Idle);
			if (this.OnPlayerIcePatchBoostingComplete != null)
			{
				this.OnPlayerIcePatchBoostingComplete();
			}
		}

		private void OnTutorialComplete()
		{
			tutorialStateObject.OnExitStateEvent -= OnTutorialComplete;
			string playerSwid = Service.Get<PlayerDataService>().PlayerData.Account.PlayerSwid;
			PlayerPrefs.SetInt("TutorialComplete:" + playerSwid, 1);
			PlayerPrefs.Save();
			gameLogger.UnsuspendGameTimer();
			SledRacerGameManager.Instance.ChangeGameState(SledRacerGameManager.GameState.GamePlay);
			TutorialMode = false;
			ActivateBoosts();
		}

		public virtual void DispatchGameEvent(GameEvent gameEvent)
		{
			if (PlayerController.OnGameEvent != null)
			{
				PlayerController.OnGameEvent(this, gameEvent);
			}
		}

		public void TriggerAnimation(string _anim)
		{
			RiderAnimator.SetTrigger(_anim);
			SledAnimator.SetTrigger(_anim);
		}

		public void ResetAnimationTrigger(string _anim)
		{
			RiderAnimator.ResetTrigger(_anim);
			SledAnimator.ResetTrigger(_anim);
		}

		public bool isIcePatchInvulnerable()
		{
			return InvulnerabilityStateObject.enabled;
		}

		private void DevTrace(string _msg)
		{
		}
	}
}
