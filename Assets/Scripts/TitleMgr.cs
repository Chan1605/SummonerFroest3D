using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class TitleMgr : MonoBehaviour
{
    public Button ReAnimBtn;
    public Button PlayBtn;
    public GameObject TitleAnim;
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
        SceneManager.LoadScene("InGame");
    }

    void AnimStart()
    {
        TitleAnim.gameObject.SetActive(true);
    }

}
