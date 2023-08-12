using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    public class sv_phys
    {

    #region SV_physics
        /**    void SV_Physics (void)
        {
            int		i;
            edict_t	*ent;
    
        // let the progs know that a new frame has started
            pr_global_struct->self = EDICT_TO_PROG(sv.edicts);  //* Copties various types of data into the progs global struct
            pr_global_struct->other = EDICT_TO_PROG(sv.edicts);
            pr_global_struct->time = sv.time;
            PR_ExecuteProgram (pr_global_struct->StartFrame);   //* Calls the start_frame function in world.qc. Doesn't actually do much
    
        //
        // treat each object in turn
        //
            ent = sv.edicts;
            for (i=0 ; i<sv.num_edicts ; i++, ent = NEXT_EDICT(ent))
            {
                if (ent->free)
                    continue;
    
                if (pr_global_struct->force_retouch)
                {
                    SV_LinkEdict (ent, true);	// force retouch even for stationary //* This relinks the entity into the BSP Tree
                }
    
                if (i > 0 && i <= svs.maxclients)   //* First edict is world. Next few are always players. 
                    SV_Physics_Client (ent, i);     //* 
                else if (ent->v.movetype == MOVETYPE_PUSH)
                    SV_Physics_Pusher (ent);        //* World is processed first as a PUSH physics object
                else if (ent->v.movetype == MOVETYPE_NONE)
                    SV_Physics_None (ent);
        #ifdef QUAKE2
                else if (ent->v.movetype == MOVETYPE_FOLLOW)
                    SV_Physics_Follow (ent);
        #endif
                else if (ent->v.movetype == MOVETYPE_NOCLIP)
                    SV_Physics_Noclip (ent);
                else if (ent->v.movetype == MOVETYPE_STEP)
                    SV_Physics_Step (ent);
                else if (ent->v.movetype == MOVETYPE_TOSS 
                || ent->v.movetype == MOVETYPE_BOUNCE
        #ifdef QUAKE2
                || ent->v.movetype == MOVETYPE_BOUNCEMISSILE
        #endif
                || ent->v.movetype == MOVETYPE_FLY
                || ent->v.movetype == MOVETYPE_FLYMISSILE)
                    SV_Physics_Toss (ent);
                else
                    Sys_Error ("SV_Physics: bad movetype %i", (int)ent->v.movetype);			
            }
            
            if (pr_global_struct->force_retouch)
                pr_global_struct->force_retouch--;	
    
            sv.time += host_frametime;
        } */
#endregion
#region Name
    
    /* void SV_Physics_Pusher (edict_t *ent)
    {
    	float	thinktime;
    	float	oldltime;
    	float	movetime;
    
    	oldltime = ent->v.ltime;
    	
    	thinktime = ent->v.nextthink;
    	if (thinktime < ent->v.ltime + host_frametime)
    	{
    		movetime = thinktime - ent->v.ltime;
    		if (movetime < 0)
    			movetime = 0;
    	}
    	else
    		movetime = host_frametime;
    
    	if (movetime)
    	{
    #ifdef QUAKE2
    		if (ent->v.avelocity[0] || ent->v.avelocity[1] || ent->v.avelocity[2])
    			SV_PushRotate (ent, movetime);
    		else
    #endif
    			SV_PushMove (ent, movetime);	// advances ent->v.ltime if not blocked
    	}
    		
    	if (thinktime > oldltime && thinktime <= ent->v.ltime)
    	{
    		ent->v.nextthink = 0;
    		pr_global_struct->time = sv.time;
    		pr_global_struct->self = EDICT_TO_PROG(ent);
    		pr_global_struct->other = EDICT_TO_PROG(sv.edicts);
    		PR_ExecuteProgram (ent->v.think);
    		if (ent->free)
    			return;
    	}
    
    } */
#endregion

}
