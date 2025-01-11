using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour,IController
{
    //unity自带的角色控制组件
    private CharacterController characterController;
    private Transform groundCheckPointTrans;
    public float rotateSpeed=120;
    public float gravity = 9.8f;
    public float verticalVelocity = 0;
    public float MaxJumpHeight = 1.7f;
    //移动
    private Vector3 motionVector;
    public float moveSpeed = 3;
    public float checkShpereRadius = 0.1f;
    public LayerMask groundLayer;
    // 跳跃
    public bool isGround = true;
    private bool isJumping = false;
    private InputController ic;
    private CharacterFSM characterFSM;
    // Start is called before the first frame update
    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        groundCheckPointTrans = transform.Find("groundCheckPointTrans");
        ic = gameObject.AddComponent<InputController>();
        ChoiceCharacterDress ccd = gameObject.AddComponent<ChoiceCharacterDress>();
        characterFSM = gameObject.AddComponent<CharacterFSM>();
        characterFSM.InitFSM(ccd.currentCharacterGo.GetComponent<Animator>(),ic);
        gameObject.AddComponent<PlayerHeadController>().InitPlayerHeadCtrl();
    }

    // Update is called once per frame
    void Update()
    {
        PlayerRotateViewControl();
        PlayerMoveAndJumpControl();
    }
    private void PlayerRotateViewControl()
    {
        //鼠标移动旋转人物视角
        transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime);
    }
    private void PlayerMoveAndJumpControl()
    {
        motionVector = Vector3.zero;
        //利用这个组件移动
        float h = ic.GetFloatInputValue(InputController.InputCode.HorizontalMoveValue);//A、D以及方向键左右箭头来触发,水平输入
        float v = ic.GetFloatInputValue(InputController.InputCode.VerticalMoveValue);//由W、S以及方向键上下箭头来触发！
        if (characterFSM.GetCurrentState() != CHARACTERSTATE.JUMP)
        {
            JudgeStateIdelOrMove(v, h);
        }
        this.SendEvent<RotateCharacterModelEvent>(new RotateModelSrc() { h = h, v = v });
        motionVector += transform.forward * moveSpeed * v * Time.deltaTime;
        motionVector += transform.right * moveSpeed * h * Time.deltaTime;
        //脚下小球检测是否与地接触，要记得给两个东西都附着碰撞体，物理西铜
        isGround =  Physics.CheckSphere(groundCheckPointTrans.position,checkShpereRadius,groundLayer);

        if (!isGround || isJumping)//正在跳跃或者高处落下
        {
 //最大速度被重力影响
            verticalVelocity -= gravity * Time.deltaTime;
            if(verticalVelocity < 0 && isJumping&&isGround)
            {
                isJumping = false;
                JudgeStateIdelOrMove(v, h);
            }
        }
        motionVector += Vector3.up * verticalVelocity * Time.deltaTime;
    /* */   if (ic.GetBoolInputValue(InputController.InputCode.JumpState)){
            if (isGround) { 
                verticalVelocity = Mathf.Sqrt(2 * gravity * MaxJumpHeight); 
                isJumping = true;
                characterFSM.ChangeState(CHARACTERSTATE.JUMP);
            }
            //初始最大速度
          
        }    
       
        //前后左右移动
        characterController.Move(motionVector); 
        if (isGround && !isJumping){
            verticalVelocity = 0;
            // 限制在地上的时候，位置
            JudgeStateIdelOrMove(v, h);
            transform.position = new Vector3(transform.position.x, -5.455f, transform.position.z);
        }

    }
    private void JudgeStateIdelOrMove(float v,float h)
    {
        if (v != 0 || h != 0)
        {
            characterFSM.ChangeState(CHARACTERSTATE.MOVE);
        }
        else
        {
            characterFSM.ChangeState(CHARACTERSTATE.IDLE);
        }
    }
    
}
