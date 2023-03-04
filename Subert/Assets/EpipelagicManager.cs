using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EpipelagicManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        FindObjectOfType<AudioManager>().Play("EpipelagicTheme");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
