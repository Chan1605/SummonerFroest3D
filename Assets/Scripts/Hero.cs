using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

public class Hero : MonoBehaviour
{
    public enum YasuoState { idle, trace, attack, hit, die, skill,skillend};

    public YasuoState yasuo = YasuoState.idle;
    [SerializeField] float m_CurHp = 100.0f;
    [SerializeField] float m_MaxHp = 100.0f;

    public Image hpbar;

    float m_MoveVelocity = 8.0f;     //��� �ʴ� �̵� �ӵ�...    
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
    bool IsDie = false;
    BoxCollider SwordCol;
    public GameObject Sword;
    Transform Taget;

    ColorCorrectionCurves colorCorrection;



    void Awake()
    {
        Cam a_CamCtrl = Camera.main.GetComponent<Cam>();
        if (a_CamCtrl != null)
            a_CamCtrl.InitCamera(this.gameObject);
        
    }

    void Start()
    {
        colorCorrection = FindObjectOfType<ColorCorrectionCurves>();
        

        GameMgr.Inst.m_refHero = this;

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

        //if(Input.GetKeyDown(KeyCode.Q))
        //{            
        //    if(GameObject.FindGameObjectWithTag("Enemy") !=null)
        //    Taget = GameObject.FindGameObjectWithTag("Enemy").transform;
        //    if (Taget == null)
        //    {
        //        yasuo = YasuoState.idle;
        //        return;
        //    }
        //    IsSkill = true;
        //    yasuo = YasuoState.skill;
        //}

        if(Input.GetKey(KeyCode.Q))
        {
            if(GameObject.FindGameObjectWithTag("Enemy") !=null)
            Taget = GameObject.FindGameObjectWithTag("Enemy").transform;
            if(Taget == null)
            {
                yasuo = YasuoState.idle;
                return;
            }
            IsSkill = true;
            yasuo = YasuoState.skill;

        }
        if(Input.GetKeyUp(KeyCode.Q))
        {
            if (yasuo != YasuoState.skill)
                return;
            yasuo = YasuoState.skillend;
        }

        MousePickUpdate();
        YasuoActionUpdate();
        if (m_isPickMvOnOff == false && IsSkill == false)
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
                    //Taget = GameObject.FindGameObjectWithTag("Enemy").transform;              
                    AnimType("IsSkill");
                
                    colorCorrection.enabled = true;

                }
                break;
            case YasuoState.skillend:
                {
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

            //���ݰ��ðŸ�... Ÿ���� �ְ�  +1.0f�� ������ ���͵� �ٰ��ò��� 
            //�� ���� ���ݾִϿ� ���� ��ö� move �ִϰ� ���� ���� ���Ѵ�.
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
                yasuo = YasuoState.idle;
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
                if (a_CacTgVec.magnitude <= m_AttackDist) //���ݰŸ�
                    yasuo = YasuoState.attack;

            }
            //m_isPickMvOnOff = MoveToPath(); //������ ��� false ������
        } //if (m_isPickMvOnOff == true)
    }// void MousePickUpdate() 


    void ClearMsPickPath() //���콺 ��ŷ�̵� ��� �Լ�
    {
        m_isPickMvOnOff = false;

        ////----��ŷ�� ���� ����ȭ �κ�
        //m_PathEndPos = transform.position;
        //if (0 < movePath.corners.Length)
        //{
        //    movePath.ClearCorners();  //��� ��� ���� 
        //}
        //m_CurPathIndex = 1;       //���� �ε��� �ʱ�ȭ
        //----��ŷ�� ���� ����ȭ �κ�

        if (GameMgr.Inst.m_CursorMark != null)
            GameMgr.Inst.m_CursorMark.SetActive(false);
    }



    float a_CacRotSpeed = 0.0f;
    public void AttackRotUpdate()
    { //���ݾִϸ��̼� ���� �� Ÿ���� ���� ȸ���ϰ� �ϴ� �Լ�

        if (m_TargetUnit == null)//Ÿ���� �������� ������...
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
        }//if (a_CacTgVec.magnitude <= m_AttackDist) //���ݰŸ�

    }//public void AttackRotUpdate()

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
        //���콺 ��ŷ�� �õ��߰� �̵� ���̸� Ÿ���� �ٽ� ���� �ʴ´�.
        if (m_isPickMvOnOff == true)
            return;

        //���� ������ ��Ȯ�� ������ ���� �ִϸ��̼Ǹ� �ϰ� ���� ����...
        //if (ISAttack() == false) //���� �ִϸ��̼��� �ƴϸ�...
        //    return;

        //���� �ִϸ��̼� ���̰� Ÿ���� ��ȿȭ �Ǿ��ٸ�... Ÿ���� ���� ����ش�.
        //Ÿ���� ��ü�� ���ݰŸ����ٴ� ���� �� ����(0.5f)�� �ΰ� �ٲٰ� �Ѵ�.
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

        if(m_CurHp <= 0.0f)
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

            }//for (int i = 0; i < a_iCount; ++i)

        //if (Type == AnimState.attack.ToString()) //���� �ִϸ��̼��̸�...
        //else if (Type == AnimState.skill.ToString())
        //{
        //    a_EffObj = EffectPool.Inst.GetEffectObj("FX_AttackCritical_01",
        //                                    Vector3.zero, Quaternion.identity);
        //    a_EffPos = transform.position;
        //    a_EffPos.y = a_EffPos.y + 1.0f;
        //    a_EffObj.transform.position = a_EffPos + (transform.forward * 2.3f);
        //    a_EffObj.transform.LookAt(a_EffPos + (-transform.forward * 2.0f));

        //    //---------�ֺ� ��� ���͸� ã�Ƽ� �������� �ش�.(��������)
        //    for (int i = 0; i < a_iCount; ++i)
        //    {
        //        a_CacTgVec = m_EnemyList[i].transform.position - transform.position;
        //        a_CacTgVec.y = 0.0f;

        //        ////���ݰ��� �ȿ� �ִ� ���
        //        ////-45���� �Ѵ� ������ �ִٴ� ��
        //        //if (Vector3.Dot(transform.forward, a_CacTgVec.normalized) < -0.7f) 
        //        //    continue;

        //        //���� ���� �ۿ� �ִ� ���
        //        if (m_AttackDist + 0.1f < a_CacTgVec.magnitude)
        //            continue;

        //        a_EffObj = EffectPool.Inst.GetEffectObj("FX_Hit_01",
        //                                Vector3.zero, Quaternion.identity);
        //        a_EffPos = m_EnemyList[i].transform.position;
        //        a_EffPos.y += 1.1f;
        //        a_EffObj.transform.position = a_EffPos + (-a_CacTgVec.normalized * 1.13f); //0.7f);
        //        a_EffObj.transform.LookAt(a_EffPos + (a_CacTgVec.normalized * 2.0f));
        //        m_EnemyList[i].GetComponent<MonsterCtrl>().TakeDamage(this.gameObject, 50);

        //    }//for (int i = 0; i < iCount; ++i)


        //} //else if (Type == AnimState.skill.ToString())

    }// void Event_AttDamage(string Type)

    void Event_AttFinish()
    { //���ݾִϸ��̼� ��������? �Ǵ��ϴ� �̺�Ʈ �Լ�
        SwordCol.enabled = false;
        if (m_isPickMvOnOff == true)      
        {            
            yasuo = YasuoState.idle;
        }


    }

}

