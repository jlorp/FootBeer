using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    //Ui stuff
    public Image fadeImage;

    //Beer Drop
    public Rigidbody beer;
    public Vector3 initialAngularVelocity;
    public ParticleSystem splashEffect;
    Vector3 beerStartPosition;
    Transform beerInitialParent;
    Quaternion beerStartRotation;

    //Dialogue handling
    public TMP_Text playerText;

    //Arm
    public ArmLogic oldArm;

    //Belly_arm
    public Transform beerHandTarget, beerHandStartPosition, beerHandEndPosition, beerhandDrinkPosition;
    public AnimationCurve beerMovementcurve, beerSipCurve;
    public HandMover handLogic;
    public Beer beerCode;

    public HandMover handMover;
    public FeetMovement feetMovement;

    void Start()
    {
        Instance = this;
        beerStartPosition = beer.transform.position;
        beerInitialParent = beer.transform.parent;
        beerStartRotation = beer.transform.rotation;
        StartCoroutine(FadeFromBlack(5,1));
    }

    public void EndBubbleScene()
    {
        StartCoroutine(GameStartStuff(2));
    }

    IEnumerator GameStartStuff(float delay)
    {
        yield return new WaitForSeconds(delay);

        PlayDialogue("oh my god", .1f, 2.5f, 0, AudioManager.Instance.omg);
        StartCoroutine(ActivateArm(7));
        DropBeer();
        PlayDialogue("fuck", .3f, 1f, 4f, AudioManager.Instance.fuck);
    }

    public void StartArmRaise()
    {
        StartCoroutine(LerpTransformPostion(1,beerHandTarget, beerHandStartPosition.position, beerHandEndPosition, beerMovementcurve));
    }

    public void TakeDrink()
    {
        StartCoroutine(LerpTransformPostion(0.75f,beerHandTarget, beerHandTarget.position, beerhandDrinkPosition, beerSipCurve, true, true));
        StartCoroutine(FadeToBlack(0.75f, 1f));
    }

    IEnumerator LerpTransformPostion(float duration, Transform _transform, Vector3 _startPosition, Transform _endPosition, AnimationCurve _curve, bool lerpRotation = false, bool kinematicOnComplete = false)
    {
        float elapsedTime = 0;
        _transform.position = _startPosition;
        handMover.leftHandRB.interpolation = RigidbodyInterpolation.None;
        handMover.leftHandRB.isKinematic = true;
        Quaternion _startRotation = _transform.rotation;

        while(elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = _curve.Evaluate(t);

            _transform.position = Vector3.Lerp(_startPosition, _endPosition.position, t);
            if(lerpRotation) _transform.rotation = Quaternion.Lerp(_startRotation, _endPosition.rotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        handMover.leftHandRB.interpolation = RigidbodyInterpolation.Interpolate;
        handMover.leftHandRB.isKinematic = kinematicOnComplete;
        _transform.position = _endPosition.position;
        handLogic.sceneActive = true;
    }

    public void DropBeer()
    {
        beerCode.OnDrop();
        beer.transform.position = beerStartPosition;
        beer.transform.rotation = beerStartRotation;
        beer.transform.SetParent(beerInitialParent);


        beer.angularVelocity = initialAngularVelocity;
        beer.velocity = Vector3.up * -2f;
        StartCoroutine(PlaySplash(.2f));
    }

    IEnumerator PlaySplash(float delay)
    {
        yield return new WaitForSeconds(delay);
        splashEffect.Play();
        AudioManager.Instance.PlaySplash();
    }

    public void PlayDialogue(string requestedPhrase, float timeBetweenWords, float timeBeforeClear, float initialDelay, AudioClip _sound)
    {
        StartCoroutine(AnimateWords(requestedPhrase, timeBetweenWords, timeBeforeClear, initialDelay, _sound));
    }

    IEnumerator FadeFromBlack(float duration, float delay)
    {
        float elapsedTime = 0;
        fadeImage.color = new Color(0.0f, 0.0f, 0.0f, 1.0f); 

        while(elapsedTime < duration + delay)
        {
            float t = elapsedTime - delay / duration;
            t = Mathf.Clamp(t, 0,1);

            fadeImage.color = new Color(0.0f, 0.0f, 0.0f, 1-t); 
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        fadeImage.color = new Color(0.0f, 0.0f, 0.0f, 0);
    }

    IEnumerator FadeToBlack(float duration, float delay)
    {
        float elapsedTime = 0;
        fadeImage.color = new Color(0.0f, 0.0f, 0.0f, 0f); 

        while(elapsedTime < duration + delay)
        {
            float t = (elapsedTime - delay) / duration;
            t = Mathf.Clamp(t, 0,1);

            fadeImage.color = new Color(0.0f, 0.0f, 0.0f, t); 
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fadeImage.color = new Color(0.0f, 0.0f, 0.0f, 1);
    }

    IEnumerator ActivateArm(float activateTime)
    {
        yield return new WaitForSeconds (activateTime);

        if(oldArm) oldArm.allowArmDrop = true;
    }

    public void StartArmDrop(float delay)
    {
        StartCoroutine(ActivateArm(delay));
    }

    IEnumerator AnimateWords(string sentance, float timeBetweenWords, float timeBeforeClear, float initialDelay, AudioClip _sound)
    {
        yield return new WaitForSeconds(initialDelay);
        AudioManager.Instance.PlayDialogue(_sound, 1, beerStartPosition);
        playerText.SetText(sentance);
        playerText.ForceMeshUpdate();
        
        int totalWords = playerText.textInfo.wordCount;
        playerText.maxVisibleWords = 0;

        for (int i = 1; i <= totalWords; i++)
        {
            playerText.maxVisibleWords = i;
            yield return new WaitForSeconds(timeBetweenWords);
        }
        yield return new WaitForSeconds(timeBeforeClear);
        playerText.SetText("");
    }
}
