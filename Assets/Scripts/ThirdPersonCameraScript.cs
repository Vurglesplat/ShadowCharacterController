

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;


////////////////////////////////////////////////////////////\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
/////This script was first made by Brandon Boras (bb), any assisstance or workf from others will be shown via comment\\\
/////Unless the code was marked as belonging to someone other than bb else, it is free to use.\\\\\\\\\\\\\\\\\\\\\\\\\\ 
////////////////////////////////////////////////////////////\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\



/*
 * using https://faramira.com/a-configurable-third-person-camera-in-unity/ as a base for the third person camera movement
 * 
 */

//
// TODO
// - Make the collections of vars a serializable struct or something
// - Finish the function ChangeCameraVars

public class ThirdPersonCameraScript : MonoBehaviour
{
    [System.Serializable]
    class CameraData
    {
        public Vector3 cameraPositonOffset;
        public Vector3 cameraAngleOffset = new Vector3(0.0f, 0.0f, 0.0f);

        public float cameraSpeed;

        public float minPitch = -30.0f;
        public float maxPitch = 30.0f;
        public float cameraSensitivity = 5.0f;
    }

    public GameObject player;
    [SerializeField] bool shouldMoveCameraInEditorAsWell;


    [SerializeField] CameraData standardCameraVars;

    [SerializeField] CameraData shadowCameraVars;


    [Space]
    [Space]

    CameraData currentData;

    [Header("Camera Collision Vars")]
    public float closestCameraCanGetToPlayer;
    public float extraPaddingBetweenCameraAndObject;

    bool isFollowingPlayer;
    bool canMouseMoveCamera = true;

    float angleX = 0.0f;
    float angleY = 0.0f;

    float normalZOffset;
    float normalYOffset;
    float zoomedZOffset;
    float zoomedYOffset;

    const float TIME_TO_ZOOM_IN_AND_OUT = 5.0f;

    void Start()
    {
        ChangeCameraMode(false);

        isFollowingPlayer = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        normalZOffset = currentData.cameraPositonOffset.z;
        normalYOffset = currentData.cameraPositonOffset.y;
        zoomedZOffset = normalZOffset / 3.0f;
        zoomedYOffset = normalYOffset / 3.0f;
    }

    private void OnEnable()
    {
        GameEvents.current.CameraZoom += ChangeCameraZoomLevel;
        GameEvents.current.EndOfLevel += StopFollowingPlayer;
        GameEvents.current.HandleCameraFollow += ChangeCameraFollowStatus;
    }


    private void OnDisable()
    {
        GameEvents.current.CameraZoom -= ChangeCameraZoomLevel;
        GameEvents.current.EndOfLevel -= StopFollowingPlayer;
        GameEvents.current.HandleCameraFollow -= ChangeCameraFollowStatus;
    }


    private void LateUpdate()
    {
        if (isFollowingPlayer)
            Follow(player.transform);
    }
    void ChangeCameraFollowStatus(bool newStatus)
    {
        isFollowingPlayer = newStatus;
    }
    void StopFollowingPlayer()
    {
        isFollowingPlayer = false;
    }

    void Follow(Transform leader)
    {
        float mx, my;

        if (canMouseMoveCamera && Mouse.current.wasUpdatedThisFrame)
        {
            // I know I swapped x and y here, but personally, a horizontal x just makes more sense
            my = Mouse.current.delta.x.ReadValue();
            mx = Mouse.current.delta.y.ReadValue();
            //mx = (Mouse.current.position.y.ReadValueFromPreviousFrame() - Mouse.current.position.y.ReadValue());
            //my = (Mouse.current.position.x.ReadValueFromPreviousFrame() - Mouse.current.position.x.ReadValue());

            // We apply the initial rotation to the camera.
            Quaternion initialRotation = Quaternion.Euler(currentData.cameraAngleOffset);

            angleX -= mx * currentData.cameraSensitivity;

            // We clamp the angle along the X axis to be between the min and max pitch.
            angleX = Mathf.Clamp(angleX, currentData.minPitch, currentData.maxPitch);
            angleY += my * currentData.cameraSensitivity;

            angleY %= 360f;

            Quaternion newRot = Quaternion.Euler(angleX, angleY, 0.0f) * initialRotation;

            transform.rotation = newRot;


            Vector3 forward = transform.rotation * Vector3.forward;
            Vector3 right = transform.rotation * Vector3.right;
            Vector3 up = transform.rotation * Vector3.up;

            Vector3 desiredPosition = leader.position
                + forward * currentData.cameraPositonOffset.z
                + right * currentData.cameraPositonOffset.x
                + up * currentData.cameraPositonOffset.y;

            desiredPosition = AvoidWallCollision(leader, desiredPosition);


            Vector3 position = Vector3.Lerp(transform.position, desiredPosition,
                Time.deltaTime * currentData.cameraSpeed);
            transform.position = position;
        }
        else
        {
            Vector3 forward = transform.rotation * Vector3.forward;
            Vector3 right = transform.rotation * Vector3.right;
            Vector3 up = transform.rotation * Vector3.up;

            Vector3 desiredPosition = leader.position
                + forward * currentData.cameraPositonOffset.z
                + right * currentData.cameraPositonOffset.x
                + up * currentData.cameraPositonOffset.y;

            desiredPosition = AvoidWallCollision(leader, desiredPosition);


            Vector3 position = Vector3.Lerp(transform.position, desiredPosition,
                Time.deltaTime * currentData.cameraSpeed);
            transform.position = position;



            transform.rotation = Quaternion.Euler(currentData.cameraAngleOffset);
        }

        

    }

    Vector3 AvoidWallCollision(Transform leader, Vector3 desiredPosition)
    {
        RaycastHit hit;
        //float sphereCastRad = 2.5f;
        float maxDistance = (transform.position - leader.position).magnitude;
        Vector3 direction = (transform.position - leader.position).normalized;

        Vector3 thisPositon = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        Vector3 thisPositonDebug = new Vector3(transform.position.x, transform.position.y - 1.0f, transform.position.z);

        Vector3 padding = direction * extraPaddingBetweenCameraAndObject;


        //Debug.DrawRay(leader.position, thisPositon - leader.position, Color.green);
        //Debug.DrawRay(leader.position, (thisPositonDebug - leader.position) + padding, Color.red);


        if (Physics.SphereCast(leader.position, 0.2f, direction, out hit, maxDistance * 1.5f))
        // if (Physics.Linecast(leader.position, desiredPosition + padding,  out hit) && hit.collider.gameObject.layer != LayerMask.NameToLayer("Player"))
        {
            Debug.Log("Camera Hitting: " + hit.collider.gameObject);

            Vector3 forward = transform.rotation * Vector3.forward;
            Vector3 right = transform.rotation * Vector3.right;
            Vector3 up = transform.rotation * Vector3.up;

            Vector3 targetPos = leader.position;

            float distanceFromPlayer;
            //Debug.Log("" + Mathf.Clamp(hit.distance - extraPaddingBetweenCameraAndObject, closestCameraCanGetToPlayer, maxDistance));
            distanceFromPlayer = Mathf.Clamp(hit.distance - extraPaddingBetweenCameraAndObject, closestCameraCanGetToPlayer, hit.distance);
            //Debug.Log("hitdistance = " + hit.distance);
            Vector3 newDesiredPosition = targetPos
                + forward * -(float)System.Math.Round(distanceFromPlayer, 3)
                + right * currentData.cameraPositonOffset.x
                + up * currentData.cameraPositonOffset.y;


            //Debug.DrawRay(leader.position, (thisPositon - leader.position) + padding, Color.cyan);

            return newDesiredPosition;
        }
        else
        {
            //Debug.Log("camera is not hitting anything");
            return desiredPosition;
        }
    }


    void ChangeCameraZoomLevel(bool isCloserZoom)
    {
        if (isCloserZoom)
            StartCoroutine(ZoomIn());
        else
            StartCoroutine(ZoomOut());
    }

    IEnumerator ZoomIn()
    {
        float currentTime = 0;

        Vector3 startingOffset = currentData.cameraPositonOffset;
        Vector3 goalOffset = currentData.cameraPositonOffset;
        goalOffset.z = zoomedZOffset;
        goalOffset.y = zoomedYOffset;

        while (currentTime < TIME_TO_ZOOM_IN_AND_OUT)
        {
            currentData.cameraPositonOffset = Vector3.Lerp(startingOffset, goalOffset, currentTime / TIME_TO_ZOOM_IN_AND_OUT);
            currentTime += Time.deltaTime;
            yield return null;
        }

        currentData.cameraPositonOffset = goalOffset;
    }

    IEnumerator ZoomOut()
    {
        float currentTime = 0;

        Vector3 startingOffset = currentData.cameraPositonOffset;
        Vector3 goalOffset = currentData.cameraPositonOffset;
        goalOffset.z = normalZOffset;
        goalOffset.y = normalYOffset;

        while (currentTime < TIME_TO_ZOOM_IN_AND_OUT)
        {
            currentData.    cameraPositonOffset = Vector3.Lerp(startingOffset, goalOffset, currentTime / TIME_TO_ZOOM_IN_AND_OUT);
            currentTime += Time.deltaTime;
            yield return null;
        }

        currentData.cameraPositonOffset = goalOffset;
    }

    public void ChangeCameraMode(bool changingToShadowForm)
    {
        if(changingToShadowForm)
        {
            currentData = shadowCameraVars;
            canMouseMoveCamera = false;
        }
        else
        {
            currentData = standardCameraVars;
            canMouseMoveCamera = true;
        }
    }


}
