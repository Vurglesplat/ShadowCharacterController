using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

////////////////////////////////////////////////////////////\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
/////This script was first made by Brandon Boras (bb), any assisstance or workf from others will be shown via comment\\\
/////Unless the code was marked as belonging to someone other than bb else, it is free to use.\\\\\\\\\\\\\\\\\\\\\\\\\\ 
////////////////////////////////////////////////////////////\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\


public class StateSwitching : MonoBehaviour
{
    [SerializeField] ThirdPersonCameraScript cameraScript;
    [SerializeField] ThirdPersonMovement standardMovement;
    [SerializeField] ShadowMovment shadowMovement;
    [Space]
    [SerializeField] GameObject standardModel;
    [SerializeField] GameObject shadowModel;
    [Space]
    [SerializeField] GameObject standardCollider;
    [SerializeField] GameObject shadowCollider;
    
    [Header("Variables to follow along to")]
    public bool isShadowForm;
    [Range(0, 1)] public float currentShadowAmount;
    [Header("Design Vars")]
    public float timeToSwitchToShadow;
    [SerializeField] Renderer playerRenderer;

    InputAction shadowSwapAction;
    bool isWaitingToReleaseSwapButton; // allows the same button to swap between forms, but won't immediately do it
    

    private void Start()
    {

        currentShadowAmount = 0;
        isWaitingToReleaseSwapButton = false;

        PlayerInput pInput = this.GetComponent<PlayerInput>();
        shadowSwapAction = pInput.actions["ShadowSwap"];

        ChangeToShadowForm(false);
    }

    void Update()
    {
        if (isShadowForm)
        {
            ShadowFormUpdate();
        }
        else
        {
            StandardFormUpdate();
        }

        if(isWaitingToReleaseSwapButton && !shadowSwapAction.IsPressed())
        {
            isWaitingToReleaseSwapButton = false;
        }
    }

    void ShadowFormUpdate()
    {
        if (shadowSwapAction.IsPressed() && !isWaitingToReleaseSwapButton)
        {
                HandleSwitching(false, false);
        }
        else if (currentShadowAmount < 1)   // if they were switching, but have stopped
        {
            HandleSwitching(false, true);
        }
    }

    void StandardFormUpdate()
    {
        if (shadowSwapAction.IsPressed() && !isWaitingToReleaseSwapButton)
        {

            HandleSwitching(true, true);
        }
        else if (currentShadowAmount > 0)   // if they were switching, but have stopped
        {
            HandleSwitching(true, false);
        }
    }

    public void SetAllMatShadowVal(float newShadowAmount)
    {
        foreach (Material mat in playerRenderer.materials)
        {
            mat.SetFloat("_amountConvertedToShadow", newShadowAmount);
        }
    }

    public void SetAllMatTransparencyUpValue(bool isTransparencyMovingUp)
    {
        foreach (Material mat in playerRenderer.materials)
        {
            mat.SetFloat("_transparencyMovesUp", isTransparencyMovingUp? 1f : 0f);
        }
        
    }

    public void HandleSwitching(bool isStartingFromStandardForm, bool isConvertingToShadow)
    {
        // enable this when I want to work on it again

        //SetAllMatTransparencyUpValue(!isStartingFromStandardForm);

        if (isConvertingToShadow)
        {
            currentShadowAmount += Time.deltaTime / timeToSwitchToShadow ;

            if (currentShadowAmount > 1)
            {
                ChangeToShadowForm(true);
                isWaitingToReleaseSwapButton = true;
                currentShadowAmount = 1;
            }

            SetAllMatShadowVal(currentShadowAmount);
        }
        else
        {
            //Debug.Log("Is going to standard form");

            currentShadowAmount -= Time.deltaTime / timeToSwitchToShadow;

            if (currentShadowAmount < 0)
            {
                ChangeToShadowForm(false);
                isWaitingToReleaseSwapButton = true;
                currentShadowAmount = 0;
            }

            SetAllMatShadowVal(currentShadowAmount);
        }
    }

    void ChangeToShadowForm(bool toShadow)
    {
        isShadowForm = toShadow;

        if (isShadowForm)
        {
            // call camera script here
            cameraScript.ChangeCameraMode(true);

            shadowMovement.enabled = true;
            standardMovement.enabled = false;

            //standardModel.SetActive(false);
            shadowModel.SetActive(true);

            standardCollider.SetActive(false);
            shadowCollider.SetActive(true);
        }
        else
        {
            // call camera script here
            cameraScript.ChangeCameraMode(false);

            shadowMovement.enabled = false;
            standardMovement.enabled = true;

            shadowModel.SetActive(false);
            standardModel.SetActive(true);

            shadowCollider.SetActive(false);
            standardCollider.SetActive(true);
        }
    }
}
