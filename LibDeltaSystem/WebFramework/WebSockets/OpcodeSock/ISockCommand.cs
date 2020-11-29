using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.WebFramework.WebSockets.OpcodeSock
{
    public interface ISockCommand
    {
        Task HandleCommand(DeltaOpcodeWebSocketService conn);
    }
}
