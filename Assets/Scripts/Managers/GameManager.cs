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

    //Dialogue handling
    public TMP_Text playerText;

    //Arm
    public ArmLogic_Simplified arm;
    public ArmLogic oldArm;

    void Start()
    {
        Instance = this;
        StartCoroutine(FadeFromBlack(5,1));
        StartCoroutine(ActivateArm(10));
    }

    void DropBeer()
    {
        PlayDialogue("oh wait - shoot", .15f, 2.5f, 0f);
        PlayDialogue("god damnit", .3f, 2f, 4f);
        beer.isKinematic = false;
        beer.angularVelocity = initialAngularVelocity;
        beer.velocity = Vector3.up * -2f;
        StartCoroutine(PlaySplash(.4f));
    }

    IEnumerator PlaySplash(float delay)
    {
        yield return new WaitForSeconds(delay);
        splashEffect.Play();
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
    }

    IEnumerator ActivateArm(float activateTime)
    {
        yield return new WaitForSeconds (activateTime);

        if(arm) arm.allowArmDrop = true;
        if(oldArm) oldArm.allowArmDrop = true;
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
