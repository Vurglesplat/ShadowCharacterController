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
    [Range(0, 1)] public float currentShadowAmount;

    [Header("Design Vars")]
    public bool isShadowForm;
    public float timeToSwitchToShadow;
    [SerializeField] Renderer playerRenderer;

    InputAction shadowSwapAction;

    

    private void Start()
    {
        isShadowForm = false;
        
        
        
        PlayerInput pInput = this.GetComponent<PlayerInput>();
        shadowSwapAction = pInput.actions["ShadowSwap"];
    }

    void Update()
    {
        if (shadowSwapAction.IsPressed())
        {
            foreach(Material mat in playerRenderer.materials)
            {
                mat.SetFloat("_amountConvertedToShadow", 1.0f);
            }
        }
        else
        {
            foreach (Material mat in playerRenderer.materials)
            {
                mat.SetFloat("_amountConvertedToShadow", 0.0f);
            }
        }
    }
}