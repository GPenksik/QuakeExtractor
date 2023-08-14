using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityQuake.Commands.cmd;
using static UnityQuake.Utils.StringUtils;
using Args = System.Collections.Generic.List<UnityQuake.Commands.cmd.cvar_t>;

namespace UnityQuake.Commands
{
    public delegate void xcommand_t(Args args);

    public class cmd : MonoBehaviour
    {
        public float a;
        public float b;

        [ContextMenuItem("Run command", "runCommand")]
        public cmd_function_t command;

        public void runCommand()
        {
            var args = new Args {
                cvar_t.GetNew(a),
                cvar_t.GetNew(b)  };
            cmd_function_t testCmd = new("testFunction", add);
            Debug.Log("From xcommand execution");
            testCmd.executeFunction(args);
        }

        [ContextMenuItem("Add string to Cbuf", "AddStringCommandToBuffer")]
        public string commandString = "";

        #region TEST METHODS
        public void AddStringCommandToBuffer()
        {
            Cbuf.Cbuf_AddText(commandString);
            Cbuf.Cbuf_Execute();
            commandString = "";
        }

        [ContextMenu("Initialize Cbuf")]
        public void InitializeCbuf()
        {
            Cbuf.Cmd_Clear();
            Cbuf.Cmd_AddStandard();
            Cbuf.Cmd_AddCommand("add", add, 2);
        }

        [ContextMenu("Execute Cbuf")]
        public void ExecuteCbufTest() => Cbuf.Cbuf_Execute();

        public void add(Args argsIn)
        {
            float a = argsIn[0].f;
            float b = argsIn[1].f;

            Cbuf.Con_Print((a + b).ToString());
        }
        #endregion TEST METHODS

        #region CMD, CVAR AND ARG CLASSES

        // COMMAND TYPE
        [System.Serializable]
        public class cmd_function_t
        {
            public string name;
            public xcommand_t function;
            public int minArgs;
            public cmd_function_t(string fcnName, xcommand_t fcnIn, int FcnMinArgs = 0)
            {
                name = fcnName;
                function = fcnIn;
                minArgs = FcnMinArgs;
            }
            public void executeFunction(Args args) => function(args);
        }


        // ARG BASE
        [System.Serializable]
        public abstract class cvar_t
        {
            public string name = "";
            // Get this cvar_t float value (if applicable ... returns 0.0f if not)
            public float f { get => GetValueAsFloat(); }
            // Get this cvar_t string value. If this is a cvar_t_f, then value is returned as value.ToString()
            public string s { get => GetValueAsString(); }
            public abstract string GetValueAsString();
            public abstract float GetValueAsFloat();
            public abstract void Set(cvar_t cvarIn);
            public static cvar_t_f GetNew(float value) { return new cvar_t_f(value); }
            public static cvar_t_f GetNew(string name, float value) { return new cvar_t_f(name, value); }
            public static cvar_t_s GetNew(string value) { return new cvar_t_s(value); }
            public static cvar_t_s GetNew(string name, string value) { return new cvar_t_s(name, value); }
        }

        // ARG FLOAT
        public class cvar_t_f : cvar_t
        {
            public float value;
            public cvar_t_f(float valueIn) { value = valueIn; }
            public cvar_t_f(string nameIn, float valueIn) { value = valueIn; name = nameIn; }

            public override float GetValueAsFloat() => value;

            public override string GetValueAsString() => value.ToString();

            public override void Set(cvar_t cvarIn) => value = cvarIn.f;
        }

        // ARG STRING
        public class cvar_t_s : cvar_t
        {
            public string value;
            public cvar_t_s(string valueIn) { value = valueIn; }
            public cvar_t_s(string nameIn, string valueIn) { value = valueIn; name = nameIn; }

            public override float GetValueAsFloat()
            {
                return 0.0f;
            }

            public override string GetValueAsString()
            {
                return value;
            }

            public override void Set(cvar_t cvarIn)
            {
                value = cvarIn.s;
            }
        }
        #endregion CMD, CVAR AND ARGS DEFS

        #region Cbuf
        public static class Cbuf
        {
            public enum cmd_source
            {
                src_command,
                clr_command
            }

            private static bool wait_cmd = false;

            public static List<(cmd_function_t, Args, cmd_source)> Cbuf_cmd = new();
            public static Dictionary<string, cmd_function_t> cmd_functions = new();
            public static Dictionary<string, cvar_t> cvar_vars = new();
            public static List<(string, cmd_source)> Cbuf_str = new();

            //* MANAGE COMMANDS
            public static void Cmd_Clear()
            {
                cmd_functions.Clear();
                Cbuf_cmd.Clear();
                cvar_vars.Clear();
                Cbuf_str.Clear();
            }

            public static void Cmd_AddStandard()
            {
                Cmd_AddCommand("echo", Con_Printf);
                Cmd_AddCommand("cvar_command", Cvar_Command);
                Cvar_RegisterVariable("s_test", "initial");
                Cvar_RegisterVariable("f_test", 0.0f);
            }

            public static void Cmd_AddCommand(string name, xcommand_t cmd, int cmdMinArgs = 0)
            {
                if (cmd_functions.ContainsKey(name))
                {
                    Con_Print(string.Format("{0} already exists as a command", name));
                    return;
                }
                else if (cvar_vars.ContainsKey(name))
                {
                    Con_Print(string.Format("{0} already exists as a variable", name));
                    return;
                }
                string lName = name.ToLower();
                cmd_functions.Add(lName, new cmd_function_t(lName, cmd, cmdMinArgs));
            }

            public static void Cvar_RegisterVariable(string name, string value) // STRING CVAR
                => Cvar_RegisterOfType(name, cvar_t.GetNew(name, value));
            public static void Cvar_RegisterVariable(string name, float value) // FLOAT CVAR
                => Cvar_RegisterOfType(name, cvar_t.GetNew(name, value));
            public static void Cvar_RegisterVariable(cvar_t cvar) => Cvar_RegisterOfType(cvar.name, cvar);
            private static void Cvar_RegisterOfType(string name, cvar_t cvar)
            {
                if (Cvar_FindVar(name, out _))
                {
                    Con_Print(string.Format("{0} already exists as a variable", name));
                }
                else if (Cmd_FindCommand(name, out _))
                {
                    Con_Print(string.Format("{0} already exists as a command", name));
                }
                else
                {
                    cvar_vars.Add(name, cvar);
                }
            }

            private static bool CheckIfNameValid(string name)
            {
                if (cmd_functions.ContainsKey(name))
                {
                    Con_Print(string.Format("{0} already exists as a command", name));
                    return false;
                }
                else if (cvar_vars.ContainsKey(name))
                {
                    Con_Print(string.Format("{0} already exists as a variable", name));
                    return false;
                }
                else
                {
                    return true;
                }
            }

            public static void Cbuf_Execute()
            {
                cmd_function_t cmd;
                Args args;
                cmd_source src;
                while (Cbuf_cmd.Count > 0)
                {
                    (cmd, args, src) = Cbuf_cmd[0];
                    if (args.Count < cmd.minArgs)
                    {
                        Con_Print("CONSOLE: Not enough arguments for function");
                    }
                    else
                    {
                        cmd.executeFunction(args);
                    }
                    Cbuf_cmd.RemoveAt(0);
                }
            }

            //* Execute a string immediately.
            // TODO Remove repeated code between this and Add To Buffer, below 
            public static void Cmd_ExecuteString(string commandString, cmd_source src) {
                // If the whole string is empty, do nothing.
                if (commandString == "") { return; }

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
                    if (Cmd_FindCommand(inputName, out cmd_function))
                    {
                        Args args = new(); // cvar_t[tokens.Length - 1];

                        for (int n_arg = 0; n_arg < tokens.Length - 1; n_arg++)
                        {
                            args.Add(ParseStringToCvar(tokens[n_arg + 1]));
                        }
                        
                        cmd_function.executeFunction(args);
                        // Cbuf_cmd.Add((cmd_function, args, src));
                        continue;
                    }

                    // Check if this matches a cvar
                    cvar_t cvar;
                    if (Cvar_FindVar(inputName, out cvar))
                    {
                        cmd_function_t cvar_command;
                        Cmd_FindCommand("cvar_command", out cvar_command);
                        Args args = new() { cvar };
                        if (tokens.Length > 1)
                        {
                            args.Add(ParseStringToCvar(tokens[1]));
                        }
                        cvar_command.executeFunction(args);
                        continue;
                    }

                    // If not found, send error message
                    cmd_function = cmd_functions["echo"];
                    Args echoArgs = new()
                    {
                        cvar_t.GetNew(string.Format("Unknown command '{0}'", inputName))
                    };
                    cmd_function.executeFunction(echoArgs);
                    continue;
                }
            }

            //* ADD TO STRING BUFFER
            public static void Cbuf_AddText(string commandString, cmd_source src = cmd_source.clr_command)
            {
                // If the whole string is empty, do nothing.
                if (commandString == "") { return; }

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
                    if (Cmd_FindCommand(inputName, out cmd_function))
                    {
                        Args args = new(); // cvar_t[tokens.Length - 1];

                        for (int n_arg = 0; n_arg < tokens.Length - 1; n_arg++)
                        {
                            args.Add(ParseStringToCvar(tokens[n_arg + 1]));
                        }

                        Cbuf_cmd.Add((cmd_function, args, src));
                        continue;
                    }

                    // Check if this matches a cvar
                    cvar_t cvar;
                    if (Cvar_FindVar(inputName, out cvar))
                    {
                        cmd_function_t cvar_command;
                        Cmd_FindCommand("cvar_command", out cvar_command);
                        Args args = new() { cvar };
                        if (tokens.Length > 1)
                        {
                            args.Add(ParseStringToCvar(tokens[1]));
                        }
                        Cbuf_cmd.Add((cvar_command, args, src));
                        continue;
                    }

                    // If not found, send error message
                    cmd_function = cmd_functions["echo"];
                    Args echoArgs = new()
                    {
                        cvar_t.GetNew(string.Format("Unknown command '{0}'", inputName))
                    };
                    Cbuf_cmd.Add((cmd_function, echoArgs, src));
                    continue;
                }
            }

            public static void Cvar_Command(Args args)
            {
                if (args.Count == 1)
                {
                    string cvar_name = args[0].name;
                    string cvar_value = args[0].s;
                    Con_Print(string.Format("{0} is {1}", cvar_name, cvar_value));
                }
                else
                {
                    Cvar_Set(args[0], args[1]);
                }
            }

            // CVAR_SET
            public static void Cvar_Set(string name, string value)
            {
                cvar_t cvarToSet;
                if (Cvar_FindVar(name, out cvarToSet))
                {
                    Cvar_Set(cvarToSet, cvar_t.GetNew(value));
                }
            }
            public static void Cvar_Set(string name, float value)
            {
                cvar_t cvarToSet;
                if (Cvar_FindVar(name, out cvarToSet))
                {
                    Cvar_Set(cvarToSet, cvar_t.GetNew(value));
                }
            }
            public static void Cvar_Set(cvar_t cvarToSet, cvar_t cvarSource)
            {
                cvarToSet.Set(cvarSource);
            }

            // PARSE TYPE OF VALUE FROM STRING
            private static cvar_t ParseStringToCvar(string stringToParse)
            {
                float possibleFloat;
                return ParseStringType(stringToParse, out possibleFloat) == StringType.aFloat
                    ? cvar_t.GetNew(possibleFloat)
                    : cvar_t.GetNew(stringToParse);
            }

            private static bool Cvar_FindVar(string cvar_name, out cvar_t cvar) => cvar_vars.TryGetValue(cvar_name, out cvar);

            private static bool Cmd_FindCommand(string functionName, out cmd_function_t cmd) => cmd_functions.TryGetValue(functionName, out cmd);

            #region STANDARD COMMANDS
            static void Con_Printf(Args args)
            {
                string printString = "";
                if (args.Count > 0)
                {
                    foreach (var arg in args)
                    {
                        printString = string.Concat(printString, " ", arg.s);
                    }
                }
                Con_Print(printString);
            }

            public static void Con_Print(string stringToPrint) => Debug.Log("<color=orange><b>CONSOLE:</b></color> " + stringToPrint.ToUpper());
            #endregion STANDARD COMMANDS

        }
        #endregion Cbuf
    }
}
