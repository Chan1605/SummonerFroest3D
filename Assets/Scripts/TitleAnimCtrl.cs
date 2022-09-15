using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleAnimCtrl : MonoBehaviour
{
    public Image BgImage;
    public Animator BGanim;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void AnimExit()
    {
        BgImage.gameObject.SetActive(false);
    }
}
