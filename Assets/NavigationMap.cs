using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//�õ��������λ���ܹ�ʵʱ��ʾ��map��
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
