//---------------------------------------------------------------------
// This is a modified version of file: 
// <copyright file="ObjectManipulationController.cs" company="Google">
// Please refer to this file for Copyright.
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleARCore;


#if UNITY_EDITOR
    // Set up touch input propagation while using Instant Preview in the editor.
    using Input = GoogleARCore.InstantPreviewInput;
#endif

public class MyObjectManipulationController : MonoBehaviour
{
    /// <summary>
    /// The first-person camera being used to render the passthrough camera image (i.e. AR
    /// background).
    /// </summary>
    public Camera FirstPersonCamera;

    /// <summary>
    /// Archor stop lists
    /// </summary>
    public GameObject Stops;
    public Text stopsList;


    /// <summary>
    /// BLE Controller
    /// </summary>
    public GameObject bleController;

    /// <summary>
    /// True if the app is in the process of quitting due to an ARCore connection error,
    /// otherwise false.
    /// </summary>
    private bool m_IsQuitting = false;

    // Bluetooth status: true = send, false = don't send
    public bool bleStatus = true;

    // Display coordinate status
    private bool coorStatus = true;

    // Next Point number
    private int nextPoint = 1;

    /// <summary>
    /// The Unity Update() method.
    /// </summary>
    public void Update()
    {
        _UpdateApplicationLifecycle();
    }

    /// <summary>
    /// The Unity Awake() method.
    /// </summary>
    public void Awake()
    {
        // Enable ARCore to target 60fps camera capture frame rate on supported devices.
        // Note, Application.targetFrameRate is ignored when QualitySettings.vSyncCount != 0.
        Application.targetFrameRate = 30;
    }

    /// <summary>
    /// Check and update the application lifecycle.
    /// </summary>
    private void _UpdateApplicationLifecycle()
    {
        //bleStatus = bleController.GetComponent<bleUARTController>().run;
        if (coorStatus)
        {
            string nextStr = "" + nextPoint;
            stopsList.text = "Camera: " + Mathf.Round(FirstPersonCamera.transform.position.x * 1000) / 10.0F + " , " + Mathf.Round(FirstPersonCamera.transform.position.y * 1000) / 10.0F + " , " + Mathf.Round(FirstPersonCamera.transform.position.z * 1000) / 10f + "\n ";
            foreach (Transform child in Stops.transform)
            {
                GameObject go = child.gameObject;
                string pNumber = go.transform.Find("Manipulator(Clone)/Marker(Clone)/TPawn").GetComponent<TextMesh>().text;
                //Debug.Log("Strings: "+ pNumber +" : "+ nextStr);
                float Xdiff =  go.transform.position.x - FirstPersonCamera.transform.position.x;
                float Zdiff =  go.transform.position.z - FirstPersonCamera.transform.position.z;
                float distance = Mathf.Round(Mathf.Sqrt(Xdiff * Xdiff + Zdiff * Zdiff) * 1000) / 10.0F;
                stopsList.text = stopsList.text +pNumber+ ": " + Mathf.Round(go.transform.position.x * 1000) / 10.0f + " , " + Mathf.Round(go.transform.position.y * 1000) / 10.0f + " , " + Mathf.Round(go.transform.position.z * 1000) / 10.0f + " - D: " + distance + "cm\n ";

                //if (bleStatus) {
                    if (String.Compare(nextStr, pNumber) == 0)
                    {
                        //Debug.Log("Ready to send");
                        bleController.GetComponent<bleUARTController>().Send("P" + Mathf.Round(Xdiff * 100) + "," + Mathf.Round(Zdiff * 100));
                    }
                //}
            }
        }

        // Exit the app when the 'back' button is pressed.
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

        // Only allow the screen to sleep when not tracking.
        if (Session.Status != SessionStatus.Tracking)
        {
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        }
        else
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        if (m_IsQuitting)
        {
            return;
        }

        // Quit if ARCore was unable to connect and give Unity some time for the toast to
        // appear.
        if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
        {
            _ShowAndroidToastMessage("Camera permission is needed to run this application.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
        else if (Session.Status.IsError())
        {
            _ShowAndroidToastMessage(
                "ARCore encountered a problem connecting.  Please start the app again.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
    }

    // Update coordianates status
    public void UpdateCoordinates()
    {
        // Toogle coordinate display
        if (coorStatus)
        {
            coorStatus = false;
        }
        else
        {
            coorStatus = true;
        }
    }

    /// <summary>
    /// Actually quit the application.
    /// </summary>
    private void _DoQuit()
    {
        Application.Quit();
    }

    /// <summary>
    /// Show an Android toast message.
    /// </summary>
    /// <param name="message">Message string to show in the toast.</param>
    private void _ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity =
            unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject =
                    toastClass.CallStatic<AndroidJavaObject>(
                        "makeText", unityActivity, message, 0);
                toastObject.Call("show");
            }));
        }
    }
}

