using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
//using HardShellStudios.CompleteControl;

//public class GamepadManager : MonoBehaviour 
//{
//    private int GamepadSlots;
//    private GameManager GameManager;
//    //private int gamepads = 0;//debug var

//    public static GamepadManager Instance;
//    public int ActiveMenuGamepadID { get; private set; }

//    private void Awake()
//    {
//        if (Instance != null)
//        {
//            Destroy(gameObject);
//            return;
//        }
//        Instance = this;
//        DontDestroyOnLoad(this.gameObject);
//    }

//	private void Start() 
//	{
//        GameManager = GameManager.Instance;
//        GamepadSlots = ActiveGamepads().Count;
//    }

//    private void Update()
//    {
//        //if (Input.GetJoystickNames().Count() != gamepads)
//        //{
//        //    Debug.Log("Number of joystick slots: " + Input.GetJoystickNames().Count());
//        //    gamepads = Input.GetJoystickNames().Count();
//        //}
//        if (Input.anyKeyDown)
//        {
//            //Debug.Log(hInput.CurrentKeyDown());
//        }
//    }

//    //maps new gamepads connected in the lobby
//    public void UpdateLobbyGamepads()
//    {
//        if (ActiveGamepads().Count != GamepadSlots)
//        {
//            MapJoinButtons();
//            GamepadSlots = ActiveGamepads().Count;
//        }
//    }

//    //handles controller disconnection and reconnection during gameplay
//    public void UpdateGameplayGamepads()
//    {
//        foreach (var player in GameManager.MenuPlayers)
//        {
//            var unusedPads = ActiveGamepads().Where(a => !GameManager.MenuPlayers.Select(b => b.JoystickID).Contains(a)).ToList();

//            if (!ActiveGamepads().Contains(player.JoystickID) && unusedPads.Any())
//            {
//                var joyID = unusedPads.FirstOrDefault();
//                player.JoystickID = joyID;
//                MapGameplayButtons(player.PlayerID, joyID);
//            }
//        }
//    }

//    //maps the all the gamepads being used to control characters
//    public void MapAllGameplayControls()
//    {
//        foreach (var player in GameManager.MenuPlayers)
//        {
//            MapGameplayButtons(player.PlayerID, player.JoystickID);
//        }
//    }

//    //maps a gamepad with the gameplay controls according to what kind of controller it is
//    public void MapGameplayButtons(int pPlayerID, int pJoystickID)
//    {
//        if (Input.GetJoystickNames()[pJoystickID - 1].ToLower().Contains("xbox"))
//        {
//            hInput.SetKey("HookP" + pPlayerID, AxisCode.Axis10, (TargetController)Enum.Parse(typeof(TargetController), "Joystick" + pJoystickID));
//            hInput.SetKey("DashP" + pPlayerID, AxisCode.Axis9, (TargetController)Enum.Parse(typeof(TargetController), "Joystick" + pJoystickID));
//            hInput.SetKey("PauseP" + pPlayerID, (KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + pJoystickID + "Button" + 7));
//        }
//        else
//        {
//            hInput.SetKey("HookP" + pPlayerID, AxisCode.Axis5, (TargetController)Enum.Parse(typeof(TargetController), "Joystick" + pJoystickID));
//            hInput.SetKey("DashP" + pPlayerID, AxisCode.Axis4, (TargetController)Enum.Parse(typeof(TargetController), "Joystick" + pJoystickID));
//            hInput.SetKey("PauseP" + pPlayerID, (KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + pJoystickID + "Button" + 9));
//        }

//        hInput.SetKey("HorizontalP" + pPlayerID, AxisCode.Axis1, (TargetController)Enum.Parse(typeof(TargetController), "Joystick" + pJoystickID));
//        hInput.SetKey("VerticalP" + pPlayerID, AxisCode.Axis2, (TargetController)Enum.Parse(typeof(TargetController), "Joystick" + pJoystickID));
//    }

//    //returns a list of gamepads in use
//    public List<int> ActiveGamepads()
//    {
//        List<int> activeGamepads = new List<int>();
//        for (int i = 0; i < Input.GetJoystickNames().Length ; i++)
//        {
//            if (!String.IsNullOrWhiteSpace(Input.GetJoystickNames()[i]))
//            {
//                activeGamepads.Add(i +1);
//            }
//        }
//        return activeGamepads;
//    }

//    //maps join buttons for the lobby for all active gamepads
//    public void MapJoinButtons()
//    {
//        var activeGamepads = ActiveGamepads();
//        for (int i = 0; i < ActiveGamepads().Count; i++)
//        {

//            if (Input.GetJoystickNames()[activeGamepads[i] - 1].ToLower().Contains("xbox"))
//            {
//                hInput.SetKey("JoinC" + activeGamepads[i], (KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + activeGamepads[i] + "Button" + 0));
//            }
//            else
//            {
//                hInput.SetKey("JoinC" + activeGamepads[i], (KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + activeGamepads[i] + "Button" + 1));
//            }
//        }
//    }

//    //maps the lobby leave button for a joined player
//    public void MapLeaveButton(int pPlayerID, int pJoystickID)
//    {
//        hInput.SetKey("HorizontalP" + pPlayerID, AxisCode.Axis1, (TargetController)Enum.Parse(typeof(TargetController), "Joystick" + pJoystickID));


//        if (Input.GetJoystickNames()[pJoystickID - 1].ToLower().Contains("xbox"))
//        {
//            hInput.SetKey("LeaveP" + pPlayerID, (KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + pJoystickID + "Button1"));
//        }
//        else
//        {
//            hInput.SetKey("LeaveP" + pPlayerID, (KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + pJoystickID + "Button2"));
//        }
//    }

//    //unmaps the lobby leave button for a player
//    public void UnmapLeaveButton(int pPlayerID)
//    {
//        hInput.SetKey("LeaveP" + pPlayerID, KeyCode.None);
//    }

//    //unmaps lobby leave buttons for all players
//    public void ClearLeaveButtons()
//    {
//        for (int i = 0; i < 4; i++)
//        {
//            UnmapLeaveButton(i + 1);
//        }
//    }

//    //maps the menu inputs according to the type of the controling gamepad
//    public void MapMenuButtons()
//    {
//        if (ActiveGamepads().Any())
//        {
//            ActiveMenuGamepadID = ActiveGamepads()[0];

//            if (Input.GetJoystickNames()[ActiveMenuGamepadID - 1].ToLower().Contains("xbox"))
//            {
//                hInput.SetKey("SelectButton", (KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + ActiveMenuGamepadID + "Button0"));
//                hInput.SetKey("BackButton", (KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + ActiveMenuGamepadID + "Button1"));
//                hInput.SetKey("MenuVertControl", AxisCode.Axis2, (TargetController)Enum.Parse(typeof(TargetController), "Joystick" + ActiveMenuGamepadID));
//                hInput.SetKey("StartGame", (KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + ActiveMenuGamepadID + "Button0"));
//            }
//            else
//            {
//                hInput.SetKey("SelectButton", (KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + ActiveMenuGamepadID + "Button1"));
//                hInput.SetKey("BackButton", (KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + ActiveMenuGamepadID + "Button2"));
//                hInput.SetKey("MenuVertControl", AxisCode.Axis2, (TargetController)Enum.Parse(typeof(TargetController), "Joystick" + ActiveMenuGamepadID));
//                hInput.SetKey("StartGame", (KeyCode)Enum.Parse(typeof(KeyCode), "Joystick" + ActiveMenuGamepadID + "Button1"));
//            }
//        }
//    }

//    //handles disconnection and reconnection in the main menu
//    public void UpdateMenuGamepad()
//    {
//        if (ActiveGamepads().Any() && !ActiveGamepads().Contains(ActiveMenuGamepadID))
//        {
//            MapMenuButtons();
//        }
//    }
//}
