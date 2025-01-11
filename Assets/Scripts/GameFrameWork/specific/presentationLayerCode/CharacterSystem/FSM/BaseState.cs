using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseState:IController
{
    protected CharacterFSM cfsm;
    protected Animator animator;
    public CHARACTERSTATE stateType;
    // Start is called before the first frame update
    public BaseState(CharacterFSM characterFSM,Animator animator,CHARACTERSTATE cs)
    {
        cfsm = characterFSM;
        this.animator = animator;
        stateType = cs;
    }

    public abstract void InitState();
    public abstract void ExitState();
    public abstract void UpdateState();
    public abstract void EnterState();
}

public enum CHARACTERSTATE
{
    NONE,
    IDLE,
    MOVE,
    JUMP,
    ATTACK,
    HIT,
    DEAD
}
