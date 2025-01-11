using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHeadController : MonoBehaviour, IController
{
    private Transform currentSpine;
    public Transform targetPoint;
    public Vector3 spineAngle = new Vector3(-40.7f, 1.7f, 84.5f);
    private Transform[] points;
    private int currentindex;
    private bool stopLooking = true;
    // Start is called before the first frame update
    void Start()
    {
        stopLooking = true;
        Transform maleLookTargetTrans = transform.Find("MaleHeadLookTarget");
        Transform femaleLookTargetTrans = transform.Find("FemaleHeadLookTarget");
        points = new Transform[maleLookTargetTrans.childCount + femaleLookTargetTrans.childCount];
        for (int i = 0; i < maleLookTargetTrans.childCount; i++)
        {
            points[i] = maleLookTargetTrans.GetChild(i);
        }
        for (int i = 0; i < femaleLookTargetTrans.childCount; i++)
        {
            points[i + maleLookTargetTrans.childCount] = femaleLookTargetTrans.GetChild(i);
        }
        this.RegistEvent<GetLookTargetIndexEvent>(GetLookTargetIndex);
        this.RegistEvent<StopLookingFunctionEvent>(StopLookingFunction);
    }

    //要在动画更新之后去处理
    void LateUpdate()
    {
        if (stopLooking)
        {
            return;
        }//没有看就直接返回，不用再处理
        currentSpine.LookAt(points[currentindex], Vector3.up);
        currentSpine.Rotate(spineAngle);

    }
    public void InitPlayerHeadCtrl()
    {
        currentSpine = GameObject.Find("character/bloodelf/male/bloodelfmale_hd_bone_9").transform;
    }
    private void GetLookTargetIndex(object offsetAngle)
    {
        float oa = (float)offsetAngle;
        switch (oa)
        {
            case 0:
                currentindex = 0;
                break;
            case 45:
                currentindex = 1;
                break;
            case -45:
                currentindex = 2;
                break;
            case -90:
                currentindex = 3;
                break;
            case 90:
                currentindex = 4;
                break;
            default:
                break;
        }
    }
    private void StopLookingFunction(object obj)
    {
        StopLookingSrc ss = (StopLookingSrc)obj;
        if (ss.delayTime > 0)
        {
            stopLooking = true;
            CancelInvoke();
            //过多少秒之后执行这个方法
            Invoke("DelayOpenLookingFunction", ss.delayTime);
        }
        else
        {
            stopLooking = ss.stop;
        }
    }
    private void DelayOpenLookingFunction()
    {

    }
}
