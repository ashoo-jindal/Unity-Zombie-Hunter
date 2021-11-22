using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class MakeRadarObject : MonoBehaviour
{

    public Image icon;


    // Start is called before the first frame update
    void Start()
    {
        Radar.RegisterRadarObject(this.gameObject, icon);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        Radar.RemoveRadarObject(this.gameObject);
    }
}
