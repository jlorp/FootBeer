using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{  
    public static CameraManager Instance;
    
    public Camera legCam;
    public Camera bellyCam;

    int peakPriority = 1;

    void Start()
    {
        Instance = this;
    }

    void ShutOffCameras()
    {
        legCam.gameObject.SetActive(false);
        bellyCam.gameObject.SetActive(false);
    }

    public void SwitchCamera(int camera)
    {
        ShutOffCameras();
        if(camera == 1)
        {
            legCam.gameObject.SetActive(true);
        }
        else if(camera == 2)
        {
            bellyCam.gameObject.SetActive(true);
        }
    }
}
