using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TestCommand : ICommand { 
    // Start is called before the first frame update
  public void Execute(object obj)
    {
        Debug.Log("cess");
    }
}
