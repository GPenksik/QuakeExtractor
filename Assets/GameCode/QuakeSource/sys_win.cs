using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sys_win : MonoBehaviour
{

//* Setup window, mem, parms etc
//! Main HOST loop

// int WINAPI WinMain (HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow)

/* // because sound is off until we become active
	Host_Init (&parms);     //* "host => Host_Init" Initialises the host

	oldtime = Sys_FloatTime ();
 */


/*  while (1)
	{
		else
		{
		// yield the CPU for a little while when paused, minimized, or not the focus
			if ((cl.paused && (!ActiveApp && !DDActive)) || Minimized || block_drawing)
			{
				SleepUntilInput (PAUSE_SLEEP);
				scr_skipupdate = 1;		// no point in bothering to draw
			}
			else if (!ActiveApp && !DDActive)
			{
				SleepUntilInput (NOT_FOCUS_SLEEP);
			}
            //* Define delta time (done automatically in Update())
			newtime = Sys_FloatTime ();
			time = newtime - oldtime;
		}
        //* Runs a frame of Host_Frame (Could just be Update() in Host?)
		Host_Frame (time);  //* "host => Host_Frame"
		oldtime = newtime;
	} */

}

//TODO 
/* 
TODO Create variables. Originally a linked list? Probably just a list of structs. 
TODO PROGS.DAT interpreter. Project in itself, but maybe a simple one? Not sure. Mostly contained in "pr_edicts.c". Start with Parser for initial data. 
TODO Entities parser for the BSP entities string. 
TODO Full player controller / "sv_user" implementation. Ignore client side updates? Just assume we're rendering the server version of the game? 
TODO Make list of currently needed global variables, and decide where to store. 
TODO Decide on actual architecture ... Main classes below seem like a decent start. 
TODO "host", "sv_main", "cl_main", "sv_user" basic implementations. Also "sv_phys"?

*/
