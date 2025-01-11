using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject.Instantiate(GameResSystem.GetRes<GameObject>("Prefabs/Character/Player"));
        GameObject.Instantiate(GameResSystem.GetRes<GameObject>("Prefabs/Scene/SilvermoonCity"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
