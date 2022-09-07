using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

public class Hero : MonoBehaviour
{
    public enum YasuoState { idle, trace, attack, hit, die, skill, skillend };

    public YasuoState yasuo = YasuoState.idle;
    [SerializeField] float m_CurHp = 100.0f;
    [SerializeField] float m_MaxHp = 100.0f;

    public Image hpbar;

    float m_MoveVelocity = 8.0f;   
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
    [HideInInspector] public bool IsDie = false;
    BoxCollider SwordCol;
    public GameObject Sword;
    Transform Taget;

    ColorCorrectionCurves colorCorrection;

    float skill_Time = 3.0f;
    float Dskill_Time = 5.0f;
    float Fskill_Time = 10.0f;

    float skill_Delay = 0.0f;
    float Dskill_Delay = 0.0f;
    float Fskill_Delay = 0.0f;

    float GuideTimer = 0.0f;

    public GameObject SkillEffect;
  


    void Awake()
    {
        Cam a_CamCtrl = Camera.main.GetComponent<Cam>();
        if (a_CamCtrl != null)
            a_CamCtrl.InitCamera(this.gameObject);

    }

    void Start()
    {
        colorCorrection = FindObjectOfType<ColorCorrectionCurves>();


        GameMgr.Inst.Yasuo = this;

        m_layerMask = 1 << LayerMask.NameToLayer("MyTerrain");
        m_layerMask |= 1 << LayerMask.NameToLayer("MyUnit"); //Unit �� ��ŷ

        m_RefAnimator = this.gameObject.GetComponent<Animator>();
        SwordCol = Sword.GetComponent<BoxCollider>();
        SwordCol.enabled = false;
        yasuo = YasuoState.idle;

        

    }



    // Update is called once per frame
    void Update()
    {
        if (IsDie == true)
            return;
        if (Input.GetMouseButtonDown(0))
        {
            if (yasuo == YasuoState.skill)
                return;

            a_MousePos = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(a_MousePos, out hitInfo, Mathf.Infinity, m_layerMask.value))
            {
                if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("MyUnit"))
                { //���� ��ŷ�� ��                     
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
        }//if (Input.GetMouseButtonDown(0))

        GameMgr.Inst.SkillCoolimg.gameObject.SetActive(true);
        GameMgr.Inst.FSkillCoolimg.gameObject.SetActive(true);
        GameMgr.Inst.DSkillCoolimg.gameObject.SetActive(true);
        skill_Delay -= Time.deltaTime;
        Dskill_Delay -= Time.deltaTime;
        Fskill_Delay -= Time.deltaTime;

        GameMgr.Inst.SkillCoolimg.fillAmount = skill_Delay / skill_Time;
        GameMgr.Inst.QSkillInfoText.text = skill_Delay.ToString("N1");
      
        GameMgr.Inst.FSkillCoolimg.fillAmount = Fskill_Delay / Fskill_Time;
        GameMgr.Inst.FSkillInfoText.text = Fskill_Delay.ToString("N1");

        GameMgr.Inst.DSkillCoolimg.fillAmount = Dskill_Delay / Dskill_Time;
        GameMgr.Inst.DSkillInfoText.text = Dskill_Delay.ToString("N1");

        if (skill_Delay <= 0.0f)
        {
            GameMgr.Inst.SkillCoolimg.gameObject.SetActive(false);
            GameMgr.Inst.QSkillInfoText.text = "Q";
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



        if (GameObject.FindGameObjectWithTag("Enemy") != null)
        {
            Taget = GameObject.FindGameObjectWithTag("Enemy").transform;
            
            if (Taget == null)
            {
                yasuo = YasuoState.idle;

                return;
            }

            if (Input.GetKey(KeyCode.Q))
            {
                //if (yasuo == YasuoState.trace)
                //    return;
                //if (m_isPickMvOnOff == true)
                //{
                //    {
                //        this.transform.position = this.transform.position +
                //                                 (m_MoveDir * Time.deltaTime * m_MoveVelocity);
                //        yasuo = YasuoState.trace;
                //        m_isPickMvOnOff = false;
                //    }
 
                //}
                if (skill_Delay > 0.0f)
                {
                    GameMgr.Inst.GuideText.gameObject.SetActive(true);
                    GuideTimer = 1.0f;
                    return;
                }

                if (skill_Delay <= 0.0f)
                {
                    if (m_isPickMvOnOff == true)
                    {
                        {
                            this.transform.position = this.transform.position +
                                                     (m_MoveDir * Time.deltaTime * m_MoveVelocity);
                            yasuo = YasuoState.trace;
                            ClearMsPickPath();
                        }

                    }
                    Time.timeScale = 0.3f;
                    IsSkill = true;
                    yasuo = YasuoState.skill;
                    

                }
            }
            if(Input.GetKeyDown(KeyCode.Q))
            {
                //Skill1 = (GameObject)Instantiate(SkillEffect, this.transform.position, Quaternion.identity);

                //Skill1.GetComponent<ParticleSystem>().Play();
                SwordCol.enabled = true;
                SkillEffect.SetActive(true);
                SkillEffect.GetComponent<ParticleSystem>().Play();
                

            }

            if (Input.GetKeyUp(KeyCode.Q))
            {
                if (yasuo != YasuoState.skill)
                    return;                
                skill_Delay = skill_Time;
                yasuo = YasuoState.skillend;
                SkillEffect.SetActive(false);
                
                //Destroy(Skill1);

            }
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            ClearMsPickPath();
            if (Dskill_Delay > 0.0f)
            {
                GameMgr.Inst.GuideText.gameObject.SetActive(true);
                GuideTimer = 1.0f;
                return;
            }

            m_CurHp += 30.0f;
            Dskill_Delay = Dskill_Time;
        }


        if (Input.GetKeyDown(KeyCode.F))
        {
            ClearMsPickPath();
            if (Fskill_Delay > 0.0f)
            {
                GameMgr.Inst.GuideText.gameObject.SetActive(true);
                GuideTimer = 1.0f;
                return;
            }            

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("MyTerrain")))
            {
                {
                    Vector3 dir = hit.point - this.transform.position;
                    dir.y = 0.0f;
                    dir.Normalize();

                    float MaxMove = 10.0f;
                    this.transform.position += dir * MaxMove;
                    Fskill_Delay = Fskill_Time;
                }
            }
        }


        GuideTimer -= Time.deltaTime;
        if (GuideTimer <= 0.0f)
        {
            GameMgr.Inst.GuideText.gameObject.SetActive(false);
            GuideTimer = 0.0f;
        }

        MousePickUpdate();
        YasuoActionUpdate();
        if (m_isPickMvOnOff == false && IsSkill == false && yasuo != YasuoState.skillend && yasuo != YasuoState.skill)
            yasuo = YasuoState.idle;


    }

    private void AnimType(string anim)
    {
        m_RefAnimator.SetBool("Idle", false);
        m_RefAnimator.SetBool("IsTrace", false);
        m_RefAnimator.SetBool("IsAttack", false);
        m_RefAnimator.SetBool("IsDie", false);
        m_RefAnimator.SetBool("IsSkill", false);
        m_RefAnimator.SetBool("SkillEnd", false);

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


                }
                break;

            case YasuoState.skill:
                {
                    AnimType("IsSkill");
                    
                    colorCorrection.enabled = true;
                    
                }
                break;
            case YasuoState.skillend:
                {
                    Time.timeScale = 0.2f;
                    Vector3 dir = Taget.position - this.gameObject.transform.position;
                    dir.y = 0;
                    dir.Normalize();

                    Vector3 tagetpos = Taget.position + (dir * 2.0f);
                    this.gameObject.transform.forward = dir;
                    AnimType("SkillEnd");
                    Taget.GetComponent<MonCtrl>().TakeDamage(100);
                    //Taget.GetComponent<Animator>().SetTrigger("IsDie");
                    this.gameObject.transform.position = tagetpos;
                    IsSkill = false;
                    colorCorrection.enabled = false;
                    SwordCol.enabled = false;
                    yasuo = YasuoState.idle;
                    Time.timeScale = 1.0f;

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
                if (a_CacTgVec.magnitude <= m_AttackDist &&
                IsSkill == false) //���ݰŸ�
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

    bool IsTargetEnemyActive(float a_ExtLen = 0.0f)
    {
        if (m_TargetUnit == null)//Ÿ���� �������� ������...
            return false;

        if (m_TargetUnit.activeSelf == false)
        {
            m_TargetUnit = null;
            return false;
        }

        //isDie �׾� �־
        MonCtrl a_Unit = m_TargetUnit.GetComponent<MonCtrl>();
        if (a_Unit.monsterState == MonCtrl.MonsterState.die)
        {
            m_TargetUnit = null;
            return false;
        }

        a_CacTgVec = m_TargetUnit.transform.position - transform.position;
        a_CacTgVec.y = 0.0f;
        if (m_AttackDist + a_ExtLen < a_CacTgVec.magnitude)
        {   //���ݰŸ� �ٱ��ʿ� ���� ��쵵 Ÿ���� ��ȿȭ �� ������

            m_TargetUnit = null; //���Ÿ��� Ÿ���� ������ �� �����ϱ�...
            return false;
        }

        return true; //Ÿ���� ���� ��ȿ �ϴٴ� �ǹ�
    }// bool IsTargetEnemyActive(float a_ExtLen = 0.0f)

    void EnemyMonitor()
    {
        
        if (m_isPickMvOnOff == true)
            return;

    
        if (IsTargetEnemyActive(0.5f) == false)
            FindEnemyTarget();
    }
    int a_iCount = 0;
    void FindEnemyTarget()
    {
        m_EnemyList = GameObject.FindGameObjectsWithTag("Enemy");

        float a_MinLen = float.MaxValue;
        a_iCount = m_EnemyList.Length;
        m_TargetUnit = null;  //�켱 Ÿ�� ��ȿȭ
        for (int i = 0; i < a_iCount; ++i)
        {
            a_CacTgVec = m_EnemyList[i].transform.position - transform.position;
            a_CacTgVec.y = 0.0f;
            if (a_CacTgVec.magnitude <= m_AttackDist)
            {   //���ݰŸ� ���ʿ� ���� ��츸 Ÿ������ ��´�.
                if (a_CacTgVec.magnitude < a_MinLen)
                {
                    a_MinLen = a_CacTgVec.magnitude;
                    m_TargetUnit = m_EnemyList[i];
                }
            }//if (a_CacTgVec.magnitude < a_MinLen)
        }//for (int i = 0; i < iCount; ++i)
    }


    public void TakeDamage(float a_Val)
    {
        if (m_CurHp == 0.0f)
            return;

        m_CurHp -= a_Val;

        hpbar.fillAmount = m_CurHp / m_MaxHp;
        GameMgr.Inst.HpInfo.text = m_CurHp + " / " + m_MaxHp;

        if (m_CurHp <= 0.0f)
        {
            m_CurHp = 0.0f;
            PlayerDie();
        }
    }

    void PlayerDie()
    {
        IsDie = true;
        Debug.Log("Player Die !!");
        AnimType("IsDie");
        colorCorrection.enabled = true;
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Enemy");
        
      
        foreach (GameObject monster in monsters)
        {

            monster.GetComponent<MonCtrl>().OnPlayerDie();            
            
        }
        

    }


    public void AttackStart()
    {
        SwordCol.enabled = true;
        MonCtrl mon = GetComponent<MonCtrl>();
        mon.TakeDamage(10);

    }


    public void AttackEnd()
    {
        SwordCol.enabled = false;
    }


    void Event_AttDamage()
    {
        SwordCol.enabled = true;
        m_EnemyList = GameObject.FindGameObjectsWithTag("Enemy");
        a_iCount = m_EnemyList.Length;
        float a_fCacLen = 0.0f;

        for (int i = 0; i < a_iCount; ++i)
        {
            a_CacTgVec = m_EnemyList[i].transform.position - transform.position;
            a_fCacLen = a_CacTgVec.magnitude;
            a_CacTgVec.y = 0.0f;

            //���ݰ��� �ȿ� �ִ� ���
            //70�� ���� //-0.7f) //-45���� �Ѵ� ������ �ִٴ� ��
            if (Vector3.Dot(transform.forward, a_CacTgVec.normalized) < 0.45f)
                continue;

            //���� ���� �ۿ� �ִ� ���
            if (m_AttackDist + 0.1f < a_fCacLen)
                continue;


            m_EnemyList[i].GetComponent<MonCtrl>().TakeDamage(10);

        }


    }

    void Event_AttFinish()
    { 
        SwordCol.enabled = false;

    }

}

