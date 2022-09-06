using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonCtrl : MonoBehaviour
{
    public enum MonsterState { idle, trace, attack, hit, die };

    public enum MonType { Zombie, Wolf };

    //몬스터의 현재 상태 정보를 저장할 Enum 변수
    public MonsterState monsterState = MonsterState.idle;

    public MonType monType = MonType.Zombie;

    private Transform monsterTr;
    private Transform playerTr;
    //private NavMeshAgent nvAgent;
    public Animator animator;

    public float traceDist = 10.0f;
    //공격 사정거리
    public float attackDist = 2.0f;

    public bool isDie = false;

    public int hp = 100;
    [SerializeField]float m_MoveVelocity = 5.0f;
    public GameObject Attackpos;
    BoxCollider MonCol;
    public GameObject bloodEffect;
    public GameObject bloodDecal;
    
    void Awake()
    {
        //traceDist = 18.0f; 
        //attackDist = 2.5f;         

        //몬스터의 Transform 할당
        monsterTr = this.gameObject.GetComponent<Transform>();
        //추적 대상인 Player의 Transform 할당
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
        //NavMeshAgent 컴포넌트 할당
        //nvAgent = this.gameObject.GetComponent<NavMeshAgent>();
        //Animator 컴포넌트 할당
        animator = this.gameObject.GetComponent<Animator>();
    }
    // Start is called before the first frame update
    void Start()
    {
        MonCol = Attackpos.GetComponent<BoxCollider>();
        MonCol.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        CheckMonStateUpdate();
        MonActionUpdate();
    }

    //일정한 간격으로 몬스터의 행동 상태를 체크하고 monsterState 값 변경
    float m_AI_Delay = 0.0f;
    void CheckMonStateUpdate()
    {
        if (isDie == true)
            return;

        //0.1초 동안 기다렸다가 다음으로 넘어감
        m_AI_Delay -= Time.deltaTime;
        if (0.0f < m_AI_Delay)
            return;

        m_AI_Delay = 0.1f;

        //몬스터와 플레이어 사이의 거리 측정
        float dist = Vector3.Distance(playerTr.position, monsterTr.position);

        if (dist <= attackDist) //공격거리 범위 이내로 들어왔는지 확인
        {
            monsterState = MonsterState.attack;
        }
        else if (dist <= traceDist) //추적거리 범위 이내로 들어왔는지 확인
        {
            monsterState = MonsterState.trace; //몬스터의 상태를 추적으로 설정
        }        
        else
        {
            monsterState = MonsterState.idle;   //몬스터의 상태를 idle 모드로 설정
        }

    }//void CheckMonStateUpdate()

    //몬스터의 상태값에 따라 적절한 동작을 수행하는 함수
    void MonActionUpdate()
    {
        if (isDie == true)
            return;

        switch (monsterState)
        {
            //idle 상태
            case MonsterState.idle:
                //추적 중지
                //nvAgent.isStopped = true; //nvAgent.Stop();
                //Animator의 IsTrace 변수를 false 로 설정
                animator.SetBool("IsTrace", false);
                break;

            //추적 상태
            case MonsterState.trace:
                {
  
                    ////추적 대상의 위치를 넘겨줌
                    //nvAgent.destination = playerTr.position;
                    ////추적을 재시작
                    //nvAgent.isStopped = false;  //nvAgent.Resume();

                    //-------- 이동 구현
                    //float m_MoveVelocity = 5.0f;      //평면 초당 이동 속도...
                    Vector3 m_MoveDir = Vector3.zero;
                    m_MoveDir = playerTr.position - this.transform.position;
                    m_MoveDir.y = 0.0f;

                    Vector3 a_StepVec = (m_MoveDir.normalized *
                                            Time.deltaTime * m_MoveVelocity);
                    transform.Translate(a_StepVec, Space.World);

                    //캐릭터 회전 
                    if (0.1f < m_MoveDir.magnitude)  //로테이션에서는 모두 들어가야 한다.
                    {
                        Quaternion a_TargetRot;
                        float m_RotSpeed = 7.0f;
                        a_TargetRot = Quaternion.LookRotation(m_MoveDir);
                        transform.rotation =
                            Quaternion.Slerp(transform.rotation, a_TargetRot,
                                             Time.deltaTime * m_RotSpeed);
                    }
                    //캐릭터 회전               
                    //-------- 이동 구현

                    //Animator의 IsAttack 변수를 false로 설정
                    animator.SetBool("IsAttack", false);
                    //Animator의 IsTrace 변숫값을 true로 설정
                    animator.SetBool("IsTrace", true);
                }
                break;

            //공격 상태
            case MonsterState.attack:
                {

                    ////추적 중지
                    //nvAgent.isStopped = true; //nvAgent.Stop();
                    //IsAttack 을 true로 설정해 attack State로 전이
                    animator.SetBool("IsAttack", true);

                    //---몬스터가 주인공을 공격하면서 바라 보도록 해야 한다.   
                    float m_RotSpeed = 6.0f;              //초당 회전 속도
                    Vector3 a_CacVLen = playerTr.position - monsterTr.position;
                    a_CacVLen.y = 0.0f;
                    Quaternion a_TargetRot =
                                Quaternion.LookRotation(a_CacVLen.normalized);
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                                              a_TargetRot, Time.deltaTime * m_RotSpeed);
                    //---몬스터가 주인공을 공격하면서 바라 보도록 해야 한다. 
                    OnPlayerDie();
                }
                break;
        }

 
    }//void MonActionUpdate()

    //void MonsterDie()
    //{
    //    Debug.Log("늑대주겅");
    //    isDie = true;
    //    monsterState = MonsterState.die;        
    //    animator.SetTrigger("IsDie");

    //    //---- 보상으로 아이템 드롭 
    //    //if (GameMgr.m_CoinItem != null)
    //    //{
    //    //    GameObject a_CoinObj = (GameObject)Instantiate(GameMgr.m_CoinItem);
    //    //    a_CoinObj.transform.position = this.transform.position;
    //    //    Destroy(a_CoinObj, 10.0f);  //10초내에 먹어야 한다.
    //    //}
    //    //---- 보상으로 아이템 드롭 
    //}

    void CreateBloodEffect(Vector3 pos)
    {
        //혈흔 효과 생성
        GameObject blood1 =
         (GameObject)Instantiate(bloodEffect, pos, Quaternion.identity);

        blood1.GetComponent<ParticleSystem>().Play();

        Destroy(blood1, 3.0f);

        //데칼 생성 위치 - 바닥에서 조금 올린 위치 산출
        Vector3 decalPos = monsterTr.position + (Vector3.up * 0.05f);

        //데칼의 회전값을 무작위로 설정

        Quaternion decalRot = Quaternion.Euler(90, 0, Random.Range(0, 360));

        //데칼 프리팹 생성

        GameObject blood2 = (GameObject)Instantiate(bloodDecal, decalPos, decalRot);
        //데칼의 크기도 불규칙적으로 나타나게끔 스케일 조정

        float scale = Random.Range(1.5f, 3.5f);

        blood2.transform.localScale = Vector3.one * scale;

        //5초 후에 혈흔효과 프리팹을 삭제

       Destroy(blood2, 3.0f);

    }


    public void TakeDamage(int a_Value)
    {
        if (hp <= 0.0f) //이렇게 하면 사망처리는 한번만 될 것이다.
            return;
        
        hp -= a_Value;   
        CreateBloodEffect(this.transform.position);
        if (hp <= 0)
        {
            hp = 0;
            isDie = true;
            monsterState = MonsterState.die;
            animator.SetTrigger("IsDie");
            Destroy(gameObject,2.0f);
            
        }        
    }


    public void OnPlayerDie()
    {
        if (isDie == true)
            return;
        //추적을 정지하고 애니메이션을 수행
        //nvAgent.isStopped = true;
        animator.SetTrigger("PlayerDie");
    }

    public void WolfAttack()
    {
        if (isDie == true)
            return;
        if (monType == MonType.Zombie)
            return;

        playerTr.GetComponent<Hero>().TakeDamage(10);
        MonCol.enabled = true;
    }

    public void WolffAttackEnd()
    {
        if (isDie == true)
            return;
        if (monType == MonType.Zombie)
            return;

        MonCol.enabled = false;
    }


    public void ZomAttack()
    {
        if (isDie == true)
            return;
        if (monType == MonType.Wolf)
            return;

        playerTr.GetComponent<Hero>().TakeDamage(10);
        MonCol.enabled = true;
    }

    public void ZomAttackEnd()
    {
        if (isDie == true)
            return;
        if (monType == MonType.Wolf)
            return;

        MonCol.enabled = false;
    }

}
