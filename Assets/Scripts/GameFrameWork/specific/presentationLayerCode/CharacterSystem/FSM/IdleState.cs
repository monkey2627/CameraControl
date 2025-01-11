using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : BaseState
{
    public IdleState(CharacterFSM characterFSM, Animator animator, CHARACTERSTATE cs) : base(characterFSM, animator, cs)
    {

    }

    public override void EnterState()
    {
        animator.SetFloat("MoveValue",0);
        this.SendEvent<StopLookingFunctionEvent>(new StopLookingSrc(){stop = true });
    }

    public override void ExitState()
    {
        animator.SetFloat("Yaw", 0);
    }

    public override void InitState()
    {
       
    }
    //animator如何控制，在代码里写逻辑
    public override void UpdateState()
    {
        float x = cfsm.GetFloatInputValue(InputController.InputCode.HorizontalRotateValue);
        if( x > 0)
        {
            animator.SetBool("RotateState", true);
            animator.SetFloat("Yaw",1);
        }else if(x < 0)
        {
            animator.SetBool("RotateState", true);
            animator.SetFloat("Yaw", -1);
        }
        else
        {
            animator.SetBool("RotateState", false);
            animator.SetFloat("Yaw", 0);
        }
    }


}
