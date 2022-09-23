using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class TitleMgr : MonoBehaviour
{
    public Image Fadeimage;
    public Button ReAnimBtn;
    public Button PlayBtn;
    public GameObject TitleAnim;
    public GameObject FadePanel;
    float fadeCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        if (ReAnimBtn != null)
            ReAnimBtn.onClick.AddListener(AnimStart);
        if (PlayBtn != null)
            PlayBtn.onClick.AddListener(GameStart);
        Time.timeScale = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GameStart()
    {        
        StartCoroutine(FadeCoroutine());                
    }

    void AnimStart()
    {
        TitleAnim.gameObject.SetActive(true);
    }

    IEnumerator FadeCoroutine()
    {
        FadePanel.SetActive(true);
        fadeCount = 0;
        while(fadeCount <1.0f)
        {
            fadeCount += 0.01f;
            yield return new WaitForSeconds(0.01f);
            Fadeimage.color = new Color(0, 0, 0, fadeCount);
        }
        SceneManager.LoadScene("InGame");
    }

}
