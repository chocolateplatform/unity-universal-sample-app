using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Chocolate;

public class Main : MonoBehaviour,
                    ChocolateInterstitialCallbackReceiver,
                    ChocolateRewardCallbackReceiver {
  Button buttonLoadInterstitial;
  Button buttonShowInterstitial;
  Button buttonLoadReward;
  Button buttonShowReward;

  bool[] buttonState = {true,false,true,false};
  bool buttonStateChanged = false;

	string latitude;
	string longitude;

	public Canvas AlertCanvas;
	public Text GUITextMessage;

  bool uiSetup = false;
  bool listenerSetup = false;

  void Update() {
    if(!uiSetup) {
      Debug.Log("settign up UI");
      SetupUI();
    }

    if(!listenerSetup) {
      SetupListener();
    }

    if(buttonStateChanged) {
      ApplyButtonState();
    }
  }

  void ApplyButtonState() {
    buttonLoadInterstitial.interactable = buttonState[0];
    buttonShowInterstitial.interactable = buttonState[1];
    buttonLoadReward.interactable = buttonState[2];
    buttonShowReward.interactable = buttonState[3];
    buttonStateChanged = false;
  }

  void ButtonSetup(string adUnitType, bool enableShow) {
    int loadIndex, showIndex;
    if(adUnitType == "interstitial") {
      loadIndex = 0;
      showIndex = 1;
    } else {
      loadIndex = 2;
      showIndex = 3;
    }

    if(enableShow) {
      buttonState[loadIndex] = false;
      buttonState[showIndex] = true;
    } else {
      buttonState[loadIndex] = true;
      buttonState[showIndex] = false;
    }

    buttonStateChanged = true;
  }

  void SetupUI() {
    buttonLoadInterstitial = GameObject.Find ("ButtonLoadInterstitial").GetComponent<Button> ();
		buttonShowInterstitial = GameObject.Find ("ButtonShowInterstitial").GetComponent<Button> ();
		buttonShowInterstitial.interactable = false;

		buttonLoadReward = GameObject.Find ("ButtonLoadReward").GetComponent<Button> ();
		buttonShowReward = GameObject.Find ("ButtonShowReward").GetComponent<Button> ();
		buttonShowReward.interactable = false;

    uiSetup = (buttonLoadInterstitial && buttonShowInterstitial &&
               buttonLoadReward && buttonShowReward);
  }

  void SetupListener() {
    // ChocolateUnityBridge.setupWithListener("Canvas");
    ChocolateUnityBridge.setInterstitialAdListener(this);
    ChocolateUnityBridge.setRewardAdListener(this);
    listenerSetup = true;
  }

	IEnumerator Start()
	{

		Screen.fullScreen = false;      //Disable Fullscreen App
		AlertCanvas.enabled = false;
		GUITextMessage.enabled = false;

    string apiKey = "";
    if(ChocolateUnityBridge.iOSEnvironment()) {
      apiKey = "X4mdFv";
    } else if(ChocolateUnityBridge.AndroidEnvironment()) {
      apiKey = "XqjhRR";
    }

    ChocolateUnityBridge.initWithAPIKey(apiKey);
    ChocolateUnityBridge.SetAdRequestTestMode(true,"dinosaur");

    ChocolateUnityBridge.setPrivacySettings(
      true,  //whether GDPR applies
      null   //consent string in format defined by IAB, or null if no consent was obtained
    );

		// First, check if user has location service enabled
		if (!Input.location.isEnabledByUser)
			yield break;

		// Start service before querying location
		Input.location.Start();

		// Wait until service initializes
		int maxWait = 20;
		while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
		{
			yield return new WaitForSeconds(1);
			maxWait--;
		}

		// Service didn't initialize in 20 seconds
		if (maxWait < 1)
		{
			print("Timed out");
			yield break;
		}

		// Connection has failed
		if (Input.location.status == LocationServiceStatus.Failed)
		{
			print("Unable to determine device location");
			yield break;
		}
		else
		{
			// Access granted and location value could be retrieved
			latitude =  string.Format("{0:N3}", Input.location.lastData.latitude);
			longitude = string.Format("{0:N3}", Input.location.lastData.longitude);
			print("Location: " +
            Input.location.lastData.latitude +
            " " + Input.location.lastData.longitude +
            " " + Input.location.lastData.altitude +
            " " + Input.location.lastData.horizontalAccuracy +
            " " + Input.location.lastData.timestamp);
		}

		// Stop service if there is no need to query location updates continuously
		Input.location.Stop();
	}



    public void buttonLoadInterstitialClicked() {     //Interstitial Ad Button Load Clicked
        Debug.Log("Button Load Interstitial Clicked...");
    		ChocolateUnityBridge.setDemograpics(23, "23/11/1990", "m", "single", "Asian");
        ChocolateUnityBridge.setLocation("999", "123123", "321321", latitude, longitude);
    		ChocolateUnityBridge.setAppInfo("UnityDemo", "Vdopia", "unity-demo.com", "vdopia.com", "", "Movie");
    		ChocolateUnityBridge.loadInterstitialAd ();
    }

    public void buttonShowInterstitialClicked()     //Interstitial Ad Button Show Clicked
    {
        Debug.Log("Button Show Interstitial Clicked...");
		    ChocolateUnityBridge.showInterstitialAd ();

    }

    public void buttonRequestRewardClicked()        //Reward Ad Button Request Clicked
    {
        Debug.Log("Button Request Reward Clicked...");
		ChocolateUnityBridge.setDemograpics(23, "23/11/1990", "m", "single", "Asian");
    ChocolateUnityBridge.setLocation("999", "123123", "321321", latitude, longitude);
		ChocolateUnityBridge.setAppInfo("UnityDemo", "Vdopia", "unity-demo.com", "vdopia.com", "", "Movie");
		ChocolateUnityBridge.loadRewardAd ();

    }

    public void buttonShowRewardClicked()           //Reward Ad Button Show Clicked
    {
        Debug.Log("Button Show Reward Clicked...");
		AlertCanvas.enabled = true;


    }



	public void YesClicked ()
	{
		Debug.Log ("Yes Clicked...");
		AlertCanvas.enabled = false;

      ButtonSetup("reward", false);
			// isRewardLoaded = false;
      //
			// buttonLoadReward.interactable = true;
			// buttonShowReward.interactable = false;

			//Parma 1 : Secret Key (Get it from Vdopia Portal : Required if server-to-server callback enabled)
			//Parma 2 : User name
			//Param 3 : Reward Name or measure
			//Param 4 : Reward Amount or quantity
			ChocolateUnityBridge.showRewardAd(30,"coin","Chocolate1","XNydpzNLIj2pBRM8");

	}

	public void NoClicked ()
	{
		Debug.Log ("No Clicked...");
		AlertCanvas.enabled = false;
	}

	IEnumerator ShowGetRewardMessage (float delay) {
		GUITextMessage.enabled = true;
		yield return new WaitForSeconds(delay);
		GUITextMessage.enabled = false;
	}

	/// <summary>
	/// Ons the interstitial loaded.
	/// </summary>
	/// <param name="id">Identifier.</param>
	public void onInterstitialLoaded(string id) {
		Debug.Log ("Unity id:" + id);
		// isInterstitialLoaded = true;
    ButtonSetup("interstitial", true);

	}
	/// <summary>
	/// Ons the interstitial failed.
	/// </summary>
	/// <param name="id">Identifier.</param>
	public void onInterstitialFailed(string id) {
		//isInterstitialLoaded = false;
    ButtonSetup("interstitial", false);
		int errorID;
		if (int.TryParse (id, out errorID))
		{
			switch (errorID) {
			case 0:
				Debug.Log ("INVALID_REQUEST:" + errorID);
				break;
			case 1:
				Debug.Log ("INTERNAL_ERROR:" + errorID);
				break;
			case 2:
				Debug.Log ("NO_FILL:" + errorID);
				break;
			case 3:
				Debug.Log ("NETWORK_ERROR:" + errorID);
				break;
			case 4:
				Debug.Log ("INVALID_RESPONSE:" + errorID);
				break;
			}
		}
	}
	/// <summary>
	/// Ons the interstitial shown.
	/// </summary>
	/// <param name="id">Identifier.</param>
	public void onInterstitialShown(string id)
	{
		Debug.Log ("Unity id:" + id);

	}
	/// <summary>
	/// Ons the interstitial clicked.
	/// </summary>
	/// <param name="id">Identifier.</param>
	public void onInterstitialClicked(string id)
	{
		Debug.Log ("Unity id:" + id);

	}
	/// <summary>
	/// Ons the interstitial dismissed.
	/// </summary>
	/// <param name="id">Identifier.</param>
	public void onInterstitialDismissed(string id) {
		//isInterstitialLoaded = false;
    ButtonSetup("interstitial", false);
    Debug.Log ("Unity id:" + id);

	}

	/// <summary>
	/// Rewardeds the video did load ad.
	/// </summary>
	/// <param name="id">Identifier.</param>
	public void onRewardLoaded(string id) {
		Debug.Log ("Unity id:" + id);
		//isRewardLoaded = true;
    ButtonSetup("reward", true);
	}

	/// <summary>
	/// Rewards the ad failed with error.
	/// </summary>
	/// <param name="id">Identifier.</param>
	public void onRewardFailed(string id) {
		// isRewardLoaded = false;
    ButtonSetup("reward", false);
		Debug.Log ("Unity id:" + id);

		int errorID;
		if (int.TryParse (id, out errorID)) {
			switch (errorID) {
			case 0:
				Debug.Log ("INVALID_REQUEST:" + errorID);
				break;
			case 1:
				Debug.Log ("INTERNAL_ERROR:" + errorID);
				break;
			case 2:
				Debug.Log ("NO_FILL:" + errorID);
				break;
			case 3:
				Debug.Log ("NETWORK_ERROR:" + errorID);
				break;
			case 4:
				Debug.Log ("INVALID_RESPONSE:" + errorID);
				break;
			}
		}

	}
	/// <summary>
	/// Rewardeds the video did finish.
	/// </summary>
	/// <param name="id">Identifier.</param>
	public void onRewardFinished(string id) {
		// isRewardCompleted = true;
    StartCoroutine(ShowGetRewardMessage (3));
		string[] array = id.Split(',');
		Debug.Log ("Unity id:" + array);
		foreach (string token in array) {
			// Parse the commands
			Debug.Log ("Reward Amount:" + array[0]);
			Debug.Log ("Reward Name:" + array[1]);
		}

		Debug.Log ("Unity id:" + id);

	}
	/// <summary>
	/// Rewardeds the video will dismiss.
	/// </summary>
	/// <param name="id">Identifier.</param>
	public void onRewardDismissed(string id) {
		Debug.Log ("Unity id:" + id);

	}

	/// <summary>
	/// Rewardeds the video did start video.
	/// </summary>
	/// <param name="id">Identifier.</param>
	public void onRewardShown(string id) {
		Debug.Log ("Unity id:" + id);

	}

	// /// <summary>
	// /// Rewardeds the video did fail to start video with error.
	// /// </summary>
	// /// <param name="id">Identifier.</param>
	// public void rewardedVideoDidFailToStartVideoWithError(string id)
	// {
	// 	Debug.Log ("Unity id:" + id);
  //
  //
	// }

}
