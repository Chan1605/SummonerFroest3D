using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameMgr : MonoBehaviour
{
    public static GameMgr Inst = null;

    public GameObject m_CoinItem;
    public GameObject HelpBox;
    public GameObject InfoBox;
    public GameObject GameOverPanel;
    public GameObject EscPanel;
    public Text EndPlayTimeTxt;
    public Text EndKillTxt;
    public Text EscPlayTimeTxt;
    public Text EscKillTxt;
    public Button TitleBtn;
    public Button RePlayBtn;
    public Button EscTitleBtn;

    public Text Qskname;
    public Text QInfo;
    public Text PlayTimeTxt;
    [HideInInspector] public float PlayTimer = 0.0f;

    public GameObject m_CursorMark = null;
    Vector3 a_CacVLen = Vector3.zero;

    [HideInInspector] public Hero Yasuo = null;

    public Text GuideText; //UI 텍스트
    public Text TutoGuideText; //UI 텍스트
    public Image QSkillicon; //스킬 이미지
    public Image WSkillicon; //스킬 이미지
    public Image DSkillicon; //스킬 이미지
    public Image FSkillicon; //스킬 이미지
    public Image SkillCoolimg;
    public Image WSkillCoolimg;
    public Image DSkillCoolimg;
    public Image FSkillCoolimg;
    public Text SkCntTxt;  //스킬 텍스트
    public Text QSkillInfoText; //스킬 가이드 텍스트
    public Text WSkillInfoText; //스킬 가이드 텍스트
    public Text DSkillInfoText; //스킬 가이드 텍스트
    public Text FSkillInfoText; //스킬 가이드 텍스트
    public Text HpInfo;
    public Text EnemyTxt;
    public Text InfoText;

    bool Isesc = false;

    void Awake()
    {
        //GameMgr 클래스를 인스턴스에 대입
        Inst = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (TitleBtn != null)
        {
            TitleBtn.onClick.AddListener(TitleFunc);
        }
        if (EscTitleBtn != null)
        {
            EscTitleBtn.onClick.AddListener(TitleFunc);
        }

        if (RePlayBtn != null)
        {
            RePlayBtn.onClick.AddListener(ReplayFunc);
        }
    }

    // Update is called once per frame
    void Update()
    {
        CursorOffObserver();
        EscCtr();

        if (IsCollSlot(QSkillicon.gameObject) == true)
        {
            Showtooltip("질풍검", "조준한 적뒤로 빠르게 이동 후 피해를 줍니다.\n 낭인의 길 사용 시 : 보유한 다이아만큼 연속으로 사용\n(재사용 대기시간 : 3초)", QSkillicon.transform.position);
        }
        else if (IsCollSlot(WSkillicon.gameObject) == true)
        {
            Showtooltip("낭인의 길", "사용 시 10초 동안 대폭 강화됩니다.\n버프가 끝나면 다이아는 1개가 됩니다.\n(재사용 대기시간 : 20초)", WSkillicon.transform.position);

        }
        else if (IsCollSlot(DSkillicon.gameObject) == true)
        {
            Showtooltip("회복", "HP를 30회복 합니다.\n(재사용 대기시간 : 10초)", DSkillicon.transform.position);
        }
        else if (IsCollSlot(FSkillicon.gameObject) == true)
        {
            Showtooltip("점멸", "마우스 커서 위치로 짧은거리를 순간이동\n(재사용 대기시간 : 20초)", FSkillicon.transform.position);
        }
        else
        {
            HelpBox.gameObject.SetActive(false);
        }


        if (PlayTimeTxt != null)
        {
            PlayTimer += Time.deltaTime;
            PlayTimeTxt.text = ((int)PlayTimer / 60 % 60).ToString("00") + " : " +
            ((int)PlayTimer % 60).ToString("00");//+ Mathf.Round(m_Timer) + "초";

        }

        if (TutoGuideText != null)
        {
            if (PlayTimer > 2.0f)
            {
                TutoGuideText.gameObject.SetActive(true);
                TutoGuideText.text = "소환사의 숲에 오신것을 환영합니다.\n" +
                    "잠시후 튜토리얼 적들이 생성됩니다.";

            }
            if (PlayTimer > 5.0f)
            {
                TutoGuideText.gameObject.SetActive(false);
                InfoBox.gameObject.SetActive(true);
            }
            if (PlayTimer > 15.0f)
            {
                EnemyTxt.gameObject.SetActive(true);
                InfoText.text = "마우스 좌클릭으로 적을 공격합니다.\n적을 처치 시 다이아가 드랍됩니다.";
            }

            if (PlayTimer > 30.0f)
            {
                TutoGuideText.gameObject.SetActive(true);
                TutoGuideText.text = "잠시 후 적들이 생성됩니다.\n 전투에 대비하십시오.";
            }
            if (PlayTimer > 35.0f)
            {
                TutoGuideText.gameObject.SetActive(false);
                TutoGuideText.text = "";
                InfoText.text = "공격과 스킬들로 적들을 처치하십시오.";
            }
            if (PlayTimer > 45.0f)
            {
                TutoGuideText.gameObject.SetActive(true);
                TutoGuideText.text = "적들이 생성되었습니다.";
                InfoBox.gameObject.SetActive(false);
            }
            if (PlayTimer > 48.0f)
            {
                TutoGuideText.gameObject.SetActive(false);

            }
        }


    }

    public void CursorMarkOn(Vector3 a_PickPos)
    {
        if (m_CursorMark == null)
            return;

        m_CursorMark.transform.position = new Vector3(a_PickPos.x, a_PickPos.y + 0.2f, a_PickPos.z);

        m_CursorMark.SetActive(true);

    }//void CursorMarkOn(Vector3 a_PickPos

    void CursorOffObserver() //---클릭마크 끄기
    {
        if (m_CursorMark == null)
            return;

        if (m_CursorMark.activeSelf == false)
            return;

        if (Yasuo == null) //주인공이 죽었다면...
            return;

        a_CacVLen = Yasuo.transform.position -
                            m_CursorMark.transform.position;
        a_CacVLen.y = 0.0f;
        if (a_CacVLen.magnitude < 1.0f)
            m_CursorMark.SetActive(false);

    }//void CursorMarkOn(Vector3 a_PickPos)


    bool IsCollSlot(GameObject a_CkObj)  //마우스가 UI 슬롯 오브젝트 위에 있느냐? 판단하는 함수
    {
        Vector3[] v = new Vector3[4];
        a_CkObj.GetComponent<RectTransform>().GetWorldCorners(v);
        if (v[0].x <= Input.mousePosition.x && Input.mousePosition.x <= v[2].x &&
           v[0].y <= Input.mousePosition.y && Input.mousePosition.y <= v[2].y)
        {
            return true;
        }

        return false;
    }

    void Showtooltip(string a_name, string a_Info, Vector3 a_pos)
    {
        HelpBox.gameObject.SetActive(true);
        Vector3 pos = a_pos;
        pos += new Vector3(HelpBox.GetComponent<RectTransform>().rect.width * 0.5f,
                        -HelpBox.GetComponent<RectTransform>().rect.height * 0.4f, 0);
        HelpBox.transform.position = pos;
        Qskname.text = a_name;
        QInfo.text = a_Info;
    }
    void EscCtr()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Isesc)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    void TitleFunc()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
    }

    void ReplayFunc()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("InGame");
    }

    void Resume()
    {
        EscPanel.SetActive(false);
        Time.timeScale = 1f;
        Isesc = false;
    }

    void Pause()
    {
        EscPanel.SetActive(true);
        Time.timeScale = 0f;
        Isesc = true;
        EscKillTxt.text = "잡은 몬스터 : " + Yasuo.Killcount;
        EscPlayTimeTxt.text = "게임 시간 : " + ((int)PlayTimer / 60 % 60).ToString("00") + " : " + ((int)PlayTimer % 60).ToString("00");
    }



    public void GameOver()
    {
        GameOverPanel.SetActive(true);
        EndKillTxt.text = "잡은 몬스터 : " + Yasuo.Killcount;
        EndPlayTimeTxt.text = "게임 시간 : " + ((int)PlayTimer / 60 % 60).ToString("00") + " : " + ((int)PlayTimer % 60).ToString("00");

    }

    public static bool IsPointerOverUIObject() //UGUI의 UI들이 먼저 피킹되는지 확인하는 함수
    {
        PointerEventData a_EDCurPos = new PointerEventData(EventSystem.current);
#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID)
       List<RaycastResult> results = new List<RaycastResult>();
       for (int i = 0; i < Input.touchCount; ++i)
       {
            a_EDCurPos.position = Input.GetTouch(i).position;  
            results.Clear();
            EventSystem.current.RaycastAll(a_EDCurPos, results);

            if (0 < results.Count)
                return true;
       }

       return false;
#else
        a_EDCurPos.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(a_EDCurPos, results);
        return (0 < results.Count);
#endif

    }//public bool IsPointerOverUIObject() 
}
