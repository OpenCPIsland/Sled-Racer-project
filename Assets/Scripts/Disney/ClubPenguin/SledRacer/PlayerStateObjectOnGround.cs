using UnityEngine;

namespace Disney.ClubPenguin.SledRacer
{
	public class PlayerStateObjectOnGround : PlayerStateObject
	{
		private ParticleSystem snowSpray;

		protected override void OnAwake()
		{
			base.gameObject.SetActive(value: true);
			snowSpray = base.gameObject.GetComponentInChildren<ParticleSystem>();
			base.gameObject.SetActive(value: false);
		}

		private void Update()
		{
			var emission = snowSpray.emission;
			emission.rateOverTime = player.GetComponent<Rigidbody> ().linearVelocity.magnitude * 10f;
		}
	}
}
