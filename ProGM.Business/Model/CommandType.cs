using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProGM.Business.Model
{
    public class SocketCommandType
    {
        public const string AUTHORIZE = "AUTHORIZE";
        public const string CHAT = "CHAT";
        public const string LOGIN = "LOGIN";
        public const string OPENCLIENT = "OPENCLIENT";
        public const string CLOSECLIENT = "CLOSECLIENT";
        public const string LOGIN_SUCCESS = "LOGIN_SUCCESS";
        public const string LOGIN_FALSED = "LOGIN_FALSED";
        public const string UPDATE_INFO_USED = "UPDATE_INFO_USED";
        public const string OUT_OF_MONEY = "OUT_OF_MONEY";
        public const string UPDATE_TOTAL_MONEY = "UPDATE_TOTAL_MONEY";

    }
}