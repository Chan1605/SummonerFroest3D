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
    public Text Qskname;
    public Text QInfo;

    public GameObject m_CursorMark = null;
    Vector3 a_CacVLen = Vector3.zero;

    [HideInInspector] public Hero Yasuo = null;

    public Text GuideText; //UI �ؽ�Ʈ
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

    void Awake()
    {
        //GameMgr Ŭ������ �ν��Ͻ��� ����
        Inst = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CursorOffObserver(); 
        
        if( IsCollSlot(QSkillicon.gameObject)== true)
        {
            Showtooltip("��ǳ��", "���� ���� �����մϴ�.\n(������ �� ��� �� ������ ���̾Ƹ�ŭ �������� ���� óġ�մϴ�.)\n (���� ���ð� : 3��)",QSkillicon.transform.position);
        }
        else if (IsCollSlot(WSkillicon.gameObject) == true)
        {
            Showtooltip("������ ��","��� �� 10�� ���� �÷��̾ ��ȭ�˴ϴ�.\n(��ǳ ��(���ӻ��),�߽��� �̵��ӵ� ����\n(���� ���ð� : 20��)", WSkillicon.transform.position);
                                
        }
        else if (IsCollSlot(DSkillicon.gameObject) == true)
        {
            Showtooltip("ȸ��", "HP�� 30ȸ�� �մϴ�.\n(���� ���ð� : 10��)", DSkillicon.transform.position);
        }
        else if (IsCollSlot(FSkillicon.gameObject) == true)
        {
            Showtooltip("����", "���콺 Ŀ�� ��ġ�� ª���Ÿ��� �����̵�\n(���� ���ð� : 20��)", FSkillicon.transform.position);
        }
        else
        {
            HelpBox.gameObject.SetActive(false);
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
