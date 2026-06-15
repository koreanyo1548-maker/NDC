using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThirdParty
{
    public class GoogleReview: MonoBehaviour
    {
        // private ReviewManager _reviewManager;
        //
        // public void Start()
        // {
        //     _reviewManager = new ReviewManager();
        //
        //     StartCoroutine(RequestReviewRoutine());
        // }
        //
        // IEnumerator RequestReviewRoutine()
        // {
        //     var requestFlowOperation = _reviewManager.RequestReviewFlow();
        //     yield return requestFlowOperation;
        //     if (requestFlowOperation.Error != ReviewErrorCode.NoError)
        //     {
        //         // Log error. For example, using requestFlowOperation.Error.ToString().
        //         yield break;
        //     }
        //     var playReviewInfo = requestFlowOperation.GetResult();
        //     
        //     var launchFlowOperation = _reviewManager.LaunchReviewFlow(playReviewInfo);
        //     yield return launchFlowOperation;
        //     playReviewInfo = null; // Reset the object
        //     if (launchFlowOperation.Error != ReviewErrorCode.NoError)
        //     {
        //         // Log error. For example, using requestFlowOperation.Error.ToString().
        //         yield break;
        //     }
        //     
        //     Destroy(gameObject);
        // }
    }
}