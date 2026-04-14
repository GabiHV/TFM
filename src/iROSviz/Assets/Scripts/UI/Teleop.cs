using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

using RosMessageTypes.Geometry;

using App.ROSUtilities;
using App.Utilities;
using App.Exceptions;

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Numerics;

public class Teleop : MonoBehaviour
{
    private Dictionary<string, List<string>> _topicsDict;
    private readonly float _defaultForwardSpeed = 1.0f;
    private readonly float _defaultVerticalSpeed = 1.0f;
    private readonly float _defaultYawSpeed = 0.50f;

    private bool pressedForward = false;
    private bool pressedBackward = false;
    private bool pressedLeft = false;
    private bool pressedRight = false;
    private bool pressedUp = false;
    private bool pressedDown = false;

    private UnityEngine.Vector2 lastVector = new();

    public TMP_Dropdown topicDropdown;
    public TMP_InputField forwardText;
    public TMP_InputField verticalText;
    public TMP_InputField yawText;
    public InputActionAsset inputActions;

    public Button forwardButton;
    public Button backButton;
    public Button leftButton;
    public Button rightButton;
    public Button upButton;
    public Button downButton;
    private Color colorNormal;
    public Color colorPressed;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InvokeRepeating(nameof(GetTopics), 0.0f, 1.0f);
        SetInitialSpeedValues();
        SetInitialButtonColors();
        EnableJoystick();
    }

    void Update()
    {
        CheckJoystickMove();
    }

    public void CheckJoystickMove()
    {
        UnityEngine.Vector2 vector = inputActions.
            FindAction("Robot/Move").
            ReadValue<UnityEngine.Vector2>();
        
        if(lastVector.x == vector.x && lastVector.y == vector.y) return;
        lastVector = vector;

        pressedLeft = vector.x > 0.9f;
        pressedRight = vector.x < -0.9f;
        pressedForward = vector.y > 0.9f;
        pressedBackward = vector.y < -0.9f;
        bool stop = vector.x == 0 && vector.y == 0;

        if(pressedLeft || pressedRight || pressedForward || pressedBackward || stop)    
            MakeMovement();
    }

    private void GetTopics()
    {
        _topicsDict = ROSTopicListService.GetTopicsWithTypes();

        AddTopicsToDropdown();
    }

    public void SetInitialSpeedValues()
    {
        forwardText.text = _defaultForwardSpeed.ToString();
        verticalText.text = _defaultVerticalSpeed.ToString();
        yawText.text = _defaultYawSpeed.ToString();
    }

    private void SetInitialButtonColors() =>
        colorNormal = forwardButton.GetComponent<Image>().color;

    private void EnableJoystick() =>
        inputActions.FindActionMap("Robot").Enable();
 
    private void AddTopicsToDropdown() => 
        DropdownHelper.ClearDropdownAndSetOption(
            topicDropdown, 
            new List<string>(_topicsDict.Keys),
            GetSelectedTopic
        );

    public void CheckForward() =>
        BoundariesHelper.CheckUFloatInputTextBoundaries(
            forwardText, 
            _defaultForwardSpeed,
            GetForward
        );


    public void CheckVertical() =>
        BoundariesHelper.CheckUFloatInputTextBoundaries(
            verticalText,
            _defaultVerticalSpeed,
            GetVertical
        );

    public void CheckYaw() =>
        BoundariesHelper.CheckUFloatInputTextBoundaries(
            yawText,
            _defaultYawSpeed,
            GetYaw
        );
    
    private string GetSelectedTopic() =>
        DropdownHelper.GetDropdownSelectedText(topicDropdown);

    private int GetSelectedTopicValue() =>
        DropdownHelper.GetDropdownSelectedValue(topicDropdown);
    
    private float GetForward() =>
        CastHelper.CastStringToFloat(GetForwardText());

    private float GetVertical() =>
        CastHelper.CastStringToFloat(GetVerticalText());

    private float GetYaw() => 
        CastHelper.CastStringToFloat(GetYawText());

    private string GetForwardText() => 
        forwardText.text;

    private string GetVerticalText() =>
        verticalText.text;

    private string GetYawText() =>
        yawText.text;

    public void MakeMovement()
    {
        TwistMsg movement = new();
        float forward = GetForward();
        float vertical = GetVertical();
        float yaw = GetYaw();

        movement.linear.x = pressedForward ? forward : pressedBackward ? -forward : 0;
        movement.linear.z = pressedUp ? vertical : pressedDown ? -vertical : 0;
        movement.angular.z = pressedLeft ? yaw : pressedRight ? -yaw : 0;

        ROSPublisherHelper.PublishMessage(GetSelectedTopic(), movement);
    }

    public void SwitchForwardState()
    {
        pressedForward = SwitchButtonState(pressedForward, forwardButton);
        if (pressedBackward) pressedBackward = SwitchButtonState(pressedBackward, backButton);
    }

    public void SwitchBackwardState() 
    {
        pressedBackward = SwitchButtonState(pressedBackward, backButton);
        if (pressedForward) pressedForward = SwitchButtonState(pressedForward, forwardButton);
    }
    
    public void SwitchLeftState()
    {
        pressedLeft = SwitchButtonState(pressedLeft, leftButton);
        if (pressedRight) pressedRight = SwitchButtonState(pressedRight, rightButton);        
    }
    
    public void SwitchRightState()
    {
        pressedRight = SwitchButtonState(pressedRight, rightButton);
        if (pressedLeft) pressedLeft = SwitchButtonState(pressedLeft, leftButton);        
    }

    public void SwitchUpState()
    {
        pressedUp = SwitchButtonState(pressedUp,  upButton);
        if (pressedDown) pressedDown = SwitchButtonState(pressedDown,  downButton);
    }

    public void SwitchDownState()
    {
        pressedDown = SwitchButtonState(pressedDown,  downButton);
        if (pressedUp) pressedUp = SwitchButtonState(pressedUp,  upButton);
    }

    public void StopMovement()
    {
        pressedForward = SwitchButtonState(true, forwardButton);
        pressedBackward = SwitchButtonState(true, backButton);
        pressedLeft = SwitchButtonState(true, leftButton);
        pressedRight = SwitchButtonState(true, rightButton);
        pressedUp = SwitchButtonState(true,  upButton);
        pressedDown = SwitchButtonState(true,  downButton);
    }
    
    private bool SwitchButtonState(bool flag, Button button)
    {
        flag =! flag;
        button.GetComponent<Image>().color = !flag ? colorNormal : colorPressed;
        return flag;
    }
}
