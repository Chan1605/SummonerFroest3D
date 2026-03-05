using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tofollow : MonoBehaviour
{
       
        public Transform toFollow;

        private void FixedUpdate()
        {
            transform.position = toFollow.position;
            transform.rotation = toFollow.rotation;
        }
  
}
