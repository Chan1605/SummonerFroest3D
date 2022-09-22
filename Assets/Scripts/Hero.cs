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
    //------ Picking ���� ���� 
    Ray a_MousePos;
    RaycastHit hitInfo;
    private LayerMask m_layerMask = -1;
    //------ Picking ���� ���� 

    //------ Picking ���� ���� 
    bool m_isPickMvOnOff = false;       //��ŷ �̵� OnOff
    Vector3 m_TargetPos = Vector3.zero; //���� ��ǥ ��ġ
    Vector3 m_MoveDir = Vector3.zero;   //��� ���� ����
    double m_MoveDurTime = 0.0;         //��ǥ������ �����ϴµ� �ɸ��� �ð�
    double m_AddTimeCount = 0.0;        //�����ð� ī��Ʈ
    float m_RotSpeed = 7.0f;            //�ʴ� ȸ�� �ӵ�
    Vector3 a_StartPos = Vector3.zero;  //���� ����
    Vector3 a_CacLenVec = Vector3.zero; //���� ����
    Quaternion a_TargetRot;             //���� ����

    [HideInInspector] public int m_CurPathIndex = 1;

    GameObject m_TargetUnit = null;
    Animator m_RefAnimator = null;

    GameObject[] m_EnemyList = null;    //�ʵ���� ���͵��� �������� ���� ����

    float m_AttackDist = 1.9f;          //���ΰ��� ���ݰŸ�    

    Vector3 a_CacTgVec = Vector3.zero;  //Ÿ�ٱ����� �Ÿ� ���� ����
    Vector3 a_CacAtDir = Vector3.zero;  //���ݽ� ������ȯ�� ����

    bool IsSkill = false;
    [HideInInspector]public bool IsBuff = false;    

    [HideInInspector] public bool IsDie = false;
    BoxCollider SwordCol;
    public GameObject Sword;
    private Transform Taget;

    ColorCorrectionCurves colorCorrection;

    float skill_Time = 3.0f;
    float Wskill_Time = 0.0f; //��Ÿ��
    float WDuration = 0.0f;   //���� ���ӽð�
    float Dskill_Time = 15.0f;
    float Fskill_Time = 20.0f;
    float skill_Delay = 0.0f;
    float Dskill_Delay = 0.0f;
    float Fskill_Delay = 0.0f;

    float GuideTimer = 0.0f;
    
    public GameObject SkillEffect; //Q Ȧ�� ��ų����Ʈ
    public GameObject SkillEnd;    //Q ���� ����Ʈ
    public GameObject HealEffect;  //D ��ų ����Ʈ
    public GameObject FlashEffect; //F ��ų����Ʈ
    public GameObject Trail;
    GameObject Skill1;             //W Instantiate��
    GameObject HealInst;           //F Instantiate��
    GameObject FlashInst;          //F Instantiate��
    
    
    int Skcnt = 1; //���̾Ƹ� ������ ����
    int Ncnt = 1;         //�Ϲ� q ��ų
    int cnt;              //���� ���� �� ī��Ʈ
    public int Killcount = 0; //ų ī��Ʈ
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
        m_layerMask |= 1 << LayerMask.NameToLayer("MyUnit"); //Unit �� ��ŷ

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
        a_StartPos = this.transform.position; //��� ��ġ    
        a_SetPickVec.y = this.transform.position.y; // ���� ��ǥ ��ġ

        a_CacLenVec = a_SetPickVec - a_StartPos;
        a_CacLenVec.y = 0.0f;

        //-------Picking Enemy ���� ó�� �κ�
        if (a_PickMon != null)
        {
            a_CacTgVec = a_PickMon.transform.position - transform.position;


            float a_AttDist = m_AttackDist;
            //if (a_PickMon.GetComponent<MonsterCtrl>().m_AggroTarget
            //                                         == this.gameObject)
            //{
            //    a_AttDist = m_AttackDist + 1.0f;
            //    //���� �����Ϸ��� �ϴ� ������ ��׷� Ÿ���� ����....
            //}

            if (a_CacTgVec.magnitude <= a_AttDist)
            {
                m_TargetUnit = a_PickMon;

                return;

            }//��� ���� �϶�
        } //if (a_PickMon != null)
        if (a_CacLenVec.magnitude < 0.5f)  //�ʹ� �ٰŸ� ��ŷ�� ��ŵ�� �ش�.
            return;

        float a_PathLen = a_CacLenVec.magnitude;

        m_TargetPos = a_SetPickVec;   //���� ��ǥ ��ġ
        m_isPickMvOnOff = true;       //��ŷ �̵� OnOff

        m_MoveDir = a_CacLenVec.normalized;
        m_MoveDurTime = a_PathLen / m_MoveVelocity; //�����ϴµ� �ɸ��� �ð�
        m_AddTimeCount = 0.0;

        a_StartPos = this.transform.position; //��� ��ġ    
        a_SetPickVec.y = this.transform.position.y; // ���� ��ǥ ��ġ

        a_CacLenVec = a_SetPickVec - a_StartPos;
        a_CacLenVec.y = 0.0f;

    }


    void MousePickUpdate()  //<--- MousePickMove()
    {
        ////-------------- ���콺 ��ŷ �̵�
        if (m_isPickMvOnOff == true)
        {
            a_CacLenVec = m_TargetPos - this.transform.position;
            a_CacLenVec.y = 0.0f;

            //ĳ���͸� �̵��������� ȸ����Ű�� �ڵ� 
            if (0.1f < a_CacLenVec.magnitude)
            {
                m_MoveDir = a_CacLenVec.normalized;
                a_TargetRot = Quaternion.LookRotation(m_MoveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                     a_TargetRot,
                                     Time.deltaTime * m_RotSpeed);
            }
            //ĳ���� ȸ��   

            m_MoveDir = a_CacLenVec.normalized;

            m_AddTimeCount = m_AddTimeCount + Time.deltaTime;
            if (m_MoveDurTime <= m_AddTimeCount) //��ǥ���� ������ ������ �����Ѵ�.
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
            { //<-- ���� �ִϸ��̼� ���̸� ���� ����� Ÿ���� �ڵ����� ��Եȴ�.
                m_isPickMvOnOff = true;
                a_CacTgVec = m_TargetUnit.transform.position -
                                                this.transform.position;
                if (a_CacTgVec.magnitude <= m_AttackDist
                && IsSkill == false) //���ݰŸ�
                {

                    yasuo = YasuoState.attack;
                }

            }

        }
    }




    void ClearMsPickPath() //���콺 ��ŷ�̵� ��� �Լ�
    {
        m_isPickMvOnOff = false;


        if (GameMgr.Inst.m_CursorMark != null)
            GameMgr.Inst.m_CursorMark.SetActive(false);
    }



    float a_CacRotSpeed = 0.0f;
    public void AttackRotUpdate()
    { //���ݾִϸ��̼� ���� �� Ÿ���� ���� ȸ���ϰ� �ϴ� �Լ�

        if (m_TargetUnit == null)
            return;

        a_CacTgVec = m_TargetUnit.transform.position - transform.position;
        a_CacTgVec.y = 0.0f;

        if (a_CacTgVec.magnitude <= (m_AttackDist + 0.3f)) //���ݰŸ�
        {
            //ĳ���� ������ ȸ��   
            a_CacAtDir = a_CacTgVec.normalized;
            if (0.0001f < a_CacAtDir.magnitude)
            {
                a_CacRotSpeed = m_RotSpeed * 3.0f;           //�ʴ� ȸ�� �ӵ�
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
                    { //���� ��ŷ�� ��          
                        yasuo = YasuoState.attack;
                        MousePicking(hitInfo.point, hitInfo.collider.gameObject);
                        if (GameMgr.Inst.m_CursorMark != null)
                            GameMgr.Inst.m_CursorMark.SetActive(false);
                    }
                    else  //���� �ٴ� ��ŷ�� �� 
                    {
                        MousePicking(hitInfo.point);
                        GameMgr.Inst.CursorMarkOn(hitInfo.point);
                    }//else  //���� �ٴ� ��ŷ�� ��
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
                    GameMgr.Inst.GuideText.text = "��ų ��Ÿ�� �Դϴ�.";
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
                GameMgr.Inst.GuideText.text = "�ּ� 2���� ���̾ư� �ʿ��մϴ�.";
                GuideTimer = 1.0f;
                return;
            }
            if (IsBuff)
            {
                GameMgr.Inst.GuideText.gameObject.SetActive(true);
                GameMgr.Inst.GuideText.text = "���� ���� �� �Դϴ�.";
                GuideTimer = 1.0f;
                return;
            }            

            if (Wskill_Time > 0.0f)
                {
                  GameMgr.Inst.GuideText.gameObject.SetActive(true);
                  GameMgr.Inst.GuideText.text = "��ų ��Ÿ�� �Դϴ�.";
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
                GameMgr.Inst.GuideText.text = "��ų ��Ÿ�� �Դϴ�.";
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
            //    GameMgr.Inst.GuideText.text = "�ִ� ü���Դϴ�.";
            //    GuideTimer = 1.0f;
            //    return;

            //}

            if (Dskill_Delay > 0.0f)
            {
                GameMgr.Inst.GuideText.gameObject.SetActive(true);
                GameMgr.Inst.GuideText.text = "��ų ��Ÿ�� �Դϴ�.";
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

