using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyHandler : MonoBehaviour
{
    public Transform W,A,S,D,UP,DOWN,LEFT,RIGHT,SPACE;
    public GameObject popParticle;
    public float secondsToPop = 0.025f;

    float Wframes, Aframes, Sframes, Dframes, Upframes, DownFrames, LeftFrames, RightFrames, SpaceFrames;
    public AnimationCurve popCurve;

    int totalPops = 0;

    public GameObject startFade;
    public Material fadeMaterial;

    void Update()
    {
        Wframes = PressButton(W, Input.GetKey(KeyCode.W),Wframes);
        Aframes = PressButton(A, Input.GetKey(KeyCode.A), Aframes);
        Sframes = PressButton(S, Input.GetKey(KeyCode.S), Sframes);
        Dframes = PressButton(D, Input.GetKey(KeyCode.D),Dframes);
        Upframes = PressButton(UP, Input.GetKey(KeyCode.UpArrow),Upframes);
        DownFrames = PressButton(DOWN, Input.GetKey(KeyCode.DownArrow),DownFrames);
        LeftFrames = PressButton(LEFT, Input.GetKey(KeyCode.LeftArrow),LeftFrames);
        RightFrames = PressButton(RIGHT, Input.GetKey(KeyCode.RightArrow),RightFrames);
        SpaceFrames = PressButton(SPACE, Input.GetKey(KeyCode.Space),SpaceFrames);
    }

    float PressButton(Transform key, bool pressed, float _framesHeld)
    {
        if(!key.gameObject.activeSelf) return 100;

        Vector3 targetScale = new Vector3(1.1f,1.1f, .2f);
        Vector3 startScale = new Vector3(1,1,1);

        if(pressed)
        {
            _framesHeld += Time.deltaTime;
        }
        else
        {
            _framesHeld = 0;
        }

        float t = popCurve.Evaluate(_framesHeld/secondsToPop);

        key.localScale = Vector3.Lerp(startScale,targetScale, t);

        if(_framesHeld > secondsToPop) DestroyKey(key);

        return _framesHeld;
    }
    void PlayBubblePop(Vector3 position)
    {
        float pitch = UnityEngine.Random.Range(0.9f, 1.25f);
        AudioManager.Instance.PlaySound(AudioManager.Instance.bubblePopSounds, 0.25f, pitch, position);
    }
    void DestroyKey(Transform key)
    {
        Instantiate(popParticle, key.position, popParticle.transform.rotation);
        key.gameObject.SetActive(false);
        totalPops +=1;

        PlayBubblePop(key.position);

        if(totalPops == 9)
        {
            StartCoroutine(FadeOut(0.5f));
        }
    }

    IEnumerator FadeOut(float duration)
    {
        float elapsedTime = 0;
        Color startColor = fadeMaterial.color;
        Color endColor = new Color(1, 1, 1, 0);
        while(elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            fadeMaterial.color =  Color.Lerp(startColor, endColor, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        startFade.SetActive(false);
        fadeMaterial.color = startColor;
        GameManager.Instance.EndBubbleScene();
    }
}
