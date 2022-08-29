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
    float m_MoveVelocity = 10.0f;     //��� �ʴ� �̵� �ӵ�...
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


        movePath = new NavMeshPath();
        nvAgent = this.gameObject.GetComponent<NavMeshAgent>();
        nvAgent.updateRotation = false;
    }

    // Update is called once per frame
    void Update()
    {
        //dir.x = Input.GetAxis("Horizontal");
        //dir.z = Input.GetAxis("Vertical");
        //dir.Normalize();

        //transform.Translate(dir * speed * Time.deltaTime);
        
        //-------------- ������ ���� �̵� ó�� �ϴ� ���
        h = Input.GetAxisRaw("Horizontal"); //ȭ��ǥŰ �¿�Ű�� �����ָ� -1.0f, 0.0f, 1.0f ���̰��� ������ �ش�.
        v = Input.GetAxisRaw("Vertical");   //ȭ��ǥŰ ���Ʒ�Ű�� �����ָ� -1.0f, 0.0f, 1.0f ���̰��� ������ �ش�.
        //-------------- ������ ���� �̵� ó�� �ϴ� ���
        if (v < 0.0f)
            v = 0.0f;

        if (0.0f != h || 0.0f != v) //Ű���� �̵�ó��
        {
            m_isPickMvOnOff = false;  //���콺 ��ŷ �̵� ���

            //-------- �Ϲ����� �̵� ����
            a_CalcRotY = transform.eulerAngles.y;
            a_CalcRotY = a_CalcRotY + (h * rotSpeed * Time.deltaTime);
            transform.eulerAngles = new Vector3(0.0f, a_CalcRotY, 0.0f);

            MoveVStep = transform.forward * v;
            MoveNextStep = MoveVStep.normalized * m_MoveVelocity * Time.deltaTime;
            transform.position = transform.position + MoveNextStep;
            //-------- �Ϲ����� �̵� ����
        }

        if (Input.GetMouseButtonDown(0))
            {
                a_MousePos = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(a_MousePos, out hitInfo, Mathf.Infinity,                                                        m_layerMask.value))
            {
                //if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("MyUnit"))
                //{ //���� ��ŷ�� �� 
                //    MousePicking(hitInfo.point, hitInfo.collider.gameObject);

                //    if (GameMgr.Inst.m_CursorMark != null)
                //        GameMgr.Inst.m_CursorMark.SetActive(false);
                //}
                //else  //���� �ٴ� ��ŷ�� �� 
                {
                    MousePicking(hitInfo.point);

                    GameMgr.Inst.CursorMarkOn(hitInfo.point);
                }//else  //���� �ٴ� ��ŷ�� ��
            }
        }//if (Input.GetMouseButtonDown(0))

        MousePickUpdate();
    }



    public void MousePicking(Vector3 a_SetPickVec, GameObject a_PickMon = null)
    {
        a_StartPos = this.transform.position; //��� ��ġ    
        a_SetPickVec.y = this.transform.position.y; // ���� ��ǥ ��ġ

        a_CacLenVec = a_SetPickVec - a_StartPos;
        a_CacLenVec.y = 0.0f;

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

        //------- Picking Enemy ���� ó�� �κ�
        //if (a_PickMon != null)
        //{
        //    a_CacTgVec = a_PickMon.transform.position - transform.position;

        //    //���ݰ��ðŸ�... Ÿ���� �ְ�  +1.0f�� ������ ���͵� �ٰ��ò��� 
        //    //�� ���� ���ݾִϿ� ���� ��ö� move �ִϰ� ���� ���� ���Ѵ�.
        //    float a_AttDist = m_AttackDist;
        //    if (a_PickMon.GetComponent<MonsterCtrl>().m_AggroTarget
        //                                             == this.gameObject)
        //    {
        //        a_AttDist = m_AttackDist + 1.0f;
        //        //���� �����Ϸ��� �ϴ� ������ ��׷� Ÿ���� ����....
        //    }

        //    if (a_CacTgVec.magnitude <= a_AttDist)
        //    {
        //        m_TargetUnit = a_PickMon;
        //        AttackOrder();  //��� ����

        //        return;

        //    }//��� ���� �϶�
        //} //if (a_PickMon != null)
        //a_StartPos = this.transform.position; //��� ��ġ    
        //a_SetPickVec.y = this.transform.position.y; // ���� ��ǥ ��ġ

        //a_CacLenVec = a_SetPickVec - a_StartPos;
        //a_CacLenVec.y = 0.0f;

        //if (a_CacLenVec.magnitude < 0.5f)  //�ʹ� �ٰŸ� ��ŷ�� ��ŵ�� �ش�.
        //    return;

        //float a_PathLen = a_CacLenVec.magnitude;
        ////---�׺���̼� �޽� ��ã�⸦ �̿��� �� �ڵ�
        //if (MyNavCalcPath(a_StartPos, a_SetPickVec, ref a_PathLen) == false)
        //    return;
        ////---�׺���̼� �޽� ��ã�⸦ �̿��� �� �ڵ�

        //m_TargetPos = a_SetPickVec;   //���� ��ǥ ��ġ
        //m_isPickMvOnOff = true;       //��ŷ �̵� OnOff

        //m_MoveDir = a_CacLenVec.normalized;
        //m_MoveDurTime = a_PathLen / m_MoveVelocity; //�����ϴµ� �ɸ��� �ð�
        //m_AddTimeCount = 0.0;

        //m_TargetUnit = a_PickMon; //Ÿ�� �ʱ�ȭ �Ǵ� ��ȿȭ 

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
            }//else
             //m_isPickMvOnOff = MoveToPath(); //������ ��� false ������
        } //if (m_isPickMvOnOff == true)
    }// void MousePickUpdate() 


   

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
}

