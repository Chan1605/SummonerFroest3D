using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Hero : MonoBehaviour
{
    Vector3 dir = Vector3.zero;
    float speed = 10.0f;

    float h = 0, v = 0;
    Vector3 MoveNextStep;            //������ ����� �ֱ� ���� ����
    Vector3 MoveHStep;
    Vector3 MoveVStep;
    float m_MoveVelocity = 8.0f;     //��� �ʴ� �̵� �ӵ�...
    float a_CalcRotY = 0.0f;
    float rotSpeed = 150.0f; //�ʴ� 150�� ȸ���϶�� �ӵ�

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

    //---------------------- ���콺 ��ŷ ���� �̵� ���� ���� 
    float m_RsvPicking = 0.0f;             //reservation ���� 
    //��ŷ �̵� ������ ��ȿ�ð� ����ϱ� ���� ���� 3.5�� �ڿ� ��ȿȭ ��
    Vector3 m_RsvTargetPos = Vector3.zero; //���� ��ǥ ��ġ
    double m_RsvMvDurTime = 0.0;           //��ǥ������ �����ϴµ� �ɸ��� �ð�

    NavMeshAgent nvAgent;    //using UnityEngine.AI;
    NavMeshPath movePath;
    Vector3 m_PathEndPos = Vector3.zero;
    [HideInInspector] public int m_CurPathIndex = 1;

    bool a_isSucessed = true;
    Vector3 a_CurCPos = Vector3.zero;
    Vector3 a_CacDestV = Vector3.zero;
    Vector3 a_TargetDir;
    float a_CacSpeed = 0.0f;
    float a_NowStep = 0.0f;
    Vector3 a_Velocity = Vector3.zero;
    Vector3 a_vTowardNom = Vector3.zero;
    int a_OldPathCount = 0;

    GameObject m_TargetUnit = null;

    Animator m_RefAnimator = null;
    string m_prevState = "";
    AnimState m_CurState = AnimState.idle; //IsMine == true �϶�
    public Anim anim;  //AnimSupporter.cs �ʿ� ���ǵǾ� ����
    AnimatorStateInfo animaterStateInfo;

    GameObject[] m_EnemyList = null;    //�ʵ���� ���͵��� �������� ���� ����

    float m_AttackDist = 1.9f;          //���ΰ��� ���ݰŸ�    

    Vector3 a_CacTgVec = Vector3.zero;  //Ÿ�ٱ����� �Ÿ� ���� ����
    Vector3 a_CacAtDir = Vector3.zero;  //���ݽ� ������ȯ�� ����

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
        m_layerMask |= 1 << LayerMask.NameToLayer("MyUnit"); //Unit �� ��ŷ

        m_RefAnimator = this.gameObject.GetComponent<Animator>();

        movePath = new NavMeshPath();
        nvAgent = this.gameObject.GetComponent<NavMeshAgent>();
        nvAgent.updateRotation = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            {
                a_MousePos = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(a_MousePos, out hitInfo, Mathf.Infinity,                                                        m_layerMask.value))
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

        MousePickUpdate();
        AttackRotUpdate();

        if (m_isPickMvOnOff == false)
            MySetAnim(AnimState.idle);
    }



    public void MousePicking(Vector3 a_SetPickVec, GameObject a_PickMon = null)
    {
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
                AttackOrder();

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
                MySetAnim(AnimState.move);
            }//else

            if (m_TargetUnit != null)
            { //<-- ���� �ִϸ��̼� ���̸� ���� ����� Ÿ���� �ڵ����� ��Եȴ�.
                a_CacTgVec = m_TargetUnit.transform.position -
                                                this.transform.position;
                if (a_CacTgVec.magnitude <= m_AttackDist) //���ݰŸ�
                    AttackOrder();
            }
            //m_isPickMvOnOff = MoveToPath(); //������ ��� false ������
        } //if (m_isPickMvOnOff == true)
    }// void MousePickUpdate() 

    //void FindEnemyTarget()
    //{
    //    m_EnemyList = GameObject.FindGameObjectsWithTag("Enemy");

    //    float a_MinLen = float.MaxValue;
    //    a_iCount = m_EnemyList.Length;
    //    m_TargetUnit = null;  //�켱 Ÿ�� ��ȿȭ
    //    for (int i = 0; i < a_iCount; ++i)
    //    {
    //        a_CacTgVec = m_EnemyList[i].transform.position - transform.position;
    //        a_CacTgVec.y = 0.0f;
    //        if (a_CacTgVec.magnitude <= m_AttackDist)
    //        {   //���ݰŸ� ���ʿ� ���� ��츸 Ÿ������ ��´�.
    //            if (a_CacTgVec.magnitude < a_MinLen)
    //            {
    //                a_MinLen = a_CacTgVec.magnitude;
    //                m_TargetUnit = m_EnemyList[i];
    //            }
    //        }//if (a_CacTgVec.magnitude < a_MinLen)
    //    }//for (int i = 0; i < iCount; ++i)
    //}



    void ClearMsPickPath() //���콺 ��ŷ�̵� ��� �Լ�
    {
        m_isPickMvOnOff = false;

        //----��ŷ�� ���� ����ȭ �κ�
        m_PathEndPos = transform.position;
        if (0 < movePath.corners.Length)
        {
            movePath.ClearCorners();  //��� ��� ���� 
        }
        m_CurPathIndex = 1;       //���� �ε��� �ʱ�ȭ
        //----��ŷ�� ���� ����ȭ �κ�

        if (GameMgr.Inst.m_CursorMark != null)
            GameMgr.Inst.m_CursorMark.SetActive(false);
    }

    Vector3 a_VecLen = Vector3.zero;
    public bool MyNavCalcPath(Vector3 a_StartPos, Vector3 a_TargetPos,
                              ref float a_PathLen) //��ã��...
    { //��� Ž�� �Լ�

        //--- ���� Ž���� ��ΰ� ������ �ʱ�ȭ �ϰ� ����Ѵ�.
        movePath.ClearCorners();  //��� ��� ���� 
        m_CurPathIndex = 1;       //���� �ε��� �ʱ�ȭ 
        m_PathEndPos = transform.position;
        //--- ���� Ž���� ��ΰ� ������ �ʱ�ȭ �ϰ� ����Ѵ�.

        if (nvAgent == null || nvAgent.enabled == false)
            return false;

        if (NavMesh.CalculatePath(a_StartPos, a_TargetPos, -1, movePath) == false)
        { //�׺���̼� �޽����� ��θ� Ž���� �ִ� �Լ�
            return false;
        }

        for (int i = 1; i < movePath.corners.Length; ++i)
        {
            //#if UNITY_EDITOR
            //            Debug.DrawLine(movePath.corners[i - 1],
            //                             movePath.corners[i], Color.cyan, 10);
            //            //�Ǹ����� ���� duration ������ ǥ���ϴ� �ð�
            //            Debug.DrawLine(movePath.corners[i],
            //                             movePath.corners[i] + Vector3.up * i,
            //                             Color.cyan, 10);
            //#endif
            a_VecLen = movePath.corners[i] - movePath.corners[i - 1];
            //a_VecLen.y = 0.0f;
            a_PathLen = a_PathLen + a_VecLen.magnitude;
        }//for (int i = 1; i < movePath.corners.Length; ++i)

        if (a_PathLen <= 0.0f)
            return false;

        //-- ĳ���Ͱ� ������ ��ġ�� �������� �� ��Ȯ�� ������ 
        // �ٶ󺸰� �ϰ� ���� ��� ������ ����� ���´�.
        m_PathEndPos = movePath.corners[(movePath.corners.Length - 1)];

        return true;

    } //public bool MyNavCalcPath(

    public bool MoveToPath(float overSpeed = 1.0f)
    {
        a_isSucessed = true;

        if (movePath == null)
            movePath = new NavMeshPath();

        a_OldPathCount = m_CurPathIndex;
        if (m_CurPathIndex < movePath.corners.Length)
        { //�ּ� m_CurPathIndex = 1 ���� ū ��쿡�� ĳ���͸� �̵����� �ش�.

            a_CurCPos = this.transform.position;
            a_CacDestV = movePath.corners[m_CurPathIndex];
            a_CurCPos.y = a_CacDestV.y;
            //���� ������ �־ ���� ������ ���ϴ� ��찡 �ִ�. 
            a_TargetDir = a_CacDestV - a_CurCPos;
            a_TargetDir.y = 0.0f;
            a_TargetDir.Normalize();

            a_CacSpeed = m_MoveVelocity;
            a_CacSpeed = a_CacSpeed * overSpeed;

            a_NowStep = a_CacSpeed * Time.deltaTime;
            //�̹��� �̵����� �� �� �����θ� ���͵� ������ ������ ������ ����.

            a_Velocity = a_CacSpeed * a_TargetDir;
            a_Velocity.y = 0.0f;
            nvAgent.velocity = a_Velocity;          //�̵� ó��...

            //�߰����� �����ߴ��� �Ǵ��ϴ� �κ�
            if ((a_CacDestV - a_CurCPos).magnitude <= a_NowStep)
            {//�߰����� ������ ������ ����.  ���⼭ a_CurCPos == Old Position�ǹ�
                movePath.corners[m_CurPathIndex] = this.transform.position;
                m_CurPathIndex = m_CurPathIndex + 1;
            }//if ((a_CacDestV - a_CurCPos).magnitude <= a_NowStep) .  

            m_AddTimeCount = m_AddTimeCount + Time.deltaTime;
            if (m_MoveDurTime <= m_AddTimeCount) //��ǥ���� ������ ������ �����Ѵ�.
            {
                m_CurPathIndex = movePath.corners.Length;
            }

        }//if (m_CurPathIndex < movePath.corners.Length) 

        if (m_CurPathIndex < movePath.corners.Length)
        {  //�������� ���� ���� ���� ���� ���

            //-------------ĳ���� ȸ�� / �ִϸ��̼� ���� ����
            a_vTowardNom = movePath.corners[m_CurPathIndex] - this.transform.position;
            a_vTowardNom.y = 0.0f;
            a_vTowardNom.Normalize();        // ���� ���͸� �����.

            if (0.0001f < a_vTowardNom.magnitude)  //�����̼ǿ����� ��� ���� �Ѵ�.
            {
                Quaternion a_TargetRot = Quaternion.LookRotation(a_vTowardNom);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                            a_TargetRot, Time.deltaTime * m_RotSpeed);
            }

    
            //-------------ĳ���� ȸ�� / �ִϸ��̼� ���� ����
        }
        else //���� �������� ������ ��� �� �÷���
        {
            //���� �������� ������ ��� �ѹ� �߻���Ű�� ���� �κ�
            if (a_OldPathCount < movePath.corners.Length)
            {
                ClearMsPickPath();

            }

            a_isSucessed = false;
            //���� �������� �������� �ʾҴٸ� �ٽ� ��� �� ���̱� ������... 
        }

        return a_isSucessed;
    }//public bool MoveToPath(float overSpeed = 1.0f)

    public void MySetAnim(AnimState newAnim,
              float CrossTime = 1.0f, string AnimName = "")
    {
        if (m_RefAnimator == null)
            return;

        if (m_prevState != null && !string.IsNullOrEmpty(m_prevState))
        {
            if (m_prevState.ToString() == newAnim.ToString())
                return;
        }

        if (!string.IsNullOrEmpty(m_prevState))
        {
            m_RefAnimator.ResetTrigger(m_prevState.ToString());
            m_prevState = null;
        }

        if (0.0f < CrossTime)
        {
            m_RefAnimator.SetTrigger(newAnim.ToString());
        }
        else
        {
            m_RefAnimator.Play(AnimName, -1, 0f);
            //����� Layer Index, �ڿ� 0f�� ó������ �ٽý���
        }

        m_prevState = newAnim.ToString(); //����������Ʈ�� ���罺����Ʈ ����
        m_CurState = newAnim;

    } //public void MySetAnim(AnimState newAnim,

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

    public void AttackOrder()
    {
        if (m_prevState == AnimState.idle.ToString()
            || m_prevState == AnimState.move.ToString())
        {
            //Immediate ����̰� Ű���� ��Ʈ���̳� ���̽�ƽ ��Ʈ�ѷ� �̵� ���̰�
            //����Ű�� ��Ÿ�ؼ� ������ �޸��� �ִϸ��̼ǿ� ��񵿾� 
            //���� �ִϰ� ������ �������߻��Ѵ�. <-- �̷� ���� ���� ����ó��

            MySetAnim(AnimState.attack);

            ClearMsPickPath();
        }
    }//public void AttackOrder()

    public bool ISAttack()
    {
        if (m_prevState != null && !string.IsNullOrEmpty(m_prevState))
        {
            if (m_prevState.ToString() == AnimState.attack.ToString() ||
                m_prevState.ToString() == AnimState.skill.ToString())
            {
                return true;
            }
        }

        return false;
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
        //MonsterCtrl a_Unit = m_TargetUnit.GetComponent<MonsterCtrl>();
        //if (a_Unit.MonState == AnimState.die)
        //{
        //    m_TargetUnit = null;
        //    return false;
        //}

        a_CacTgVec = m_TargetUnit.transform.position - transform.position;
        a_CacTgVec.y = 0.0f;
        if (m_AttackDist + a_ExtLen < a_CacTgVec.magnitude)
        {   //���ݰŸ� �ٱ��ʿ� ���� ��쵵 Ÿ���� ��ȿȭ �� ������

            //m_TargetUnit = null; //���Ÿ��� Ÿ���� ������ �� �����ϱ�...
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
        if (ISAttack() == false) //���� �ִϸ��̼��� �ƴϸ�...
            return;

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
}

