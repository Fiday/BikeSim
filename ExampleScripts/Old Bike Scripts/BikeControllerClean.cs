using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BikeControllerClean : MonoBehaviour
{
    public GameObject BikeBase;
    public GameObject ReferenceCube;
    public GameObject Camera;
    public GameObject leftController;
    public Transform handlebar;
    private Quaternion initalHandlebarRotation, initialControllerRotation;
    public static float steeringAngle = 0.0f;
    private GameControllerScript gameControllerScript;

    void Start()
    {
        gameControllerScript = BikeBase.GetComponent<GameControllerScript>();
        //"important"
        {
            initalHandlebarRotation = handlebar.transform.rotation;
            initialControllerRotation = leftController.transform.rotation;
        }
    }

    void Update()
    {
        var rgb = this.GetComponent<Rigidbody>();
        rgb.velocity = (transform.forward * gameControllerScript.BikeSpeed) * (Time.deltaTime * 2f);

        //"important"
        {
            // 0 to 360 degrees                                                                                                                            
            steeringAngle = (Quaternion.Inverse(transform.rotation) * initialControllerRotation * leftController.transform.rotation * initalHandlebarRotation).eulerAngles.y;

            if (steeringAngle > 90 && steeringAngle <= 180)
            {  //limit right
                steeringAngle = 90;
            }
            else if (steeringAngle > 180 && steeringAngle < 270)
            {  //limit left
                steeringAngle = -90;
            }
            else if (steeringAngle >= 270 && steeringAngle <= 360)
            {  //remap left
                steeringAngle = steeringAngle - 360;
            }

            var steeringVec = new Vector3(0.0f, steeringAngle, 0.0f);
            handlebar.transform.localEulerAngles = steeringVec;

            float wheelbase = 1.5f;
            float turnRadius = wheelbase / (Mathf.Sin(Mathf.Abs(steeringAngle) * Mathf.Deg2Rad));

            if (turnRadius > 85)
            {
                turnRadius = Mathf.Infinity;
            }

            Vector3 turningCenterCurve = (transform.position + (transform.right.normalized * turnRadius));
            int sign = 0; // curve direction 

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

            float speedInMS = gameControllerScript.BikeSpeed / 3.6f;

            if (steeringAngle != 0 && turnRadius != Mathf.Infinity) // curve
            {
                this.transform.RotateAround(turningCenterCurve, Vector3.up, sign * ((speedInMS * 1f) / (2f * Mathf.PI * turnRadius) * 360f) * Time.deltaTime);

            }
            else // straight ahead
            {
                this.transform.position = transform.position + transform.forward * Time.deltaTime * speedInMS;

            }
        }
    }
}

