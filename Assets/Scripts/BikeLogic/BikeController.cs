using System;
using UnityEditor;
using UnityEngine;

/*
This script handles the transformation of the bike model and VR player.
speed and steering angle are public and set externally.

this script is meant to be used with the 'City Bike with LOD' asset, but should also work fine with other models.
https://assetstore.unity.com/packages/3d/vehicles/land/city-bike-with-lod-54310

LODs have been removed

!the bike body transform (child of the bike prefab, containing all parts and meshes) has a rotation of (-90, 0, 0)!
this transform is not used directly but beware of the affect on the local rotation of the wheels and handle bar
*/

[RequireComponent(typeof(Rigidbody))]
public class BikeController : MonoBehaviour
{
    public CheckableVariable<bool> resetBike = new(false);

    private enum InputMode
    {
        Gamepad,
        VR
    };

    private enum CameraMode
    {
        ThirdPerson,
        VR
    };

    [SerializeField] private InputMode inputMode = InputMode.Gamepad;
    [SerializeField] private CameraMode cameraMode = CameraMode.ThirdPerson;
    [SerializeField] private GameObject thirdPersonCamera;
    [SerializeField] private GameObject VRCamera;
    [SerializeField] private Transform vRPlayerPositionTransform; //empty to use for adjusting the VR player position
    [SerializeField] private Transform handlebar;
    [SerializeField] private Transform frontWheel;
    [SerializeField] private Transform rearWheel;
    [SerializeField] private float wheelRadius;

    [SerializeField] private float
        wheelbase = 1.0f; //distance between the center of both the front and rear axle of the bike. just play around until the turning radius looks good

    [Range(-10.0f, 10.0f)] [SerializeField]
    private float tiltFactor = 1.0f;

    public float steeringAngle = 0.0f;
    public float speedInMetersPerSecond;
    private Quaternion initialHandlebarRotation;
    private Rigidbody bikeRigidbody;
    private float lastBikeRotationY;
    private Vector3 lastRearWheelPosition;
    private Vector3 lastFrontWheelPosition;
    private Quaternion smoothBikeRotation;
    private float smoothTilt = 0.0f;
    private BikeGamePadInput gamePadInput;
    private BikeVRInput vrInput;
    private GameObject bike;
    private Vector3 initialBikePosition;
    private Quaternion initialBikeRotation;

    //debug
    [SerializeField] private bool drawDebug = false;
    private float smoothLeftWheelTravelDistance = 0.0f;
    private float smoothRightWheelTravelDistance = 0.0f;

    void Start()
    {
        bike = gameObject;
        initialBikePosition = bike.transform.position;
        initialBikeRotation = bike.transform.rotation;
        bikeRigidbody = GetComponent<Rigidbody>();
        if (cameraMode == CameraMode.VR)
        {
            thirdPersonCamera.SetActive(false);
        }

        lastRearWheelPosition = rearWheel.transform.position;
        lastFrontWheelPosition = frontWheel.transform.position;
        initialHandlebarRotation = handlebar.localRotation;
        lastBikeRotationY = bike.transform.localEulerAngles.y;
        smoothBikeRotation = bike.transform.rotation;

        gamePadInput = GetComponent<BikeGamePadInput>();
        vrInput = GetComponent<BikeVRInput>();

        if (gamePadInput)
            gamePadInput.enabled = false;
        else
            Debug.LogWarning("No BikeGamePadInput component available");

        if (vrInput)
            vrInput.enabled = false;
        else
            Debug.LogWarning("No BikeVRInput component available");
    }

    /*
    //FixedUpdate would be nicer for physics, but resulted in significant jitter.
    I believe the FixedUpdate rate is determined by the XR toolkit (in our case).
    Maybe interpolation could be a solution? Anyways works fine for now.
    */
    void Update()
    {
        //enabeling or disabeling input scripts based on selection

        if (gamePadInput) gamePadInput.enabled = inputMode == InputMode.Gamepad;
        if (vrInput) vrInput.enabled = inputMode == InputMode.VR;

        //rotating the wheels based on individual distance traveled in wheel direction
        Vector3 frontWheelPositionDelta = frontWheel.position - lastFrontWheelPosition;
        Vector3 rearWheelPositionDelta = rearWheel.position - lastRearWheelPosition;
        lastRearWheelPosition = rearWheel.position;
        lastFrontWheelPosition = frontWheel.position;
        Vector3 frontWheelForwardVector = Vector3.ProjectOnPlane(-handlebar.up, bike.transform.up).normalized;
        Vector3 rearWheelForwardVector = bike.transform.forward;
        float frontWheelRollDistance = Vector3.Dot(frontWheelPositionDelta, frontWheelForwardVector);
        float rearWheelRollDistance = Vector3.Dot(rearWheelPositionDelta, rearWheelForwardVector);
        float frontWheelRotation = 180 * frontWheelRollDistance / (Mathf.PI * wheelRadius);
        float rearWheelRotation = 180 * rearWheelRollDistance / (Mathf.PI * wheelRadius);
        frontWheel.Rotate(Vector3.right * frontWheelRotation, Space.Self);
        rearWheel.Rotate(Vector3.right * rearWheelRotation, Space.Self);

        //rigid body verlocity is simpy set to the current speedInMetersPerSecond in the direction of travel
        //I don't really like this, should have proper physics handling 
        float downwardsVelocity = bikeRigidbody.velocity.y; //keeping the current downwards velocity
        Vector3 velocity = bike.transform.forward * (speedInMetersPerSecond * (Time.deltaTime * 2f));
        velocity.y = downwardsVelocity;
        bikeRigidbody.velocity = velocity;

        Vector3 rot = handlebar.localRotation.eulerAngles;
        handlebar.localRotation = Quaternion.Euler(rot.x, rot.y, steeringAngle);

        float tilt = 0;
        if (Mathf.Abs(steeringAngle) < 0.01f) //Going straight
        {
            bike.transform.position += bike.transform.forward * (Time.deltaTime * speedInMetersPerSecond);
        }
        else //Curve
        {
            float turnRadius = wheelbase / (Mathf.Sin(Mathf.Abs(steeringAngle) * Mathf.Deg2Rad));
            float sign = Mathf.Sign(steeringAngle);
            Vector3 turningCurveCenter =
                (bike.transform.position + (bike.transform.right.normalized * (sign * turnRadius)));
            bike.transform.RotateAround(turningCurveCenter, Vector3.up,
                sign * ((speedInMetersPerSecond * 1f) / (2f * Mathf.PI * turnRadius) * 360f) * Time.deltaTime);

            tilt = Mathf.Atan((speedInMetersPerSecond * speedInMetersPerSecond) / (turnRadius * Physics.gravity.y)) *
                   Mathf.Rad2Deg * sign * tiltFactor;
        }

        //smoothing tilt is probably unnecessary
        float lastSmoothTilt = smoothTilt;
        smoothTilt = Mathf.Lerp(smoothTilt, tilt, Time.deltaTime * 30f);
        bike.transform.localEulerAngles = new Vector3(bike.transform.localEulerAngles.x,
            bike.transform.localEulerAngles.y, smoothTilt);

        //Moving and turning the VR Player with the bike
        //Smoothing VR Camera transforms slightly to reduce jitter and motion sickness
        VRCamera.transform.position = Vector3.Lerp(VRCamera.transform.position, vRPlayerPositionTransform.position,
            Time.deltaTime * 30f);
        smoothBikeRotation = Quaternion.Lerp(smoothBikeRotation, bike.transform.rotation, Time.deltaTime * 20f);
        float deltaBikeRotationY = smoothBikeRotation.eulerAngles.y - lastBikeRotationY;
        lastBikeRotationY = smoothBikeRotation.eulerAngles.y;
        Vector3 vrRotation = VRCamera.transform.rotation.eulerAngles;
        float smoothTiltDelta = smoothTilt - lastSmoothTilt;
        Quaternion deltaRotationY = Quaternion.Euler(0, deltaBikeRotationY, 0);
        Quaternion deltaTiltRotation = Quaternion.Euler(0, 0, smoothTiltDelta);
        //VRCamera.transform.rotation = deltaRotationY * deltaTiltRotation * VRCamera.transform.rotation;
        VRCamera.transform.rotation = Quaternion.Euler(vrRotation.x, vrRotation.y + deltaBikeRotationY,
            vrRotation.z + smoothTiltDelta);


        //Debug Lines
        if (!drawDebug) return;
        smoothLeftWheelTravelDistance =
            Mathf.Lerp(smoothLeftWheelTravelDistance, frontWheelRotation, Time.deltaTime * 5f);
        smoothRightWheelTravelDistance =
            Mathf.Lerp(smoothRightWheelTravelDistance, rearWheelRotation, Time.deltaTime * 5f);
        Vector3 rearwheelflorrpos = rearWheel.position;
        Vector3 frontwheelflorrpos = frontWheel.position;
        rearwheelflorrpos.y = bike.transform.position.y;
        frontwheelflorrpos.y = bike.transform.position.y;
        DrawThickDebugLine(frontwheelflorrpos,
            frontwheelflorrpos + frontWheelForwardVector * (frontWheelRollDistance * 100f), Color.red, 0.01f);
        DrawThickDebugLine(rearwheelflorrpos,
            rearwheelflorrpos + rearWheelForwardVector * (rearWheelRollDistance * 100f), Color.red, 0.01f);
        if (Mathf.Abs(steeringAngle) > 0.01f)
        {
            float turnRadius = wheelbase / (Mathf.Sin(Mathf.Abs(steeringAngle) * Mathf.Deg2Rad));
            float sign = Mathf.Sign(steeringAngle);
            Vector3 turningCurveCenter =
                bike.transform.position + bike.transform.right.normalized * (sign * turnRadius);
            DrawThickDebugLine(turningCurveCenter, turningCurveCenter + bike.transform.up * 0.3f, Color.green, 0.01f);
            DrawThickDebugLine(turningCurveCenter, turningCurveCenter + bike.transform.right * 0.3f, Color.blue, 0.01f);
            DrawThickDebugLine(turningCurveCenter, turningCurveCenter + bike.transform.forward * 0.3f, Color.red,
                0.01f);
            DrawThickDebugLine(bike.transform.position, turningCurveCenter, Color.yellow, 0.01f);
            turningCurveCenter.y = bike.transform.position.y;
            DrawThickDebugCircle(turningCurveCenter, Vector3.up, turnRadius, 64, Color.yellow, 0.01f);
        }
    }

    public void ResetBike() //not tested yet sorry
    {
        VRCamera.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        bike.transform.position = initialBikePosition;
        bike.transform.rotation = initialBikeRotation;
        bikeRigidbody.velocity = Vector3.zero;
        bikeRigidbody.angularVelocity = Vector3.zero;
        lastRearWheelPosition = rearWheel.transform.position;
        lastFrontWheelPosition = frontWheel.transform.position;
        handlebar.localRotation = initialHandlebarRotation;
        smoothTilt = 0.0f;
        lastBikeRotationY = bike.transform.localEulerAngles.y;
        smoothBikeRotation = bike.transform.rotation;
    }

    //debug
    public static void DrawThickDebugLine(Vector3 start, Vector3 end, Color color, float thickness)
    {
        Vector3 direction = end - start;
        Vector3 normal = Vector3.Cross(direction, Vector3.up).normalized;
        for (int i = 0; i < 10; i++)
        {
            float angle = i * 36;
            Vector3 offset = Quaternion.AngleAxis(angle, direction) * normal * thickness;
            Debug.DrawLine(start + offset, end + offset, color);
        }
    }

    public static void DrawThickDebugCircle(Vector3 center, Vector3 upVector, float radius, int nSegments, Color color,
        float thickness)
    {
        Vector3 lastPoint = center + (Quaternion.AngleAxis(360f / nSegments, upVector) *
                                      (Vector3.ProjectOnPlane(Vector3.forward, upVector).normalized * radius));
        for (int i = 1; i <= nSegments; i++)
        {
            float angle = i * 360f / nSegments;
            Vector3 point = center + (Quaternion.AngleAxis(angle, upVector) *
                                      (Vector3.ProjectOnPlane(Vector3.forward, upVector).normalized * radius));
            DrawThickDebugLine(lastPoint, point, color, thickness);
            lastPoint = point;
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(BikeController))]
    public class BikeVRInputEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            //button
            if (GUILayout.Button("Reset Bike"))
            {
                Debug.Log("Bike Reset");
                ((BikeController) target).ResetBike();
            }

            DrawDefaultInspector();
        }
    }
#endif
}