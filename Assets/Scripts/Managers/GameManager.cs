using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    //Ui stuff
    public Image fadeImage;

    void Start()
    {
        Instance = this;
        StartCoroutine(FadeFromBlack(5,1));
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
}
