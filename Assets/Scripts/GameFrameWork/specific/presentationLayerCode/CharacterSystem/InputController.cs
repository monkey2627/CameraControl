using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//���������
public class InputController : MonoBehaviour
{
    private Dictionary<string, bool> inputBoolValueDict;
    private Dictionary<string, float> inputFloatValueDict;
    // Start is called before the first frame update
    void Awake()
    {
        inputBoolValueDict = new Dictionary<string, bool>() {
            { InputCode.JumpState,false}
        };
        inputFloatValueDict = new Dictionary<string, float>()
        {
            { InputCode.HorizontalMoveValue,0},
            { InputCode.VerticalMoveValue,0},
            { InputCode.HorizontalRotateValue, 0}

        };
    }

    // Update is called once per frame
    void Update()
    {
        //��ȡϵͳȻ��ʵʱ���µ��ֵ���
        SetInputValue(InputCode.HorizontalMoveValue,Input.GetAxisRaw("Horizontal"));
        SetInputValue(InputCode.VerticalMoveValue, Input.GetAxisRaw("Vertical"));
        SetInputValue(InputCode.HorizontalRotateValue, Input.GetButtonDown("Mouse X"));
        SetInputValue(InputCode.JumpState, Input.GetButtonDown("Jump"));
    }
    public void SetInputValue(string inputCode,bool inputValue)
    {
        if (inputBoolValueDict.ContainsKey(inputCode)){
            inputBoolValueDict[inputCode] = inputValue;
        }
        else
        {

        }
    }
    //��ȡ��ǰĳ�������״̬
    public bool GetBoolInputValue(string inputCode)
    {
        if (inputBoolValueDict.ContainsKey(inputCode))
        {
            return inputBoolValueDict[inputCode];
        }
        return false;
    }
    public void SetInputValue(string inputCode, float inputValue)
    {
        if (inputFloatValueDict.ContainsKey(inputCode))
        {
            inputFloatValueDict[inputCode] = inputValue;
        }
        else
        {

        }
    }
    //��ȡ��ǰĳ�������״̬
    public float GetFloatInputValue(string inputCode)
    {
        if (inputFloatValueDict.ContainsKey(inputCode))
        {
            return inputFloatValueDict[inputCode];
        }
        return 0;
    }
    public static class InputCode
    {
        public const string HorizontalMoveValue = "HorizontalMoveValue";
        public const string VerticalMoveValue = "VerticalMoveValue";
        public const string MoveRotateState = "MoveRotateState";
        public const string HorizontalRotateValue = "HorizontalRotateValue";

        //
        public const string JumpState = "JumpState";
        public const string EquipState = "EquipState";
        public const string AttackState = "AttackState";
        public static string[] skillsState = new string[] { "SkillState0", "SkillState1", "SkillState2", "SkillState3", "SkillState4", "SkillState5", "SkillState" };
    }
}
