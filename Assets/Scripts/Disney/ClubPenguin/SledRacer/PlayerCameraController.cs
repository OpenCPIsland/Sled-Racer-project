using UnityEngine;

namespace Disney.ClubPenguin.SledRacer
{
	public class PlayerCameraController : MonoBehaviour
	{
		public Transform Player;

		public Transform CameraTarget;

		private Vector3 rotationVector;

		private float heightOffsetUp;

		private float heightOffsetDown;

		private float prevGoalY;

		private float tweenGoalY;

		private float tweenGoalY2;

		private PlayerController playerController;

		private float camYStart;

		private ConfigController config;

		private void Start()
		{
			config = Service.Get<ConfigController>();
			Vector3 position = Player.position;
			prevGoalY = position.y + config.CameraHeight;
			tweenGoalY2 = prevGoalY;
		}

		private void LateUpdate()
		{
			rotationVector.y = Player.eulerAngles.y;
			heightOffsetUp *= 0.98f;
			heightOffsetDown *= 0.98f;
			AdjustVerticalOffset();
			config.CameraHeight = 5f + heightOffsetUp + heightOffsetDown;
			tweenGoalY2 = tweenGoalY;
			tweenGoalY = (prevGoalY + Player.position.y + config.CameraHeight) / 2f;
			prevGoalY = Player.position.y + config.CameraHeight;
			float y = rotationVector.y;
			float b = tweenGoalY2;
			float y2 = base.transform.eulerAngles.y;
			float y3 = base.transform.position.y;
			y2 = Mathf.LerpAngle(y2, y, config.CameraRotationDamping * Time.deltaTime);
			y3 = Mathf.LerpAngle(y3, b, config.CameraHeightDamping * Time.deltaTime);
			Quaternion quaternion = Quaternion.Euler(0f, y2, 0f);
			Vector3 position = base.transform.position;
			base.transform.position = Player.position;
			base.transform.position -= quaternion * Vector3.forward * config.CameraDistance;
			base.transform.position = new Vector3(base.transform.position.x, y3, base.transform.position.z);
			base.transform.position = Vector3.Lerp(position, base.transform.position, 0.8f);
			Quaternion rotation = base.transform.rotation;
			SelectLookAtTarget();
			base.transform.rotation = Quaternion.Lerp(rotation, base.transform.rotation, 0.1f);
			base.transform.rotation = new Quaternion(base.transform.rotation.x, 0f, base.transform.rotation.z, base.transform.rotation.w);
		}

		private void SelectLookAtTarget()
		{
			if (playerController == null)
			{
				playerController = Player.GetComponent<PlayerController>();
			}
			base.transform.LookAt(CameraTarget);
		}

		private void AdjustVerticalOffset()
		{
			Vector3 velocity = Player.GetComponent<Rigidbody>().linearVelocity;
			if (velocity.y < -5f)
			{
				heightOffsetUp = Mathf.Min(4f, heightOffsetUp + 0.03f);
				return;
			}
			Vector3 velocity2 = Player.GetComponent<Rigidbody>().linearVelocity;
			if (velocity2.y > 5f)
			{
				heightOffsetDown = Mathf.Max(-4f, heightOffsetDown - 0.03f);
			}
		}
	}
}
