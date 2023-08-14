using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityQuake.Common
{
    public static class MSG
    {
        public class MSG_buffer {

            protected List<MSG_t> buffer = new();

            public void Add(MSG_t msg) {
                buffer.Add(msg);
            }

        }
    
        public abstract class MSG_t {

        }

        public class MSG_t_byte : MSG_t {

            byte value;
            public MSG_t_byte(byte value) { this.value = value;}

        }

        public static void MSG_Write(MSG_buffer msg_buffer, MSG_t message) {
            msg_buffer.Add(message);
        }

    }
}
