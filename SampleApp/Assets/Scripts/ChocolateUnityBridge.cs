using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;

namespace Chocolate {

	public interface ChocolateInterstitialCallbackReceiver {
		void onInterstitialLoaded(string msg);
		void onInterstitialFailed(string msg);
		void onInterstitialShown(string msg);
	 	void onInterstitialClicked(string msg);
	 	void onInterstitialDismissed(string msg);
	}

	public interface ChocolateRewardCallbackReceiver {
	 	void onRewardLoaded(string msg);
	 	void onRewardFailed(string msg);
	 	void onRewardShown(string msg);
	 	void onRewardFinished(string msg);
		void onRewardDismissed(string msg);
	}

	public class ChocolateUnityBridge : MonoBehaviour {
		[System.Runtime.InteropServices.DllImport("__Internal")]
		extern static public void _initWithAPIKey(string apiKey);

		[System.Runtime.InteropServices.DllImport("__Internal")]
		extern static public void _setupWithListener(string listenerName);

		[System.Runtime.InteropServices.DllImport("__Internal")]
		extern static public void _loadInterstitialAd();

		[System.Runtime.InteropServices.DllImport("__Internal")]
		extern static public void _showInterstitialAd();

		[System.Runtime.InteropServices.DllImport("__Internal")]
		extern static public void _loadRewardAd();

		[System.Runtime.InteropServices.DllImport("__Internal")]
		extern static public void _showRewardAd(int rewardAmount, string rewardName, string userId, string secretKey);

		[System.Runtime.InteropServices.DllImport("__Internal")]
		extern static public void _setDemograpics(int age, string birthDate, string gender, string maritalStatus,
		                     string ethnicity);

		[System.Runtime.InteropServices.DllImport("__Internal")]
		extern static public void _setLocation(string dmaCode, string postal, string curPostal, string latitude, string longitude);

		[System.Runtime.InteropServices.DllImport("__Internal")]
		extern static public void _setAppInfo(string appName, string pubName,
		                 string appDomain, string pubDomain,
		                 string storeUrl, string iabCategory);

		[System.Runtime.InteropServices.DllImport("__Internal")]
		extern static public void _setPrivacySettings(bool gdprApplies, string gdprConsentString);

		[System.Runtime.InteropServices.DllImport("__Internal")]
		extern static public void _setCustomSegmentProperty(string key, string value);

		[System.Runtime.InteropServices.DllImport("__Internal")]
		extern static public string _getCustomSegmentProperty(string key);

		[System.Runtime.InteropServices.DllImport("__Internal")]
		extern static public string _getAllCustomSegmentProperties();

		[System.Runtime.InteropServices.DllImport("__Internal")]
		extern static public void _deleteCustomSegmentProperty(string key);

		[System.Runtime.InteropServices.DllImport("__Internal")]
		extern static public void _deleteAllCustomSegmentProperties();

		public static bool iOSEnvironment() {
			return (Application.platform == RuntimePlatform.OSXPlayer
	            || Application.platform == RuntimePlatform.IPhonePlayer);
		}

		public static bool AndroidEnvironment() {
			return (Application.platform == RuntimePlatform.Android);
		}

		private static AndroidJavaObject VDONativePlugin;
		private static AndroidJavaObject CreateAndroidPluginInstance() {
			using (var pluginClass = new AndroidJavaClass("com.vdopia.unity.plugin.VdopiaPlugin")) {
					VDONativePlugin = pluginClass.CallStatic<AndroidJavaObject>("GetInstance");
			}

			if (VDONativePlugin != null) {
	        AndroidJavaClass javaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
	        AndroidJavaObject currentActivity = javaClass.GetStatic<AndroidJavaObject>("currentActivity");

	        VDONativePlugin.Call("SetActivity", currentActivity);
	        VDONativePlugin.Call("SetUnityAdListener", VdopiaAndroidListener.GetInstance());
					VdopiaAndroidListener.GetInstance().VdopiaAdDelegateEventHandler += onVdopiaEventReceiver;
	    } else {
	        Debug.Log("Unable to Initialize VdopiaPlugin...");
	    }
			return VDONativePlugin;
		}
		private static AndroidJavaObject AndroidPluginInstance() {
			return VDONativePlugin == null ? CreateAndroidPluginInstance() : VDONativePlugin;
		}

		private static string apiKeyFromInit = null;

		//private static GameObject callbackListener;
		private static ChocolateInterstitialCallbackReceiver interRec;
		private static ChocolateRewardCallbackReceiver rewardRec;

		//invokes the Objective-C fuctions only on iOS, not in Unity
		public static void initWithAPIKey(string apiKey) {
			if(iOSEnvironment()) {
				_initWithAPIKey(apiKey);
			} else if(AndroidEnvironment() &&
								AndroidPluginInstance() != null &&
								apiKeyFromInit == null) {
				apiKeyFromInit = apiKey;
				VDONativePlugin.Call("ChocolateInit", apiKey);
			}
		}

		public static void setInterstitialAdListener(ChocolateInterstitialCallbackReceiver listener) {
			if(iOSEnvironment()) {
				_setupWithListener(listener.ToString());
			} else if(AndroidEnvironment()) {
				interRec = listener;
			}
		}

		public static void setRewardAdListener(ChocolateRewardCallbackReceiver listener) {
			if(iOSEnvironment()) {
				_setupWithListener(listener.ToString());
			} else if(AndroidEnvironment()) {
				rewardRec = listener;
			}
		}

		public static void removeInterstitialAdListener() {
			if(AndroidEnvironment()) {
				interRec = null;
			}
		}

		public static void removeRewardAdListener() {
			if(AndroidEnvironment()) {
				rewardRec = null;
			}
		}

		// public static void setupWithListener(string listenerName) {
		// 	if(iOSEnvironment()) {
		// 		_setupWithListener(listenerName);
		// 	} else if(AndroidEnvironment()) {
		// 		//callbackListener = GameObject.Find(listenerName);
		// 		interRec = GameObject.Find(listenerName).GetComponent<ChocolateInterstitialCallbackReceiver>();
		// 		rewardRec = GameObject.Find(listenerName).GetComponent<ChocolateRewardCallbackReceiver>();
		// 	}
		// }

		public static void loadInterstitialAd(){
			if(iOSEnvironment()) {
				_loadInterstitialAd();
			} else if(AndroidEnvironment() && AndroidPluginInstance() != null) {
				VDONativePlugin.Call("LoadInterstitialAd", apiKeyFromInit);
			}
		}

		public static void showInterstitialAd() {
			if(iOSEnvironment()) {
				_showInterstitialAd();
			} else if(AndroidEnvironment() && AndroidPluginInstance() != null) {
				VDONativePlugin.Call("ShowInterstitialAd");
			}
		}

		public static void loadRewardAd(){
			if(iOSEnvironment()) {
				_loadRewardAd();
			} else if(AndroidEnvironment() && AndroidPluginInstance() != null) {
				VDONativePlugin.Call("LoadRewardAd", apiKeyFromInit);
			}
		}

		public static void showRewardAd(int rewardAmount,string rewardName, string userId, string secretKey) {
			if(iOSEnvironment()) {
				_showRewardAd(rewardAmount, rewardName, userId, secretKey);
			} else if(AndroidEnvironment() && AndroidPluginInstance() != null) {
				VDONativePlugin.Call("ShowRewardAd",
														 secretKey,
														 userId,
														 rewardName,
														 Convert.ToString(rewardAmount));
			}
		}

		public static void setDemograpics(int age, string birthDate, string gender, string maritalStatus,
		                     string ethnicity) {
			if(iOSEnvironment()) {
				 _setDemograpics(age,birthDate,gender,maritalStatus,ethnicity);
			} else if(AndroidEnvironment() && AndroidPluginInstance() != null) {
				VDONativePlugin.Call("SetAdRequestUserParams",
					Convert.ToString(age),
					birthDate,
					gender,
					maritalStatus,
					ethnicity, "", "", "", "", "");
			}
		}

		public static void setLocation(string dmaCode, string postal, string curPostal, string latitude, string longitude) {
			if(iOSEnvironment()) {
				_setLocation(dmaCode,postal,curPostal,latitude,longitude);
			} else if(AndroidEnvironment() && AndroidPluginInstance() != null) {
				VDONativePlugin.Call("SetAdRequestUserParams",
					"", "", "", "", "",
					dmaCode,
					postal,
					curPostal,
					latitude,
					longitude);
			}
		}

		public static void setAppInfo(string appName, string pubName,
		                 string appDomain, string pubDomain,
		                 string storeUrl, string iabCategory) {
		  if(iOSEnvironment()) {
				_setAppInfo(appName,pubName,appDomain,pubDomain,storeUrl,iabCategory);
		  } else if(AndroidEnvironment() && AndroidPluginInstance() != null) {
				VDONativePlugin.Call("SetAdRequestAppParams",
					appName,
					pubName,
					appDomain,
					pubDomain,
	        storeUrl,
					iabCategory);
			}
	  }

		public static void setPrivacySettings(bool gdprApplies, string gdprConsentString) {
			if(iOSEnvironment()) {
				_setPrivacySettings(gdprApplies,gdprConsentString);
			}
		}

		public static void setCustomSegmentProperty(string key, string value) {
			if(iOSEnvironment()) {
				_setCustomSegmentProperty(key,value);
			} else if(AndroidEnvironment()) {

			}
		}

		public static string getCustomSegmentProperty(string key) {
			if(iOSEnvironment()) {
				return _getCustomSegmentProperty(key);
			} else if(AndroidEnvironment()) {

			}
			return null;
		}

		public static Dictionary<string,string> getAllCustomSegmentProperties() {
			if(iOSEnvironment()) {
				string jsonRep = _getAllCustomSegmentProperties();
				return JsonUtility.FromJson<Dictionary<string,string>>(jsonRep);
			} else if(AndroidEnvironment()) {

			}
			return null;
		}

		public static void deleteCustomSegmentProperty(string key) {
			if(iOSEnvironment()) {
				_deleteCustomSegmentProperty(key);
			} else if(AndroidEnvironment()) {

			}
		}

		public static void deleteAllCustomSegmentProperties() {
			if(iOSEnvironment()) {
				_deleteAllCustomSegmentProperties();
			} else if(AndroidEnvironment()) {

			}
		}

		//MARK: - Android-only
		public static void SetAdRequestTestMode(bool isTestMode, string testID) {
	      Debug.Log("SetAdRequestTestMode...");
	      if (AndroidEnvironment() && AndroidPluginInstance() != null) {
	          VDONativePlugin.Call("SetTestModeEnabled", isTestMode, testID);
	      }
	  }

		//This method calls Native Method to Check Reward Ad Availability
	  //Returns true if Available and ready else return false
	  public static bool IsRewardAdAvailableToShow() {
	      Debug.Log("VdopiaPluginCheck Reward...");
	      bool isAvailable = false;
	      if (AndroidEnvironment() && AndroidPluginInstance() != null) {
	          isAvailable = VDONativePlugin.Call<bool>("IsRewardAdAvailableToShow");
	      }

	      Debug.Log("Is Reward Ad available: " + isAvailable);
	      return isAvailable;
	  }

	  public static bool IsInterstitialAdAvailableToShow() {
	     Debug.Log("VdopiaPluginCheck Interstitial...");
	     bool isAvailable = false;
	     if (AndroidEnvironment() && AndroidPluginInstance() != null) {
	         isAvailable = VDONativePlugin.Call<bool>("IsInterstitialAdAvailableToShow");
	     }

	     Debug.Log("Is Interstitial Ad available: " + isAvailable);
	     return isAvailable;
	 }

	  //OPTIONAL! Set unique user id of your application, if you wish.
	  public static void SetUserId(string userId) {
	     Debug.Log("SetUserId: " + userId);
	     if (AndroidEnvironment() && AndroidPluginInstance() != null) {
	         VDONativePlugin.Call("SetUserId", userId);
	     }
	  }

	  public static string GetRewardAdWinner() {
	      try {
	          return VDONativePlugin.Call<string>("GetRewardAdWinner");
	      } catch (Exception e) {
	          Debug.Log("GetRewardAdWinner failed: " + e);
	          return "";
	      }
	  }

	  public static string GetInterstitialAdWinner() {
	      try {
	          return VDONativePlugin.Call<string>("GetInterstitialAdWinner");
	      }catch(Exception e) {
	          Debug.Log("GetInterstitialAdWinner failed: " + e);
	          return "";
	      }
	  }

		//Mark: - callbacks for Android
		private static void onVdopiaEventReceiver(string adType, string eventName) {
	        Debug.Log("Ad Event Received : " + eventName + " : For Ad Type : " + adType);
					if((eventName.Contains("INTERSTITIAL") && interRec == null) ||
					   (eventName.Contains("REWARD") && rewardRec == null)) {
						Debug.Log("No callback listener detected");
						return;
					}

	        if (eventName == "INTERSTITIAL_LOADED") {
						interRec.onInterstitialLoaded("");
	        } else if (eventName == "INTERSTITIAL_FAILED") {
						interRec.onInterstitialFailed("");
	        } else if (eventName == "INTERSTITIAL_SHOWN") {
						interRec.onInterstitialShown("");
	        } else if (eventName == "INTERSTITIAL_DISMISSED") {
						interRec.onInterstitialDismissed("");
	        } else if (eventName == "INTERSTITIAL_CLICKED") {
						interRec.onInterstitialClicked("");
	        } else if (eventName == "REWARD_AD_LOADED") {
						rewardRec.onRewardLoaded("");
	        } else if (eventName == "REWARD_AD_FAILED") {
						rewardRec.onRewardFailed("");
	        } else if (eventName == "REWARD_AD_SHOWN") {
						rewardRec.onRewardShown("");
	        } else if (eventName == "REWARD_AD_SHOWN_ERROR") {
						rewardRec.onRewardFailed("");
	        } else if (eventName == "REWARD_AD_DISMISSED") {
						rewardRec.onRewardDismissed("");
	        } else if (eventName == "REWARD_AD_COMPLETED") {
						rewardRec.onRewardFinished("");
	            //If you setup server-to-server (S2S) rewarded callbacks you can
	            //assume your server url will get hit at this time.
	            //Or you may choose to reward your user from the client here.

	        }
	   }
	}
}
