using System;
using arriety.login.network.factories;
using arriety.utils.network;

namespace arriety.login.network
{
    public class ServerPacketFactory
    {
        public static ServerPacket Create(byte opcode) => opcode switch
        {
            0x02 => new SM_INIT_Ecs(),
            0x11 => new SM_LOGIN_RESPONSE_Ecs(),
            _ => null
        };
    }
}