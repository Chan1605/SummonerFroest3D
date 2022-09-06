using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameMgr : MonoBehaviour
{
    public static GameMgr Inst = null;

    public GameObject m_CursorMark = null;
    Vector3 a_CacVLen = Vector3.zero;

    [HideInInspector] public Hero Yasuo = null;

    public Text GuideText; //UI 텍스트
    public Image QSkillicon; //스킬 이미지
    public Image DSkillicon; //스킬 이미지
    public Image FSkillicon; //스킬 이미지
    public Image SkillCoolimg;
    public Image DSkillCoolimg;
    public Image FSkillCoolimg;
    public Text QSkillInfoText; //스킬 가이드 텍스트
    public Text DSkillInfoText; //스킬 가이드 텍스트
    public Text FSkillInfoText; //스킬 가이드 텍스트
    public Text HpInfo;
   
    void Awake()
    {
        //GameMgr 클래스를 인스턴스에 대입
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
