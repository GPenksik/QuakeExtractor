using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pf_cmds
{

//* SET A LIGHTSTYLE VARIABLE IN SV_MAIN
//* Called by: world.qc => lightstyle(MOVETYPE_BOUNCE, "mmamammmmammamamaaamammma");
// void PF_lightstyle (void)
// {
// 	int		style;
// 	char	*val;
//* DEFINED IN 
// 	client_t	*client;
// 	int			j;
	
//* THIS PARSES THE METHOD ARGUMENTS AS INPUTS (THROUGH A BUFFER?)
// 	style = G_FLOAT(OFS_PARM0);
// 	val = G_STRING(OFS_PARM1);

//* SETS LIGHTSTYLE ARRAY TO VAL IN SV_MAIN
// // change the string in sv
// 	sv.lightstyles[style] = val;
	
// // send message to all clients on this server
// 	if (sv.state != ss_active)
// 		return;

//* SENDS MESSAGE TO CLIENT IF A BUNCH OF STUFF IS TRUE. CAN BE ASSUMED TRUE FOR NOW
// 	for (j=0, client = svs.clients ; j<svs.maxclients ; j++, client++)
// 		if (client->active || client->spawned)
// 		{
// 			MSG_WriteChar (&client->message, svc_lightstyle);
// 			MSG_WriteChar (&client->message,style);
// 			MSG_WriteString (&client->message, val);
// 		}
// }


}
