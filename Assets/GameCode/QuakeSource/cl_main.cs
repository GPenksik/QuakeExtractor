using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace QUAKESOURCE
{
	public static class cl_main
	{

	public class client_t
	{
	//* 	qboolean		active;				// false = client is free
	//* 	qboolean		spawned;			// false = don't send datagrams
	// 	qboolean		dropasap;			// has been told to go to another level
	// 	qboolean		privileged;			// can execute any host command
	// 	qboolean		sendsignon;			// only valid before spawned

	// 	double			last_message;		// reliable messages must be sent
	// 										// periodically

	// 	struct qsocket_s *netconnection;	// communications handle

	// 	usercmd_t		cmd;				// movement
	// 	vec3_t			wishdir;			// intended motion calced from cmd

	// 	sizebuf_t		message;			// can be added to at any time,
	// 										// copied and clear once per frame
	// 	byte			msgbuf[MAX_MSGLEN];
	// 	edict_t			*edict;				// EDICT_NUM(clientnum+1)
	// 	char			name[32];			// for printing to other people
	// 	int				colors;
			
	// 	float			ping_times[NUM_PING_TIMES];
	// 	int				num_pings;			// ping_times[num_pings%NUM_PING_TIMES]

	// // spawn parms are carried from level to level
	// 	float			spawn_parms[NUM_SPAWN_PARMS];

	// // client known data for deltas	
	// 	int				old_frags;
	}


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
}