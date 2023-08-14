using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityQuake.Progs.Progs;
using static UnityQuake.Progs.ProgDefs;
using static UnityQuake.Commands.cmd;
using static UnityQuake.Source.HOST_Cmd;
using static UnityQuake.Source.cl_main;
using static UnityQuake.Source.sv_main;

using Args = System.Collections.Generic.List<UnityQuake.Commands.cmd.cvar_t>;
using static UnityQuake.Common.MSG;

namespace UnityQuake.Source
{

	public static class sv_main
	{
        public static int MAX_MODELS = 256; //* Was in quakedefs
		
		public static server_t sv;
		public static server_static_t svs;

		//* REGEX \{"(\w{1,})",\s{0,}"(\d{0,}\.{0,}\d{0,})".{0,} => cvar_t.GetNew("$1", $2f);
		//* Moved from pr_cmds
		public static cvar_t sv_aim  = cvar_t.GetNew("sv_aim", 0.93f);

		//* Moved from SV_user
		public static cvar_t sv_maxspeed = cvar_t.GetNew("sv_maxspeed", 320f);
		public static cvar_t sv_accelerate = cvar_t.GetNew("sv_accelerate", 10f);
		public static cvar_t sv_edgefriction = cvar_t.GetNew("edgefriction", 2f);
		public static cvar_t sv_idealpitchscale = cvar_t.GetNew("sv_idealpitchscale", 0.8f);

		//* MOVED FROM SV_PHYS
		public static cvar_t sv_friction = cvar_t.GetNew("sv_friction", 4f);
		public static cvar_t sv_stopspeed = cvar_t.GetNew("sv_stopspeed",100f);
		public static cvar_t sv_gravity = cvar_t.GetNew("sv_gravity", 800f);
		public static cvar_t sv_maxvelocity = cvar_t.GetNew("sv_maxvelocity",2000f);
		public static cvar_t sv_nostep = cvar_t.GetNew("sv_nostep",0f);



        public class server_static_t
		{

			//* NEEDED
			public int maxclients;
			public int maxclientslimit;
			//* NEEDED
			public List<client_t> s_clients;       // [maxclients] //* POINTER
			public int serverflags;        // episode completion information
			public bool changelevel_issued;    // cleared when at SV_SpawnServer

		}

		public class server_t
		{
			// 	qboolean	active;				// false if only a net client

			// 	qboolean	paused;
			// 	qboolean	loadgame;			// handle connections specially

			// 	double		time;

			// 	int			lastcheck;			// used by PF_checkclient
			// 	double		lastchecktime;

			public string name = "";			// map name

			// 	char		modelname[64];		// maps/<name>.bsp, for model_precache[0]
			// 	struct model_s 	*worldmodel;
			// 	char		*model_precache[MAX_MODELS];	// NULL terminated
			// 	struct model_s	*models[MAX_MODELS];
			// 	char		*sound_precache[MAX_SOUNDS];	// NULL terminated

			// 	//* 	PF_lightstyles => sv.lightstyles[style] = val; 
			//* 	char		*lightstyles[MAX_LIGHTSTYLES]; //* MAX_LIGHTSTYLES = 64
			// 	int			num_edicts;
			public int max_edicts;
			public List<edict_t> edicts = new();			// can NOT be array indexed, because
			// 									// edict_t is variable sized, but can
			// 									// be used to reference the world ent
			//* 	server_state_t	state;			// some actions are only valid during load

				sizebuf_t	datagram;
			// 	byte		datagram_buf[MAX_DATAGRAM];

			// 	sizebuf_t	reliable_datagram;	// copied to all clients at end of frame
			// 	byte		reliable_datagram_buf[MAX_DATAGRAM];

			// 	sizebuf_t	signon;
			// 	byte		signon_buf[8192];
		}

		public class sizebuf_t
		{
			// qboolean	allowoverflow;	// if false, do a Sys_Error
			// qboolean	overflowed;		// set to true if the buffer size failed
			MSG_buffer data;
			// int maxsize = 1024;
			int cursize = 0;
		} // sizebuf_t;


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
		
		public static void SV_Init ()
		{
			int		i;

			Cbuf.Cvar_RegisterVariable (sv_maxvelocity);
			Cbuf.Cvar_RegisterVariable (sv_gravity);
			Cbuf.Cvar_RegisterVariable (sv_friction);
			Cbuf.Cvar_RegisterVariable (sv_edgefriction);
			Cbuf.Cvar_RegisterVariable (sv_stopspeed);
			Cbuf.Cvar_RegisterVariable (sv_maxspeed);
			Cbuf.Cvar_RegisterVariable (sv_accelerate);
			Cbuf.Cvar_RegisterVariable (sv_idealpitchscale);
			Cbuf.Cvar_RegisterVariable (sv_aim);
			Cbuf.Cvar_RegisterVariable (sv_nostep);

			for (i=0 ; i<MAX_MODELS ; i++)
			{
                // sprintf(localmodels[i], "*%i", i)
			}
		}
	}

	public static class SV_Cmd
	{
		public static int MAX_ENT_LEAFS = 16;
        private static int MAX_EDICTS = 600; //* Does this really apply?

        public static void SV_SpawnServer(Args levelName)
		{
			edict_t ent;
			int i;

			//* Do later	// let's not have any servers with no name
			/** // if (hostname.string[0] == 0)
			//	Cvar_Set ("hostname", "UNNAMED");
			// scr_centertime_off = 0; //* For messages that are printed to the centre of the screen

			// 	Con_DPrintf ("SpawnServer: %s\n",server);
			 */
			
			sv_main.svs.changelevel_issued = false;		// now safe to issue another

			//* // tell all connected clients that we are going to a new level
			// 	if (sv.active) //* Always true for now
			SV_SendReconnect ();

			//* // make cvars consistant
			//* NOT NEEED RIGHT NOW
			/* 	if (coop.value)
			// 		Cvar_SetValue ("deathmatch", 0);
			// 	current_skill = (int)(skill.value + 0.5);
			// 	if (current_skill < 0)
			// 		current_skill = 0;
			// 	if (current_skill > 3)
			// 		current_skill = 3;

			// 	Cvar_SetValue ("skill", (float)current_skill); 
			*/

			// TODO Not yet fully implemented // set up the new server
			Host_ClearMemory ();

            // 	memset (&sv, 0, sizeof(sv));
			// 	strcpy (sv.name, server);
            sv_main.sv = new()
            {
                name = levelName[0].s
            };
    

            //* // load progs to get entity field count
            PR_LoadProgs ();

            //* // allocate server memory
            sv_main.sv.max_edicts = MAX_EDICTS;

			// sv.edicts = Hunk_AllocName (sv.max_edicts*pr_edict_size, "edicts");
			sv.edicts = new();

            // 	sv.datagram.maxsize = sizeof(sv.datagram_buf);
            // 	sv.datagram.cursize = 0;
            // 	sv.datagram.data = sv.datagram_buf;

            // 	sv.reliable_datagram.maxsize = sizeof(sv.reliable_datagram_buf);
            // 	sv.reliable_datagram.cursize = 0;
            // 	sv.reliable_datagram.data = sv.reliable_datagram_buf;

            // 	sv.signon.maxsize = sizeof(sv.signon_buf);
            // 	sv.signon.cursize = 0;
            // 	sv.signon.data = sv.signon_buf;

            // // leave slots at start for clients only
            // 	sv.num_edicts = svs.maxclients+1;
            // 	for (i=0 ; i<svs.maxclients ; i++)
            // 	{
            // 		ent = EDICT_NUM(i+1);
            // 		svs.clients[i].edict = ent;
            // 	}

            // 	sv.state = ss_loading;
            // 	sv.paused = false;

            // 	sv.time = 1.0;

            // 	strcpy (sv.name, server);
            // 	sprintf (sv.modelname,"maps/%s.bsp", server);
            // 	sv.worldmodel = Mod_ForName (sv.modelname, false);
            // 	if (!sv.worldmodel)
            // 	{
            // 		Con_Printf ("Couldn't spawn server %s\n", sv.modelname);
            // 		sv.active = false;
            // 		return;
            // 	}
            // 	sv.models[1] = sv.worldmodel;

            // //
            // // clear world interaction links
            // //
            // 	SV_ClearWorld ();

            // 	sv.sound_precache[0] = pr_strings;

            // 	sv.model_precache[0] = pr_strings;
            // 	sv.model_precache[1] = sv.modelname;
            // 	for (i=1 ; i<sv.worldmodel->numsubmodels ; i++)
            // 	{
            // 		sv.model_precache[1+i] = localmodels[i];
            // 		sv.models[i+1] = Mod_ForName (localmodels[i], false);
            // 	}

            // //
            // // load the rest of the entities
            // //	
            // 	ent = EDICT_NUM(0);
            // 	memset (&ent->v, 0, progs->entityfields * 4);
            // 	ent->free = false;
            // 	ent->v.model = sv.worldmodel->name - pr_strings;
            // 	ent->v.modelindex = 1;		// world model
            // 	ent->v.solid = SOLID_BSP;
            // 	ent->v.movetype = MOVETYPE_PUSH;

            // 	if (coop.value)
            // 		pr_global_struct->coop = coop.value;
            // 	else
            // 		pr_global_struct->deathmatch = deathmatch.value;

            // 	pr_global_struct->mapname = sv.name - pr_strings;
            // #ifdef QUAKE2
            // 	pr_global_struct->startspot = sv.startspot - pr_strings;
            // #endif

            // // serverflags are for cross level information (sigils)
            // 	pr_global_struct->serverflags = svs.serverflags;

            // 	ED_LoadFromFile (sv.worldmodel->entities);

            // 	sv.active = true;

            // // all setup is completed, any further precache statements are errors
            // 	sv.state = ss_active;

            // // run two frames to allow everything to settle
            // 	host_frametime = 0.1;
            // 	SV_Physics ();
            // 	SV_Physics ();

            // // create a baseline for more efficient communications
            // 	SV_CreateBaseline ();

            // // send serverinfo to all connected clients
            // 	for (i=0,host_client = svs.clients ; i<svs.maxclients ; i++, host_client++)
            // 		if (host_client->active)
            // 			SV_SendServerinfo (host_client);

            // 	Con_DPrintf ("Server spawned.\n");
        }


		/* ================
		SV_SendReconnect

		Tell all the clients that the server is changing levels
		================ */
		public static void SV_SendReconnect ()
		{
			// if (cls.state != ca_dedicated)
			// {
				Cbuf.Cmd_ExecuteString ("reconnect\n", Cbuf.cmd_source.src_command);
			// }

		}
	} // END SV_Cmd



	public class edict_t
	{
		public bool free;
		// link_t		area;				// linked to a division node or leaf

		// int			num_leafs;
		// short		leafnums[MAX_ENT_LEAFS];

		public entity_state_t baseline;

		public float freetime;         // sv.time when the object was freed
		public entvars_t v;                    // C exported fields from progs
										// other fields from progs come immediately after
	} // edict_t;

	public class entity_state_t
	{
		public Vector3 origin;
		public Vector3 angles;
		public int modelindex;
		public int frame;
		public int colormap;
		public int skin;
		public int effects;
	} //entity_state_t;



}