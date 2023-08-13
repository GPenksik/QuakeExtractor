using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityQuake.Utils.StringUtils;

namespace UnityQuake.Commands
{
    public delegate void xcommand_t(cmd.arg_t[] args);
    
    public class cmd : MonoBehaviour
    {
        public int a;
        public int b;

        [ContextMenuItem("Run command", "runCommand")]
        public cmd_function_t command;

        public void runCommand() {
            var args = new arg_t[2];
            args[0] = new arg_t<int>(a);
            args[1] = new arg_t<float>(b);
            Debug.Log("Method Direct");
            add(args);
            // xcommand_t func = testIntMethod;
            cmd_function_t testCmd = new("testFunction", add);
            Debug.Log("From xcommand execution");
            testCmd.executeFunction(args);
        }
        
        [ContextMenuItem("Add string to Cbuf", "AddStringCommandToBuffer")]
        public string commandString = "";


        #region TEST METHODS
    
            public void AddStringCommandToBuffer() {
                Cbuf.AddStringToCmdBuffer(commandString);
                commandString = "";
            }
    
            [ContextMenu("Initialize Cbuf")]
            public void InitializeCbuf() {
                Cbuf.Clear_cmd_functions();
                Cbuf.AddStdCommands();
                Cbuf.AddCommand("add", add, 2);
            }
    
            [ContextMenu("Execute Cbuf")]
            public void ExecuteCbufTest() 
            {
                Cbuf.Cbuf_Execute();
            }
    
            public void add(arg_t[] argsIn) {
    
                arg_t<int> a = argsIn[0] as arg_t<int>;
                arg_t<float> b = argsIn[1] as arg_t<float>;
    
                Cbuf.Con_Print((a.value + b.value).ToString());
            }
        #endregion

        #region CMD, CVAR AND ARG CLASSES
            [System.Serializable]
            public class cmd_function_t {
                public string name;
                public xcommand_t function;
                public int minArgs;
                public cmd_function_t(string fcnName, xcommand_t fcnIn, int FcnMinArgs = 0) 
                {
                    name = fcnName;
                    function = fcnIn;
                    minArgs = FcnMinArgs;
                }
                public void executeFunction(arg_t[] args) {
                    function(args);
                }
            }
            
            [System.Serializable]
            public class arg_t {
            }
    
            public class arg_t<T> : arg_t {
                public T value;
                public arg_t(T valueIn) 
                {
                    value = valueIn;
                }
            }
        
            public class cvar_s
            {
                char	name;
                char	valueS;
                // bool    archive;		// set to true to cause it to be saved to vars.rc
                // bool    server;		// notifies players when changed
                float	valueF;
            } //cvar_t;
        #endregion

        #region Cbuf
            public static class Cbuf {
                
                public enum cmd_source {
                    src_command,
                    clr_command
                }
    
                private static bool wait_cmd = false;
    
                public static List<(cmd_function_t, arg_t[], cmd_source)> Cbuf_cmd = new();
                public static Dictionary<string, cmd_function_t> cmd_functions = new();
                public static List<(string, cmd_source)> Cbuf_str = new();
    
                //* REGISTER COMMANDS
                public static void Clear_cmd_functions() {
                    cmd_functions.Clear();
                }
    
                public static void AddStdCommands() {
                    AddCommand("echo", Con_Printf);
                    // Add_Command("print", Con_Printf);
                }
    
                public static void AddCommand(string name, xcommand_t cmd, int cmdMinArgs = 0) {
                    //TODO Check if name clashes with an existing command, alias or cvar
                    string lName = name.ToLower();
                    cmd_functions.Add(lName, new cmd_function_t(lName, cmd, cmdMinArgs));
                }
    
                public static void Cbuf_Execute() {
                    cmd_function_t cmd;
                    arg_t[] args;
                    cmd_source src;
                    while (Cbuf_cmd.Count > 0) {
                        (cmd, args, src) = Cbuf_cmd[0];
                        if (args.Length < cmd.minArgs) {
                            Con_Print("CONSOLE: Not enough arguments for function");
                        } else {
                            cmd.executeFunction(args);
                        }
                        Cbuf_cmd.RemoveAt(0);
                    }
                }
    
    
                //* ADD TO STRING BUFFER
                public static void AddStringToCmdBuffer(string commandString, cmd_source src = cmd_source.clr_command) {
                    // If the whole string is empty, do nothing.
                    if (commandString == "") { return;}
                    
                    // Check if the line is actually several commands in one, delimited by ";" and process separately
                    string[] subCommandStrings = commandString.Split(";");
                    foreach (string subCommandString in subCommandStrings)
                {
                    if (subCommandString == "") { continue; }
                    // Delimit to tokens, using whitespace.
                    string[] tokens = subCommandString.Trim().Split((char[])null);

                    // Name should always be checked with lower case
                    string inputName = tokens[0].ToLower();

                    // Check if this matches a cmd
                    cmd_function_t cmd_function;
                    if (FindCommandByName(inputName, out cmd_function))
                    {
                        arg_t[] args = new arg_t[tokens.Length - 1];

                        for (int n_arg = 0; n_arg < tokens.Length - 1; n_arg++)
                        {
                            args[n_arg] = ParseArgType(tokens[n_arg + 1]); 
                        }

                        Cbuf_cmd.Add((cmd_function, args, src));
                        continue;
                    }
                    // TODO Implement aliases?
                    // Check if this matches a cvar
                    cvar_s cvar;
                    if (FindCvarByName(inputName, out cvar)) 
                    {
                        // Check cvars, whether to return or set
                        continue;
                    }

                    // If not found, send error message
                    cmd_function = cmd_functions["echo"];
                    arg_t[] echoArgs = new arg_t[1];
                    echoArgs[0] = new arg_t<string>(string.Format("Unknown command '{0}'", inputName));
                    Cbuf_cmd.Add((cmd_function, echoArgs, src));
                    continue;
                }
            }

            private static arg_t ParseArgType(string stringToParse)
            {
                float possibleFloat;
                if (ParseStringType(stringToParse, out possibleFloat) == StringType.aFloat) {
                    return new arg_t<float>(possibleFloat);
                } else {
                    return new arg_t<string>(stringToParse);
                }
            }

            private static bool FindCvarByName(string functionName, out cvar_s cvar)
            {
                cvar = null;
                return false;
            }

            private static bool FindCommandByName(string functionName, out cmd_function_t cmd)
            {
                if (cmd_functions.TryGetValue(functionName, out cmd)) {
                    return true;
                } else {
                    return false;
                }
            }
    
            #region STANDARD COMMANDS
                static void Con_Printf(arg_t[] args) {
                    string printString = "";
                    if (args.Length > 0) {
                        foreach (var arg in args) {
                            arg_t<string> stringArg = arg as arg_t<string>;
                            printString = string.Concat(printString, " ", stringArg.value);
                        }
                    }
                    Con_Print(printString);
                }
    
                public static void Con_Print(string stringToPrint) {
                    Debug.Log("<color=orange><b>CONSOLE:</b></color> " + stringToPrint.ToUpper());
                }
    
                // static void Cbuf_wait(arg_t[] _) {
                //     wait_cmd = true;
                // }
            #endregion
    
            }
        #endregion
        }
}
