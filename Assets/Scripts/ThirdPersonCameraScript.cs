

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
public class ThirdPersonCameraScript : MonoBehaviour
{
    public GameObject player;
    [Header("General Camera Vars")]
    [SerializeField] bool shouldMoveCameraInEditorAsWell;
    public Vector3 cameraPositonOffset;
    public Vector3 cameraAngleOffset = new Vector3(0.0f, 0.0f, 0.0f);

    public float cameraSpeed;

    public float minPitch = -30.0f;
    public float maxPitch = 30.0f;
    public float cameraSensitivity = 5.0f;

    [Header("Camera Collision Vars")]
    public float closestCameraCanGetToPlayer;
    public float extraPaddingBetweenCameraAndObject;

    bool isFollowingPlayer;


    float angleX = 0.0f;
    float angleY = 0.0f;

    float normalZOffset;
    float normalYOffset;
    float zoomedZOffset;
    float zoomedYOffset;

    const float TIME_TO_ZOOM_IN_AND_OUT = 5.0f;

    void Start()
    {
        isFollowingPlayer = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        normalZOffset = cameraPositonOffset.z;
        normalYOffset = cameraPositonOffset.y;
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


    void ChangeCameraFollowStatus(bool newStatus)
    {
        isFollowingPlayer = newStatus;
    }

    private void LateUpdate()
    {
        if (isFollowingPlayer)
            Follow(player.transform);
    }

    void StopFollowingPlayer()
    {
        isFollowingPlayer = false;
    }

    void Follow(Transform leader)
    {
        float mx, my;

        if (Mouse.current.wasUpdatedThisFrame)
        {
            // I know I swapped x and y here, but personally, a horizontal x just makes more sense
            my = Mouse.current.delta.x.ReadValue();
            mx = Mouse.current.delta.y.ReadValue();
            //mx = (Mouse.current.position.y.ReadValueFromPreviousFrame() - Mouse.current.position.y.ReadValue());
            //my = (Mouse.current.position.x.ReadValueFromPreviousFrame() - Mouse.current.position.x.ReadValue());

            // We apply the initial rotation to the camera.
            Quaternion initialRotation = Quaternion.Euler(cameraAngleOffset);

            angleX -= mx * cameraSensitivity;

            // We clamp the angle along the X axis to be between the min and max pitch.
            angleX = Mathf.Clamp(angleX, minPitch, maxPitch);
            angleY += my * cameraSensitivity;

            angleY %= 360f;

            Quaternion newRot = Quaternion.Euler(angleX, angleY, 0.0f) * initialRotation;

            transform.rotation = newRot;
        }

        Vector3 forward = transform.rotation * Vector3.forward;
        Vector3 right = transform.rotation * Vector3.right;
        Vector3 up = transform.rotation * Vector3.up;

        Vector3 desiredPosition = leader.position
            + forward * cameraPositonOffset.z
            + right * cameraPositonOffset.x
            + up * cameraPositonOffset.y;

        desiredPosition = AvoidWallCollision(leader, desiredPosition);


        Vector3 position = Vector3.Lerp(transform.position, desiredPosition,
            Time.deltaTime * cameraSpeed);
        transform.position = position;

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
                + right * cameraPositonOffset.x
                + up * cameraPositonOffset.y;


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

        Vector3 startingOffset = cameraPositonOffset;
        Vector3 goalOffset = cameraPositonOffset;
        goalOffset.z = zoomedZOffset;
        goalOffset.y = zoomedYOffset;

        while (currentTime < TIME_TO_ZOOM_IN_AND_OUT)
        {
            cameraPositonOffset = Vector3.Lerp(startingOffset, goalOffset, currentTime / TIME_TO_ZOOM_IN_AND_OUT);
            currentTime += Time.deltaTime;
            yield return null;
        }

        cameraPositonOffset = goalOffset;
    }

    IEnumerator ZoomOut()
    {
        float currentTime = 0;

        Vector3 startingOffset = cameraPositonOffset;
        Vector3 goalOffset = cameraPositonOffset;
        goalOffset.z = normalZOffset;
        goalOffset.y = normalYOffset;

        while (currentTime < TIME_TO_ZOOM_IN_AND_OUT)
        {
            cameraPositonOffset = Vector3.Lerp(startingOffset, goalOffset, currentTime / TIME_TO_ZOOM_IN_AND_OUT);
            currentTime += Time.deltaTime;
            yield return null;
        }

        cameraPositonOffset = goalOffset;
    }



    [CustomEditor(typeof(ThirdPersonCameraScript)), CanEditMultipleObjects]

    public class ThirdPersonCameraScriptEditor : Editor
    {


        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ThirdPersonCameraScript script = (ThirdPersonCameraScript)target;

            if (script.shouldMoveCameraInEditorAsWell)
            {
                script.Follow(script.player.transform);
            }
        }
    }
}
