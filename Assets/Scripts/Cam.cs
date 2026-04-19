using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cam : MonoBehaviour
{
    public GameObject m_Player = null;

    private Vector3 m_TargetPos = Vector3.zero;
    private Vector3 StartCamera = Vector3.zero;

    //---- 카메라 회전 / 거리 관련 변수
    private float m_RotH = 0.0f;
    private float m_RotV = 0.0f;
    private float hSpeed = 5.0f;
    private float vSpeed = 2.4f;
    private float vMinLimit = -7.0f;
    private float vMaxLimit = 80.0f;
    private float zoomSpeed = 1.0f;
    private float maxDist = 50.0f;
    private float minDist = 3.0f;
    //---- 카메라 회전 / 거리 관련 변수

    //---- 기본 카메라 값
    private float m_DefaltRotH = 100.0f;
    private float m_DefaltRotV = 17.0f;
    private float m_DefaltDist = 10.0f;
    //---- 기본 카메라 값

    //---- 계산용 변수
    private Quaternion a_BuffRot;
    private Vector3 a_BasicPos = Vector3.zero;
    private float distance = 30.0f;
    private Vector3 a_BuffPos;
    //---- 계산용 변수

    [Header("Skill Camera Follow")]
    [SerializeField] private float targetHeight = 3.0f;
    [SerializeField] private float followSmooth = 8.0f;   // 평소 따라갈 때 부드러움
    [SerializeField] private float catchUpSmooth = 4.0f;  // 지연 후 복귀할 때 부드러움

    private bool isFollowDelayed = false;
    private float followDelayTimer = 0.0f;

    public void InitCamera(GameObject a_Player)
    {
        m_Player = a_Player;
    }

    /// <summary>
    /// 카메라가 플레이어를 일정 시간 따라가지 않도록 함
    /// </summary>
    public void DelayFollow(float delay)
    {
        isFollowDelayed = true;
        followDelayTimer = delay;
    }

    void Start()
    {
        if (m_Player == null)
            return;

        m_TargetPos = m_Player.transform.position;
        m_TargetPos.y += 1.4f;

        m_RotH = m_DefaltRotH;
        m_RotV = m_DefaltRotV;
        distance = m_DefaltDist;

        a_BuffRot = Quaternion.Euler(m_RotV, m_RotH, 0);
        a_BasicPos = new Vector3(0.0f, 0.0f, -distance);
        a_BuffPos = (a_BuffRot * a_BasicPos) + m_TargetPos;

        transform.position = a_BuffPos;
        transform.LookAt(m_TargetPos);
    }

    void LateUpdate()
    {
        if (m_Player == null)
            return;

        // 1) 플레이어 따라가기 지연 처리
        if (isFollowDelayed == true)
        {
            followDelayTimer -= Time.deltaTime;
            if (followDelayTimer <= 0.0f)
            {
                followDelayTimer = 0.0f;
                isFollowDelayed = false;
            }
        }

        // 2) 지연 중이 아닐 때만 타겟 위치를 플레이어 쪽으로 갱신
        if (isFollowDelayed == false)
        {
            Vector3 desiredTargetPos = m_Player.transform.position;
            desiredTargetPos.y += targetHeight;

            // 지연 후 복귀하거나 평상시 따라갈 때 너무 딱딱하지 않게 보간
            float currentSmooth = catchUpSmooth;

            m_TargetPos = Vector3.Lerp(m_TargetPos, desiredTargetPos, Time.deltaTime * currentSmooth);
        }

        // 3) 우클릭 회전
        if (Input.GetMouseButton(1))
        {
            m_RotH += Input.GetAxis("Mouse X") * hSpeed;
            m_RotV -= Input.GetAxis("Mouse Y") * vSpeed;
            m_RotV = ClampAngle(m_RotV, vMinLimit, vMaxLimit);
        }

        // 4) 줌
        if (Input.GetAxis("Mouse ScrollWheel") < 0 && distance < maxDist)
        {
            distance += zoomSpeed;
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0 && distance > minDist)
        {
            distance -= zoomSpeed;
        }

        // 5) 카메라 위치 계산
        a_BuffRot = Quaternion.Euler(m_RotV, m_RotH, 0);
        a_BasicPos.x = 0.0f;
        a_BasicPos.y = 0.0f;
        a_BasicPos.z = -distance;

        a_BuffPos = a_BuffRot * a_BasicPos + m_TargetPos;

        // 6) 카메라도 부드럽게 이동
        transform.position = a_BuffPos;//Vector3.Lerp(transform.position, a_BuffPos, Time.deltaTime * followSmooth);

        // 7) 타겟 바라보기
        transform.LookAt(m_TargetPos);
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360.0f)
            angle += 360.0f;
        if (angle > 360.0f)
            angle -= 360.0f;

        return Mathf.Clamp(angle, min, max);
    }
}