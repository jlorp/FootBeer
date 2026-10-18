using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacePieceSwapper : MonoBehaviour
{
    public AnimationCurve bounceCurve;
    public float bounceTime;

    public GameObject angry, idle;

    private Coroutine bounceRoutine;
    public string currentEmote;
    [SerializeField] private ParticleSystem screamPFX;

    public 

    void Start()
    {
        currentEmote = "idle";
    }

    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.H))
    //     {
    //         SwapPiece(bounceTime, "Angry");
    //     }
    //     if (Input.GetKeyDown(KeyCode.G))
    //     {
    //         SwapPiece(bounceTime, "Idle");
    //     }
    // }

    public void SwapPiece(float _duration, string _facePiece)
    {
        if(_facePiece == currentEmote) return;
        currentEmote = _facePiece;

        if(bounceRoutine != null) StopCoroutine(bounceRoutine);
        bounceRoutine = StartCoroutine(BounceMouth(_duration, _facePiece));
    }

    void SetMouthObject(string _facePiece)
    {
        TurnOffMouths();

        if(_facePiece == "Angry")
        {
            angry.SetActive(true);
            screamPFX.Play();
        }
        else if(_facePiece == "Idle")
        {
            idle.SetActive(true);
        }
    }

    void TurnOffMouths()
    {
        angry.SetActive(false);
        idle.SetActive(false);
    }

    IEnumerator BounceMouth(float _duration, string _facePiece)
    {
        float elapsedTime = 0f;
        bool mouthSwapped = false;

        while (elapsedTime < _duration)
        {
            float percentComplete = elapsedTime/_duration;

            if(percentComplete > 0.2f && !mouthSwapped) 
            {
                SetMouthObject(_facePiece);
                mouthSwapped = true;
            } 

            float squash = bounceCurve.Evaluate(percentComplete);
            float stretch = 2 - squash;

            transform.localScale = new Vector3(squash, stretch, 1);

            elapsedTime += Time.deltaTime;
            yield return null; 
        }
        transform.localScale = Vector3.one;
    }
}
