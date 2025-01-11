using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frostwurmnorthrend : MonoBehaviour
{
    public ParticleSystem smokePS;
    public ParticleSystem firePS;
    //触发事件时，将对应方法的脚本挂载在装了animator的物体上
    private void Awake()
    {
        smokePS = transform.Find("Smoke").GetComponent<ParticleSystem>();
        firePS = DeepFindTransform.DeepFindChild(transform,"FrostSpray").GetComponent<ParticleSystem>();
        HideAll();
    }
    public void PlayGroundSmoke()
    {
        smokePS.gameObject.SetActive(true);
        smokePS.Play();
    }
    public void PlayIceFire()
    {
        firePS.gameObject.SetActive(true);
        firePS.Play();
    }
    public void HideAll()
    {
        firePS.Stop();
        smokePS.Stop();
        firePS.gameObject.SetActive(false);
        smokePS.gameObject.SetActive(false);
    }
    public void StopIceFire()
    {
        firePS.gameObject.SetActive(false);
        firePS.Stop();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
