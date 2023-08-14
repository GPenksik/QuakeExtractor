using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityQuake.Commands.cmd;
using Args = System.Collections.Generic.List<UnityQuake.Commands.cmd.cvar_t>;

namespace UnityQuake.Source
{
    public class host : MonoBehaviour
    {
        //* Initialise host with quakeparms_t


        /* void Host_Init (quakeparms_t *parms)
        {
            host_parms = *parms; //*

            com_argc = parms->argc;
            com_argv = parms->argv;

            Memory_Init (parms->membase, parms->memsize);
            Cbuf_Init ();
            Cmd_Init ();	
            V_Init ();  //* Init a lot of view parameters
            Chase_Init ();
            Host_InitVCR (parms);
            COM_Init (parms->basedir);  //* Initialise paths
            Host_InitLocal ();  //* Register commands
            W_LoadWadFile ("gfx.wad");  //* Process lumps and headers for main asset library
            Key_Init ();
            Con_Init ();    //* Console initialisation
            M_Init ();      //* Menu init
            PR_Init ();     //* Just registers some variables and commands
            Mod_Init ();    //* Just initialises the memory for the NOVIS MODEL
            NET_Init ();    //* Network stuff
            SV_Init ();     //* Actually just register more cvars


            R_InitTextures ();		//* Allocates checkered texture in all buffers

            if (cls.state != ca_dedicated)
            {
                host_basepal = (byte *)COM_LoadHunkFile ("gfx/palette.lmp");
                host_colormap = (byte *)COM_LoadHunkFile ("gfx/colormap.lmp");

                VID_Init (host_basepal);        //* Setup graphics size/palette/etc

                Draw_Init ();       //* Draws backtile
                SCR_Init ();        //* Tests some screen ... sets sce_initialised = true
                R_Init ();          //* r_main sets clipping planes => Init_particles and D_init


                CDAudio_Init ();
                Sbar_Init ();       //* Sets up status bar
                CL_Init ();         //* Sets up inputs, controls, commands that are in menu etc
                IN_Init ();         //* Input commands
            }

            Cbuf_InsertText ("exec quake.rc\n");

            Hunk_AllocName (0, "-HOST_HUNKLEVEL-");
            host_hunklevel = Hunk_LowMark ();       //* More memory initialisation

            host_initialized = true;                //* Goes back to "sys_win"

            Sys_Printf ("========Quake Initialized=========\n");	
        }
     */
        /*void Host_Frame (float time) {
         {
            double	time1, time2;
            static double	timetotal;
            static int		timecount;
            int		i, c, m;

            if (!serverprofile.value)       //* Always true in normal play
            {
                _Host_Frame (time);
                return;
            }
        } */


        //*! MAIN HOST LOOP
        /* void _Host_Frame (float time)
        {
            static double		time1 = 0;
            static double		time2 = 0;
            static double		time3 = 0;
            int			pass1, pass2, pass3;

            if (setjmp (host_abortserver) )
                return;			// something bad happened, or the server disconnected

        // keep the random time dependent
            rand ();

        // decide the simulation time
            if (!Host_FilterTime (time))
                return;			// don't run too fast, or packets will flood out

        // get new key events
            Sys_SendKeyEvents (); //* Processes and sends key events

        // allow mice or other external controllers to add commands
            IN_Commands (); //* Only for joysticks

        // process console commands
            Cbuf_Execute (); //* Processes incoming console commands and executes whats in them. Kinda the main thing here.

            NET_Poll();     //* Process internet cmds?

        // if running the server locally, make intentions now
            if (sv.active)              //* This is true on SP game
                CL_SendCmd ();          
            //* CL_BaseMove     //* Angles are changed directly by key state. Move directions are added to the cmd buffer. 
                    //* => CL_AdjustAngles - CL_KeyStates etc ... calculates how much movement needed to change "cl.viewangles" with limits etc
                    //* The built up "cmd" message is then sent to the server


        //-------------------
        //
        // server operations
        //
        //-------------------

        // check for commands typed to the host
            Host_GetConsoleCommands (); //* Checks for incomming cbuf commands, and add them to another command buffer (Dont execute yet?)

            if (sv.active)
                Host_ServerFrame (); //*! Next main loop. Still in "host"

        //-------------------
        //
        // client operations
        //
        //-------------------

        // if running the server remotely, send intentions now after
        // the incoming messages have been read
            if (!sv.active)
                CL_SendCmd ();

            host_time += host_frametime;

        // fetch results from server
            if (cls.state == ca_connected)
            {
                CL_ReadFromServer ();   //* After server has processed ... everything?
                                        //* Updates, for example, the client time, and player_state. Messages are processed immediately with switch statement. 
                                        //!*Main replication script, basically?
            }

        // update video
            SCR_UpdateScreen ();        //*! Main Render loop ... will check after.

            host_framecount++;
        } */


        //* Called from Host_Frame above. Method still in "host"
        /* void Host_ServerFrame (void)
        {
        // run the world state	
            pr_global_struct->frametime = host_frametime;

        // set the time and clear the general datagram
            SV_ClearDatagram (); //* Struct declared on server. Basically a buffer for client-server communication, to replicate game state. Has a reliable buffer, a normal buffer 
                                //* and "cmd" for movement commands.

        // check for new clients
            SV_CheckForNewClients ();   //* NET only

        // read client messages
            SV_RunClients ();       //* This is a loop run on "sv_user" for each client. Basically a player controller that calculates where they want to move.

        // move things around and think
        // always pause in single player if in console or menus
            if (!sv.paused && (svs.maxclients > 1 || key_dest == key_game) )
                SV_Physics ();

        // send all messages to the clients
            SV_SendClientMessages ();
        } */


    }

    public static class HOST_Cmd
    {

        public static void Host_Changelevel_f(Args args)
        {
            Args levelName;

            if (args.Count > 0)
            {
                Cbuf.Con_Print("changelevel <levelname> : continue game on a new level\n");
                return;
            }
            //* CHECK IF COMING FROM SERVER
            // TODO Implement this at some point
            // SV_SaveSpawnparms ();
            levelName = new Args() { args[0] };
            SV_Cmd.SV_SpawnServer(levelName);
        }

        // TODO THIS IS NOT FINISHED
        public static void Host_ClearMemory()
        {
            // Con_DPrintf ("Clearing memory\n");
            // D_FlushCaches ();
            // TODO Create new object for this. Clears all model data (i.e. wipe models)
            //* e.g.  UnityQuake.MapReader.MakeMesh.DestroyObjects();
            // Mod_ClearAll ();

            //* This is a hunk of the main bytearray that stores level data?
            // if (host_hunklevel)
                // Hunk_FreeToLowMark (host_hunklevel);

            cl_main.cls.signon = 0;
            
            // memset (&sv, 0, sizeof(sv));
            sv_main.sv = new();
            // memset (&cl, 0, sizeof(cl));
            cl_main.cl = new();
        }
    }
}