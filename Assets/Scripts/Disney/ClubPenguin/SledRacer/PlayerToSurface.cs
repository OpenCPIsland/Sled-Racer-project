using UnityEngine;

namespace Disney.ClubPenguin.SledRacer
{
	public class PlayerToSurface : MonoBehaviour
	{
		private Vector3 groundAngles;

		private Quaternion normalsOffsetAngle;

		private Quaternion targetGroundAngle;

		private PlayerController player;

		private ConfigController config;

		private void Start()
		{
			config = Service.Get<ConfigController>();
			player = base.gameObject.GetComponent<PlayerController>();
			Reset();
		}

		private void Update()
		{
			if (player.SurfaceRay.distance <= config.PlayerSurfaceMatchingDistance)
			{
				groundAngles = player.SurfaceRay.normal;
				groundAngles += player.LsideRay.normal + player.RsideRay.normal + player.UpcomingRay.normal + player.TrailingRay.normal;
				groundAngles.Normalize();
			}
			if (player.currentMoveState == PlayerController.PlayerMoveState.OnGround || player.currentMoveState == PlayerController.PlayerMoveState.Other)
			{
				targetGroundAngle = Quaternion.FromToRotation(Vector3.up, groundAngles);
			}
			else
			{
				groundAngles = Vector3.zero;
				targetGroundAngle = Quaternion.Lerp(targetGroundAngle, Quaternion.identity, Time.deltaTime);
			}
			normalsOffsetAngle = Quaternion.Lerp(normalsOffsetAngle, targetGroundAngle, 10f * Time.deltaTime);
			Quaternion rotation = player.PlayerScaleObject.transform.rotation;
			Transform transform = player.PlayerScaleObject.transform;
			Vector3 position = base.transform.position;
			float x = position.x;
			Vector3 velocity = GetComponent<Rigidbody>().linearVelocity;
			float x2 = x + velocity.x;
			Vector3 position2 = base.transform.position;
			float y = position2.y;
			Vector3 velocity2 = GetComponent<Rigidbody>().linearVelocity;
			float y2 = y + velocity2.y;
			Vector3 position3 = base.transform.position;
			float z = position3.z;
			Vector3 velocity3 = GetComponent<Rigidbody>().linearVelocity;
			transform.LookAt(new Vector3(x2, y2, z + Mathf.Max(velocity3.z, 1f)));
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

		public void Reset()
		{
			normalsOffsetAngle = Quaternion.identity;
			if (player != null)
			{
				player.PlayerScaleObject.transform.rotation = Quaternion.identity;
			}
		}
	}
}
