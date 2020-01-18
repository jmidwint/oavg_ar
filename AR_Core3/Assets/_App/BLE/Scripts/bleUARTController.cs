using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class bleUARTController : MonoBehaviour
{
    public string DeviceName = "SP_BLE";
    public string ServiceUUID = "0001";
    public string SubscribeCharacteristic = "0003";
    public string ReadCharacteristic = "0003";
    public string WriteCharacteristic = "0002";
    public Text str2;
    public Text stat;
	public Text received;
    public Text sendText;

    public GameObject AppController;

    enum States
    {
        None,
        Scan,
        ScanRSSI,
        Connect,
        Subscribe,
        Read,
        Unsubscribe,
        Disconnect,
    }

    public bool run = false;

    private bool _connected = false;
    private float _timeout = 0f;
    private States _state = States.None;
    private string _deviceAddress;
    private bool _foundSubscribeID = false;
    private bool _foundReadID = false;
    private bool _foundWriteID = false;
    private byte[] _dataBytes = null;
    private bool _rssiOnly = false;
    private int _rssi = 0;

    private string txt = "";

    void Reset()
    {
        _connected = false;
        _timeout = 0f;
        _state = States.None;
        _deviceAddress = null;
        _foundSubscribeID = false;
        _foundWriteID = false;
        _dataBytes = null;
        _rssi = 0;
    }

    void SetState(States newState, float timeout)
    {
        _state = newState;
        _timeout = timeout;
    }

    string FullUUID(string uuid)
    {
        return "6E40" + uuid + "-B5A3-F393-E0A9-E50E24DCCA9E";
    }

    void StartProcess()
    {
        Reset();
        BluetoothLEHardwareInterface.Initialize(true, false, () => {

            SetState(States.Scan, 0.1f);
            stat.text = "Scan";

        }, (error) => {

            BluetoothLEHardwareInterface.Log("Error during initialize: " + error);
        });
    }

    void Start()
    {
        StartProcess();
    }

    // Update is called once per frame
    void Update()
    {
        if (_timeout > 0f)
        {
            _timeout -= Time.deltaTime;
            if (_timeout <= 0f)
            {
                _timeout = 0f;

                switch (_state)
                {
                    case States.None:
                        break;

                    case States.Scan:
                        BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null, (address, name) =>
                        {
                            if (!_rssiOnly)
                            {
                                if (name.Contains(DeviceName))
                                {
                                    txt += DeviceName + " found.";str2.text = txt;
                                    BluetoothLEHardwareInterface.StopScan();
                                    _deviceAddress = address;
                                    SetState(States.Connect, 0.5f);
                                    stat.text = "Connect";
                                }
                            }
                        }, (address, name, rssi, bytes) =>
                        {
                            if (name.Contains(DeviceName))
                            {
                                txt += DeviceName + " fnd."; str2.text = txt;
                                if (_rssiOnly)
                                {
                                    _rssi = rssi;
                                }
                                else
                                {
                                    BluetoothLEHardwareInterface.StopScan();
                                    _deviceAddress = address;
                                    SetState(States.Connect, 0.5f);
                                    stat.text = "Connect";
                                }
                            }
                        }, _rssiOnly);
                        txt += "."; str2.text = txt;

                        if (_rssiOnly)
                            SetState(States.ScanRSSI, 0.5f);
                        break;

                    case States.ScanRSSI:
                        break;

                    case States.Connect:
                        _foundReadID = false;
                        _foundWriteID = false;

                        BluetoothLEHardwareInterface.ConnectToPeripheral(_deviceAddress, null, null, (address, serviceUUID, characteristicUUID) =>
                        {

                            if (IsEqual(serviceUUID, ServiceUUID))
                            {
                                //_foundSubscribeID = _foundSubscribeID || IsEqual(characteristicUUID, SubscribeCharacteristic);
                                _foundReadID = _foundReadID || IsEqual(characteristicUUID, ReadCharacteristic);
                                _foundWriteID = _foundWriteID || IsEqual(characteristicUUID, WriteCharacteristic);

                                if (_foundReadID && _foundWriteID)
                                {
                                    _connected = true;
                                    txt +=  " connected/"; str2.text = txt;
                                    SendByte((byte)0x01);
                                    SetState(States.Subscribe, 2f);
                                    stat.text = "Subscribe";
                                }
                            }
                        });
                        break;

                    case States.Read:
                        /*
                        BluetoothLEHardwareInterface.WriteCharacteristic(_deviceAddress, FullUUID(ServiceUUID), FullUUID(WriteCharacteristic), data, data.Length, true, (characteristicUUID) => {
                        BluetoothLEHardwareInterface.ReadCharacteristic(_deviceAddress, FullUUID(ServiceUUID), FullUUID(ReadCharacteristic), null, (address, characteristicUUID, bytes) =>
                        {
                            _state = States.Read;
                            _dataBytes = bytes;
                            txt += _dataBytes + "/"; str2.text = txt;
                        });
                        */
                        break;

                    case States.Subscribe:
                        BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(_deviceAddress, FullUUID(ServiceUUID), FullUUID(ReadCharacteristic), null, (address, characteristicUUID, bytes) =>
                        {
                            _state = States.Subscribe;
                            _dataBytes = bytes;
							//string receive = ByteArrayToString(_dataBytes);
							string receive = Encoding.ASCII.GetString(_dataBytes);

                            // process the command receive over BLE
                            processCommand(receive);
							
                        });
                           
                        break;


                    case States.Unsubscribe:
                        BluetoothLEHardwareInterface.UnSubscribeCharacteristic(_deviceAddress, FullUUID(ServiceUUID), ReadCharacteristic, null);
                        SetState(States.Disconnect, 4f);
                        stat.text = "Disconnect";
                        txt += " Unsubscribe/"; str2.text = txt;
                        break;

                    case States.Disconnect:
                        if (_connected)
                        {
                            BluetoothLEHardwareInterface.DisconnectPeripheral(_deviceAddress, (address) => {
                                BluetoothLEHardwareInterface.DeInitialize(() => {

                                    _connected = false;
                                    txt += " disconnected/"; str2.text = txt;
                                    _state = States.None;
                                    SetState(States.Scan, 0.5f);
                                    stat.text = "Scan";
                                });
                            });
                        }
                        else
                        {
                            BluetoothLEHardwareInterface.DeInitialize(() => {

                                _state = States.None;
                                stat.text = "None";
                            });
                        }
                        break;
                }
            }
        }

    }

    /** 
     * Process thew command received over BLE
     */
     void processCommand(string input)
    {
        char command = input[0];
        string arg = input.Substring(1);

        switch (command)
        {
            case 'I':  // Information
                received.text = "I: "+ arg;
                break;

            case 'C': // Compass 
                received.text = "C: ";
                string [] args = arg.Split(char.Parse(",")); // to convert to float use float.Parse(string)
                for (int i=0; i<args.Length; i++)
                {
                    received.text += args[i] + "  ";
                }
                break;
        }
    }

    bool IsEqual(string uuid1, string uuid2)
    {
        if (uuid1.Length == 4)
            uuid1 = FullUUID(uuid1);
        if (uuid2.Length == 4)
            uuid2 = FullUUID(uuid2);

        return (uuid1.ToUpper().CompareTo(uuid2.ToUpper()) == 0);
    }


    /*
    void SendByte(byte value)
    {
        byte[] data = new byte[] { value };
        BluetoothLEHardwareInterface.WriteCharacteristic(_deviceAddress, ServiceUUID, WriteCharacteristic, data, data.Length, true, (characteristicUUID) => {

            BluetoothLEHardwareInterface.Log("Write Succeeded");
            txt += " Write Succeeded/"; str2.text = txt;
        });
    }
    */
    void SendByte(byte value)
    {
        byte[] data = new byte[] { value };
        BluetoothLEHardwareInterface.WriteCharacteristic(_deviceAddress, FullUUID(ServiceUUID), FullUUID(WriteCharacteristic), data, data.Length, true, (characteristicUUID) => {

            BluetoothLEHardwareInterface.Log("Write Succeeded");
            //txt += " Write Succ/"; str2.text = txt;
        });
    }

    void SendBytes(byte[] data)
    {
        BluetoothLEHardwareInterface.Log(string.Format("data length: {0} uuid: {1}", data.Length.ToString(), FullUUID(WriteCharacteristic)));
        BluetoothLEHardwareInterface.WriteCharacteristic(_deviceAddress, FullUUID(ServiceUUID), FullUUID(WriteCharacteristic), data, data.Length, true, (characteristicUUID) => {

            BluetoothLEHardwareInterface.Log("Write Succeeded");
            //txt += " Write Succ/"; str2.text = txt;
        });
    }

    string ByteArrayToString(byte[] val)
    {
        string b = "";
        int len = val.Length;

        for (int i = 0; i < len; i++)
        {
            if (i != 0)
            {
                b += ",";
            }
            b += val[i].ToString();
        }

        return b;
    }

    // Send All Motors Stop 
    public void Stop()
    {
        Send("M0,0");
        run = false;
        AppController.GetComponent<MyObjectManipulationController>().bleStatus = false;
    }

    // Send Resume
    public void Run()
    {
        run = true;
        AppController.GetComponent<MyObjectManipulationController>().bleStatus = true;
        Send("R100,0");
    }



    public void Send(string msg)
    {
        Debug.Log("Sending:  " + msg);
        if (_state == States.Subscribe)
        {
            //if (msg.Length > 0)
            //{
                byte[] bytes = ASCIIEncoding.UTF8.GetBytes(msg);
                if (bytes.Length > 0)
                {
                    Debug.Log("Sending Text");
                    SendBytes(bytes);
                    txt += ".";//msg+" sent/"; str2.text = txt;
                }

                //Send.text = "";
            //}
        }
    }

    public void Exit()
    {
        if (_connected)
        {
            BluetoothLEHardwareInterface.UnSubscribeCharacteristic(_deviceAddress, FullUUID(ServiceUUID), ReadCharacteristic, null);
            BluetoothLEHardwareInterface.DisconnectPeripheral(_deviceAddress, (address) => {
                BluetoothLEHardwareInterface.DeInitialize(() => {
                    _connected = false;
                    txt += " disconnected/"; str2.text = txt;
                    _state = States.None;
                });
            });
        }
        Application.Quit();
    }
}
