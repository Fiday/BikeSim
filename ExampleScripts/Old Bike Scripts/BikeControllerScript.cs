using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MotionSystems;

public class BikeControllerScript : MonoBehaviour
{

    private BikeMode bikeMode;

    public GameObject BikeBase;
    public GameObject ReferenceCube;
    public GameObject Camera;
    public GameObject leftController;

    private Transform handlebar;
    private Quaternion initalHandlebarRotation, initialControllerRotation;
    public static float steeringAngle = 0.0f;

    GameControllerScript gameControllerScript;
    // Start is called before the first frame update
    void Start()
    {

        gameControllerScript = BikeBase.GetComponent<GameControllerScript>();
        bikeMode = gameControllerScript.bikemode;

        handlebar = this.transform.Find("WheelHandleBar");

        if (bikeMode == BikeMode.Forschungsfest)
        {
            initalHandlebarRotation = handlebar.transform.rotation;
            initialControllerRotation = leftController.transform.rotation;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        if (BikeBase != null)
        {
            GameControllerScript p = BikeBase.GetComponent<GameControllerScript>();

            var rgb = this.GetComponent<Rigidbody>();

            rgb.velocity = (transform.forward * p.BikeSpeed) * (Time.deltaTime * 2f);

            if (handlebar != null)
            {
                
                if (bikeMode == BikeMode.Lab)
                {
                    
                    //handlebar.transform.Rotate(0f, p.ISteeringAngle, 0f, Space.Self);
                    var rotationVec = new Vector3(0f, p.ISteeringAngle, 0f);
                    handlebar.transform.localEulerAngles = rotationVec;

                    //  this.transform.localEulerAngles = rotationVec;


                    //var tiltVec = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, -p.ITiltAngle*2);
                    var tiltVec = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, -p.ITiltAngle);
                    if (p.RollMultiplier != 0 || 1 == 1)
                    {
                        // maybe we add the slerp here to reduce the statter

                        //tiltVec is our TARGET, IF WE ARE NOT ALREADY AT THE TARGET; LETS MOVE THERE (INTERPOLATED IN 0.2 SECONDS)

                        /*   if (-p.ITiltAngle != Mathf.RoundToInt(this.transform.localEulerAngles.z))
                           {

                               lastTiltVec += Time.deltaTime;
                               Vector3 euler = this.transform.localEulerAngles;
                               // euler.z = Mathf.Lerp(transform.localEulerAngles.z-p.ITiltAngle, -p.ITiltAngle, Time.deltaTime);
                               if(-p.ITiltAngle>=this.transform.localEulerAngles.z)
                                   euler.z = Mathf.Lerp(euler.z, -p.ITiltAngle, lastTiltVec);

                               this.transform.localEulerAngles = euler;
                           }
                           else
                           {
                               lastTiltVec = 0;
                           }*/






                        this.transform.localEulerAngles = tiltVec;

                        //float angle = Mathf.LerpAngle(0, -p.ITiltAngle, Time.time);
                        //this.transform.localEulerAngles = new Vector3(0, angle, 0);

                        //this.transform.localEulerAngles=Vector3.Slerp(this.transform.localEulerAngles, tiltVec, Time.deltaTime);

                        //this.transform.localEulerAngles = Vector3.Lerp(this.transform.localEulerAngles,tiltVec, 0.1F);

                        // var _targetRotation = Quaternion.Euler(tiltVec);
                        // this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, _targetRotation, 1 * Time.deltaTime);

                        // Debug.Log(tiltVec);
                    }
                    float wheelbase = 1.5f;
                    float turnRadius = wheelbase / (Mathf.Sin(Mathf.Abs(p.ISteeringAngle) * Mathf.Deg2Rad));
                    p.ICurveRadius = turnRadius;
                    //Debug.Log("Turnradius " + turnRadius);

                    Vector3 turningCenterCurve = (transform.position + (transform.right.normalized * turnRadius));
                    Vector3 turningCenterTilt = new Vector3(0f, 0f, 0f);


                    int sign = 0;

                    if (p.ISteeringAngle < 0)
                    {
                        Vector3 curDirection = turningCenterCurve - transform.position;
                        turningCenterCurve = transform.position - curDirection;
                        sign = -1;
                    }
                    else if (p.ISteeringAngle > 0)
                    {
                        sign = 1;
                    }

                    float speedInMS = p.BikeSpeed / 3.6f;


                    if (p.ISteeringAngle != 0)
                    {
                        this.transform.RotateAround(turningCenterCurve, Vector3.up, sign * ((speedInMS * 1f) / (2f * Mathf.PI * turnRadius) * 360f) * Time.deltaTime);
                    }
                    else
                    {
                        this.transform.position = transform.position + transform.forward * Time.deltaTime * speedInMS;

                    }

                    // var referenceCube = this.transform.Find("ReferenceCube");

                    /*  Vector3 targetPostition = new Vector3(ReferenceCube.transform.position.x,
                                             this.transform.position.y,
                                             ReferenceCube.transform.position.z);*/

                    //  this.transform.LookAt(targetPostition*(p.BikeSpeed/100));
                }
                if (bikeMode == BikeMode.Forschungsfest) { 
                // Todo fix this!
                /*if (Input.GetKeyDown("h"))
                { //If you press c
                    Debug.Log("Reset handlebarrotation");
                    handlebar.transform.rotation = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
                    leftController.transform.rotation = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
                    set = false;
                }*/

                
                    //handlebar.transform.rotation = initialControllerRotation * rightController.transform.rotation * initalHandlebarRotation;
                    steeringAngle = (Quaternion.Inverse(transform.rotation) * initialControllerRotation * leftController.transform.rotation * initalHandlebarRotation).eulerAngles.y; // 0 to 360 degrees
                    //handlebar.transform.rotation = Quaternion.Euler(0.0f, steeringAngle, 0.0f);
                    Debug.LogWarning("checking formula: steeringangle before maping: " + steeringAngle);
                    Debug.LogWarning("checking formula: leftcontrollerrotation: " + leftController.transform.rotation);
                    Debug.LogWarning("checking formula: initialControllerRotation: " + initialControllerRotation);
                    Debug.LogWarning("checking formula: initalHandlebarRotation: " + initalHandlebarRotation);
                    // fixedUpdate


                    //steeringAngle = steeringAngle.Remap(0, 360, 120.89f,-120.89f);
                    //steeringAngle = steeringAngle.Remap(0, 360, 90, -90);

                    if (steeringAngle > 90 && steeringAngle <= 180)
                    {  //beschränkung rechts
                        steeringAngle = 90;
                    }
                    else if (steeringAngle > 180 && steeringAngle < 270)
                    { //beschränkung links
                        steeringAngle = -90;
                    }
                    else if (steeringAngle >= 270 && steeringAngle <= 360) {  //eigenes Remap links
                        steeringAngle = steeringAngle - 360;
                    }

                    var steeringVec = new Vector3(0.0f, steeringAngle, 0.0f);
                    handlebar.transform.localEulerAngles = steeringVec;

                


                    Debug.LogWarning("!!!!! steeringangle new: " + steeringAngle);

                    float wheelbase = 1.5f;
                    float turnRadius = wheelbase / (Mathf.Sin(Mathf.Abs(steeringAngle) * Mathf.Deg2Rad));

                    if (turnRadius > 85) {
                        turnRadius = Mathf.Infinity;
                    }

               

                    Debug.LogWarning("turning: steeringangle after radius: " + steeringAngle);
                    Debug.LogWarning("turning: TurnRadius: " + turnRadius);
                    Debug.LogWarning("turning: Transform position: " + transform.position);
                    Debug.LogWarning("turning: Transform right: " + transform.right.normalized);

                    Vector3 turningCenterCurve = (transform.position + (transform.right.normalized * turnRadius));

                    int sign = 0; // curve direction 

                    Debug.LogWarning("before turningCenterCurve: " + turningCenterCurve);

                    if (steeringAngle < 0) // left
                    {
                        Vector3 curDirection = turningCenterCurve - transform.position;
                        turningCenterCurve = transform.position - curDirection;
                        sign = -1;
                    }
                    else if (steeringAngle > 0) // right
                    {
                        sign = 1;
                    }

                    float speedInMS = p.BikeSpeed / 3.6f;


                    Debug.LogWarning("after turningCenterCurve: " + turningCenterCurve);

                    Debug.LogWarning("speedInMS: " + speedInMS);

                    if (steeringAngle != 0 && turnRadius != Mathf.Infinity) // curve
                    {
                        //this.transform.RotateAround(turningCenterCurve, Vector3.up, sign * ((speedInMS * 1f) / (2f * Mathf.PI * turnRadius) * 360f) * Time.deltaTime);
                        this.transform.RotateAround(turningCenterCurve, Vector3.up, sign * ((speedInMS * 1f) / (2f * Mathf.PI * turnRadius) * 360f) * Time.deltaTime);

                    }
                    else // straight ahead
                    {
                        this.transform.position = transform.position + transform.forward * Time.deltaTime * speedInMS;

                    }
                }
            }
        }
    }

    /*void OnCollisionEnter(Collision collision)
    {
        Debug.Log("IN COLLISION ENTER");
        set = false;
    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log("IN COLLISION EXIT");
        set = false;
    }*/

}

