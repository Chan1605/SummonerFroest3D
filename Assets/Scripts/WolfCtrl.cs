using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfCtrl : MonoBehaviour
{
    public enum MonsterState { idle, trace, attack, hit, die };

    //������ ���� ���� ������ ������ Enum ����
    public MonsterState monsterState = MonsterState.idle;

    private Transform monsterTr;
    private Transform playerTr;
    //private NavMeshAgent nvAgent;
    private Animator animator;

    public float traceDist = 10.0f;
    //���� �����Ÿ�
    public float attackDist = 2.0f;

    private bool isDie = false;

    private int hp = 100;

    void Awake()
    {
        traceDist = 18.0f; //100.0f;
        attackDist = 2.5f; //1.8f;

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
                    float m_MoveVelocity = 5.0f;      //��� �ʴ� �̵� �ӵ�...
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

    void MonsterDie()
    {
        isDie = true;
        monsterState = MonsterState.die;        
        animator.SetTrigger("IsDie");

        //---- �������� ������ ��� 
        //if (GameMgr.m_CoinItem != null)
        //{
        //    GameObject a_CoinObj = (GameObject)Instantiate(GameMgr.m_CoinItem);
        //    a_CoinObj.transform.position = this.transform.position;
        //    Destroy(a_CoinObj, 10.0f);  //10�ʳ��� �Ծ�� �Ѵ�.
        //}
        //---- �������� ������ ��� 
    }
}