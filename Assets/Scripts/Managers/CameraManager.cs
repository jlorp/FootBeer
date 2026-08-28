using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{  
    public static CameraManager Instance;
    
    public Camera legCam;
    public Camera bellyCam;

    public Camera activeCamera;

    public GameObject BellySceneLights,LegSceneLights;

    int currentCamera;
    public Canvas _canvas;

    void Start()
    {
        Instance = this;
        SwitchCamera(1, false);
    }

    void ShutOffCameras()
    {
        legCam.gameObject.SetActive(false);
        bellyCam.gameObject.SetActive(false);
    }

    public void SwitchCamera(int camera, bool reset)
    {
        if(camera == currentCamera) return;

        ShutOffCameras();
        if(currentCamera == 2) OnExitScene2();
        if(currentCamera == 1) OnExitScene1();

        if(camera == 1)
        {
            legCam.gameObject.SetActive(true);
            activeCamera=legCam;
            _canvas.worldCamera = legCam;
            if(reset) ResetScene1();
        }
        else if(camera == 2)
        {
            bellyCam.gameObject.SetActive(true);
            activeCamera = bellyCam;
            _canvas.worldCamera = bellyCam;
            if(reset) ResetScene2();
        }
        
        currentCamera = camera;
    }

    void OnExitScene2()
    {
        GameManager.Instance.handMover.sceneActive = false;
        AudioManager.Instance.EnterWater();
        BellySceneLights.SetActive(false);
        LegSceneLights.SetActive(true);
    }

    void OnExitScene1()
    {
        GameManager.Instance.feetMovement.sceneActive = false;
        //reset feet + crotch position/velocity
        GameManager.Instance.feetMovement.ResetPlayerPosition();
        BellySceneLights.SetActive(true);
        LegSceneLights.SetActive(false);
    }

    void ResetScene1()
    {
        GameManager.Instance.feetMovement.sceneActive = true;
        //drop beer/reset beer position
        GameManager.Instance.DropBeer();

        //reset arm
        ArmLogic _arm = GameManager.Instance.oldArm;
        _arm.holdingBeer = false;
        _arm.sceneActive = true;
        _arm.allowArmDrop = false;
        _arm.ForceArmUp();
        GameManager.Instance.StartArmDrop(5);
    }

    void ResetScene2()
    {
        //reset hand position/pose
        GameManager.Instance.handMover.ResetHandScene();
    }
}
