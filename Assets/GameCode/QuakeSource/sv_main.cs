using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static QUAKESOURCE.cl_main;

namespace QUAKESOURCE {

	public static class sv_main
	{
		public static server_t sv;
		public static server_static_t svs;

		public class server_static_t {

			//* NEEDED
			int			maxclients;
			int			maxclientslimit;
			//* NEEDED
			List<client_t>	s_clients;		// [maxclients] //* POINTER
			int			serverflags;		// episode completion information
			bool	changelevel_issued;	// cleared when at SV_SpawnServer

		}

		public class server_t {
		// 	qboolean	active;				// false if only a net client

		// 	qboolean	paused;
		// 	qboolean	loadgame;			// handle connections specially

		// 	double		time;
			
		// 	int			lastcheck;			// used by PF_checkclient
		// 	double		lastchecktime;
			
		// 	char		name[64];			// map name

		// 	char		modelname[64];		// maps/<name>.bsp, for model_precache[0]
		// 	struct model_s 	*worldmodel;
		// 	char		*model_precache[MAX_MODELS];	// NULL terminated
		// 	struct model_s	*models[MAX_MODELS];
		// 	char		*sound_precache[MAX_SOUNDS];	// NULL terminated

		// 	//* 	PF_lightstyles => sv.lightstyles[style] = val; 
		//* 	char		*lightstyles[MAX_LIGHTSTYLES]; //* MAX_LIGHTSTYLES = 64
		// 	int			num_edicts;
		// 	int			max_edicts;
		// 	edict_t		*edicts;			// can NOT be array indexed, because
		// 									// edict_t is variable sized, but can
		// 									// be used to reference the world ent
		//* 	server_state_t	state;			// some actions are only valid during load

		// 	sizebuf_t	datagram;
		// 	byte		datagram_buf[MAX_DATAGRAM];

		// 	sizebuf_t	reliable_datagram;	// copied to all clients at end of frame
		// 	byte		reliable_datagram_buf[MAX_DATAGRAM];

		// 	sizebuf_t	signon;
		// 	byte		signon_buf[8192];
		}


	//*? SV_RunClients
	//* Called from "Host_ServerFrame" in "host"
	/* 	void SV_RunClients (void)
	{
		int				i;
		
		for (i=0, host_client = svs.clients ; i<svs.maxclients ; i++, host_client++) //* For each client ...
		{
			if (!host_client->active)
				continue;
		
			sv_player = host_client->edict;		//* Pointer to the player edict_t struct (which stores ... lots of things)

			if (!SV_ReadClientMessage ())		//* Reads console type commands sent by the player through the buffer, and executes them. 
												//* Most importantly "prespawn", "spawn", "begin". Also movements
			{
				SV_DropClient (false);	// client misbehaved...
				continue;
			}

			if (!host_client->spawned)
			{
			// clear client movement until a new packet is received
				memset (&host_client->cmd, 0, sizeof(host_client->cmd));	//* Preallocate a memory buffed for movement commands
				continue;
			}

	// always pause in single player if in console or menus
			if (!sv.paused && (svs.maxclients > 1 || key_dest == key_game) )	
				SV_ClientThink (); //* See below
		} */

//*? SV_ClientThink
	/* void SV_ClientThink (void)
	{
		vec3_t		v_angle;

		if (sv_player->v.movetype == MOVETYPE_NONE) 	//* sv_player is an *edict_t defined in "sv_user"
			return;
		
		onground = (int)sv_player->v.flags & FL_ONGROUND;	//* Check flag against onground flag

		origin = sv_player->v.origin;
		velocity = sv_player->v.velocity;

		DropPunchAngle ();								//* Decrease punch angle (temporary angle due to damage or recoil) with time
		
	//
	// if dead, behave differently
	//
		if (sv_player->v.health <= 0)
			return;

	//
	// angles
	// show 1/3 the pitch angle and all the roll angle
		cmd = host_client->cmd;
		angles = sv_player->v.angles;
		
		VectorAdd (sv_player->v.v_angle, sv_player->v.punchangle, v_angle);
		angles[ROLL] = V_CalcRoll (sv_player->v.angles, sv_player->v.velocity)*4;
		if (!sv_player->v.fixangle)
		{
			angles[PITCH] = -v_angle[PITCH]/3;
			angles[YAW] = v_angle[YAW];
		}

		if ( (int)sv_player->v.flags & FL_WATERJUMP ) 	//* Jumping out of water is handled specially
		{
			SV_WaterJump ();
			return;
		}
	//
	// walk
	//
		if ( (sv_player->v.waterlevel >= 2)
		&& (sv_player->v.movetype != MOVETYPE_NOCLIP) )
		{
			SV_WaterMove ();
			return;
		}

		SV_AirMove ();	
	} */ //*? End of sv_user related functions








		// public class server_static_t {}


	// Takes you here: 
	//* DEFINE SERVER STRUCT = sv
	/* typedef struct //* server_t
	{

	} server_t;
	*/

	//* PF_LIGHTMAP CODE DUMP
	/* 	
		for (j=0, client = svs.clients ; j<svs.maxclients ; j++, client++)
			if (client->active || client->spawned)
			{ 
				MSG_WriteChar (&client->message, svc_lightstyle);
				MSG_WriteChar (&client->message,style);
				MSG_WriteString (&client->message, val);
			} 
	*/

	//* DEFINE CLIENT STRUCT FOR LIGHTMAP 
	/* typedef struct client_s
	{
		//* NEEDED
		qboolean		active;				// false = client is free
		//* NEEDED
		qboolean		spawned;			// false = don't send datagrams
		qboolean		dropasap;			// has been told to go to another level
		qboolean		privileged;			// can execute any host command
		qboolean		sendsignon;			// only valid before spawned

		double			last_message;		// reliable messages must be sent
											// periodically

		struct qsocket_s *netconnection;	// communications handle

		usercmd_t		cmd;				// movement
		vec3_t			wishdir;			// intended motion calced from cmd

		sizebuf_t		message;			// can be added to at any time,
											// copied and clear once per frame
		byte			msgbuf[MAX_MSGLEN];
		edict_t			*edict;				// EDICT_NUM(clientnum+1)
		char			name[32];			// for printing to other people
		int				colors;
			
		float			ping_times[NUM_PING_TIMES];
		int				num_pings;			// ping_times[num_pings%NUM_PING_TIMES]

	// spawn parms are carried from level to level
		float			spawn_parms[NUM_SPAWN_PARMS];

	// client known data for deltas	
		int				old_frags;
	} client_t;    
	*/    

	}
}