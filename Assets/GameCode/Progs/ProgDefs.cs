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
    [System.Serializable]
    public class dprograms_t
    {
        public int	version;
        public int	crc;			// check of header file
        
        public int	ofs_statements;
        public int	numstatements;	// statement 0 is an error
        
        public int	ofs_globaldefs;
        public int	numglobaldefs;
        
        public int	ofs_fielddefs;
        public int	numfielddefs;
        
        public int	ofs_functions;
        public int	numfunctions;	// function 0 is an empty
        
        public int	ofs_strings;
        public int	numstrings;		// first string is a null string
        
        public int	ofs_globals;
        public int	numglobals;
        
        public int	entityfields;
    } //dprograms_t;
    
    [System.Serializable]
    public class dfunction_t
    {
        public int		first_statement;	// negative numbers are builtins
        public int		parm_start;
        public int		locals;				// total ints of parms + locals
        
        public int		profile;		// runtime
        
        public int		s_name;
        public int		s_file;			// source file defined in
        
        public int		numparms;
        public byte[]	parm_size;

        public string   UQname;
        public int      UQmemPtr; 
        public static int bytes = 36;
    } //dfunction_t;

    public static void fill(this dfunction_t function) {

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

        public int this[int i] {
            get => bytes * i;
        }

    } //ddef_t;

    public static void Update(this ddef_t ddef, int indexOffset) {
        ProgsReader.MarkPosition();
        ProgsReader.Set(ProgsReader.progs.ofs_globaldefs + ddef[indexOffset]);
        ddef.fill();
        ProgsReader.ReturnToMark();
    }

    public static void fill(this ddef_t def) {
        def.UQmemPtr = ProgsReader.GetPosition();
        
        def.type = ProgsReader.reader.ReadUInt16();
        def.ofs = ProgsReader.reader.ReadUInt16();
        def.s_name = ProgsReader.reader.ReadInt32();

        def.UQname = GetStringFromProg(def.s_name);
        def.UQType = (ddef_types)(def.type^(2<<15)/2);     
    }

    private static string GetStringFromProg(int offset, int maxLength = 20)
    {
        List<byte> nameBytes = new();
        ProgsReader.MarkPosition();
        ProgsReader.Set(offset + ProgsReader.progs.ofs_strings);
        for (int i = 0; i < maxLength; i++)
        {
            if (ProgsReader.reader.BaseStream.Position >= ProgsReader.reader.BaseStream.Length) {
                Debug.LogError("RAN OUT OF BUFFER ON GET STRING");
                break;
            }
            nameBytes.Add(ProgsReader.reader.ReadByte());
            if (nameBytes[i] == 0)
            {
                break;
            }
        }
        ProgsReader.ReturnToMark();
        return System.Text.Encoding.UTF8.GetString(nameBytes.ToArray());
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

}
}