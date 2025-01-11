using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeepFindTransform 
{
    // Start is called before the first frame update
   
    public static Transform DeepFindChild(Transform root,string childName)
    {
        Transform result = null;
        result = root.Find(childName);
        if (!result)
        {
            foreach(Transform child in root)
            {
                result = DeepFindChild(child, childName);
                if(result!= null)
                {
                    return result;
                }
            }
        }
        return result;
    }
}
