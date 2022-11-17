using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Furion.InstantMessaging;
using Microsoft.AspNetCore.SignalR;

namespace FileServer
{
    [MapHub("/hubs/chathub")]
    public class ChatHub : Hub
    {

    }
}