using System.Text;
using System.Linq.Expressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnityQuake.Progs
{
    public static class ProgDefs
    {
        public const int MAX_PARMS = 8;

        #region Struct Definitions

        [System.Serializable]
        public class dprograms_t
        {
            public int version;
            public int crc;         // check of header file

            public int ofs_statements;
            public int numstatements;   // statement 0 is an error

            public int ofs_globaldefs;
            public int numglobaldefs;

            public int ofs_fielddefs;
            public int numfielddefs;

            public int ofs_functions;
            public int numfunctions;    // function 0 is an empty

            public int ofs_strings;
            public int numstrings;      // first string is a null string

            public int ofs_globals;
            public int numglobals;

            public int entityfields;
        } //dprograms_t;

        [System.Serializable]
        public class dfunction_t
        {
            public string UQname;
            public int first_statement;	// negative numbers are builtins
            public int parm_start;
            public int locals;              // total ints of parms + locals

            public int profile;     // runtime

            public int s_name;
            public int s_file;          // source file defined in

            public int numparms;
            public byte[] parm_size;

            public int UQmemPtr;
            public static int bytes = 36;

            public int this[int i]
            {
                get => bytes * i;
            }
        } //dfunction_t;

        [System.Serializable]
        public class ddef_t
        {
            public ushort type;		        // if DEF_SAVEGLOBGAL bit is set the variable needs to be saved in savegames
            public ushort ofs;
            public int s_name;
            public string UQname;
            public int UQmemPtr;
            public ddef_types UQType;
            public static int bytes = 8;

            public int this[int i]
            {
                get => bytes * i;
            }

        } //ddef_t;
        #endregion

        #region FILL FUNCS
        public static void fill(this dfunction_t function)
        {

            function.parm_size = new byte[MAX_PARMS];
            function.first_statement = ProgsReader.reader.ReadInt32();
            function.parm_start = ProgsReader.reader.ReadInt32();
            function.locals = ProgsReader.reader.ReadInt32();
            function.profile = ProgsReader.reader.ReadInt32();
            function.s_name = ProgsReader.reader.ReadInt32();
            function.s_file = ProgsReader.reader.ReadInt32();
            function.numparms = ProgsReader.reader.ReadInt32();
            for (int i = 0; i < MAX_PARMS; i++)
            {
                function.parm_size[i] = ProgsReader.reader.ReadByte();
            }

            function.UQname = GetStringFromProg(function.s_name);
        }

        public static void fill(this ddef_t def)
        {
            def.UQmemPtr = ProgsReader.GetPosition();

            def.type = ProgsReader.reader.ReadUInt16();
            def.ofs = ProgsReader.reader.ReadUInt16();
            def.s_name = ProgsReader.reader.ReadInt32();

            def.UQname = GetStringFromProg(def.s_name);
            def.UQType = (ddef_types)(def.type ^ (2 << 15) / 2);
        }
        #endregion

        #region UPDATE FUNCS
        public static void Update(this ddef_t ddef, int indexOffset)
        {
            ProgsReader.MarkPosition();
            ProgsReader.Set(ProgsReader.progs.ofs_globaldefs + ddef[indexOffset]);
            ddef.fill();
            ProgsReader.ReturnToMark();
        }

        public static void Update(this dfunction_t function, int indexOffset)
        {
            ProgsReader.MarkPosition();
            ProgsReader.Set(ProgsReader.progs.ofs_functions + function[indexOffset]);
            function.fill();
            ProgsReader.ReturnToMark();
        }
        #endregion

        public static int FindFunctionByName(this dfunction_t function, string name)
        {
            ProgsReader.MarkPosition();
            ProgsReader.Set(ProgsReader.progs.ofs_functions);
            int indexCounter = 0;
            while (ProgsReader.GetPosition() < ProgsReader.progs.ofs_functions + ProgsReader.progs.numfunctions * dfunction_t.bytes)
            {
                function.fill();
                if (function.UQname == name)
                {
                    ProgsReader.ReturnToMark();
                    return indexCounter;
                }
                indexCounter++; ;
            }
            ProgsReader.ReturnToMark();
            return -1;
        }

        public static int FindDdefByName(this ddef_t ddef, string name)
        {
            ProgsReader.MarkPosition();
            ProgsReader.Set(ProgsReader.progs.ofs_globaldefs);
            int indexCounter = 0;
            while (ProgsReader.GetPosition() < ProgsReader.progs.ofs_globaldefs + ProgsReader.progs.numglobaldefs * dfunction_t.bytes)
            {
                ddef.fill();
                if (ddef.UQname == name)
                {
                    return indexCounter;
                }
                indexCounter++; ;
            }
            ProgsReader.ReturnToMark();
            return -1;
        }

        private static string GetStringFromProg(int offset, int maxLength = 20)
        {
            List<byte> nameBytes = new();
            byte latestChar = 0;
            ProgsReader.MarkPosition();
            ProgsReader.Set(offset + ProgsReader.progs.ofs_strings);
            for (int i = 0; i < maxLength; i++)
            {
                if (ProgsReader.reader.BaseStream.Position >= ProgsReader.reader.BaseStream.Length)
                {
                    Debug.LogError("RAN OUT OF BUFFER ON GET STRING");
                    break;
                }

                latestChar = ProgsReader.reader.ReadByte();

                if (latestChar == 0)
                {
                    break;
                }
                else
                {
                    nameBytes.Add(latestChar);
                }
            }
            ProgsReader.ReturnToMark();
            return Encoding.UTF8.GetString(nameBytes.ToArray());
        }

        public enum ddef_types
        {
            ev_void,
            ev_string,
            ev_float,
            ev_vector,
            ev_entity,
            ev_field,
            ev_function,
            ev_pointer
        } //etype_t;

        public class entvars_t
        {
            public float modelindex;
            public Vector3 absmin;
            public Vector3 absmax;
            public float ltime;
            public float movetype;
            public float solid;
            public Vector3 origin;
            public Vector3 oldorigin;
            public Vector3 velocity;
            public Vector3 angles;
            public Vector3 avelocity;
            public Vector3 punchangle;
            public int classname; // String ptr
            public int model; // String ptr
            public float frame;
            public float skin;
            public float effects;
            public Vector3 mins;
            public Vector3 maxs;
            public Vector3 size;
            public int touch; // Function ptr
            public int use; // Function ptr
            public int think; // Function ptr
            public int blocked; // Function ptr
            public float nextthink;
            public int groundentity;
            public float health;
            public float frags;
            public float weapon;
            public int weaponmodel; // String ptr
            public float weaponframe;
            public float currentammo;
            public float ammo_shells;
            public float ammo_nails;
            public float ammo_rockets;
            public float ammo_cells;
            public float items;
            public float takedamage;
            public int chain;
            public float deadflag;
            public Vector3 view_ofs;
            public float button0;
            public float button1;
            public float button2;
            public float impulse;
            public float fixangle;
            public Vector3 v_angle;
            public float idealpitch;
            public int netname; // String ptr
            public int enemy;
            public float flags;
            public float colormap;
            public float team;
            public float max_health;
            public float teleport_time;
            public float armortype;
            public float armorvalue;
            public float waterlevel;
            public float watertype;
            public float ideal_yaw;
            public float yaw_speed;
            public int aiment;
            public int goalentity;
            public float spawnflags;
            public int target; // String ptr
            public int targetname; // String ptr
            public float dmg_take;
            public float dmg_save;
            public int dmg_inflictor;
            public int owner;
            public Vector3 movedir;
            public int message; // String ptr
            public float sounds;
            public int noise; // String ptr
            public int noise1; // String ptr
            public int noise2; // String ptr
            public int noise3; // String ptr
        } //entvars_t;
    }
}