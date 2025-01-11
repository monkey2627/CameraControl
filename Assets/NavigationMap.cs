using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//让导览相机的位置能够实时显示在map上
public class NavigationMap : MonoBehaviour
{
    private RectTransform rect;
    public Transform navigationCamera;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Disable()
    {
        this.gameObject.SetActive(false);
    }
    public void Enable()
    {
        this.gameObject.SetActive(true);
    }
}
