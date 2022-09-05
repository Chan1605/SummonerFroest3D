using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonCtrl : MonoBehaviour
{
    public enum MonsterState { idle, trace, attack, hit, die };

    public enum MonType { Zombie, Wolf };

    //������ ���� ���� ������ ������ Enum ����
    public MonsterState monsterState = MonsterState.idle;

    public MonType monType = MonType.Zombie;

    private Transform monsterTr;
    private Transform playerTr;
    //private NavMeshAgent nvAgent;
    public Animator animator;

    public float traceDist = 10.0f;
    //���� �����Ÿ�
    public float attackDist = 2.0f;

    public bool isDie = false;

    public int hp = 100;
    [SerializeField]float m_MoveVelocity = 5.0f;
    public GameObject Attackpos;
    BoxCollider MonCol;
    void Awake()
    {
        //traceDist = 18.0f; 
        //attackDist = 2.5f;         

        //������ Transform �Ҵ�
        monsterTr = this.gameObject.GetComponent<Transform>();
        //���� ����� Player�� Transform �Ҵ�
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
        //NavMeshAgent ������Ʈ �Ҵ�
        //nvAgent = this.gameObject.GetComponent<NavMeshAgent>();
        //Animator ������Ʈ �Ҵ�
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

    //������ �������� ������ �ൿ ���¸� üũ�ϰ� monsterState �� ����
    float m_AI_Delay = 0.0f;
    void CheckMonStateUpdate()
    {
        if (isDie == true)
            return;

        //0.1�� ���� ��ٷȴٰ� �������� �Ѿ
        m_AI_Delay -= Time.deltaTime;
        if (0.0f < m_AI_Delay)
            return;

        m_AI_Delay = 0.1f;

        //���Ϳ� �÷��̾� ������ �Ÿ� ����
        float dist = Vector3.Distance(playerTr.position, monsterTr.position);

        if (dist <= attackDist) //���ݰŸ� ���� �̳��� ���Դ��� Ȯ��
        {
            monsterState = MonsterState.attack;
        }
        else if (dist <= traceDist) //�����Ÿ� ���� �̳��� ���Դ��� Ȯ��
        {
            monsterState = MonsterState.trace; //������ ���¸� �������� ����
        }        
        else
        {
            monsterState = MonsterState.idle;   //������ ���¸� idle ���� ����
        }

    }//void CheckMonStateUpdate()

    //������ ���°��� ���� ������ ������ �����ϴ� �Լ�
    void MonActionUpdate()
    {
        if (isDie == true)
            return;

        switch (monsterState)
        {
            //idle ����
            case MonsterState.idle:
                //���� ����
                //nvAgent.isStopped = true; //nvAgent.Stop();
                //Animator�� IsTrace ������ false �� ����
                animator.SetBool("IsTrace", false);
                break;

            //���� ����
            case MonsterState.trace:
                {
  
                    ////���� ����� ��ġ�� �Ѱ���
                    //nvAgent.destination = playerTr.position;
                    ////������ �����
                    //nvAgent.isStopped = false;  //nvAgent.Resume();

                    //-------- �̵� ����
                    //float m_MoveVelocity = 5.0f;      //��� �ʴ� �̵� �ӵ�...
                    Vector3 m_MoveDir = Vector3.zero;
                    m_MoveDir = playerTr.position - this.transform.position;
                    m_MoveDir.y = 0.0f;

                    Vector3 a_StepVec = (m_MoveDir.normalized *
                                            Time.deltaTime * m_MoveVelocity);
                    transform.Translate(a_StepVec, Space.World);

                    //ĳ���� ȸ�� 
                    if (0.1f < m_MoveDir.magnitude)  //�����̼ǿ����� ��� ���� �Ѵ�.
                    {
                        Quaternion a_TargetRot;
                        float m_RotSpeed = 7.0f;
                        a_TargetRot = Quaternion.LookRotation(m_MoveDir);
                        transform.rotation =
                            Quaternion.Slerp(transform.rotation, a_TargetRot,
                                             Time.deltaTime * m_RotSpeed);
                    }
                    //ĳ���� ȸ��               
                    //-------- �̵� ����

                    //Animator�� IsAttack ������ false�� ����
                    animator.SetBool("IsAttack", false);
                    //Animator�� IsTrace �������� true�� ����
                    animator.SetBool("IsTrace", true);
                }
                break;

            //���� ����
            case MonsterState.attack:
                {

                    ////���� ����
                    //nvAgent.isStopped = true; //nvAgent.Stop();
                    //IsAttack �� true�� ������ attack State�� ����
                    animator.SetBool("IsAttack", true);

                    //---���Ͱ� ���ΰ��� �����ϸ鼭 �ٶ� ������ �ؾ� �Ѵ�.   
                    float m_RotSpeed = 6.0f;              //�ʴ� ȸ�� �ӵ�
                    Vector3 a_CacVLen = playerTr.position - monsterTr.position;
                    a_CacVLen.y = 0.0f;
                    Quaternion a_TargetRot =
                                Quaternion.LookRotation(a_CacVLen.normalized);
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                                              a_TargetRot, Time.deltaTime * m_RotSpeed);                    
                    //---���Ͱ� ���ΰ��� �����ϸ鼭 �ٶ� ������ �ؾ� �Ѵ�. 
                }
                break;
        }

 
    }//void MonActionUpdate()

    //void MonsterDie()
    //{
    //    Debug.Log("�����ְ�");
    //    isDie = true;
    //    monsterState = MonsterState.die;        
    //    animator.SetTrigger("IsDie");
        
    //    //---- �������� ������ ��� 
    //    //if (GameMgr.m_CoinItem != null)
    //    //{
    //    //    GameObject a_CoinObj = (GameObject)Instantiate(GameMgr.m_CoinItem);
    //    //    a_CoinObj.transform.position = this.transform.position;
    //    //    Destroy(a_CoinObj, 10.0f);  //10�ʳ��� �Ծ�� �Ѵ�.
    //    //}
    //    //---- �������� ������ ��� 
    //}




    public void TakeDamage(int a_Value)
    {
        if (hp <= 0.0f) //�̷��� �ϸ� ���ó���� �ѹ��� �� ���̴�.
            return;
        
        hp -= a_Value;
        if (hp <= 0)
        {
            hp = 0;
            isDie = true;
            monsterState = MonsterState.die;
            animator.SetTrigger("IsDie");
            Destroy(gameObject,0.7f);
            
        }        
    }


    void OnPlayerDie()
    {
        if (isDie == true)
            return;
        //������ �����ϰ� �ִϸ��̼��� ����
        //nvAgent.isStopped = true;
        animator.SetTrigger("IsPlayerDie");
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
