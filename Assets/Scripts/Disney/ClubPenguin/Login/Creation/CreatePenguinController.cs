using DevonLocalization;
using DevonLocalization.Core;
using Disney.ClubPenguin.CPModuleUtils;
using Disney.ClubPenguin.Login.Authentication;
using Disney.ClubPenguin.Service.MWS;
using Disney.ClubPenguin.Service.PDR;
using Disney.ClubPenguin.SledRacer;
using Disney.HTTP.Client;
using ErrorPopup;
using ErrorPopup.Core;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Disney.ClubPenguin.Login.Creation
{
	[RequireComponent(typeof(PenguinColor), typeof(LoginController))]
	public class CreatePenguinController : MonoBehaviour
	{
		public delegate void CreationSucceededDelegate(IGetAuthTokenResponse response, string parentEmail, string penguinName, string password);

		public delegate void CreationFailedDelegate();

		public delegate void CreationAttempStartedDelegate();

		public delegate void HtmlLinkClickedDelegate(string url);

		private const int USERNAME_MIN_LENGTH = 4;

		private const int USERNAME_MAX_LENGTH = 12;

		public CreationSucceededDelegate CreationSucceeded;

		public CreationFailedDelegate CreationFailed;

		public CreationAttempStartedDelegate CreationAttemptStarted;

		public HtmlLinkClickedDelegate HtmlLinkClicked;

		public AudioClip OnClickAudioClip;

		public InputField penguinNameInputField;

		public InputField passwordInputField;

		public InputField passwordConfirmationInputField;

		public InputField parentEmailInputField;

		public LocalizedWebLink TermsOfUseWebLink;

		public LocalizedWebLink CpRulesWebLink;

		public Toggle TermsOfUseToggle;

		public Toggle CpRulesToggle;

		public Toggle PrivacyToggle;

		public AudioClip ErrorPopupAudioClip;

		public PrivacyPracticesController privacyPracticesPrefab;

		private IMWSClient _Client;

		private IPDRClient _PDRClient;

		[HideInInspector]
		public AudioSource RootAudioSource;

		private bool hasAcceptedTermsOfUse;

		private bool hasAcceptedRules;

		private bool hasAcceptedPrivacyPolicy;

		private LoginController loginController;

		public IMWSClient Client
		{
			get;
			set;
		}

		public IPDRClient PDRClient
		{
			get;
			set;
		}

		public event Action<string> ProtectedWebLinkClicked;

		private void Start()
		{
			hasAcceptedTermsOfUse = false;
			parentEmailInputField.keyboardType = TouchScreenKeyboardType.EmailAddress;
			loginController = GetComponent<LoginController>();
			NormalizePasswordFields();
			LocalizedWebLink cpRulesWebLink = CpRulesWebLink;
			cpRulesWebLink.WebLinkClicked = (LocalizedWebLink.WebLinkClickedDelegate)Delegate.Combine(cpRulesWebLink.WebLinkClicked, new LocalizedWebLink.WebLinkClickedDelegate(OnWebLinkClicked));
			LocalizedWebLink termsOfUseWebLink = TermsOfUseWebLink;
			termsOfUseWebLink.WebLinkClicked = (LocalizedWebLink.WebLinkClickedDelegate)Delegate.Combine(termsOfUseWebLink.WebLinkClicked, new LocalizedWebLink.WebLinkClickedDelegate(OnProtectedWebLinkClicked));
		}

		private void OnDestroy()
		{
			LocalizedWebLink cpRulesWebLink = CpRulesWebLink;
			cpRulesWebLink.WebLinkClicked = (LocalizedWebLink.WebLinkClickedDelegate)Delegate.Remove(cpRulesWebLink.WebLinkClicked, new LocalizedWebLink.WebLinkClickedDelegate(OnWebLinkClicked));
			LocalizedWebLink termsOfUseWebLink = TermsOfUseWebLink;
			termsOfUseWebLink.WebLinkClicked = (LocalizedWebLink.WebLinkClickedDelegate)Delegate.Remove(termsOfUseWebLink.WebLinkClicked, new LocalizedWebLink.WebLinkClickedDelegate(OnProtectedWebLinkClicked));
			CreationAttemptStarted = null;
			CreationSucceeded = null;
			CreationFailed = null;
		}

		private void OnProtectedWebLinkClicked(string url)
		{
			HideAllErrorPopups();
			if (this.ProtectedWebLinkClicked != null)
			{
				this.ProtectedWebLinkClicked(url);
			}
		}

		private void OnWebLinkClicked(string url)
		{
			RootAudioSource.PlayOneShot(OnClickAudioClip);
			HideAllErrorPopups();
			HtmlLinkClicked(url);
		}

		public void OnPrivacyPracticesClicked()
		{
			RootAudioSource.PlayOneShot(OnClickAudioClip);
			HideAllErrorPopups();
			PrivacyPracticesController privacyPracticesController = UnityEngine.Object.Instantiate(privacyPracticesPrefab) as PrivacyPracticesController;
			privacyPracticesController.GetComponent<RectTransform>().SetParent(GetComponent<RectTransform>(), worldPositionStays: false);
			privacyPracticesController.RootAudioSource = RootAudioSource;
		}

		public void OnShowPasswordToggled(bool toggled)
		{
			NormalizePasswordFields();
			if (toggled)
			{
				passwordInputField.inputType = InputField.InputType.Standard;
				passwordConfirmationInputField.inputType = InputField.InputType.Standard;
			}
			else
			{
				passwordInputField.inputType = InputField.InputType.Password;
				passwordConfirmationInputField.inputType = InputField.InputType.Password;
			}
			passwordInputField.enabled = false;
			passwordInputField.enabled = true;
			passwordConfirmationInputField.enabled = false;
			passwordConfirmationInputField.enabled = true;
		}

		private void NormalizePasswordFields()
		{
			NormalizePasswordField(passwordInputField);
			NormalizePasswordField(passwordConfirmationInputField);
		}

		private static void NormalizePasswordField(InputField inputField)
		{
			if (inputField == null)
			{
				return;
			}
			inputField.interactable = true;
			inputField.readOnly = false;
			inputField.lineType = InputField.LineType.SingleLine;
			inputField.characterValidation = InputField.CharacterValidation.None;
			inputField.contentType = InputField.ContentType.Password;
		}

		public void OnAcceptTermsToggled(bool toggled)
		{
			UnityEngine.Debug.Log("TOGGLE TERMS");
			hasAcceptedTermsOfUse = toggled;
		}

		public void OnAcceptRules(bool toggled)
		{
			UnityEngine.Debug.Log("Accept rules");
			hasAcceptedRules = toggled;
		}

		public void OnAcceptPolicy(bool toggled)
		{
			UnityEngine.Debug.Log("Accept policy");
			hasAcceptedPrivacyPolicy = toggled;
		}

		public void OnSubmit()
		{
			HideAllErrorPopups();
			if (!isUsernameValid() || !IsPasswordValid())
			{
				return;
			}
			if (!IsCorrectEmailFormat(parentEmailInputField.text))
			{
				ShowErrorPopup("9", parentEmailInputField.GetComponent<ErrorPopupComponent>());
			}
			else if (hasAcceptedLegal())
			{
				UnityEngine.Debug.Log("ATTEMPT LOGIN");
				RootAudioSource.PlayOneShot(OnClickAudioClip);
				string username = InputFieldStringUtils.ToTitleCase(penguinNameInputField.text);
				SavedPlayerData createdPlayer;
				string errorCode;
				if (!LocalPlayerAccountService.TryCreateAccount(username, passwordInputField.text, (int)GetComponent<PenguinColor>().Color, out createdPlayer, out errorCode))
				{
					InputField inputField = (errorCode == "-32297") ? parentEmailInputField : penguinNameInputField;
					ShowErrorPopup(errorCode, inputField.GetComponent<ErrorPopupComponent>());
					if (CreationFailed != null)
					{
						CreationFailed();
					}
					return;
				}
				if (CreationAttemptStarted != null)
				{
					CreationAttemptStarted();
				}
				if (CreationSucceeded != null)
				{
					CreationSucceeded(LocalPlayerAccountService.CreateAuthResponse(createdPlayer), parentEmailInputField.text, createdPlayer.UserName, passwordInputField.text);
				}
			}
		}

		private bool isUsernameValid()
		{
			if (penguinNameInputField.text.Length < 4 || penguinNameInputField.text.Length > 12)
			{
				ShowErrorPopup("3", penguinNameInputField.GetComponent<ErrorPopupComponent>());
				return false;
			}
			if (penguinNameInputField.text.IndexOf("  ") > -1)
			{
				ShowErrorPopup("10", penguinNameInputField.GetComponent<ErrorPopupComponent>());
				return false;
			}
			return true;
		}

		private bool hasAcceptedLegal()
		{
			if (!hasAcceptedTermsOfUse)
			{
				ShowErrorPopup("6", TermsOfUseToggle.GetComponent<ErrorPopupComponent>());
				return false;
			}
			if (!hasAcceptedRules)
			{
				ShowErrorPopup("7", CpRulesToggle.GetComponent<ErrorPopupComponent>());
				return false;
			}
			if (!hasAcceptedPrivacyPolicy)
			{
				ShowErrorPopup("8", PrivacyToggle.GetComponent<ErrorPopupComponent>());
				return false;
			}
			return true;
		}

		private bool IsCorrectEmailFormat(string address)
		{
			if (address.IndexOf(" ") > -1)
			{
				return false;
			}
			int num = address.IndexOf("@");
			if (num <= 0 || num == address.Length - 1)
			{
				return false;
			}
			int num2 = address.LastIndexOf(".");
			if (num2 <= num || num2 == address.Length - 1)
			{
				return false;
			}
			return true;
		}

		private bool IsPasswordValid()
		{
			if (passwordInputField.text != passwordConfirmationInputField.text)
			{
				ShowErrorPopup("4", passwordInputField.GetComponent<ErrorPopupComponent>());
				return false;
			}
			if (passwordInputField.text.Length == 0)
			{
				ShowErrorPopup("1", passwordInputField.GetComponent<ErrorPopupComponent>());
				return false;
			}
			return true;
		}

		private void OnCreateAccountResponseReceived(ICreateAccountResponse response)
		{
			if (response.IsError)
			{
				OnAccountCreationFailed(response);
			}
			else
			{
				OnAccountCreationSucceeded(response);
			}
		}

		private void OnAccountCreationFailed(ICreateAccountResponse response)
		{
			UnityEngine.Debug.Log("--- FAIL: " + response.StatusMessage);
			if (response.StatusCode == 409)
			{
				ShowErrorPopup(response.StatusCode.ToString(), penguinNameInputField.GetComponent<ErrorPopupComponent>());
			}
			else if (response.StatusCode == 403)
			{
				ShowErrorPopup(response.StatusCode.ToString(), parentEmailInputField.GetComponent<ErrorPopupComponent>());
			}
			if (response.StatusCode == 500)
			{
				if (response.ResponseError == null)
				{
					ShowErrorPopup("0", penguinNameInputField.GetComponent<ErrorPopupComponent>());
				}
				else
				{
					InputField inputField = null;
					switch (response.ResponseError.ErrorResponse.errorCode)
					{
					case -32297:
						inputField = parentEmailInputField;
						break;
					case -32283:
						inputField = penguinNameInputField;
						break;
					case -32292:
						inputField = passwordInputField;
						break;
					case -32296:
						inputField = parentEmailInputField;
						break;
					case -32275:
						inputField = parentEmailInputField;
						break;
					case -32295:
						inputField = parentEmailInputField;
						break;
					case -32299:
						inputField = parentEmailInputField;
						break;
					default:
						inputField = penguinNameInputField;
						break;
					}
					ShowErrorPopup(response.ResponseError.ErrorResponse.errorCode.ToString(), inputField.GetComponent<ErrorPopupComponent>());
				}
			}
			if (CreationFailed != null)
			{
				CreationFailed();
			}
		}

		private void OnAccountCreationSucceeded(ICreateAccountResponse response)
		{
			UnityEngine.Debug.Log("--- SUCCESS: ");
			loginController.Client = Client;
			loginController.PDRClient = PDRClient;
			loginController.ShowErrorPopups = false;
			LoginController obj = loginController;
			obj.LoginSucceeded = (LoginController.LoginSucceededDelegate)Delegate.Combine(obj.LoginSucceeded, new LoginController.LoginSucceededDelegate(OnLoginSucceeded));
			loginController.LoginFailed += OnLoginFailed;
			loginController.DoLogin(penguinNameInputField.text, passwordInputField.text, savePassword: false);
		}

		private void OnLoginSucceeded(IGetAuthTokenResponse response, string username, string password)
		{
			LoginController obj = loginController;
			obj.LoginSucceeded = (LoginController.LoginSucceededDelegate)Delegate.Remove(obj.LoginSucceeded, new LoginController.LoginSucceededDelegate(OnLoginSucceeded));
			loginController.LoginFailed -= OnLoginFailed;
			UnityEngine.Debug.Log("LOGGED IN!");
			if (CreationSucceeded != null)
			{
				CreationSucceeded(response, parentEmailInputField.text, username, password);
			}
		}

		private void OnLoginFailed(IHTTPResponse response)
		{
			LoginController obj = loginController;
			obj.LoginSucceeded = (LoginController.LoginSucceededDelegate)Delegate.Remove(obj.LoginSucceeded, new LoginController.LoginSucceededDelegate(OnLoginSucceeded));
			loginController.LoginFailed -= OnLoginFailed;
			UnityEngine.Debug.Log("LoginFailed");
			if (CreationFailed != null)
			{
				CreationFailed();
			}
		}

		private void ShowErrorPopup(string errorCode, ErrorPopupComponent errorComponent)
		{
			string errorMessage = ErrorsMap.Instance.GetErrorMessage(errorCode);
			string tokenTranslation = Localizer.Instance.GetTokenTranslation(errorMessage);
			errorComponent.ShowError(tokenTranslation);
			RootAudioSource.PlayOneShot(ErrorPopupAudioClip);
		}

		public void HideAllErrorPopups()
		{
			ErrorPopupComponent[] componentsInChildren = GetComponentsInChildren<ErrorPopupComponent>();
			ErrorPopupComponent[] array = componentsInChildren;
			foreach (ErrorPopupComponent errorPopupComponent in array)
			{
				errorPopupComponent.HideError();
			}
		}
	}
}
