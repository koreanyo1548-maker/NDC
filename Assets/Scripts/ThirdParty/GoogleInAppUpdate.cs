using System.Collections;
using Managers;
using UIs.Etc;
using UnityEngine;
using Utils;

namespace ThirdParty
{
    public class GoogleInAppUpdate: MonoBehaviour
    {
        // private AppUpdateManager appUpdateManager;
        // private AppUpdateInfo appUpdateInfoResult;
        // public void Start()
        // {
        //     #if !UNITY_EDITOR
        //     appUpdateManager = new AppUpdateManager();
        //     StartCoroutine(CheckForUpdate());
        //     #endif
        // }
        //
        // IEnumerator CheckForUpdate()
        // {
        //     var appUpdateInfoOperation =
        //         appUpdateManager.GetAppUpdateInfo();
        //
        //     // Wait until the asynchronous operation completes.
        //     yield return appUpdateInfoOperation;
        //
        //     if (appUpdateInfoOperation.IsSuccessful)
        //     {
        //         appUpdateInfoResult = appUpdateInfoOperation.GetResult();
        //         // Check AppUpdateInfo's UpdateAvailability, UpdatePriority,
        //         // IsUpdateTypeAllowed(), etc. and decide whether to ask the user
        //         // to start an in-app update.
        //         
        //         if (appUpdateInfoResult.UpdateAvailability == UpdateAvailability.UpdateAvailable)
        //         {
        //             var go = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Popup/UI_AppUpdate"));
        //             go.AddComponent<UI_AppUpdate>().Set(UpdateApp);
        //         }
        //         else
        //         {
        //             Destroy(gameObject);
        //         }
        //     }
        //     else
        //     {
        //         // Log appUpdateInfoOperation.Error.
        //     }
        // }
        //
        // private void UpdateApp()
        // {
        //     StartCoroutine(StartImmediateUpdate());
        // }
        //
        // IEnumerator StartImmediateUpdate()
        // {
        //     var appUpdateOptions = AppUpdateOptions.ImmediateAppUpdateOptions();
        //     // Creates an AppUpdateRequest that can be used to monitor the
        //     // requested in-app update flow.
        //     var startUpdateRequest = appUpdateManager.StartUpdate(
        //         // The result returned by PlayAsyncOperation.GetResult().
        //         appUpdateInfoResult,
        //         // The AppUpdateOptions created defining the requested in-app update
        //         // and its parameters.
        //         appUpdateOptions);
        //     yield return startUpdateRequest;
        //
        //     // If the update completes successfully, then the app restarts and this line
        //     // is never reached. If this line is reached, then handle the failure (for
        //     // example, by logging result.Error or by displaying a message to the user).
        // }
    }
}