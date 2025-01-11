using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceCharacterDress : MonoBehaviour,IController
{
    private SkinnedMeshRenderer[] smrs;
    private GameObject[] gos;
    //当前人物控制的游戏物体
    public GameObject currentCharacterGo;
    private float initAngle = 90;
    private float offsetAngle = 0;
   // private IUISystem uiSystem;
    // Start is called before the first frame update
    private void Awake()
    {
        smrs = new SkinnedMeshRenderer[2];
        gos = new GameObject[2];
        for(int i = 0; i < gos.Length; i++)
        {
            gos[i] = transform.GetChild(i).gameObject;
            smrs[i] = gos[i].GetComponentInChildren<SkinnedMeshRenderer>();
        }
        //将函数注册到事件上，发生这个事件，调用相应函数
        this.RegistEvent<RotateCharacterModelEvent>(RotateCharacterModel);
     //   uiSystem = this.GetSystem<IUISystem>();
        SetMaterial();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetMaterial()
    {
       // uiSystem.ChoicePlayerData = new PlayerData();
        PlayerData pd = new PlayerData();
        for(int i = 0; i < gos.Length; i++)
        {
            gos[i].SetActive(false);
        }
        currentCharacterGo = gos[(int)pd.gender];//默认是男性
        currentCharacterGo.SetActive(true);
        Material m = GameResSystem.GetRes<Material>("Materials/" + pd.gender.ToString() + "/" + (int)pd.role);
        for (int i = 0; i < smrs[(int)pd.gender].sharedMaterials.Length; i++)
        {
            if(pd.gender == GENDER.MALE)
            {
                if (i != 1 && i != 2 && i != 10)
                {
                    smrs[(int)pd.gender].materials[i].CopyPropertiesFromMaterial(m);
                }
            }
            else
            {
                if (i != 0 && i != 2 && i != 10)
                {
                    smrs[(int)pd.gender].materials[i].CopyPropertiesFromMaterial(m);
                }
            }
        }
    }

    //旋转人物模型
    private void RotateCharacterModel(object rotateSrc)
    {
        if (!currentCharacterGo)
        {
            return;
        }
        RotateModelSrc rs = (RotateModelSrc) rotateSrc;
        float h = rs.h;
        float v = rs.v;
        if(v != 0)//前后
        {
            if (v > 0)//左上右上
            {
                offsetAngle = 45 * h;
            }
            else//左上右下
            {
                offsetAngle = -45 * h;
            }
        }
        else//只有左右
        {
            if (h != 0)
            {
                offsetAngle = 90 * h;
            }
            else
            {
                offsetAngle = 0;
            }
        }
        
        currentCharacterGo.transform.localEulerAngles = new Vector3(0, initAngle + offsetAngle,0);
        this.SendEvent<GetLookTargetIndexEvent>(offsetAngle);
    }
}
