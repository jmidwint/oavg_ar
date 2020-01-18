using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using Vuforia;

public class displayCoordinates : MonoBehaviour {
    public GameObject self;
    public Text xcoor;
    public Text ycoor;
    public Text zcoor;
    public Text distance;
    public Text angle;
    public GameObject imageTarget;

    private Renderer renderer;
    //TrackableBehaviour mTrackableBehaviour;

    private static bleUARTController ble;

    private float time = 0.0f;
    public float interpolationPeriod = 0.1f;

    private float oldx = 0, oldy = 0, oldz = 0;

    // Use this for initialization
    void Start () {
        ble = self.GetComponent<bleUARTController>();
        //mTrackableBehaviour = GetComponent<TrackableBehaviour>();

        //renderer = imageTarget.GetComponent<Renderer>();

    }

    // Update is called once per frame
    void Update () {

        // Camera is at origin x, y, z position = 0
        xcoor.text = "x = " + Mathf.Round(imageTarget.transform.position.x * 1000) / 10.0f + " cm";
        ycoor.text = "y = " + Mathf.Round(imageTarget.transform.position.y * 1000) / 10.0f + " cm";
        zcoor.text = "z = " + Mathf.Round(imageTarget.transform.position.z * 1000) / 10.0f + " cm";



        //float dist= Vector3.Distance(imageTarget.transform.position, transform.position);
        //Debug.Log(dist);
        //distance.text = "Dist = " + Mathf.Round(dist * 1000) / 10.0f +" cm";

        /* Calculate distance in plan Level: X-Z where the robot move
         * The camera at the origine, so, the distance between the origine and the point
         */
        float dist = Mathf.Sqrt((imageTarget.transform.position.x * imageTarget.transform.position.x) + (imageTarget.transform.position.z * imageTarget.transform.position.z));
        distance.text = "Dist = " + Mathf.Round(dist * 1000) / 10.0f + " cm";


        // Calculate Angle
        Vector3 dir = imageTarget.transform.position - transform.position;
        dir = imageTarget.transform.InverseTransformDirection(dir);
        //float angleValue = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;  // Y is the elevation now. The plane is Z-X
        float angleValue = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg - 90.0F;

        angle.text = "Angle: " + angleValue;

        time += Time.deltaTime; // update time interval

        if ((ble != null) && (time >= interpolationPeriod)) {
                //(ble != null) && (time >= interpolationPeriod)
                time = time - interpolationPeriod;  //reset time interval


            //if (dist > 0.0f)
            if (oldx == imageTarget.transform.position.x && oldy == imageTarget.transform.position.y && oldz == imageTarget.transform.position.z)
            {
                ble.Send("M0,0");

            }
            else
            {
                int Yaxis = 0;
                int Xaxis = 0;
                int Zposition = (int) Mathf.Round(imageTarget.transform.position.z * 100.0f);
                int Xposition = (int) Mathf.Round(imageTarget.transform.position.x * 100.0f);
                if (Zposition > 100) // Y more hte 1 meter go full speed
                {
                    Yaxis = 100;
                } else if (Zposition > 50) { // Slow down between 50 cm and 1 meter.
                    Yaxis = Zposition;
                } else {    // Z less then 50 cm stop
                    Yaxis = 0;
                }

                if(Mathf.Abs(Xposition) < 70)
                {
                    Xaxis = Xposition;
                } else if(Xposition > 0) {
                    Xaxis = 70;
                } else {
                    Xaxis = -70;
                }
                Debug.Log("M" + Xaxis + "," + Yaxis+" Sending Data x=" + transform.position.x + " oldX: " + oldx);
                ble.Send("M"+Xaxis+","+Yaxis);
            }
        }

        oldx = imageTarget.transform.position.x;
        oldy = imageTarget.transform.position.y;
        oldz = imageTarget.transform.position.z;
    }
}
