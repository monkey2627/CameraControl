using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour,IController
{
    //unity�Դ��Ľ�ɫ�������
    private CharacterController characterController;
    private Transform groundCheckPointTrans;
    public float rotateSpeed=120;
    public float gravity = 9.8f;
    public float verticalVelocity = 0;
    public float MaxJumpHeight = 1.7f;
    //�ƶ�
    private Vector3 motionVector;
    public float moveSpeed = 3;
    public float checkShpereRadius = 0.1f;
    public LayerMask groundLayer;
    // ��Ծ
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
        //����ƶ���ת�����ӽ�
        transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime);
    }
    private void PlayerMoveAndJumpControl()
    {
        motionVector = Vector3.zero;
        //�����������ƶ�
        float h = ic.GetFloatInputValue(InputController.InputCode.HorizontalMoveValue);//A��D�Լ���������Ҽ�ͷ������,ˮƽ����
        float v = ic.GetFloatInputValue(InputController.InputCode.VerticalMoveValue);//��W��S�Լ���������¼�ͷ��������
        if (characterFSM.GetCurrentState() != CHARACTERSTATE.JUMP)
        {
            JudgeStateIdelOrMove(v, h);
        }
        this.SendEvent<RotateCharacterModelEvent>(new RotateModelSrc() { h = h, v = v });
        motionVector += transform.forward * moveSpeed * v * Time.deltaTime;
        motionVector += transform.right * moveSpeed * h * Time.deltaTime;
        //����С�����Ƿ���ؽӴ���Ҫ�ǵø�����������������ײ�壬������ͭ
        isGround =  Physics.CheckSphere(groundCheckPointTrans.position,checkShpereRadius,groundLayer);

        if (!isGround || isJumping)//������Ծ���߸ߴ�����
        {
 //����ٶȱ�����Ӱ��
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
            //��ʼ����ٶ�
          
        }    
       
        //ǰ�������ƶ�
        characterController.Move(motionVector); 
        if (isGround && !isJumping){
            verticalVelocity = 0;
            // �����ڵ��ϵ�ʱ��λ��
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
