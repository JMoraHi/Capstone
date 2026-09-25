using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Events;

public class LoadingScreenBarSystem : MonoBehaviour {

    public GameObject bar;
    public Text loadingText;
    public bool backGroundImageAndLoop;
    public float LoopTime;
    public GameObject[] backgroundImages;
    [Range(0,1f)]public float vignetteEfectVolue; // Must be a value between 0 and 1
    AsyncOperation async;
    Image vignetteEfect;

    public void loadingScreen (int sceneNo)
    {
        this.gameObject.SetActive(true);
        StartCoroutine(Loading(sceneNo));
        
    }

    private void Start()
    {
        vignetteEfect = transform.Find("VignetteEfect").GetComponent<Image>();
        vignetteEfect.color = new Color(vignetteEfect.color.r,vignetteEfect.color.g,vignetteEfect.color.b,vignetteEfectVolue);

        if (backGroundImageAndLoop)
            StartCoroutine(transitionImage());
    }

    IEnumerator transitionImage ()
    {
        for (int i = 0; i < backgroundImages.Length; i++)
        {
            yield return new WaitForSeconds(LoopTime);
            for (int j = 0; j < backgroundImages.Length; j++)
                backgroundImages[j].SetActive(false);
            backgroundImages[i].SetActive(true);           
        }
    }

    IEnumerator Loading (int sceneNo)
    {
        async = SceneManager.LoadSceneAsync(sceneNo);
        bar.transform.localScale = new Vector3(0,0.9f,1);
        async.allowSceneActivation = false;
        while (async.isDone == false)
        {
            
            float progress = Mathf.Clamp01(async.progress / 0.9f);
            bar.transform.localScale = new Vector3(progress, 0.9f, 1);

            if (loadingText != null)
                loadingText.text = (100 * bar.transform.localScale.x ).ToString("####") + "%";

            if (async.progress == 0.9f)
            {
                bar.transform.localScale = new Vector3(1, 0.9f, 1);
                yield return new WaitForSeconds(3);
                async.allowSceneActivation = true;
            }
            yield return null;
        
        }
    }




}
