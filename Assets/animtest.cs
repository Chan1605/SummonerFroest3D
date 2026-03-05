using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animtest : MonoBehaviour
{
    private Animator anim;
    int hashattack = Animator.StringToHash("AttackCount");
    public GameObject Wep;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        //Wep.gameObject.SetActive(false);
    }

    public int AttackCount
    {
        get => anim.GetInteger(hashattack);
        set => anim.SetInteger(hashattack, value);        
            
    }

    public void AttackStart(int throwing = 0)
    {
        Debug.Log((throwing));
        //meleeWeapon.BeginAttack(throwing != 0);
        //m_InAttack = true;
    }
    // Update is called once per frame
    void Update()
    {        
        if (Input.GetKey(KeyCode.Alpha1))
        {
            anim.SetTrigger("run");
        }
        if (Input.GetKey(KeyCode.Alpha2))
        {
            anim.SetTrigger("Fastrun");
        }
        else if(Input.GetButton("Fire1"))
        {
            //Wep.gameObject.SetActive(true);
            anim.SetTrigger("ComboAttack");
   
        }
        else
        {
            //Wep.gameObject.SetActive(false);
            anim.SetBool("Idle", true);
        }
            
    }
}
