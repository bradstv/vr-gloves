using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;

public class SerialCommunication : MonoBehaviour
{
    private SerialPort sp = new SerialPort("COM4", 115200);
    private VRGlove glove;
    private GameplayHand hand;

    // Start is called before the first frame update
    void Start()
    {
        glove = GetComponent<VRGlove>();
        hand = glove.hand.GetComponent<GameplayHand>();

        sp.Open();
        sp.ReadTimeout = 1000;
        sp.WriteTimeout = 1000;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        sendSerialData();
    }

    void sendSerialData()
    {
        if (sp.IsOpen)
        {
            try
            {
                sp.Write(glove.thermoValue + ";" + hand.fingerCurlAverages[0] + ";" + hand.fingerCurlAverages[1] + ";" + hand.fingerCurlAverages[2] + ";" + hand.fingerCurlAverages[3] + ";" + hand.fingerCurlAverages[4] + ";" + glove.buzzerToggles[0] + ";" + glove.buzzerToggles[1] + ";" + glove.buzzerToggles[2] + ";" + glove.buzzerToggles[3] + ";" + glove.buzzerToggles[4] + "\n");
            }
            catch (System.Exception)
            {
                sp.Close();
                throw;
            }
        }
    }
}
