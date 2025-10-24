using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snapdi.Services.Hubs
{
    public class BookingHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        // Cho phép client join group theo customerId
        public async Task JoinCustomerGroup(int customerId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"customer-{customerId}");
        }
    }
}
