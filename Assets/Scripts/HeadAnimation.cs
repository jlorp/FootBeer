using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadAnimation : MonoBehaviour
{
    public Rigidbody crotchRB;
    public Transform neck, neck1, head;
    public float crotchYspeedEffect;

    Vector3 neckBase, neck1Base, headBase;
    float neckZtarget, neck1Ztarget, headZtarget;

    public float wobbleSpeed, wobbleStrength;

    //eyeStuff

    public Transform rPupil, lPupil;
    
    public Vector2 blinkTimeRange;

    public Vector2 blinkduration;

    public SpriteRenderer rEye, lEye;
    public Sprite rEyeOpen, rEyeBlink, lEyeOpen, lEyeBlink;

    float timeToBlink;

    void Start()
    {
        neckBase = neck.localEulerAngles;
        neck1Base = neck1.localEulerAngles;
        headBase = head.localEulerAngles;
        timeToBlink = Random.Range(blinkTimeRange.x, blinkTimeRange.y);
    }

    IEnumerator Blink()
    {
        rEye.sprite = rEyeBlink;
        lEye.sprite = lEyeBlink;
        rPupil.gameObject.SetActive(false);
        lPupil.gameObject.SetActive(false);

        float blinkTime = Random.Range(blinkduration.x, blinkduration.y);

        yield return new WaitForSeconds(blinkTime);

        rEye.sprite = rEyeOpen;
        lEye.sprite = lEyeOpen;
        timeToBlink = Random.Range(blinkTimeRange.x, blinkTimeRange.y);
        rPupil.gameObject.SetActive(true);
        lPupil.gameObject.SetActive(true);
    }

    void Update()
    {
        neckZtarget = neckBase.z;
        neck1Ztarget = neck1Base.z;
        headZtarget = headBase.z;

        CrotchMovementEffect();
        WobbleEffect();
        ApplyRotation();

        HandleBlink();
    }

    void HandleBlink()
    {
        if(timeToBlink > 0)
        {
            timeToBlink -= Time.deltaTime;
            if (timeToBlink <=0)
            {
                StartCoroutine(Blink());
            }
        }
    }

    void WobbleEffect()
    {
        float scrollingNoise = Mathf.PerlinNoise(wobbleSpeed * Time.time, 1);
        
        neckZtarget += scrollingNoise * wobbleStrength;
        neck1Ztarget += scrollingNoise * wobbleStrength;
        headZtarget += scrollingNoise * wobbleStrength;
    }

    void CrotchMovementEffect()
    {
        float _crotchEffect = crotchRB.velocity.y * crotchYspeedEffect;
        neckZtarget += _crotchEffect;
        neck1Ztarget += _crotchEffect;
        headZtarget += _crotchEffect;
    }

    void ApplyRotation()
    {
        Quaternion neckTargetQuat = Quaternion.Euler(neckBase.x, neckBase.y, neckZtarget);
        neck.localRotation = Quaternion.Lerp(neck.localRotation, neckTargetQuat, Time.deltaTime * 8f);

        Quaternion neck1TargetQuat = Quaternion.Euler(neck1Base.x, neck1Base.y, neck1Ztarget);
        neck1.localRotation = Quaternion.Lerp(neck1.localRotation, neck1TargetQuat, Time.deltaTime * 6f);

        Quaternion headTargetQuat = Quaternion.Euler(headBase.x, headBase.y, headZtarget);
        head.localRotation = Quaternion.Lerp(head.localRotation, headTargetQuat, Time.deltaTime * 4f);
    }
}
