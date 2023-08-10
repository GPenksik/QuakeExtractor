using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 
/// </summary>
public class pr_cmds
{

// LINK Assets/GameCode/QuakeSource/sv_main.cs#one

//* Set "lightstyle" variable to "var" in "sv_main.c"
//* Called by: 
//* world.qc => lightstyle(MOVETYPE_BOUNCE, "mmamammmmammamamaaamammma");
/* void PF_lightstyle (void)
{
	int		style;
	char	*val;
//* DEFINED IN 
	client_t	*client;
	int			j;
	
//* THIS PARSES THE METHOD ARGUMENTS AS INPUTS (THROUGH A BUFFER?)
	style = G_FLOAT(OFS_PARM0);
	val = G_STRING(OFS_PARM1);
Takes you here: 

//* Set "lightstyle" variable to "var" in "sv_main.c"
// change the string in sv
	sv.lightstyles[style] = val;
	
// send message to all clients on this server
	if (sv.state != ss_active)
		return;

//* Sends message to client to replicate lightstyle array
//* Client defined as struct in server.h (sv_main.cs)
//* svs is the "server_state_t" declared in sv_main.c and defined in server.h 
	for (j=0, client = svs.clients ; j<svs.maxclients ; j++, client++)
		if (client->active || client->spawned)
		{
			MSG_WriteChar (&client->message, svc_lightstyle);
			MSG_WriteChar (&client->message,style);
			MSG_WriteString (&client->message, val);
		}
} */

//* The above MSG does this in protocol
/* 		case svc_lightstyle:
			i = MSG_ReadByte ();
			if (i >= MAX_LIGHTSTYLES)
				Sys_Error ("svc_lightstyle > MAX_LIGHTSTYLES");
			Q_strcpy (cl_lightstyle[i].map,  MSG_ReadString());
			cl_lightstyle[i].length = Q_strlen(cl_lightstyle[i].map);
			break;
 */
}
