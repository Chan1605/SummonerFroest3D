using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGenerator : MonoBehaviour
{
    public GameObject EnemyGroup;
    public GameObject TutorialGroup;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(GameMgr.Inst.PlayTimer > 15.0f)
        {
            TutorialGroup.gameObject.SetActive(true);
        }
        if(GameMgr.Inst.PlayTimer > 45.0f)
        {
            EnemyGroup.gameObject.SetActive(true);
        }
    }
}
