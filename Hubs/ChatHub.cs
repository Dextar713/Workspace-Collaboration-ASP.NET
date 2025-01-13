using Microsoft.AspNetCore.SignalR;

namespace Discord2.Hubs
{
    public class ChatHub : Hub
    {
        public async Task JoinGroup(string groupId)
        {
            //await Groups.AddToGroupAsync(connectionId, groupId);
            //System.Diagnostics.Debug.WriteLine("Group Id from Join: " + groupId);
            //System.Diagnostics.Debug.WriteLine("Connection Id from join: " + Context.ConnectionId);
            //System.Diagnostics.Debug.WriteLine("---------------------- ");
            //System.Diagnostics.Debug.WriteLine("----------------------- " );
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
            //await Clients.Group(channelName).SendAsync("ReceiveMessage", "System", $"{Context.User.Identity?.Name} has joined the channel.");
        }

        public async Task LeaveGroup(string groupId)
        {
            //await Groups.RemoveFromGroupAsync(connectionId, groupId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
            //await Clients.Group(channelName).SendAsync("ReceiveMessage", "System", $"{Context.User.Identity?.Name} has left the channel.");
        }

        public async Task SendMessageToGroup(string groupId, object data_msg)
        {
            //System.Diagnostics.Debug.WriteLine("Group Id from send: " + data_msg);
            //System.Diagnostics.Debug.WriteLine("Connection Id from send: " + Context.ConnectionId);
            //System.Diagnostics.Debug.WriteLine("---------------------- ");
            //System.Diagnostics.Debug.WriteLine("----------------------- ");
            await Clients.All.SendAsync("ReceiveMessage", data_msg);
        }

        public async Task DeleteMessage(string messageId)
        {
            await Clients.All.SendAsync("DeleteMessage", messageId);
        }
    }
} 