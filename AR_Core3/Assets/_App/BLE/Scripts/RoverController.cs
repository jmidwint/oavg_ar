using UnityEngine;
using UnityEngine.UI;


public class RoverController : MonoBehaviour
{
    public GameObject self;
    public Text X;
    public Text Y;
    public Text M1;
    public Text M2;
    public Slider Speed;

    private float lastX, lastY;


    private static bleUARTController ble;

    private float m1, m2;


	void Start()
	{
        ble = self.GetComponent<bleUARTController>();
	}


	void Update()
	{

        /*
        // Rotate target object.
        transform.Rotate( Vector3.down, 500.0f * Time.deltaTime * inputDevice.Direction.X, Space.World );
		transform.Rotate( Vector3.right, 500.0f * Time.deltaTime * inputDevice.Direction.Y, Space.World );

        X.text = "Xaxis= " + Mathf.Round(100 * inputDevice.Direction.X);
        Y.text = "Yaxis= " + Mathf.Round(100 * inputDevice.Direction.Y);
        */
        /* --- 
        m1 = inputDevice.Direction.Y + inputDevice.Direction.X * -1;
        m2 = inputDevice.Direction.Y + inputDevice.Direction.X;
        if (m1 > 1) m1 = 1;
        if (m1 < -1) m1 = -1;
        if (m2 > 1) m2 = 1;
        if (m2 < -1) m2 = -1;

        M1.text = "M1= " + Mathf.Round(m1 * 100);
        M2.text = "M2= " + Mathf.Round(m2 * 100);
        
        //if (inputDevice.Direction.X != 0 && inputDevice.Direction.Y != 0)
        //{
        //ble.Send("M" + Mathf.Round(m1 * 100) + "," + Mathf.Round(m2 * 100));
        //-- send Xaxis and Yaxis of the gamepad.
        m1 = Mathf.Round(inputDevice.Direction.X * 100);
        m2 = Mathf.Round(inputDevice.Direction.Y * 100);
        if (m1 != lastX && m2 != lastY)
        {
            ble.Send("M" + m1 + "," + m2);
            lastX = m1;
            lastY = m2;
        }
        //}
        */
    }

    public void SendSpeed(float value) {
        //ble.Send("V" + Speed.text.Trim() + ",0");
        ble.Send("V" + Mathf.Round(value) + ",0");
    }

}

