using System.Runtime.InteropServices;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using Uduino;
using System;
using UnityEngine.SceneManagement;
using System.IO;

public static class BikeModeExtensions
{
    public static string GetTacxString(this BikeMode mode)
    {
        switch (mode)
        {
            case BikeMode.Lab:
                return "Tacx Flux-2 36688";
            case BikeMode.Forschungsfest:
                return "Tacx Flux-2 18201";
            default:
                return "Bikmode not set";
        }
    }
}

public class GameControllerScript : MonoBehaviour
{
    public BikeMode bikemode;
    public bool loadArduinoWithNewCode = false;
    public float SteeringAngle = 0.0f;
    public float BikeSpeed = 0f;
    public float previousBikeSpeed = 0f;
    public float ISteeringAngle = 0f;
    public GameObject Bicycle = null;

    void Start()
    {
        //Uduino
        UduinoManager.Instance.OnDataReceived += ReadEternityBike;

        var spawns = GameObject.Find("Spawns");
        var tmp = spawns.GetComponentsInChildren<Transform>(true);
        spawnPoints = new Transform[tmp.Length - 1];
        for (int i = 1; i < tmp.Length; ++i)
        {
            spawnPoints[i - 1] = tmp[i];
        }
        var bike = GameObject.Find("EternityBike");
    }
    public bool controller_mode = true;

    Transform[] spawnPoints = null;
    int currentSpawnPoint = 0;

    void Update()
    {
        // for testing issues
        if (loadArduinoWithNewCode)
        {
            /*String[] words = bikemode.GetTacxString().Split(' ');
            UduinoManager.Instance.sendCommand("setTacx0", words[0]);
            UduinoManager.Instance.sendCommand("setTacx1", words[1]);
            UduinoManager.Instance.sendCommand("setTacx2", words[2]);*/

            String[] words = bikemode.GetTacxString().Split(' ');
            // Debug.Log("TACX: " + words[2]);
            UduinoManager.Instance.sendCommand("tacx", words[2]);


            //UduinoManager.Instance.CloseAllDevices();
            //UduinoManager.Instance.DiscoverPorts();
            // TODO deactivate bike until everything is set up!
        }

        if (Input.GetKeyDown("r"))
        { //If you press R
            SceneManager.LoadScene("TableLogPos_Unity"); //Load scene called Game
            Debug.Log("Reset Scene");
        }
        if (Input.GetKeyDown("p"))
        {
            Bicycle = GameObject.Find("EternityBike");
            var spawnpoint = spawnPoints[currentSpawnPoint];
            currentSpawnPoint = (currentSpawnPoint + 1) % spawnPoints.Length;

            Bicycle.transform.position = spawnpoint.position;
            Bicycle.transform.rotation = spawnpoint.rotation;

            Debug.Log("Load Parking lot");
        }
        if (Input.GetKeyDown("c"))
        {
            controller_mode = !controller_mode;
            Debug.Log("Controller Mode " + controller_mode);
        }
    }

    private void UpdateValue(ref float value, float input, float step, float min, float max)
    {
        if (0 < input)
        {
            value = Mathf.Clamp(value + step, min, max);
        }
        else if (0 > input)
        {
            value = Mathf.Clamp(value - step, min, max);
        }
        else if (value > 0)
        {
            value = Mathf.Clamp(value - step, 0, max);
        }
        else if (value < 0)
        {
            value = Mathf.Clamp(value + step, min, 0);
        }
    }

    void ReadEternityBike(string data, UduinoDevice device)
    {

        Debug.Log("Controller Mode: " + controller_mode);
        Bicycle = GameObject.Find("EternityBike");
        string[] values = data.Split(',');

        Debug.Log("values len " + values.Length);
        Debug.Log(values[0]);

        //READ AND VALIDATE INPUT
        float Velocity = 0;
        if (controller_mode)
        {
            float vertical = Input.GetAxis("Vertical");
            Velocity = vertical * 20;
            Debug.Log("bikespeed: " + Velocity + ", vertical: " + vertical);
        }
        else
        {
            int i;
            if (int.TryParse(values[0].Substring(0, 1), out i))
            {
                Debug.LogWarning("values[0]: " + values[0]);
                Velocity = float.Parse(values[0]);
                Debug.LogWarning("bikespeed: " + Velocity);
            }

        }
        BikeSpeed = Velocity;
        previousBikeSpeed = BikeSpeed;
        float iSteeringAngle = 0;

        if (controller_mode)
        {
            float vertical = Input.GetAxis("Horizontal");
            iSteeringAngle = Math.Max(Math.Min(vertical, 1.0f), -1.0f) * 180;

        }
        else
        {
            iSteeringAngle = BikeControllerScript.steeringAngle; // -90, 0, +90
        }
        Debug.Log("iSteeringAngle before 450: " + iSteeringAngle);

        // -90 to 90
        ISteeringAngle = iSteeringAngle;

        // -1 to 1
        SteeringAngle = iSteeringAngle.Remap(90, -90, 1.0f, -1.0f);
        SteeringAngle = (float)Math.Round(SteeringAngle * 100f) / 100f;

        Debug.Log("Steering ANGLE == " + SteeringAngle + " IST " + ISteeringAngle);
        Debug.Log("Velocity: " + Velocity + " SteeringAngle: " + iSteeringAngle + " SteeringAngle: " + SteeringAngle);
    }
}

public static class ExtensionMethods
{
    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
