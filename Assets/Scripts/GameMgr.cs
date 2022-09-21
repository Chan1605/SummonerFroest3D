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
    public Button EscBtn;

    public Text Qskname;
    public Text QInfo;
    public Text PlayTimeTxt;
    [HideInInspector] public float PlayTimer = 0.0f;

    public GameObject TutorialGroup;
    public GameObject DiaTooltip;
    public Image Dia;
    public GameObject m_CursorMark = null;
    Vector3 a_CacVLen = Vector3.zero;

    public Sprite[] TierImgs;
    public Image ShowTier;
    public Text TierTxt;

    public Sprite[] EscTierImgs;
    public Image EscShowTier;
    public Text EscTierTxt;

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
    bool IsStart = false;
    

    [Header("-------- Monster Spawn --------")]
    //몬스터가 출현할 위치를 담을 배열
    public Transform[] points;
    //몬스터 프리팹을 할당할 변수
    public GameObject[] monsterPrefab;
    //몬스터를 발생시킬 주기
    public float createTime = 1.0f;    
    //몬스터의 최대 발생 개수
    public int maxMonster = 15;

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
        if (EscBtn != null)
        {
            EscBtn.onClick.AddListener(EscFunc);
        }

        points = GameObject.Find("SpawnPoint").GetComponentsInChildren<Transform>();
        //if (PlayTimer > 45.0f)
        //{
        //    if (points.Length > 0)
        //    {
        //        //몬스터 생성 코루틴 함수 호출
        //        StartCoroutine(this.CreateMonster());
        //    }
        //}
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
        else if (IsCollSlot(Dia.gameObject) == true)
        {                    
            Showtooltip("다이아", "낭인의 길(W)을 사용하기 위한 필수 재화입니다.\n 적 처치시 드랍가능\n(최소 2개이상)",
                DiaTooltip.transform.position);
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
                TutorialGroup.gameObject.SetActive(true);
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
               
                if (points.Length > 0)
                {
                    //몬스터 생성 코루틴 함수 호출                 
                    if (!IsStart)
                    {                        
                        StartCoroutine(this.CreateMonster());
                        IsStart = true;
                    }

                }
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
            EscBtn.gameObject.SetActive(false);
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

    void EscFunc()
    {
        EscPanel.SetActive(true);
        EscBtn.gameObject.SetActive(false);
    }

    void Resume()
    {
        EscPanel.SetActive(false);
        Time.timeScale = 1f;
        Isesc = false;
        EscBtn.gameObject.SetActive(true);
        
    }

    void Pause()
    {
        EscPanel.SetActive(true);
        Time.timeScale = 0f;
        Isesc = true;
        EscKillTxt.text = "잡은 몬스터 : " + Yasuo.Killcount;
        EscPlayTimeTxt.text = "게임 시간 : " + ((int)PlayTimer / 60 % 60).ToString("00") + " : " + ((int)PlayTimer % 60).ToString("00");

        if (Yasuo.Killcount <= 10)
        {
            EscShowTier.sprite = EscTierImgs[0];
            EscTierTxt.color = new Color32(135, 65, 0, 255);
            EscTierTxt.text = "브론즈";
        }
        else if (Yasuo.Killcount <= 20)
        {
            EscShowTier.sprite = EscTierImgs[1];
            EscTierTxt.color = new Color32(255, 255, 255, 157);
            EscTierTxt.text = "실버";
        }
        else if (Yasuo.Killcount <= 30)
        {
            EscShowTier.sprite = EscTierImgs[2];
            EscTierTxt.color = new Color32(255, 234, 0, 255);
            EscTierTxt.text = "골드";
        }
        else if (Yasuo.Killcount <= 40)
        {
            EscShowTier.sprite = EscTierImgs[3];
            EscTierTxt.color = new Color32(0, 255, 0, 255);
            EscTierTxt.text = "플레티넘";
        }
        else if (Yasuo.Killcount <= 50)
        {
            EscShowTier.sprite = EscTierImgs[4];
            EscTierTxt.color = new Color32(0, 0, 255, 255);
            EscTierTxt.text = "다이아";
        }
        else if (Yasuo.Killcount > 60)
        {
            EscShowTier.sprite = EscTierImgs[5];
            EscTierTxt.color = new Color32(255, 255, 0, 255);
            EscTierTxt.text = "챌린저";
        }
    }



    public void GameOver()
    {
        GameOverPanel.SetActive(true);
        EndKillTxt.text = "잡은 몬스터 : " + Yasuo.Killcount;
        EndPlayTimeTxt.text = "게임 시간 : " + ((int)PlayTimer / 60 % 60).ToString("00") + " : " + ((int)PlayTimer % 60).ToString("00");
        if(Yasuo.Killcount <= 10)
        {
            ShowTier.sprite = TierImgs[0];
            TierTxt.color = new Color32(135, 65, 0, 255);
            TierTxt.text = "브론즈";
        }
        else if (Yasuo.Killcount <= 20)
        {
            ShowTier.sprite = TierImgs[1];
            TierTxt.color = new Color32(255, 255, 255, 157);
            TierTxt.text = "실버";
        }
        else if (Yasuo.Killcount <= 30)
        {
            ShowTier.sprite = TierImgs[2];
            TierTxt.color = new Color32(255, 234, 0, 255);
            TierTxt.text = "골드";
        }
        else if (Yasuo.Killcount <= 40)
        {
            ShowTier.sprite = TierImgs[3];
            TierTxt.color = new Color32(0, 255, 0, 255);
            TierTxt.text = "플레티넘";
        }
        else if (Yasuo.Killcount <= 50)
        {
            ShowTier.sprite = TierImgs[4];
            TierTxt.color = new Color32(0, 0, 255, 255);
            TierTxt.text = "다이아";
        }
        else if (Yasuo.Killcount > 60)
        {
            ShowTier.sprite = TierImgs[5];
            TierTxt.color = new Color32(255, 255, 0, 255);
            TierTxt.text = "챌린저";
        }


    }

    IEnumerator CreateMonster()
    {
        //게임 종료 시까지 무한 루프
        while (!Yasuo.IsDie)
        {
            //몬스터 생성 주기 시간만큼 메인 루프에 양보
            yield return new WaitForSeconds(createTime);


            //플레이어가 사망했을 때 코루틴을 종료해 다음 루틴을 진행하지 않음
            if (Yasuo.yasuo == Hero.YasuoState.die)
                yield break;     //코루틴 함수에서 함수를 빠져나가는 명령 

            ////오브젝트 풀의 처음부터 끝까지 순회
            //foreach (GameObject monster in monsterPool)
            //{
            //    //비활성화 여부로 사용 가능한 몬스터를 판단
            //    if (!monster.activeSelf)
            //    {
            //        //몬스터를 출현시킬 위치의 인덱스값을 추출
            //        int idx = Random.Range(1, points.Length);
            //        //몬스터의 출현위치를 설정
            //        monster.transform.position = points[idx].position;
            //        //몬스터를 활성화함
            //        monster.SetActive(true);
            //        //오브젝트 풀에서 몬스터 프리팹 하나를 활성화한 후 for 루프를 빠져나감
            //        break;
            //    }
            //} //foreach(GameObject monster in monsterPool)


            //현재 생성된 몬스터 개수 산출
            int monsterCount = (int)GameObject.FindGameObjectsWithTag("Enemy").Length;

            //몬스터의 최대 생성 개수보다 작을 때만 몬스터 생성
            if (monsterCount < maxMonster)
            {
                //몬스터의 생성 주기 시간만큼 대기
                yield return new WaitForSeconds(createTime);

                //불규칙적인 위치 산출
                int idx = Random.Range(1, points.Length);
                //몬스터의 동적 생성
                int Randmon = Random.Range(0, 2);                
                Instantiate(monsterPrefab[Randmon], points[idx].position, points[idx].rotation);
            }
            else
            {
                yield return null;
            }

        } //while (!isGameOver)
    } //IEnumerator CreateMonster()

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
