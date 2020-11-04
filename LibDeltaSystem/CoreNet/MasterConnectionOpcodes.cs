using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreNet
{
    public static class MasterConnectionOpcodes
    {
        //Opcodes from manager to master
        public const short OPCODE_MASTER_LOGIN = 10001;
        public const short OPCODE_MASTER_CONFIG = 10002;
        public const short OPCODE_MASTER_STATUS = 10003;
        public const short OPCODE_MASTER_M_ADDPACKAGE = 10004;
        public const short OPCODE_MASTER_M_ADDVERSION = 10005;
        public const short OPCODE_MASTER_M_ADDINSTANCE = 10006;
        public const short OPCODE_MASTER_M_LISTPACKAGES = 10007;
        public const short OPCODE_MASTER_M_LISTVERSIONS = 10008;
        public const short OPCODE_MASTER_M_LISTINSTANCES = 10009;
        public const short OPCODE_MASTER_M_UPDATEINSTANCE = 10010;
        public const short OPCODE_MASTER_M_DESTROYINSTANCE = 10011;
        public const short OPCODE_MASTER_M_DELETEVERSION = 10012;
        public const short OPCODE_MASTER_M_ADDSITE = 10013;
        public const short OPCODE_MASTER_M_LISTSITES = 10014;
        public const short OPCODE_MASTER_M_ASSIGNSITE = 10015;
    }
}
