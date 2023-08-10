using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class cl_main
{


//* Updated from "PF_lightstyles" in  pr_cmds via MSG

// #define	MAX_STYLESTRING	64 //* Should be in quakedef.h

/* typedef struct //* lightstyle_t
{
	int		length;
	char	map[MAX_STYLESTRING]; //* size 64 Maps a string to a light value in animation sequence
} lightstyle_t; */

/*     lightstyle_t	cl_lightstyle[MAX_LIGHTSTYLES]; //* 64
 */

//* Client static struct defined as cls in client.h
//* This is defined as global(extern) "cvs" in "client.h"
/* typedef struct //* client_static_t
{
	cactive_t	state;

// personalization data sent to server	
	char		mapstring[MAX_QPATH];
	char		spawnparms[MAX_MAPSTRING];	// to restart a level

// demo loop control
	int			demonum;		// -1 = don't play demos
	char		demos[MAX_DEMOS][MAX_DEMONAME];		// when not playing

// demo recording info must be here, because record is started before
// entering a map (and clearing client_state_t)
	qboolean	demorecording;
	qboolean	demoplayback;
	qboolean	timedemo;
	int			forcetrack;			// -1 = use normal cd track
	FILE		*demofile;
	int			td_lastframe;		// to meter out one message a frame
	int			td_startframe;		// host_framecount at start
	float		td_starttime;		// realtime at second frame of timedemo


// connection information
	int			signon;			// 0 to SIGNONS
	struct qsocket_s	*netcon;
	sizebuf_t	message;		// writing buffer to send to server
	
} client_static_t;
 */
}
