using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Slot.Application.Ports.In.Conversations;
using System.Security.Claims;

namespace Slot.Host.Hubs;

[Authorize]
public class ChatHub(IMessageService messageService) : Hub
{
    /// <summary>
    /// Join a conversation group to receive real-time messages.
    /// </summary>
    public async Task JoinConversation(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
    }

    /// <summary>
    /// Leave a conversation group.
    /// </summary>
    public async Task LeaveConversation(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
    }

    /// <summary>
    /// Send a message to a conversation. Broadcasts "ReceiveMessage" to all group members.
    /// </summary>
    public async Task SendMessage(string conversationId, string text)
    {
        var identityId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(identityId)) return;

        if (!Guid.TryParse(conversationId, out var convId)) return;

        var result = await messageService.SendAsync(identityId, convId, new SendMessageRequest(text));
        if (!result.IsSuccess) return;

        await Clients.Group(conversationId).SendAsync("ReceiveMessage", result.Value);
    }
}
