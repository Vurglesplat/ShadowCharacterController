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
        isShadowForm = false;
        currentShadowAmount = 0;
        isWaitingToReleaseSwapButton = false;

        PlayerInput pInput = this.GetComponent<PlayerInput>();
        shadowSwapAction = pInput.actions["ShadowSwap"];
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
    public void HandleSwitching(bool isStartingFromStandardForm, bool isConvertingToShadow)
    {
        if (isConvertingToShadow)
        {
            currentShadowAmount += Time.deltaTime / timeToSwitchToShadow ;

            if (currentShadowAmount > 1)
            {
                isShadowForm = true;
                isWaitingToReleaseSwapButton = true;
                currentShadowAmount = 1;
            }

            SetAllMatShadowVal(currentShadowAmount);
        }
        else
        {
            Debug.Log("Is going to standard form");

            currentShadowAmount -= Time.deltaTime / timeToSwitchToShadow;

            if (currentShadowAmount < 0)
            {
                isShadowForm = false;
                isWaitingToReleaseSwapButton = true;
                currentShadowAmount = 0;
            }

            SetAllMatShadowVal(currentShadowAmount);
        }
    }
}
