using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using System.Security.Cryptography;
using UnityEngine;

public class OffsetParentRotation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }
}
