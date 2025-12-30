using Microsoft.AspNetCore.SignalR;

namespace Aviation.WebApi.Hubs;

public class MaintenanceHub : Hub
{
    private const string DispatchersGroup = "dispatchers";

    // Усі диспетчери (таблиця заявок) підписуються на цю групу
    public async Task JoinDispatchers()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, DispatchersGroup);
    }

    // За бажанням: підписка на конкретний літак
    public async Task JoinAircraftGroup(int aircraftId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"aircraft-{aircraftId}");
    }
}
