using Microsoft.AspNetCore.SignalR;

namespace Aviation.WebApi.Hubs;

public class MaintenanceHub : Hub
{
    private const string DispatchersGroup = "dispatchers";

    // Все диспетчеры (таблица заявок) подписываются на эту группу
    public async Task JoinDispatchers()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, DispatchersGroup);
    }

    // По желанию: подписка на конкретный самолёт
    public async Task JoinAircraftGroup(int aircraftId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"aircraft-{aircraftId}");
    }
}
