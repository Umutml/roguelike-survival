using System;
using System.Collections;
using _Scripts.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_IOS
using Unity.Advertisement.IosSupport;
using static Unity.Advertisement.IosSupport.ATTrackingStatusBinding;
using UnityEngine.iOS;
#endif


namespace Managers
{
    public class PermissionsManager : MonoBehaviour
    {
        private bool _attAsked;
        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!_attAsked)
            {
                _attAsked = true;
                InitPermissionsAskATT();
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void InitPermissionsAskATT()
        {
#if UNITY_IOS && !UNITY_EDITOR
            try
            {
                AskATTRequest();
            }
            catch (Exception e)
            {
                LoggerNS.LogError($"ATT: Ask Permissions Error - AskATTRequest() {e.Message}");
            }
#endif
        }

        public void AskATTRequest()
        {
#if UNITY_IOS && !UNITY_EDITOR
            LoggerNS.Log("ATT: Ask Permissions");
            // check with iOS to see if the user has accepted or declined tracking
            var status = GetAuthorizationTrackingStatus();
            LoggerNS.Log("ATT: Ask Permissions - Status: " + status.ToString());
            
            Version currentVersion = new Version(Device.systemVersion);
            LoggerNS.Log("ATT: Current Version: " + currentVersion.ToString());
            
            Version ios14 = new Version("14.5");

            if (status == AuthorizationTrackingStatus.NOT_DETERMINED && currentVersion >= ios14)
            {
                RequestAuthorizationTracking();
                LoggerNS.Log("ATT: Permission Asked");
                StartCoroutine(CheckAuthorizationStatus());
                
            }
#else


            LoggerNS.Log("ATT Unity iOS Support: App Tracking Transparency status not checked, " +
                "because the platform is not iOS.");
#endif
        }

#if UNITY_IOS && !UNITY_EDITOR
        private IEnumerator CheckAuthorizationStatus()
        {
#if UNITY_IOS
            while (true)
            {
                var status = GetAuthorizationTrackingStatus();

                if (status != ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
                {
                    AuthorizationTrackingReceived((int)status);
                    yield break;
                }
                yield return new WaitForSeconds(1f); // Check every second
            } 
            #else
            yield break;
            #endif
        }
#endif

        private void AuthorizationTrackingReceived(int status)
        {
#if UNITY_IOS && !UNITY_EDITOR
            //Statuslar hakkında detaylı bilgi için :
            //https://developer.apple.com/documentation/apptrackingtransparency/attrackingmanager/authorizationstatus
            Debug.LogFormat("ATT: Tracking status received: {0}", status);
            
            //İstek sonrası izin verildiğinde.
            if (status == (int)AuthorizationTrackingStatus.AUTHORIZED)
            {
               
            }
            //İstek sonrası izin verilmediğinde.
            if (status == (int)AuthorizationTrackingStatus.DENIED)
            {
                
            }

            //Kullanıcı herhangi bir seçeneği seçmediyse. Muhtemelen kod buraya gelmeyecek.
            if (status == (int)AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                //Bilerek boş bırakıldı. İhtiyaç halinde araştırılacak.
            }
            //Host tarafından izin verilmezse. Muhtemelen kod buraya gelmeyecek.
            if (status == (int)AuthorizationTrackingStatus.RESTRICTED)
            {
                //Bilerek boş bırakıldı. İhtiyaç halinde araştırılacak.
            }
#endif
        }
    }
}
