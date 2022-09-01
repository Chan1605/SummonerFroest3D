using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hero : MonoBehaviour
{
    public enum YasuoState { idle, trace, attack, hit, die, skill };

    public YasuoState yasuo = YasuoState.idle;
    [SerializeField] float m_CurHp = 100.0f;
    [SerializeField] float m_MaxHp = 100.0f;

    public Image hpbar;

    float m_MoveVelocity = 8.0f;     //평면 초당 이동 속도...    
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
    
    BoxCollider SwordCol;
    public GameObject Sword;


    void Awake()
    {
        Cam a_CamCtrl = Camera.main.GetComponent<Cam>();
        if (a_CamCtrl != null)
            a_CamCtrl.InitCamera(this.gameObject);
        
    }

    void Start()
    {
        GameMgr.Inst.m_refHero = this;

        m_layerMask = 1 << LayerMask.NameToLayer("MyTerrain");
        m_layerMask |= 1 << LayerMask.NameToLayer("MyUnit"); //Unit 도 피킹

        m_RefAnimator = this.gameObject.GetComponent<Animator>();
        SwordCol = Sword.GetComponent<BoxCollider>();
        SwordCol.enabled = false;
        yasuo = YasuoState.idle;
    }



    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //yasuo = YasuoState.trace;
            a_MousePos = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(a_MousePos, out hitInfo, Mathf.Infinity, m_layerMask.value))
            {
                if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("MyUnit"))
                { //몬스터 픽킹일 때                     
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
        }//if (Input.GetMouseButtonDown(0))




        MousePickUpdate();
        YasuoActionUpdate();
        if (m_isPickMvOnOff == false)
            yasuo = YasuoState.idle;


    }

    private void AnimType(string anim)
    {
        m_RefAnimator.SetBool("Idle", false);
        m_RefAnimator.SetBool("IsTrace", false);
        m_RefAnimator.SetBool("IsAttack", false);
        m_RefAnimator.SetBool("IsDie", false);

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
            //추적 상태
            case YasuoState.trace:
                {
                    AnimType("IsTrace");
                }
                break;

            //공격 상태
            case YasuoState.attack:
                {
                    if (m_TargetUnit == null)
                        return;
                    AttackRotUpdate();
                    AnimType("IsAttack");
                       

                }
                break;

        }
    }



    public void MousePicking(Vector3 a_SetPickVec, GameObject a_PickMon = null)
    {
        a_StartPos = this.transform.position; //출발 위치    
        a_SetPickVec.y = this.transform.position.y; // 최종 목표 위치

        a_CacLenVec = a_SetPickVec - a_StartPos;
        a_CacLenVec.y = 0.0f;

        //-------Picking Enemy 공격 처리 부분
        if (a_PickMon != null)
        {
            a_CacTgVec = a_PickMon.transform.position - transform.position;

            //공격가시거리... 타겟이 있고  +1.0f면 어차피 몬스터도 다가올꺼고 
            //좀 일찍 공격애니에 들어가야 잠시라도 move 애니가 끼어 들지 못한다.
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
                yasuo = YasuoState.idle;
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
                if (a_CacTgVec.magnitude <= m_AttackDist) //공격거리
                    yasuo = YasuoState.attack;

            }
            //m_isPickMvOnOff = MoveToPath(); //도착한 경우 false 리턴함
        } //if (m_isPickMvOnOff == true)
    }// void MousePickUpdate() 


    void ClearMsPickPath() //마우스 픽킹이동 취소 함수
    {
        m_isPickMvOnOff = false;

        ////----피킹을 위한 동기화 부분
        //m_PathEndPos = transform.position;
        //if (0 < movePath.corners.Length)
        //{
        //    movePath.ClearCorners();  //경로 모두 제거 
        //}
        //m_CurPathIndex = 1;       //진행 인덱스 초기화
        //----피킹을 위한 동기화 부분

        if (GameMgr.Inst.m_CursorMark != null)
            GameMgr.Inst.m_CursorMark.SetActive(false);
    }



    float a_CacRotSpeed = 0.0f;
    public void AttackRotUpdate()
    { //공격애니메이션 중일 때 타겟을 향해 회전하게 하는 함수

        if (m_TargetUnit == null)//타겟이 존재하지 않으면...
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
        }//if (a_CacTgVec.magnitude <= m_AttackDist) //공격거리

    }//public void AttackRotUpdate()

    bool IsTargetEnemyActive(float a_ExtLen = 0.0f)
    {
        if (m_TargetUnit == null)//타겟이 존재하지 않으면...
            return false;

        if (m_TargetUnit.activeSelf == false)
        {
            m_TargetUnit = null;
            return false;
        }

        //isDie 죽어 있어도
        MonCtrl a_Unit = m_TargetUnit.GetComponent<MonCtrl>();
        if (a_Unit.monsterState == MonCtrl.MonsterState.die)
        {
            m_TargetUnit = null;
            return false;
        }

        a_CacTgVec = m_TargetUnit.transform.position - transform.position;
        a_CacTgVec.y = 0.0f;
        if (m_AttackDist + a_ExtLen < a_CacTgVec.magnitude)
        {   //공격거리 바깥쪽에 있을 경우도 타겟을 무효화 해 버린다

            m_TargetUnit = null; //원거리라도 타겟을 공격할 수 있으니까...
            return false;
        }

        return true; //타겟이 아직 유효 하다는 의미
    }// bool IsTargetEnemyActive(float a_ExtLen = 0.0f)

    void EnemyMonitor()
    {
        //마우스 피킹을 시도했고 이동 중이면 타겟을 다시 잡지 않는다.
        if (m_isPickMvOnOff == true)
            return;

        //보간 때문에 정확히 정밀한 공격 애니메이션만 하고 있을 때만...
        //if (ISAttack() == false) //공격 애니메이션이 아니면...
        //    return;

        //공격 애니메이션 중이고 타겟이 무효화 되었다면... 타겟을 새로 잡아준다.
        //타겟의 교체는 공격거리보다는 조금 더 여유(0.5f)를 두고 바꾸게 한다.
        if (IsTargetEnemyActive(0.5f) == false)
            FindEnemyTarget();
    }
    int a_iCount = 0;
    void FindEnemyTarget()
    {
        m_EnemyList = GameObject.FindGameObjectsWithTag("Enemy");

        float a_MinLen = float.MaxValue;
        a_iCount = m_EnemyList.Length;
        m_TargetUnit = null;  //우선 타겟 무효화
        for (int i = 0; i < a_iCount; ++i)
        {
            a_CacTgVec = m_EnemyList[i].transform.position - transform.position;
            a_CacTgVec.y = 0.0f;
            if (a_CacTgVec.magnitude <= m_AttackDist)
            {   //공격거리 안쪽에 있을 경우만 타겟으로 잡는다.
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
    }

    void PlayerDie()
    {
        Debug.Log("Player Die !!");

    }

    void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.layer == LayerMask.NameToLayer("MyUnit"))
        {
            if (m_CurHp <= 0.0f)
                return;
            m_CurHp -= 10;

            if (hpbar != null)
                hpbar.fillAmount = m_CurHp / m_MaxHp;
            //Player의 생명이 0이하이면 사망 처리
            if (m_CurHp <= 0)
            {
                PlayerDie();
            }
        }//if(coll.gameObject.tag == "PUNCH")
        //else if (coll.gameObject.name.Contains("CoinPrefab") == true)
        //{
        //    GameMgr.Inst.AddGold();

        //    if (Ad_Source != null && CoinSfx != null)
        //        Ad_Source.PlayOneShot(CoinSfx, 0.3f);

        //    Destroy(coll.gameObject);
        //}
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
        

        m_EnemyList = GameObject.FindGameObjectsWithTag("Enemy");
        a_iCount = m_EnemyList.Length;
        float a_fCacLen = 0.0f;

            for (int i = 0; i < a_iCount; ++i)
            {
                a_CacTgVec = m_EnemyList[i].transform.position - transform.position;
                a_fCacLen = a_CacTgVec.magnitude;
                a_CacTgVec.y = 0.0f;

                //공격각도 안에 있는 경우
                //70도 정도 //-0.7f) //-45도를 넘는 범위에 있다는 뜻
                if (Vector3.Dot(transform.forward, a_CacTgVec.normalized) < 0.45f)
                    continue;

                //공격 범위 밖에 있는 경우
                if (m_AttackDist + 0.1f < a_fCacLen)
                    continue;


                m_EnemyList[i].GetComponent<MonCtrl>().TakeDamage(10);

            }//for (int i = 0; i < a_iCount; ++i)

        //if (Type == AnimState.attack.ToString()) //공격 애니메이션이면...
        //else if (Type == AnimState.skill.ToString())
        //{
        //    a_EffObj = EffectPool.Inst.GetEffectObj("FX_AttackCritical_01",
        //                                    Vector3.zero, Quaternion.identity);
        //    a_EffPos = transform.position;
        //    a_EffPos.y = a_EffPos.y + 1.0f;
        //    a_EffObj.transform.position = a_EffPos + (transform.forward * 2.3f);
        //    a_EffObj.transform.LookAt(a_EffPos + (-transform.forward * 2.0f));

        //    //---------주변 모든 몬스터를 찾아서 데이지를 준다.(범위공격)
        //    for (int i = 0; i < a_iCount; ++i)
        //    {
        //        a_CacTgVec = m_EnemyList[i].transform.position - transform.position;
        //        a_CacTgVec.y = 0.0f;

        //        ////공격각도 안에 있는 경우
        //        ////-45도를 넘는 범위에 있다는 뜻
        //        //if (Vector3.Dot(transform.forward, a_CacTgVec.normalized) < -0.7f) 
        //        //    continue;

        //        //공격 범위 밖에 있는 경우
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

    //void Event_AttFinish(string Type)
    //{ //공격애니메이션 끝났는지? 판단하는 이벤트 함수

    //    if ((0.0f != h || 0.0f != v) || 0.0f < m_JoyMvLen
    //             || m_isPickMvOnOff == true)
    //    //키보드 이동조작이 있거나, 조이스틱 조작이 있는 경우
    //    {   //스킬 사용 중에 이동을 누르고 있다가 이쪽으로 들어오면 
    //        //예약 즉시 취소를 위한 코드
    //        MySetAnim(AnimState.move);
    //        m_RsvPicking = 0.0f;
    //        return;
    //    }

    //    if (0.0f < m_RsvPicking) //마우스 피킹 예약이 있었다면...
    //    {
    //        ////---- 길찾기를 이용하지 않고 예약 이동 처리를 할 때 코드
    //        //m_TargetUnit = m_RsvTgUnit;  //타겟도 바꾸거나 무효화 시켜 버린다.
    //        //m_RsvPicking = 0.0f;
    //        ////스킬 끝나는 시점에 한번만 적용되니까 
    //        ////스킬 끝나는 시점에 키보드 마우스 이동이 있으면 다음 Update에서 취소될 것이다.
    //        //m_TargetPos = m_RsvTargetPos;
    //        //m_MoveDurTime = m_RsvMvDurTime;
    //        //m_isPickMvOnOff = true;
    //        //m_AddTimeCount = 0.0;
    //        ////---- 길찾기를 이용하지 않고 예약 이동 처리를 할 때 코드

    //        //---- 네비게이션 메시를 이용한 이동 방식
    //        m_TargetUnit = m_RsvTgUnit;  //타겟도 바꾸거나 무효화 시켜 버린다.
    //        a_StartPos = this.transform.position; //출발 위치     
    //        m_TargetPos = m_RsvTargetPos;
    //        m_TargetPos.y = this.transform.position.y; // 최종 목표 위치

    //        float a_PathLen = 0.0f;
    //        if (MyNavCalcPath(a_StartPos, m_TargetPos, ref a_PathLen) == true)
    //        {
    //            m_isPickMvOnOff = true;                 //피킹 이동 OnOff
    //            a_CacLenVec = m_TargetPos - a_StartPos;
    //            a_CacLenVec.y = 0.0f;
    //            m_MoveDir = a_CacLenVec.normalized;
    //            m_MoveDurTime = a_PathLen / m_MoveVelocity; //도착하는데 걸리는 시간
    //            m_AddTimeCount = 0.0;
    //        }
    //        //---- 네비게이션 메시를 이용한 이동 방식

    //        MySetAnim(AnimState.move);

    //        return;
    //    } //if (0.0f < m_RsvPicking) //마우스 피킹 예약이 있었다면...

    //    //Skill상태일때는 Skill상태로 끝나야 하고 
    //    //Attack상태일대는 Attack상태로 끝나야 하고 
    //    //상태는 Skill인데 Attack애니메이션 끝이 들어온 경우라면 제외시켜버린다.
    //    //공격 애니 중에 스킬 발동시 공격 끝나는 이벤트 함수가 들어와서 스킬이
    //    //취소되는 현상이 있을 수 있어서 예외 처리함
    //    //또 하나의 해결책은 Event_SkillFinish()함수를 따로 만들어 주면 된다.
    //    if (m_prevState.ToString() == AnimState.skill.ToString() &&
    //        Type == AnimState.attack.ToString())
    //        return;

    //    if (m_prevState.ToString() == AnimState.attack.ToString() &&
    //        Type == AnimState.skill.ToString())
    //        return;

    //    if (IsTargetEnemyActive(0.2f) == true) //공격 거리 안에 타겟이 있느냐?
    //    {
    //        //스킬 애니메이션으로 끝난 경우 자동 공격 애니메이션을 하게 하고 싶은 경우
    //        MySetAnim(AnimState.attack);  //다시 공격 애니
    //        ClearMsPickPath();
    //    }
    //    else
    //    {
    //        MySetAnim(AnimState.idle);
    //    }
    //}

}

