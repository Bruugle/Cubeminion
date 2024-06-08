using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;

namespace Utilities.DeveloperConsole
{
    public enum Command
    {
        help,
        server_info,
        kill,
        start_game,
        end_game,
        set_herd_pop,
        spawn_item
    }

    public class ConsoleController : MonoBehaviour
    {
        public bool consoleAllowed = true;
        private bool consoleActive = false;
        public bool permissions = true;
        string textInput;
        Vector2 scrollPosition;

        private List<string> history;
        private Dictionary<string, Action<string[]>> commandDictionary;
        private Dictionary<string, string> helpDictionary;

        public static ConsoleController Singleton { get; private set; }

        private void Awake()
        {
            if (Singleton == null)
            {
                Singleton = this;
                Init();
            }
            else if (Singleton != this)
            {
                Destroy(gameObject);
            }
        }

        private void Init()
        {
            commandDictionary = new();
            history = new();
            DontDestroyOnLoad(gameObject);

            #region HELP

            StartListening(Command.help, OnHelp);

            helpDictionary = new Dictionary<string, string>(){
                { Command.server_info.ToString(), "display some information about the network" },
                { Command.kill.ToString(), "kills a player by name" }
            };

            #endregion

        }

        private void OnGUI()
        {
            if (!consoleActive || !consoleAllowed) { return; }

            GUI.Box(new Rect(50, Screen.height - 300, 500, 200), "");
            var viewPort = new Rect(0, 0, 480, 20 * history.Count + 180);
            scrollPosition = GUI.BeginScrollView(new Rect(50, Screen.height - 300, 500, 200), scrollPosition, viewPort);

            for (int i = 0; i < history.Count; i++)
            {
                GUI.Label(new Rect(10, viewPort.height - 20 * (i+1), viewPort.width, 20), history[history.Count-1-i]);
            }

            GUI.EndScrollView();

            GUILayout.BeginArea(new Rect(50, Screen.height - 100 , 500, 100));

            GUI.SetNextControlName("console");
            textInput = GUILayout.TextField(textInput);
            GUI.FocusControl("console");

            GUILayout.EndArea();
        }

        public void OnToggle(InputAction.CallbackContext context)
        {
            if(!context.performed && !consoleAllowed) { return; }
            consoleActive = !consoleActive;
            textInput = "";
        }

        public void OnSubmit(InputAction.CallbackContext context)
        {
            if(!consoleActive || !context.performed || textInput == "") { return; }
            ProcessInput(textInput);
            textInput = "";
            scrollPosition = new Vector2(0, history.Count * 20 + 180);
        }

        private void ProcessInput(string input)
        {
            string[] inputSplit = input.Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries);
            string commandName = inputSplit[0];
            string[] args = inputSplit.Skip(1).ToArray();

            Action<string[]> thisCommand = null;
            if (Singleton.commandDictionary.TryGetValue(commandName, out thisCommand))
            {
                history.Add(input);
                thisCommand?.Invoke(args);
            }
            else
            {
                history.Add($"unrecognized command \"{commandName}\"");
            }
        }

        public void AddLog(string message)
        {
            history.Add(message);
        }

        public void StartListening(Command command, Action<string[]> listener)
        {
            Action<string[]> thisCommand;
            if (Singleton.commandDictionary.TryGetValue(command.ToString(), out thisCommand))
            {
                thisCommand += listener;
                Singleton.commandDictionary[command.ToString()] = thisCommand;
            }
            else
            {
                thisCommand += listener;
                Singleton.commandDictionary.Add(command.ToString(), thisCommand);
            }
        }

        public void StopListening(Command command, Action<string[]> listener)
        {
            Action<string[]> thisCommand;
            if(Singleton.commandDictionary.TryGetValue(command.ToString(), out thisCommand))
            {
                thisCommand -= listener;
                Singleton.commandDictionary[command.ToString()] = thisCommand;
            }
        }

        private void OnHelp(string[] args)
        {

            if (args.Length > 0)
            {
                string msg;
                if(helpDictionary.TryGetValue(args[0], out msg))
                {
                    string[] content = { args[0], msg };
                    AddLog(string.Join(" ", content));
                }
            }
            else
            {
                AddLog("command list:");
                foreach(string key in helpDictionary.Keys)
                {
                    AddLog("     " + key);
                }
                AddLog("type \"help <command name>\" to receive more information");
            }
        }

    }
}


