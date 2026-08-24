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
    public Transform beerHandTarget, beerHandStartPosition, beerHandEndPosition;
    public AnimationCurve beerMovementcurve;
    public HandMover handLogic;
    public Beer beerCode;

    public HandMover handMover;

    void Start()
    {
        Instance = this;
        StartCoroutine(FadeFromBlack(5,1));
        PlayDialogue("oh wait, shit", .15f, 2.5f, 5.75f);
        StartCoroutine(ActivateArm(10));
        beerStartPosition = beer.transform.position;
        beerInitialParent = beer.transform.parent;
        beerStartRotation = beer.transform.rotation;
    }

    public void StartArmRaise()
    {
        StartCoroutine(LerpTransformPostion(1,beerHandTarget, beerHandStartPosition.position, beerHandEndPosition.position));
    }

    IEnumerator LerpTransformPostion(float duration, Transform _transform, Vector3 _startPosition, Vector3 _endPosition)
    {
        float elapsedTime = 0;
        _transform.position = _startPosition;

        while(elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = beerMovementcurve.Evaluate(t);

            _transform.position = Vector3.Lerp(_startPosition, _endPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _transform.position = _endPosition;

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

    public void PlayDialogue(string requestedPhrase, float timeBetweenWords, float timeBeforeClear, float initialDelay)
    {
        StartCoroutine(AnimateWords(requestedPhrase, timeBetweenWords, timeBeforeClear, initialDelay));
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
        DropBeer();
        PlayDialogue("god damnit", .3f, 2f, 4f);
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

    IEnumerator AnimateWords(string sentance, float timeBetweenWords, float timeBeforeClear, float initialDelay)
    {
        yield return new WaitForSeconds(initialDelay);
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
