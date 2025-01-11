using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState : BaseState
{
    public MoveState(CharacterFSM characterFSM, Animator animator, CHARACTERSTATE cs) : base(characterFSM, animator, cs)
    {
    }

    public override void EnterState()
    {
        animator.SetBool("MoveState", true);
        //如果上一个状态就是idle，不需要延时开
        if(cfsm.GetLastState() == CHARACTERSTATE.IDLE)
        {
            this.SendEvent<StopLookingFunctionEvent>(new StopLookingSrc() { stop = false});
        }
        else
        {
        this.SendEvent<StopLookingFunctionEvent>(new StopLookingSrc() { stop = false ,delayTime = 0.3f});
        }

    }

    public override void ExitState()
    {
        animator.SetBool("MoveState", false);
    }

    public override void InitState()
    {
     
    }

    public override void UpdateState()
    {
        float h = cfsm.GetFloatInputValue(InputController.InputCode.HorizontalMoveValue);
        float v = cfsm.GetFloatInputValue(InputController.InputCode.VerticalMoveValue);
        if(v != 0)
        {
            animator.SetFloat("MoveValue", v);
        }
        else
        {
            if (h != 0)
            {
                animator.SetFloat("MoveValue", 1);
            }
        }
    }
}
