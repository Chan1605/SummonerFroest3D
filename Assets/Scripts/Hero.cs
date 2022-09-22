using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

public class Hero : MonoBehaviour
{
    public enum YasuoState { idle, trace, attack, hit, die, skill,skilling, skillend };

    public YasuoState yasuo = YasuoState.idle;
    [SerializeField] float m_CurHp = 100.0f;
    [SerializeField] float m_MaxHp = 100.0f;

    public Image hpbar;
    public RectTransform Aim;
    public Image AimF;
    public GameObject TrEff;    

    public float m_MoveVelocity = 8.0f;
    //------ Picking 관련 변수 
    Ray a_MousePos;
    RaycastHit hitInfo;
    private LayerMask m_layerMask = -1;
    //------ Picking 관련 변수 

    //------ Picking 관련 변수 
    bool m_isPickMvOnOff = false;       //피킹 이동 OnOff
    Vector3 m_TargetPos = Vector3.zero; //최종 목표 위치
    Vector3 m_MoveDir = Vector3.zero;   //평면 진행 방향
    double m_MoveDurTime = 0.0;         //목표점까지 도착하는데 걸리는 시간
    double m_AddTimeCount = 0.0;        //누적시간 카운트
    float m_RotSpeed = 7.0f;            //초당 회전 속도
    Vector3 a_StartPos = Vector3.zero;  //계산용 변수
    Vector3 a_CacLenVec = Vector3.zero; //계산용 변수
    Quaternion a_TargetRot;             //계산용 변수

    [HideInInspector] public int m_CurPathIndex = 1;

    GameObject m_TargetUnit = null;
    Animator m_RefAnimator = null;

    GameObject[] m_EnemyList = null;    //필드상의 몬스터들을 가져오기 위한 변수

    float m_AttackDist = 1.9f;          //주인공의 공격거리    

    Vector3 a_CacTgVec = Vector3.zero;  //타겟까지의 거리 계산용 변수
    Vector3 a_CacAtDir = Vector3.zero;  //공격시 방향전환용 변수

    bool IsSkill = false;
    [HideInInspector]public bool IsBuff = false;    

    [HideInInspector] public bool IsDie = false;
    BoxCollider SwordCol;
    public GameObject Sword;
    private Transform Taget;

    ColorCorrectionCurves colorCorrection;

    float skill_Time = 3.0f;
    float Wskill_Time = 0.0f; //쿨타임
    float WDuration = 0.0f;   //버프 지속시간
    float Dskill_Time = 15.0f;
    float Fskill_Time = 20.0f;
    float skill_Delay = 0.0f;
    float Dskill_Delay = 0.0f;
    float Fskill_Delay = 0.0f;

    float GuideTimer = 0.0f;
    
    public GameObject SkillEffect; //Q 홀드 스킬이펙트
    public GameObject SkillEnd;    //Q 엔드 이펙트
    public GameObject HealEffect;  //D 스킬 이펙트
    public GameObject FlashEffect; //F 스킬이펙트
    public GameObject Trail;
    GameObject Skill1;             //W Instantiate용
    GameObject HealInst;           //F Instantiate용
    GameObject FlashInst;          //F Instantiate용
    
    
    int Skcnt = 1; //다이아를 먹으면 증가
    int Ncnt = 1;         //일반 q 스킬
    int cnt;              //현재 남은 몹 카운트
    public int Killcount = 0; //킬 카운트
    TrailRenderer AttTrEff;

    public static Hero Inst = null;

    void Awake()
    {
        Cam a_CamCtrl = Camera.main.GetComponent<Cam>();
        if (a_CamCtrl != null)
            a_CamCtrl.InitCamera(this.gameObject);

        Inst = this;

    }

    void Start()
    {
        colorCorrection = FindObjectOfType<ColorCorrectionCurves>();

        m_CurHp = m_MaxHp;

        GameMgr.Inst.Yasuo = this;

        m_layerMask = 1 << LayerMask.NameToLayer("MyTerrain");
        m_layerMask |= 1 << LayerMask.NameToLayer("MyUnit"); //Unit 도 피킹

        AttTrEff = GameObject.Find("Katana").GetComponent<TrailRenderer>();
        m_RefAnimator = this.gameObject.GetComponent<Animator>();
        SwordCol = Sword.GetComponent<BoxCollider>();
        SwordCol.enabled = false;
        yasuo = YasuoState.idle;
        EnemyCheck();
        
    }



    // Update is called once per frame
    void Update()
    {
        if (IsDie == true)
            return;
            
        MousePick();
        MousePickUpdate();
        YasuoActionUpdate();
        UseSkill();
        UseWSkill();
        UseFlash();
        UseHeal();
        UiInfo();    
        
        if (m_isPickMvOnOff == false && IsSkill == false && m_TargetUnit == null && IsBuff == false)
            yasuo = YasuoState.idle;



    }

    void EnemyCheck()
    {
        MonCtrl[] mon = FindObjectsOfType<MonCtrl>();        

        //for (int i = 0; i < mon.Length; i++)
        //{
        //    if (0 > mon[i].hp)
        //        count++;
        //}        

        GameMgr.Inst.EnemyTxt.text = "Kill Count : " +  Killcount;
        
    }

    private void Update_MousePosition()
    {
        Vector2 mousePos = Input.mousePosition;

        float w = Aim.rect.width;
        float h = Aim.rect.height;
        Aim.position = Input.mousePosition;
    }

    private void AnimType(string anim)
    {
        if (anim != "IsAttack")
        {
            AttTrEff.emitting = false;
        }
        m_RefAnimator.SetBool("Idle", false);
        m_RefAnimator.SetBool("IsTrace", false);
        m_RefAnimator.SetBool("IsAttack", false);
        m_RefAnimator.SetBool("IsDie", false);
        m_RefAnimator.SetBool("IsSkill", false);        

        m_RefAnimator.SetBool(anim, true);
    }



    void YasuoActionUpdate()
    {
        switch (yasuo)
        {

            case YasuoState.idle:
                {
                    AnimType("Idle");

                }
                break;

            case YasuoState.trace:
                {
                    AnimType("IsTrace");
                }
                break;


            case YasuoState.attack:
                {
                    AttackRotUpdate();
                    AnimType("IsAttack");                    
                    AttTrEff.emitting = true;
                    EnemyCheck();
                }
                break;

            case YasuoState.skill:
                {
                    AnimType("IsSkill");
                    colorCorrection.enabled = true;
                    Time.timeScale = 0.5f;
                    TrEff.GetComponent<TrailRenderer>().emitting = true;

                    Update_MousePosition();

                }
                break;
            case YasuoState.skilling:
                {                   
                    Vector3 dir = Taget.position - this.gameObject.transform.position;
                    dir.y = 0;
                    dir.Normalize();

                    Vector3 tagetpos = Taget.position + (dir * 2.0f);
                    this.gameObject.transform.forward = dir;
                    
                    Taget.GetComponent<MonCtrl>().TakeDamage(100);
                    //Ncnt--;
                    DiaCheck();
                    EnemyCheck();
                    this.gameObject.transform.position = tagetpos;
                    Vector3 effectpos = tagetpos;
                    effectpos.y += 2.0f;

                    Skill1 = (GameObject)Instantiate(SkillEnd, effectpos, Quaternion.identity);
                    Skill1.GetComponent<ParticleSystem>().Play();
                    Destroy(Skill1, 1.0f);                    
                    yasuo = YasuoState.skill;
                }
                break;
            case YasuoState.skillend:
                {
                    IsSkill = false;
                    colorCorrection.enabled = false;
                    SwordCol.enabled = false;
                    Aim.gameObject.SetActive(false);
                    SkillEffect.SetActive(false);
                   
                    Time.timeScale = 1.0f;                    
                    skill_Delay = skill_Time;
                    TrEff.GetComponent<TrailRenderer>().emitting = false;
                   
            
                    yasuo = YasuoState.idle;
                    


                }
                break;


        }
    }



    public void MousePicking(Vector3 a_SetPickVec, GameObject a_PickMon = null)
    {
        if (yasuo == YasuoState.skill)
            return;
        a_StartPos = this.transform.position; //출발 위치    
        a_SetPickVec.y = this.transform.position.y; // 최종 목표 위치

        a_CacLenVec = a_SetPickVec - a_StartPos;
        a_CacLenVec.y = 0.0f;

        //-------Picking Enemy 공격 처리 부분
        if (a_PickMon != null)
        {
            a_CacTgVec = a_PickMon.transform.position - transform.position;


            float a_AttDist = m_AttackDist;
            //if (a_PickMon.GetComponent<MonsterCtrl>().m_AggroTarget
            //                                         == this.gameObject)
            //{
            //    a_AttDist = m_AttackDist + 1.0f;
            //    //지금 공격하려고 하는 몬스터의 어그로 타겟이 나면....
            //}

            if (a_CacTgVec.magnitude <= a_AttDist)
            {
                m_TargetUnit = a_PickMon;

                return;

            }//즉시 공격 하라
        } //if (a_PickMon != null)
        if (a_CacLenVec.magnitude < 0.5f)  //너무 근거리 피킹은 스킵해 준다.
            return;

        float a_PathLen = a_CacLenVec.magnitude;

        m_TargetPos = a_SetPickVec;   //최종 목표 위치
        m_isPickMvOnOff = true;       //피킹 이동 OnOff

        m_MoveDir = a_CacLenVec.normalized;
        m_MoveDurTime = a_PathLen / m_MoveVelocity; //도착하는데 걸리는 시간
        m_AddTimeCount = 0.0;

        a_StartPos = this.transform.position; //출발 위치    
        a_SetPickVec.y = this.transform.position.y; // 최종 목표 위치

        a_CacLenVec = a_SetPickVec - a_StartPos;
        a_CacLenVec.y = 0.0f;

    }


    void MousePickUpdate()  //<--- MousePickMove()
    {
        ////-------------- 마우스 피킹 이동
        if (m_isPickMvOnOff == true)
        {
            a_CacLenVec = m_TargetPos - this.transform.position;
            a_CacLenVec.y = 0.0f;

            //캐릭터를 이동방향으로 회전시키는 코드 
            if (0.1f < a_CacLenVec.magnitude)
            {
                m_MoveDir = a_CacLenVec.normalized;
                a_TargetRot = Quaternion.LookRotation(m_MoveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                     a_TargetRot,
                                     Time.deltaTime * m_RotSpeed);
            }
            //캐릭터 회전   

            m_MoveDir = a_CacLenVec.normalized;

            m_AddTimeCount = m_AddTimeCount + Time.deltaTime;
            if (m_MoveDurTime <= m_AddTimeCount) //목표점에 도착한 것으로 판정한다.
            {
                m_isPickMvOnOff = false;

            }
            else
            {
                this.transform.position = this.transform.position +
                                         (m_MoveDir * Time.deltaTime * m_MoveVelocity);
                yasuo = YasuoState.trace;
            }//else

            if (m_TargetUnit != null)
            { //<-- 공격 애니매이션 중이면 가장 가까운 타겟을 자동으로 잡게된다.
                m_isPickMvOnOff = true;
                a_CacTgVec = m_TargetUnit.transform.position -
                                                this.transform.position;
                if (a_CacTgVec.magnitude <= m_AttackDist
                && IsSkill == false) //공격거리
                {

                    yasuo = YasuoState.attack;
                }

            }

        }
    }




    void ClearMsPickPath() //마우스 픽킹이동 취소 함수
    {
        m_isPickMvOnOff = false;


        if (GameMgr.Inst.m_CursorMark != null)
            GameMgr.Inst.m_CursorMark.SetActive(false);
    }



    float a_CacRotSpeed = 0.0f;
    public void AttackRotUpdate()
    { //공격애니메이션 중일 때 타겟을 향해 회전하게 하는 함수

        if (m_TargetUnit == null)
            return;

        a_CacTgVec = m_TargetUnit.transform.position - transform.position;
        a_CacTgVec.y = 0.0f;

        if (a_CacTgVec.magnitude <= (m_AttackDist + 0.3f)) //공격거리
        {
            //캐릭터 스프링 회전   
            a_CacAtDir = a_CacTgVec.normalized;
            if (0.0001f < a_CacAtDir.magnitude)
            {
                a_CacRotSpeed = m_RotSpeed * 3.0f;           //초당 회전 속도
                Quaternion a_TargetRot = Quaternion.LookRotation(a_CacAtDir);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                        a_TargetRot,
                                        Time.deltaTime * a_CacRotSpeed);
            }
        }

    }



    public void TakeDamage(float a_Val)
    {
        if (m_CurHp <= 0.0f)
            return;

        m_CurHp -= a_Val;

        if (m_CurHp < 0.0f)
            m_CurHp = 0.0f;

        hpbar.fillAmount = m_CurHp / m_MaxHp;
        GameMgr.Inst.HpInfo.text = m_CurHp + " / " + m_MaxHp;

        if (m_CurHp <= 0.0f)
        {            
            PlayerDie();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "attackpos")
        {           
            TakeDamage(10);
        }
        if (other.gameObject.name.Contains("CoinPrefab") == true)
        {
            Ncnt++;            
            DiaCheck();
            Destroy(other.gameObject);
        }
    }

    void DiaCheck()
    {
        if(Ncnt < 1)
        {
            Ncnt = 1;
        }
        GameMgr.Inst.SkCntTxt.text = "x " + Ncnt;
    }


    void PlayerDie()
    {
        IsDie = true;
        AnimType("IsDie");
        colorCorrection.enabled = true;
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject monster in monsters)
        {
            monster.GetComponent<MonCtrl>().OnPlayerDie();
        }
        GameMgr.Inst.GameOver();
    }



    void AttDamage()
    {
        SwordCol.enabled = true;

    }

    void AttFinish()
    {
        SwordCol.enabled = false;
    }

    void MousePick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (GameMgr.IsPointerOverUIObject() == false)
            {
                if (yasuo == YasuoState.skill)
                    return;         

                a_MousePos = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(a_MousePos, out hitInfo, Mathf.Infinity, m_layerMask.value))
                {
                    if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("MyUnit"))
                    { //몬스터 픽킹일 때          
                        yasuo = YasuoState.attack;
                        MousePicking(hitInfo.point, hitInfo.collider.gameObject);
                        if (GameMgr.Inst.m_CursorMark != null)
                            GameMgr.Inst.m_CursorMark.SetActive(false);
                    }
                    else  //지형 바닥 픽킹일 때 
                    {
                        MousePicking(hitInfo.point);
                        GameMgr.Inst.CursorMarkOn(hitInfo.point);
                    }//else  //지형 바닥 픽킹일 때
                }
            }
        }//if (Input.GetMouseButtonDown(0))
    }


    void UseSkill()
    {
        if (GameObject.FindGameObjectWithTag("Enemy") != null)
        {

            if (Input.GetKeyDown(KeyCode.Q))
            {

                if (m_isPickMvOnOff == true)
                {
                    {
                        this.transform.position = this.transform.position +
                                                 (m_MoveDir * Time.deltaTime * m_MoveVelocity);
                        yasuo = YasuoState.idle;
                        ClearMsPickPath();
                    }

                }

                if (skill_Delay > 0.0f)
                {
                    GameMgr.Inst.GuideText.gameObject.SetActive(true);
                    GameMgr.Inst.GuideText.text = "스킬 쿨타임 입니다.";
                    GuideTimer = 1.0f;
                    return;
                }

                SwordCol.enabled = true;
                IsSkill = true;
                yasuo = YasuoState.skill;
                Aim.gameObject.SetActive(true);
                SkillEffect.SetActive(true);
                SkillEffect.GetComponent<ParticleSystem>().Play();
                StartCoroutine(Detecting());


            }

        }
    }

    void UseWSkill()
    {
        
        if(Input.GetKeyDown(KeyCode.W))
        {          
            if(Ncnt <= 1)
            {
                GameMgr.Inst.GuideText.gameObject.SetActive(true);
                GameMgr.Inst.GuideText.text = "최소 2개의 다이아가 필요합니다.";
                GuideTimer = 1.0f;
                return;
            }
            if (IsBuff)
            {
                GameMgr.Inst.GuideText.gameObject.SetActive(true);
                GameMgr.Inst.GuideText.text = "버프 지속 중 입니다.";
                GuideTimer = 1.0f;
                return;
            }            

            if (Wskill_Time > 0.0f)
                {
                  GameMgr.Inst.GuideText.gameObject.SetActive(true);
                  GameMgr.Inst.GuideText.text = "스킬 쿨타임 입니다.";
                  GuideTimer = 1.0f;
                  return;
                }            
            WDuration = 10.0f;            
            IsBuff = true;            
        }
    }

    void UseFlash()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (m_isPickMvOnOff == true)
            {
                {
                    this.transform.position = this.transform.position +
                                             (m_MoveDir * Time.deltaTime * m_MoveVelocity);
                    yasuo = YasuoState.idle;
                    ClearMsPickPath();
                }

            }
            if (Fskill_Delay > 0.0f)
            {
                GameMgr.Inst.GuideText.gameObject.SetActive(true);
                GameMgr.Inst.GuideText.text = "스킬 쿨타임 입니다.";
                GuideTimer = 1.0f;
                return;
            }

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("MyTerrain")))
            {
                {
                    Vector3 effectpos = this.transform.position;
                    effectpos.y += 1.5f;
                    FlashInst = (GameObject)Instantiate(FlashEffect, effectpos, Quaternion.identity);
                    FlashInst.GetComponent<ParticleSystem>().Play();

                    Vector3 dir = hit.point - this.transform.position;
                    dir.y = 0.0f;
                    dir.Normalize();
                    float MaxMove = 8.0f;
                    this.transform.position += dir * MaxMove;


                    Destroy(FlashInst, 2.0f);
                    Fskill_Delay = Fskill_Time;
                }
            }
        }
    }

    void UseHeal()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (m_isPickMvOnOff == true)
            {
                {
                    this.transform.position = this.transform.position +
                                             (m_MoveDir * Time.deltaTime * m_MoveVelocity);
                    yasuo = YasuoState.idle;
                    //ClearMsPickPath();
                }

            }

            //if (m_CurHp > 100.0f)
            //{
            //    GameMgr.Inst.GuideText.gameObject.SetActive(true);
            //    GameMgr.Inst.GuideText.text = "최대 체력입니다.";
            //    GuideTimer = 1.0f;
            //    return;

            //}

            if (Dskill_Delay > 0.0f)
            {
                GameMgr.Inst.GuideText.gameObject.SetActive(true);
                GameMgr.Inst.GuideText.text = "스킬 쿨타임 입니다.";
                GuideTimer = 1.0f;
                return;
            }

            Vector3 effectpos = this.transform.position;
            effectpos.y += 1.5f;
            HealInst = (GameObject)Instantiate(HealEffect, effectpos, Quaternion.identity);
            HealInst.GetComponent<ParticleSystem>().Play();
            Destroy(HealInst, 2.0f);
            m_CurHp += 50.0f;
            if (m_CurHp > 100)
            {
                m_CurHp = 100.0f;
            }
            GameMgr.Inst.HpInfo.text = m_CurHp + " / " + m_MaxHp;
            hpbar.fillAmount = m_CurHp / m_MaxHp;
            Dskill_Delay = Dskill_Time;
        }
    }

    void UiInfo()
    {
        GameMgr.Inst.SkillCoolimg.gameObject.SetActive(true);
        GameMgr.Inst.WSkillCoolimg.gameObject.SetActive(true);
        GameMgr.Inst.FSkillCoolimg.gameObject.SetActive(true);
        GameMgr.Inst.DSkillCoolimg.gameObject.SetActive(true);
        skill_Delay -= Time.deltaTime;        
        Dskill_Delay -= Time.deltaTime;
        Fskill_Delay -= Time.deltaTime;


        if (IsBuff == true)
        {
            WDuration -= Time.deltaTime;
            if (WDuration > 0.0f)
            {
                Trail.gameObject.SetActive(true);
                TrEff.GetComponent<TrailRenderer>().emitting = true;
                GameMgr.Inst.WSkillCoolimg.gameObject.SetActive(false);            
                IsBuff = true;
                m_MoveVelocity = 20.0f;
                Skcnt = Ncnt;
                DiaCheck();
                GameMgr.Inst.WSkillInfoText.text = WDuration.ToString("N1");
            }
            if (WDuration <= 0.0f)
            {              
                Trail.gameObject.SetActive(false);
                TrEff.GetComponent<TrailRenderer>().emitting = false;
                Ncnt = 1;
                Skcnt = Ncnt;
                DiaCheck();
                m_MoveVelocity = 8.0f;
                WDuration = 0.0f;
                Wskill_Time = 20.0f;
                IsBuff = false;
            }

        }
        else
        {
            Wskill_Time -= Time.deltaTime;
        }

        GameMgr.Inst.SkillCoolimg.fillAmount = skill_Delay / skill_Time;
        GameMgr.Inst.QSkillInfoText.text = skill_Delay.ToString("N1");

        if (Wskill_Time > 0.0f)
        {            
            GameMgr.Inst.WSkillCoolimg.fillAmount = Wskill_Time / 30.0f;
            GameMgr.Inst.WSkillInfoText.text = Wskill_Time.ToString("N1");
        }

        GameMgr.Inst.FSkillCoolimg.fillAmount = Fskill_Delay / Fskill_Time;
        GameMgr.Inst.FSkillInfoText.text = Fskill_Delay.ToString("N1");

        GameMgr.Inst.DSkillCoolimg.fillAmount = Dskill_Delay / Dskill_Time;
        GameMgr.Inst.DSkillInfoText.text = Dskill_Delay.ToString("N1");

        if (skill_Delay <= 0.0f)
        {
            GameMgr.Inst.SkillCoolimg.gameObject.SetActive(false);
            GameMgr.Inst.QSkillInfoText.text = "Q";
        }


        if (Wskill_Time <= 0.0f && WDuration <= 0.0f)
        {
            GameMgr.Inst.WSkillCoolimg.gameObject.SetActive(false);
            GameMgr.Inst.WSkillInfoText.text = "W";
        }

        if (Dskill_Delay <= 0.0f)
        {
            GameMgr.Inst.DSkillCoolimg.gameObject.SetActive(false);
            GameMgr.Inst.DSkillInfoText.text = "D";
        }

        if (Fskill_Delay <= 0.0f)
        {
            GameMgr.Inst.FSkillCoolimg.gameObject.SetActive(false);
            GameMgr.Inst.FSkillInfoText.text = "F";
        }

        GuideTimer -= Time.deltaTime;
        if (GuideTimer <= 0.0f)
        {
            GameMgr.Inst.GuideText.gameObject.SetActive(false);
            GuideTimer = 0.0f;
            GameMgr.Inst.GuideText.text = "";
        }

    }

    IEnumerator Detecting()
    {
        cnt = GameObject.FindGameObjectsWithTag("Enemy").Length;
        int nowCnt = 0;
        while (cnt > nowCnt)
        {
            while (AimF.fillAmount < 1)
            {
                a_MousePos = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(a_MousePos, out hitInfo, Mathf.Infinity, m_layerMask.value))
                {
                    if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("MyUnit"))
                    {
                        Taget = hitInfo.transform;
                        AimF.fillAmount += (0.001f * Time.unscaledTime);
                        if (AimF.fillAmount >= 1f)
                        {
                            nowCnt++;           
                            Taget.gameObject.layer = 0;                            
                            AimF.fillAmount = 0.0f;                            
                            yasuo = YasuoState.skilling;
                        }
                    }
                    else
                    {
                        AimF.fillAmount = 0.0f;                        
                    }
                    
 
                }
                yield return null;
               if(Skcnt == nowCnt || nowCnt == cnt ||Input.GetKeyDown(KeyCode.Q))
                {
                    if (yasuo != YasuoState.skill)
                        yield break;
                    //AnimType("SkillEnd");
                    yasuo = YasuoState.skillend;
                    yield break;                    
                }
            }
            yield return null;            
        }
        yield break;
        
    }

}

