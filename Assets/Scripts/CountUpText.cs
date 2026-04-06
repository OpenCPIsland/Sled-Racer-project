using Disney.ClubPenguin.SledRacer;
using UnityEngine;
using UnityEngine.UI;

public class CountUpText : MonoBehaviour
{
	public Text[] TextField;

	private bool[] isCounting;

	private int[] targetValue;

	private float[] totalTime;

	private float[] currentTime;

	private SFXEvent[] audioSequence;

	private SFXEvent activeLoop;

	private void Awake()
	{
		isCounting = new bool[TextField.Length];
		targetValue = new int[TextField.Length];
		totalTime = new float[TextField.Length];
		currentTime = new float[TextField.Length];
		audioSequence = new SFXEvent[TextField.Length];
	}

	public void StartCounting1(float countSeconds)
	{
		StartCounting(countSeconds, SFXEvent.UI_Leaderboard_CoinsEarnedTally, 0);
	}

	public void StartCounting2(float countSeconds)
	{
		StartCounting(countSeconds, SFXEvent.UI_Leaderboard_TotalCoinsTally, 1);
	}

	public void StartCounting3(float countSeconds)
	{
		StartCounting(countSeconds, SFXEvent.UI_Leaderboard_RunDistanceTally, 2);
	}

	public void StartCounting4(float countSeconds)
	{
		StartCounting(countSeconds, SFXEvent.UI_Leaderboard_BestDistanceTally, 3);
	}

	private void StartCounting(float countSeconds, SFXEvent audioClip, int textFieldIndex)
	{
		if (int.TryParse(TextField[textFieldIndex].text, out targetValue[textFieldIndex]))
		{
			isCounting[textFieldIndex] = true;
			totalTime[textFieldIndex] = countSeconds;
			currentTime[textFieldIndex] = 0f;
			TextField[textFieldIndex].text = "0";
			audioSequence[textFieldIndex] = audioClip;
			startSFXLoop(audioClip);
		}
	}

	private void Update()
	{
		for (int i = 0; i < isCounting.Length; i++)
		{
			if (isCounting[i])
			{
				currentTime[i] += Time.deltaTime;
				int num = Mathf.CeilToInt((float)targetValue[i] * (currentTime[i] / totalTime[i]));
				if (num >= targetValue[i])
				{
					num = targetValue[i];
					isCounting[i] = false;
					stopSFXLoop(audioSequence[i]);
				}
				TextField[i].text = num.ToString();
			}
		}
	}

	private void startSFXLoop(SFXEvent loop)
	{
		Service.Get<IAudio>().SFX.Play(loop);
	}

	private void stopSFXLoop(SFXEvent loop)
	{
		Service.Get<IAudio>().SFX.Stop(loop);
	}
}
