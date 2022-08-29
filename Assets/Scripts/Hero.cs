using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Hero : MonoBehaviour
{
    Vector3 dir = Vector3.zero;
    float speed = 10.0f;

    float h = 0, v = 0;
    Vector3 MoveNextStep;            //보폭을 계산해 주기 위한 변수
    Vector3 MoveHStep;
    Vector3 MoveVStep;
    float m_MoveVelocity = 10.0f;     //평면 초당 이동 속도...
    float a_CalcRotY = 0.0f;
    float rotSpeed = 150.0f; //초당 150도 회전하라는 속도

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

    //---------------------- 마우스 피킹 예약 이동 관련 변수 
    float m_RsvPicking = 0.0f;             //reservation 예약 
    //피킹 이동 예약의 유효시간 계산하기 위한 변수 3.5초 뒤에 무효화 됨
    Vector3 m_RsvTargetPos = Vector3.zero; //최종 목표 위치
    double m_RsvMvDurTime = 0.0;           //목표점까지 도착하는데 걸리는 시간

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
        m_layerMask |= 1 << LayerMask.NameToLayer("MyUnit"); //Unit 도 피킹


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
        
        //-------------- 가감속 없이 이동 처리 하는 방법
        h = Input.GetAxisRaw("Horizontal"); //화살표키 좌우키를 눌러주면 -1.0f, 0.0f, 1.0f 사이값을 리턴해 준다.
        v = Input.GetAxisRaw("Vertical");   //화살표키 위아래키를 눌러주면 -1.0f, 0.0f, 1.0f 사이값을 리턴해 준다.
        //-------------- 가감속 없이 이동 처리 하는 방법
        if (v < 0.0f)
            v = 0.0f;

        if (0.0f != h || 0.0f != v) //키보드 이동처리
        {
            m_isPickMvOnOff = false;  //마우스 피킹 이동 취소

            //-------- 일반적인 이동 계산법
            a_CalcRotY = transform.eulerAngles.y;
            a_CalcRotY = a_CalcRotY + (h * rotSpeed * Time.deltaTime);
            transform.eulerAngles = new Vector3(0.0f, a_CalcRotY, 0.0f);

            MoveVStep = transform.forward * v;
            MoveNextStep = MoveVStep.normalized * m_MoveVelocity * Time.deltaTime;
            transform.position = transform.position + MoveNextStep;
            //-------- 일반적인 이동 계산법
        }

        if (Input.GetMouseButtonDown(0))
            {
                a_MousePos = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(a_MousePos, out hitInfo, Mathf.Infinity,                                                        m_layerMask.value))
            {
                //if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("MyUnit"))
                //{ //몬스터 픽킹일 때 
                //    MousePicking(hitInfo.point, hitInfo.collider.gameObject);

                //    if (GameMgr.Inst.m_CursorMark != null)
                //        GameMgr.Inst.m_CursorMark.SetActive(false);
                //}
                //else  //지형 바닥 픽킹일 때 
                {
                    MousePicking(hitInfo.point);

                    GameMgr.Inst.CursorMarkOn(hitInfo.point);
                }//else  //지형 바닥 픽킹일 때
            }
        }//if (Input.GetMouseButtonDown(0))

        MousePickUpdate();
    }



    public void MousePicking(Vector3 a_SetPickVec, GameObject a_PickMon = null)
    {
        a_StartPos = this.transform.position; //출발 위치    
        a_SetPickVec.y = this.transform.position.y; // 최종 목표 위치

        a_CacLenVec = a_SetPickVec - a_StartPos;
        a_CacLenVec.y = 0.0f;

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

        //------- Picking Enemy 공격 처리 부분
        //if (a_PickMon != null)
        //{
        //    a_CacTgVec = a_PickMon.transform.position - transform.position;

        //    //공격가시거리... 타겟이 있고  +1.0f면 어차피 몬스터도 다가올꺼고 
        //    //좀 일찍 공격애니에 들어가야 잠시라도 move 애니가 끼어 들지 못한다.
        //    float a_AttDist = m_AttackDist;
        //    if (a_PickMon.GetComponent<MonsterCtrl>().m_AggroTarget
        //                                             == this.gameObject)
        //    {
        //        a_AttDist = m_AttackDist + 1.0f;
        //        //지금 공격하려고 하는 몬스터의 어그로 타겟이 나면....
        //    }

        //    if (a_CacTgVec.magnitude <= a_AttDist)
        //    {
        //        m_TargetUnit = a_PickMon;
        //        AttackOrder();  //즉시 공격

        //        return;

        //    }//즉시 공격 하라
        //} //if (a_PickMon != null)
        //a_StartPos = this.transform.position; //출발 위치    
        //a_SetPickVec.y = this.transform.position.y; // 최종 목표 위치

        //a_CacLenVec = a_SetPickVec - a_StartPos;
        //a_CacLenVec.y = 0.0f;

        //if (a_CacLenVec.magnitude < 0.5f)  //너무 근거리 피킹은 스킵해 준다.
        //    return;

        //float a_PathLen = a_CacLenVec.magnitude;
        ////---네비게이션 메쉬 길찾기를 이용할 때 코드
        //if (MyNavCalcPath(a_StartPos, a_SetPickVec, ref a_PathLen) == false)
        //    return;
        ////---네비게이션 메쉬 길찾기를 이용할 때 코드

        //m_TargetPos = a_SetPickVec;   //최종 목표 위치
        //m_isPickMvOnOff = true;       //피킹 이동 OnOff

        //m_MoveDir = a_CacLenVec.normalized;
        //m_MoveDurTime = a_PathLen / m_MoveVelocity; //도착하는데 걸리는 시간
        //m_AddTimeCount = 0.0;

        //m_TargetUnit = a_PickMon; //타겟 초기화 또는 무효화 

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
            }//else
             //m_isPickMvOnOff = MoveToPath(); //도착한 경우 false 리턴함
        } //if (m_isPickMvOnOff == true)
    }// void MousePickUpdate() 


   

    void ClearMsPickPath() //마우스 픽킹이동 취소 함수
    {
        m_isPickMvOnOff = false;

        //----피킹을 위한 동기화 부분
        m_PathEndPos = transform.position;
        if (0 < movePath.corners.Length)
        {
            movePath.ClearCorners();  //경로 모두 제거 
        }
        m_CurPathIndex = 1;       //진행 인덱스 초기화
        //----피킹을 위한 동기화 부분

        if (GameMgr.Inst.m_CursorMark != null)
            GameMgr.Inst.m_CursorMark.SetActive(false);
    }

    Vector3 a_VecLen = Vector3.zero;
    public bool MyNavCalcPath(Vector3 a_StartPos, Vector3 a_TargetPos,
                              ref float a_PathLen) //길찾기...
    { //경로 탐색 함수

        //--- 기존 탐색된 경로가 있으면 초기화 하고 계산한다.
        movePath.ClearCorners();  //경로 모두 제거 
        m_CurPathIndex = 1;       //진행 인덱스 초기화 
        m_PathEndPos = transform.position;
        //--- 기존 탐색된 경로가 있으면 초기화 하고 계산한다.

        if (nvAgent == null || nvAgent.enabled == false)
            return false;

        if (NavMesh.CalculatePath(a_StartPos, a_TargetPos, -1, movePath) == false)
        { //네비게이션 메쉬에서 경로를 탐색해 주는 함수
            return false;
        }

        for (int i = 1; i < movePath.corners.Length; ++i)
        {
            //#if UNITY_EDITOR
            //            Debug.DrawLine(movePath.corners[i - 1],
            //                             movePath.corners[i], Color.cyan, 10);
            //            //맨마지막 인자 duration 라인을 표시하는 시간
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

        //-- 캐릭터가 마지막 위치에 도착했을 때 정확한 방향을 
        // 바라보게 하고 싶은 경우 때문에 계산해 놓는다.
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
        { //최소 m_CurPathIndex = 1 보다 큰 경우에는 캐릭터를 이동시켜 준다.

            a_CurCPos = this.transform.position;
            a_CacDestV = movePath.corners[m_CurPathIndex];
            a_CurCPos.y = a_CacDestV.y;
            //높이 오차가 있어서 도착 판정을 못하는 경우가 있다. 
            a_TargetDir = a_CacDestV - a_CurCPos;
            a_TargetDir.y = 0.0f;
            a_TargetDir.Normalize();

            a_CacSpeed = m_MoveVelocity;
            a_CacSpeed = a_CacSpeed * overSpeed;

            a_NowStep = a_CacSpeed * Time.deltaTime;
            //이번에 이동했을 때 이 안으로만 들어와도 무조건 도착한 것으로 본다.

            a_Velocity = a_CacSpeed * a_TargetDir;
            a_Velocity.y = 0.0f;
            nvAgent.velocity = a_Velocity;          //이동 처리...

            //중간점에 도착했는지 판단하는 부분
            if ((a_CacDestV - a_CurCPos).magnitude <= a_NowStep)
            {//중간점에 도착한 것으로 본다.  여기서 a_CurCPos == Old Position의미
                movePath.corners[m_CurPathIndex] = this.transform.position;
                m_CurPathIndex = m_CurPathIndex + 1;
            }//if ((a_CacDestV - a_CurCPos).magnitude <= a_NowStep) .  

            m_AddTimeCount = m_AddTimeCount + Time.deltaTime;
            if (m_MoveDurTime <= m_AddTimeCount) //목표점에 도착한 것으로 판정한다.
            {
                m_CurPathIndex = movePath.corners.Length;
            }

        }//if (m_CurPathIndex < movePath.corners.Length) 

        if (m_CurPathIndex < movePath.corners.Length)
        {  //목적지에 아직 도착 하지 않은 경우

            //-------------캐릭터 회전 / 애니메이션 방향 조정
            a_vTowardNom = movePath.corners[m_CurPathIndex] - this.transform.position;
            a_vTowardNom.y = 0.0f;
            a_vTowardNom.Normalize();        // 단위 벡터를 만든다.

            if (0.0001f < a_vTowardNom.magnitude)  //로테이션에서는 모두 들어가야 한다.
            {
                Quaternion a_TargetRot = Quaternion.LookRotation(a_vTowardNom);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                            a_TargetRot, Time.deltaTime * m_RotSpeed);
            }

    
            //-------------캐릭터 회전 / 애니메이션 방향 조정
        }
        else //최종 목적지에 도착한 경우 매 플레임
        {
            //최종 목적지에 도착한 경우 한번 발생시키기 위한 부분
            if (a_OldPathCount < movePath.corners.Length)
            {
                ClearMsPickPath();

            }

            a_isSucessed = false;
            //아직 목적지에 도착하지 않았다면 다시 잡아 줄 것이기 때문에... 
        }

        return a_isSucessed;
    }//public bool MoveToPath(float overSpeed = 1.0f)
}

