using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sv_main : MonoBehaviour
{
//* DEFINE SERVER STATIC STRUCT = svs

/*typedef struct //* server_static_t
{
    //* NEEDED
	int			maxclients;
	int			maxclientslimit;
    //* NEEDED
	struct client_s	*clients;		// [maxclients]
	int			serverflags;		// episode completion information
	qboolean	changelevel_issued;	// cleared when at SV_SpawnServer
} server_static_t;
*/
// Takes you here: 
//* DEFINE SERVER STRUCT = sv
/* typedef struct //* server_t
{
	qboolean	active;				// false if only a net client

	qboolean	paused;
	qboolean	loadgame;			// handle connections specially

	double		time;
	
	int			lastcheck;			// used by PF_checkclient
	double		lastchecktime;
	
	char		name[64];			// map name

	char		modelname[64];		// maps/<name>.bsp, for model_precache[0]
	struct model_s 	*worldmodel;
	char		*model_precache[MAX_MODELS];	// NULL terminated
	struct model_s	*models[MAX_MODELS];
	char		*sound_precache[MAX_SOUNDS];	// NULL terminated

//* ANCHOR[id=one]
    //* 	PF_lightstyles => sv.lightstyles[style] = val; 
	char		*lightstyles[MAX_LIGHTSTYLES]; //* MAX_LIGHTSTYLES = 64
	int			num_edicts;
	int			max_edicts;
	edict_t		*edicts;			// can NOT be array indexed, because
									// edict_t is variable sized, but can
									// be used to reference the world ent
	server_state_t	state;			// some actions are only valid during load

	sizebuf_t	datagram;
	byte		datagram_buf[MAX_DATAGRAM];

	sizebuf_t	reliable_datagram;	// copied to all clients at end of frame
	byte		reliable_datagram_buf[MAX_DATAGRAM];

	sizebuf_t	signon;
	byte		signon_buf[8192];
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
