using DevonLocalization.Core;
using Disney.ClubPenguin.CPModuleUtils;
using Disney.ClubPenguin.SledRacer;
using Disney.ClubPenguin.WebPageViewer;
using Disney.DMOAnalytics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

[RequireComponent(typeof(DisplayNativeWebPage), typeof(AudioSource))]
public class MembershipBenefits : MonoBehaviour
{
	public delegate void MembershipBenefitsBackDelegate();

	private static string MEMBERBENEFITS_LOC_PATH = "Assets/Framework/MembershipBenefits/Resources/Translations";

	private static string MEMBERBENEFITS_MODULE = "MEMBERBENEFITS";

	private static string URLTokenAndroid = "memberbenefits.url.android";

	public static string CPAndroidAppURL = "com.disney.cpcompanion_goo";

	public static string CPAndroidStoreURL = "https://play.google.com/store/apps/details?id=com.disney.cpcompanion_goo";

	private static string CONTEXT = "context";

	private static string CONTEXT_VALUE = "get_membership_attempt";

	private static string ACTION = "action";

	private static string ACTION_VALUE = string.Empty;

	private static string PAGE_VIEW = "page_view";

	private static string PLAYER_ID = "player_id";

	private static string PLAYER_NULL = "NULL";

	private static string LOCATION = "location";

	private static string MEMBER_BENEFITS = "membership_page";

	public Button BuyNowButton;

	public Image BuyNowMemberBadge;

	public GameObject GaryImage;

	public Button BackButton;

	public AudioClip BackButtonSound;

	public AudioClip BuyNowButtonSound;

	public long PlayerID;

	public MembershipBenefitsBackDelegate BackButtonPressed;

	private DisplayNativeWebPage displayNativeWebPage;

	private string TranslatedPageURL;

	private bool enableMembershipPlanButtonButton = true;

	private void Awake()
	{
		GaryImage.SetActive(value: false);
		SetupLocalization();
		GetLocalizedWebPage(URLTokenAndroid);
	}

	private void Start()
	{
		HardwareBackButtonDispatcher.SetTargetClickHandler(BackButton, visible: false);
		SendBIPageView();
	}

	private void GetLocalizedWebPage(string urlToken)
	{
		TranslatedPageURL = Localizer.Instance.GetTokenTranslation(urlToken);
	}

	private void SetupLocalization()
	{
		ILocalizedTokenFilePath path = new ModuleTokensFilePath(MEMBERBENEFITS_LOC_PATH, MEMBERBENEFITS_MODULE, Platform.global);
		Localizer.Instance.LoadTokensFromLocalJSON(path, OnTokensLoaded);
	}

	private void OnTokensLoaded(bool tokensUpdated)
	{
		UnityEngine.Debug.Log("Memberbenefits Tokens Loaded.");
	}

	public void ShowWebPage()
	{
		if (displayNativeWebPage == null)
		{
			displayNativeWebPage = GetComponent<DisplayNativeWebPage>();
		}
		StartCoroutine(CheckInternetReachability());
	}

	private IEnumerator CheckInternetReachability()
	{
		UnityWebRequest www = UnityWebRequest.Get(TranslatedPageURL);
		yield return www.SendWebRequest();
		if (www.error != null)
		{
			DisplayOfflinePage();
		}
		else
		{
			DisplayOnlinePage();
		}
	}

	private void DisplayOfflinePage()
	{
		GaryImage.SetActive(value: true);
		BuyNowButton.interactable = false;
		BuyNowMemberBadge.color = BuyNowButton.colors.disabledColor;
	}

	private void DisplayOnlinePage()
	{
		displayNativeWebPage.DisplayPage(TranslatedPageURL);
	}

	public void OnMembershipPlanButtonPress()
	{
		if (enableMembershipPlanButtonButton)
		{
			enableMembershipPlanButtonButton = false;
			StartCoroutine(reEnableMembershipPlanButton());
			GetComponent<AudioSource>().PlayOneShot(BuyNowButtonSound);
			AppIconsController.TrackedApp trackedApp = default(AppIconsController.TrackedApp);
			trackedApp.id = AppIconsController.AppId.CLUB_PENGUIN;
			trackedApp.location = AppIconsController.IconLocation.MEMBERSHIP;
			Service.Get<UIManager>().OpenExternalApp(trackedApp);
		}
	}

	private IEnumerator reEnableMembershipPlanButton()
	{
		yield return new WaitForSeconds(0.5f);
		enableMembershipPlanButtonButton = true;
	}

	public void OnBackButton()
	{
		GetComponent<AudioSource>().PlayOneShot(BackButtonSound);
		StartCoroutine(WaitForSound());
	}

	private IEnumerator WaitForSound()
	{
		yield return null;
		if (displayNativeWebPage != null)
		{
			displayNativeWebPage.OnCloseButton();
		}
		if (BackButtonPressed != null)
		{
			BackButtonPressed();
		}
	}

	private void SendBIPageView()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (PlayerID == 0L)
		{
			dictionary.Add(PLAYER_ID, PLAYER_NULL);
		}
		else
		{
			dictionary.Add(PLAYER_ID, PlayerID);
		}
		dictionary.Add(LOCATION, MEMBER_BENEFITS);
		DMOAnalytics.SharedAnalytics.LogAnalyticsEventWithContext(PAGE_VIEW, dictionary);
	}

	private void SendBIGameActionExitApp()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add(CONTEXT, CONTEXT_VALUE);
		dictionary.Add(ACTION, ACTION_VALUE);
		if (PlayerID == 0L)
		{
			dictionary.Add(PLAYER_ID, PLAYER_NULL);
		}
		else
		{
			dictionary.Add(PLAYER_ID, PlayerID);
		}
		DMOAnalytics.SharedAnalytics.LogGameAction(dictionary);
	}
}
