using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTest : MonoBehaviour
{
    public GameObject Player;

    Vector3 Tagetpos = Vector3.zero;
    float m_Time = 2.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetAimSpeed(float a_Time)
    {
        Tagetpos = Player.transform.position - transform.position;
        
        m_Time = a_Time;
    }
}
