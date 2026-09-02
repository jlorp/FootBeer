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

    void Start()
    {
        neckBase = neck.localEulerAngles;
        neck1Base = neck1.localEulerAngles;
        headBase = head.localEulerAngles;
    }

    void Update()
    {
        neckZtarget = neckBase.z;
        neck1Ztarget = neck1Base.z;
        headZtarget = headBase.z;

        CrotchMovementEffect();
        WobbleEffect();
        ApplyRotation();
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
        neck.localRotation = Quaternion.Lerp(neck.localRotation, neckTargetQuat, Time.deltaTime * 4f);

        Quaternion neck1TargetQuat = Quaternion.Euler(neck1Base.x, neck1Base.y, neck1Ztarget);
        neck1.localRotation = Quaternion.Lerp(neck1.localRotation, neck1TargetQuat, Time.deltaTime * 3f);

        Quaternion headTargetQuat = Quaternion.Euler(headBase.x, headBase.y, headZtarget);
        head.localRotation = Quaternion.Lerp(head.localRotation, headTargetQuat, Time.deltaTime * 2f);
    }
}
