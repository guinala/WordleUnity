using System;
using UnityEngine;
using UnityEngine.Advertisements;
public class AdsController : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public static AdsController Instance;
    public static Action OnRewardedAdCompleted;

    private bool rewardedAdFlowInProgress;

    public string androidGameID;
    public string iOSGameID;
    private string selectedGameID;
    public string idAdvertisementsAndroid;
    public string idAdvertisementsIOS;
    private string selectedAdvertisementsID;
    
    public bool testMode = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            StartAdvertisements();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void StartAdvertisements()
    {
#if UNITY_ANDROID
            selectedGameID = androidGameID;
            selectedAdvertisementsID = idAdvertisementsAndroid;
#elif UNITY_IOS
            selectedGameID = iOSGameID;
            selectedAdvertisementsID = idAdvertisementsIOS;
#elif UNITY_EDITOR
            selectedGameID = androidGameID; 
            selectedAdvertisementsID = idAdvertisementsAndroid;
#endif

        if (!Advertisement.isInitialized)
        {
            Advertisement.Initialize(selectedGameID, testMode, this);
        }
    }

    public void OnInitializationComplete()
    {
        Debug.Log("OnInitializationComplete");
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log("OnInitializationFailed: " + message);
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        Advertisement.Show(placementId, this);
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        rewardedAdFlowInProgress = false;
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        rewardedAdFlowInProgress = false;
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        rewardedAdFlowInProgress = false;

        if(placementId.Equals(selectedAdvertisementsID) && showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            OnRewardedAdCompleted?.Invoke();
        }
    }

    public bool IsRewardedAdFlowInProgress() => rewardedAdFlowInProgress;

    public bool ShowAdvertisement()
    {
        if (rewardedAdFlowInProgress)
        {
            return false;
        }

        rewardedAdFlowInProgress = true;
        Advertisement.Load(selectedAdvertisementsID, this);
        return true;
    }
}
