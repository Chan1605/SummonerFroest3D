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

    public Text GuideText; //UI �ؽ�Ʈ
    public Text TutoGuideText; //UI �ؽ�Ʈ
    public Image QSkillicon; //��ų �̹���
    public Image WSkillicon; //��ų �̹���
    public Image DSkillicon; //��ų �̹���
    public Image FSkillicon; //��ų �̹���
    public Image SkillCoolimg;
    public Image WSkillCoolimg;
    public Image DSkillCoolimg;
    public Image FSkillCoolimg;
    public Text SkCntTxt;  //��ų �ؽ�Ʈ
    public Text QSkillInfoText; //��ų ���̵� �ؽ�Ʈ
    public Text WSkillInfoText; //��ų ���̵� �ؽ�Ʈ
    public Text DSkillInfoText; //��ų ���̵� �ؽ�Ʈ
    public Text FSkillInfoText; //��ų ���̵� �ؽ�Ʈ
    public Text HpInfo;
    public Text EnemyTxt;
    public Text InfoText;

    bool Isesc = false;
    bool IsStart = false;
    

    [Header("-------- Monster Spawn --------")]
    //���Ͱ� ������ ��ġ�� ���� �迭
    public Transform[] points;
    //���� �������� �Ҵ��� ����
    public GameObject[] monsterPrefab;
    //���͸� �߻���ų �ֱ�
    public float createTime = 1.0f;    
    //������ �ִ� �߻� ����
    public int maxMonster = 15;

    void Awake()
    {
        //GameMgr Ŭ������ �ν��Ͻ��� ����
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
        //        //���� ���� �ڷ�ƾ �Լ� ȣ��
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
            Showtooltip("��ǳ��", "������ ���ڷ� ������ �̵� �� ���ظ� �ݴϴ�.\n ������ �� ��� �� : ������ ���̾Ƹ�ŭ �������� ���\n(���� ���ð� : 3��)", QSkillicon.transform.position);
        }
        else if (IsCollSlot(WSkillicon.gameObject) == true)
        {
            Showtooltip("������ ��", "��� �� 10�� ���� ���� ��ȭ�˴ϴ�.\n������ ������ ���̾ƴ� 1���� �˴ϴ�.\n(���� ���ð� : 20��)", WSkillicon.transform.position);

        }
        else if (IsCollSlot(DSkillicon.gameObject) == true)
        {
            Showtooltip("ȸ��", "HP�� 30ȸ�� �մϴ�.\n(���� ���ð� : 10��)", DSkillicon.transform.position);
        }
        else if (IsCollSlot(FSkillicon.gameObject) == true)
        {
            Showtooltip("����", "���콺 Ŀ�� ��ġ�� ª���Ÿ��� �����̵�\n(���� ���ð� : 20��)", FSkillicon.transform.position);
        }
        else if (IsCollSlot(Dia.gameObject) == true)
        {                    
            Showtooltip("���̾�", "������ ��(W)�� ����ϱ� ���� �ʼ� ��ȭ�Դϴ�.\n �� óġ�� �������\n(�ּ� 2���̻�)",
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
            ((int)PlayTimer % 60).ToString("00");//+ Mathf.Round(m_Timer) + "��";

        }

        if (TutoGuideText != null)
        {
            if (PlayTimer > 2.0f)
            {
                TutoGuideText.gameObject.SetActive(true);
                TutoGuideText.text = "��ȯ���� ���� ���Ű��� ȯ���մϴ�.\n" +
                    "����� Ʃ�丮�� ������ �����˴ϴ�.";

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
                InfoText.text = "���콺 ��Ŭ������ ���� �����մϴ�.\n���� óġ �� ���̾ư� ����˴ϴ�.";
            }

            if (PlayTimer > 30.0f)
            {
                TutoGuideText.gameObject.SetActive(true);
                TutoGuideText.text = "��� �� ������ �����˴ϴ�.\n ������ ����Ͻʽÿ�.";
            }
            if (PlayTimer > 35.0f)
            {
                TutoGuideText.gameObject.SetActive(false);
                TutoGuideText.text = "";
                InfoText.text = "���ݰ� ��ų��� ������ óġ�Ͻʽÿ�.";
            }
            if (PlayTimer > 45.0f)
            {
                TutoGuideText.gameObject.SetActive(true);
                TutoGuideText.text = "������ �����Ǿ����ϴ�.";
                InfoBox.gameObject.SetActive(false);
               
                if (points.Length > 0)
                {
                    //���� ���� �ڷ�ƾ �Լ� ȣ��                 
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

    void CursorOffObserver() //---Ŭ����ũ ����
    {
        if (m_CursorMark == null)
            return;

        if (m_CursorMark.activeSelf == false)
            return;

        if (Yasuo == null) //���ΰ��� �׾��ٸ�...
            return;

        a_CacVLen = Yasuo.transform.position -
                            m_CursorMark.transform.position;
        a_CacVLen.y = 0.0f;
        if (a_CacVLen.magnitude < 1.0f)
            m_CursorMark.SetActive(false);

    }//void CursorMarkOn(Vector3 a_PickPos)


    bool IsCollSlot(GameObject a_CkObj)  //���콺�� UI ���� ������Ʈ ���� �ִ���? �Ǵ��ϴ� �Լ�
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
        EscKillTxt.text = "���� ���� : " + Yasuo.Killcount;
        EscPlayTimeTxt.text = "���� �ð� : " + ((int)PlayTimer / 60 % 60).ToString("00") + " : " + ((int)PlayTimer % 60).ToString("00");

        if (Yasuo.Killcount <= 10)
        {
            EscShowTier.sprite = EscTierImgs[0];
            EscTierTxt.color = new Color32(135, 65, 0, 255);
            EscTierTxt.text = "�����";
        }
        else if (Yasuo.Killcount <= 20)
        {
            EscShowTier.sprite = EscTierImgs[1];
            EscTierTxt.color = new Color32(255, 255, 255, 157);
            EscTierTxt.text = "�ǹ�";
        }
        else if (Yasuo.Killcount <= 30)
        {
            EscShowTier.sprite = EscTierImgs[2];
            EscTierTxt.color = new Color32(255, 234, 0, 255);
            EscTierTxt.text = "���";
        }
        else if (Yasuo.Killcount <= 40)
        {
            EscShowTier.sprite = EscTierImgs[3];
            EscTierTxt.color = new Color32(0, 255, 0, 255);
            EscTierTxt.text = "�÷�Ƽ��";
        }
        else if (Yasuo.Killcount <= 50)
        {
            EscShowTier.sprite = EscTierImgs[4];
            EscTierTxt.color = new Color32(0, 0, 255, 255);
            EscTierTxt.text = "���̾�";
        }
        else if (Yasuo.Killcount > 60)
        {
            EscShowTier.sprite = EscTierImgs[5];
            EscTierTxt.color = new Color32(255, 255, 0, 255);
            EscTierTxt.text = "ç����";
        }
    }



    public void GameOver()
    {
        GameOverPanel.SetActive(true);
        EndKillTxt.text = "���� ���� : " + Yasuo.Killcount;
        EndPlayTimeTxt.text = "���� �ð� : " + ((int)PlayTimer / 60 % 60).ToString("00") + " : " + ((int)PlayTimer % 60).ToString("00");
        if(Yasuo.Killcount <= 10)
        {
            ShowTier.sprite = TierImgs[0];
            TierTxt.color = new Color32(135, 65, 0, 255);
            TierTxt.text = "�����";
        }
        else if (Yasuo.Killcount <= 20)
        {
            ShowTier.sprite = TierImgs[1];
            TierTxt.color = new Color32(255, 255, 255, 157);
            TierTxt.text = "�ǹ�";
        }
        else if (Yasuo.Killcount <= 30)
        {
            ShowTier.sprite = TierImgs[2];
            TierTxt.color = new Color32(255, 234, 0, 255);
            TierTxt.text = "���";
        }
        else if (Yasuo.Killcount <= 40)
        {
            ShowTier.sprite = TierImgs[3];
            TierTxt.color = new Color32(0, 255, 0, 255);
            TierTxt.text = "�÷�Ƽ��";
        }
        else if (Yasuo.Killcount <= 50)
        {
            ShowTier.sprite = TierImgs[4];
            TierTxt.color = new Color32(0, 0, 255, 255);
            TierTxt.text = "���̾�";
        }
        else if (Yasuo.Killcount > 60)
        {
            ShowTier.sprite = TierImgs[5];
            TierTxt.color = new Color32(255, 255, 0, 255);
            TierTxt.text = "ç����";
        }


    }

    IEnumerator CreateMonster()
    {
        //���� ���� �ñ��� ���� ����
        while (!Yasuo.IsDie)
        {
            //���� ���� �ֱ� �ð���ŭ ���� ������ �纸
            yield return new WaitForSeconds(createTime);


            //�÷��̾ ������� �� �ڷ�ƾ�� ������ ���� ��ƾ�� �������� ����
            if (Yasuo.yasuo == Hero.YasuoState.die)
                yield break;     //�ڷ�ƾ �Լ����� �Լ��� ���������� ��� 

            ////������Ʈ Ǯ�� ó������ ������ ��ȸ
            //foreach (GameObject monster in monsterPool)
            //{
            //    //��Ȱ��ȭ ���η� ��� ������ ���͸� �Ǵ�
            //    if (!monster.activeSelf)
            //    {
            //        //���͸� ������ų ��ġ�� �ε������� ����
            //        int idx = Random.Range(1, points.Length);
            //        //������ ������ġ�� ����
            //        monster.transform.position = points[idx].position;
            //        //���͸� Ȱ��ȭ��
            //        monster.SetActive(true);
            //        //������Ʈ Ǯ���� ���� ������ �ϳ��� Ȱ��ȭ�� �� for ������ ��������
            //        break;
            //    }
            //} //foreach(GameObject monster in monsterPool)


            //���� ������ ���� ���� ����
            int monsterCount = (int)GameObject.FindGameObjectsWithTag("Enemy").Length;

            //������ �ִ� ���� �������� ���� ���� ���� ����
            if (monsterCount < maxMonster)
            {
                //������ ���� �ֱ� �ð���ŭ ���
                yield return new WaitForSeconds(createTime);

                //�ұ�Ģ���� ��ġ ����
                int idx = Random.Range(1, points.Length);
                //������ ���� ����
                int Randmon = Random.Range(0, 2);                
                Instantiate(monsterPrefab[Randmon], points[idx].position, points[idx].rotation);
            }
            else
            {
                yield return null;
            }

        } //while (!isGameOver)
    } //IEnumerator CreateMonster()

    public static bool IsPointerOverUIObject() //UGUI�� UI���� ���� ��ŷ�Ǵ��� Ȯ���ϴ� �Լ�
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
