using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterFSM : MonoBehaviour
{
    private Dictionary<CHARACTERSTATE, BaseState> stateDict;
    private BaseState currentState;
    private BaseState lastState;
    private InputController ic;

    //初始化状态机
    public void InitFSM(Animator currentAnimator,InputController inputController)
    {
        stateDict = new Dictionary<CHARACTERSTATE, BaseState>()
        {
            { CHARACTERSTATE.IDLE,new IdleState(this,currentAnimator,CHARACTERSTATE.IDLE)},
              { CHARACTERSTATE.MOVE,new MoveState(this,currentAnimator,CHARACTERSTATE.MOVE)},
                { CHARACTERSTATE.JUMP,new JumpState(this,currentAnimator,CHARACTERSTATE.JUMP)},
        };//初始化的时候每个新建一个实例并管理，常用的编程控制方法
        ic = inputController;
        SetDefaultState();
    }
    public CHARACTERSTATE GetCurrentState()
    {
        return currentState.stateType;
    }
    public CHARACTERSTATE GetLastState()
    {
        return lastState.stateType;
    }
    private void SetDefaultState()
    {
        foreach (var i in stateDict){
            i.Value.InitState();
        }
        currentState = stateDict[CHARACTERSTATE.IDLE];
        currentState.EnterState();
    }
    public void ChangeState(CHARACTERSTATE newStateType)
    {
        if (stateDict.ContainsKey(newStateType))
        {
            BaseState changeState = stateDict[newStateType];
            if(changeState != currentState)
            {
                currentState.ExitState();
                lastState = currentState;
                currentState = changeState;
                currentState.EnterState();
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(currentState != null){
            currentState.UpdateState();
        }
    }
    public void GetInputValue()
    {

    }
    public void SetInputValue(string inputCode, bool inputValue)
    {
        ic.SetInputValue(inputCode, inputValue);
    }
    //获取当前某个输入的状态
    public bool GetBoolInputValue(string inputCode)
    {
        return ic.GetBoolInputValue(inputCode);
    }
    public void SetInputValue(string inputCode, float inputValue)
    {
        ic.SetInputValue(inputCode, inputValue);
    }
    //获取当前某个输入的状态
    public float GetFloatInputValue(string inputCode)
    {
       return  ic.GetFloatInputValue(inputCode);
    }
   
}
