using UnityEngine;

public class GameManager : MonoBehaviour
{
	private static GameManager _instance;

	public static GameManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindInstance();
			}
			return _instance;
		}
	}

	private static GameManager FindInstance()
	{
		GameObject gameObject = GameObject.Find("/GameManager");
		if (gameObject == null)
		{
			return null;
		}
		return gameObject.GetComponent<GameManager>();
	}

	public static T GetInstanceAs<T>() where T : GameManager
	{
		return (T)Instance;
	}

	private void Awake()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			Application.targetFrameRate = 120; 
		}
	}
}
