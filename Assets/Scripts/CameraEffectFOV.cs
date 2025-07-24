using Disney.ClubPenguin.SledRacer;
using UnityEngine;

public class CameraEffectFOV : MonoBehaviour
{
	public Camera targetCamera;

	public float FOVmin = 60f;

	public float FOVmax = 160f;

	public float FOVcurr;

	public bool autoTrack;

	private Camera currCamera;

	private ConfigController config;

	public Transform Player;

	private float startZDistance;

	private float farthestFOVDistance = 2.5f;

	private void Awake()
	{
		config = Service.Get<ConfigController>();
		if (!targetCamera)
		{
			targetCamera = GetComponent<Camera>();
		}
	}

	private void SetCamera(Camera _cam)
	{
		if ((bool)_cam)
		{
			currCamera = _cam;
			currCamera.fieldOfView = config.CameraDefaultFOV;
			Vector3 position = Player.transform.position;
			float z = position.z;
			Vector3 position2 = currCamera.transform.position;
			startZDistance = z - position2.z;
			FOVcurr = currCamera.fieldOfView;
		}
	}

	private void Update()
	{
		CheckForChanges();
		float cameraDefaultFOV = config.CameraDefaultFOV;
		Vector3 vector = base.transform.InverseTransformDirection(Player.GetComponent<Rigidbody>().linearVelocity);
		float num = Mathf.Clamp(vector.z - config.CameraFOVSpeedMinimum, 0f, config.CameraMaximumFOV);
		cameraDefaultFOV = Mathf.Lerp(GetComponent<Camera>().fieldOfView, config.CameraDefaultFOV + num * config.CameraZoomRatio, 0.1f);
		GetComponent<Camera>().fieldOfView = cameraDefaultFOV;
	}

	private void CheckForChanges()
	{
		if (targetCamera != currCamera)
		{
			SetCamera(targetCamera);
		}
	}
}
