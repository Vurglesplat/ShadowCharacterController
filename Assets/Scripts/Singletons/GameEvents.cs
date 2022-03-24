using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// credits to game dev guide's video here for reference to this, https://youtu.be/gx0Lt4tCDE0?list=PLKbW2Ucua2LMuxZbsjVscO2TQspPqFnzv
public class GameEvents
{
    private static GameEvents _current;
    public static GameEvents current
    {
        get
        {
            if (_current == null)
            {
                _current = new GameEvents();
            }
            return _current;
        }

        set
        {
            if (value != null)
            {
                _current = value;
            }
        }
    }



    public event Action CheckForNoSaves;
    public void SaveRefresh()
    {
        if (CheckForNoSaves != null)
            CheckForNoSaves();
    }



    //public event Action<int> OnFocusCanvas;
    //public void FocusCanvas(int canvasID)
    //{
    //    if (OnFocusCanvas != null)
    //        OnFocusCanvas(canvasID);
    //}
    //public event Action EnableSaveSettingsChangesButton;
    //public void SettingsModified()
    //{
    //    if (EnableSaveSettingsChangesButton != null)
    //        EnableSaveSettingsChangesButton();
    //}
    
    public event Action WhenCageIsOpened;
    public void OpenCage()
    {
        if (WhenCageIsOpened != null)
            WhenCageIsOpened();
    }

    public event Action EndOfLevel;
    public void FireEndOfLevel()
    {
        if (EndOfLevel != null)
            EndOfLevel();
    }

    public event Action PlayerIsInteracting;
    public void FirePlayerIsInteracting()
    {
        if (PlayerIsInteracting != null)
            PlayerIsInteracting();
    }

    public event Action TallOneChaseSting;
    public void FireTallOneChaseSting()
    {
        if (TallOneChaseSting != null)
            TallOneChaseSting();
    }

    public event Action TallOnePeerSting;
    public void FireTallOnePeerSting()
    {
        if (TallOnePeerSting != null)
            TallOnePeerSting();
    }

    public event Action<bool> HandleCameraFollow;

    public void ToggleCameraFollow(bool newFollowStatus)
    {
        if (HandleCameraFollow != null)
            HandleCameraFollow(newFollowStatus);
    }

    public event Action<bool> CameraZoom;
    public void ChangeCameraZoom(bool isZooming)
    {
        if (CameraZoom != null)
            CameraZoom(isZooming);
    }


    public event Action SwipeConnect;
    public void FireSwipeConnect()
    {
        if (SwipeConnect != null)
            SwipeConnect();
    }
}
