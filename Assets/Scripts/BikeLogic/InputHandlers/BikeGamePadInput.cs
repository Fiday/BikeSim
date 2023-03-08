using UnityEngine;
using UnityEngine.InputSystem;

/*
This script allows controlling the bike with a gamepad.
steering: left stick
pedal: right trigger
brake/backwads: left trigger

the code simply sets the speed and steering angle in bike controller script.
*/

[RequireComponent(typeof(BikeController))]
public class BikeGamePadInput : MonoBehaviour
{
    [SerializeField] private float maxSteeringAngle = 60f;
    [SerializeField] private float maxPedalStrength = 2f;
    [SerializeField] private float drag = 0.1f;
    [SerializeField] private float friction = 0.1f;
    [SerializeField] private float maxBrakeStrength = 5f;
    private BikeController bikeControllerScript;
    private bool controllerConnectedWarning = true;
    private bool driveBackwards = false;
    void Start()
    {
        bikeControllerScript = GetComponent<BikeController>();
    }
    void Update()
    {
        if(Gamepad.current == null)
        {
            if (controllerConnectedWarning)
            {
                Debug.LogWarning("Gamepad Input is active, but no controller is connected");
                controllerConnectedWarning = false;
            }
            return;
        }
        controllerConnectedWarning = true;

        float steeringDiff = Gamepad.current.leftStick.ReadValue().x * maxSteeringAngle - bikeControllerScript.steeringAngle;
        float pedalStrength = Gamepad.current.rightTrigger.ReadValue();
        float brakeStrength = Gamepad.current.leftTrigger.ReadValue();

        float currentSpeed = bikeControllerScript.speedInMetersPerSecond;

        if(currentSpeed < 0.01f && brakeStrength == 0)
            driveBackwards = true;
        if (currentSpeed > 0.02f)
            driveBackwards = false;

        if (driveBackwards && pedalStrength == 0)
            pedalStrength = -brakeStrength / 5f;

        float speedAbs = Mathf.Abs(currentSpeed);
        float speed = Mathf.Sign(currentSpeed) * (speedAbs - (speedAbs * speedAbs * drag + friction) * Time.deltaTime); //reducing speed depending on friction and drag
        speed += pedalStrength * maxPedalStrength * Time.deltaTime; //add pedal power
        if(!driveBackwards) speed = Mathf.Max(0, speed - brakeStrength * maxBrakeStrength * Time.deltaTime); //brake

        bikeControllerScript.speedInMetersPerSecond = speed;
        bikeControllerScript.steeringAngle += steeringDiff * Time.deltaTime; //steering is interpolated
    }
}