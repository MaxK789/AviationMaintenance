using Aviation.Maintenance.Grpc;
using Grpc.Net.Client;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Aviation.Client.Console (gRPC)");

        // адрес тот же, что ты указал в WebApi для WorkOrderServiceClient
        var grpcAddress = "http://localhost:5004";

        using var channel = GrpcChannel.ForAddress(grpcAddress);
        var client = new WorkOrderService.WorkOrderServiceClient(channel);

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("1) List work orders");
            Console.WriteLine("2) Create work order");
            Console.WriteLine("3) Change status");
            Console.WriteLine("4) Delete work order");
            Console.WriteLine("0) Exit");
            Console.Write("Select: ");
            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    await ListWorkOrders(client);
                    break;
                case "2":
                    await CreateWorkOrder(client);
                    break;
                case "3":
                    await ChangeStatus(client);
                    break;
                case "4":
                    await DeleteWorkOrder(client);
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Unknown option");
                    break;
            }
        }
    }

    private static async Task ListWorkOrders(WorkOrderService.WorkOrderServiceClient client)
    {
        try
        {
            var response = await client.ListWorkOrdersAsync(new ListWorkOrdersRequest());
            Console.WriteLine($"Total: {response.WorkOrders.Count}");
            foreach (var w in response.WorkOrders)
            {
                Console.WriteLine($"{w.Id}: AC={w.AircraftId}, {w.Title} [{w.Status}]");
            }
        }
        catch (Grpc.Core.RpcException ex)
        {
            Console.WriteLine($"gRPC error: {ex.StatusCode} - {ex.Status.Detail}");
        }
    }

    private static async Task CreateWorkOrder(WorkOrderService.WorkOrderServiceClient client)
    {
        Console.Write("AircraftId: ");
        var aircraftIdStr = Console.ReadLine();
        Console.Write("Title: ");
        var title = Console.ReadLine() ?? string.Empty;
        Console.Write("Description: ");
        var description = Console.ReadLine() ?? string.Empty;

        if (!int.TryParse(aircraftIdStr, out var aircraftId))
        {
            Console.WriteLine("Invalid aircraft id");
            return;
        }

        var request = new CreateWorkOrderRequest
        {
            AircraftId = aircraftId,
            Title = title,
            Description = description,
            Priority = WorkOrderPriority.Medium,
            PlannedStart = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc))
        };

        try
        {
            var response = await client.CreateWorkOrderAsync(request);
            Console.WriteLine($"Created work order with id: {response.WorkOrder.Id}");
        }
        catch (Grpc.Core.RpcException ex)
        {
            Console.WriteLine($"gRPC error: {ex.StatusCode} - {ex.Status.Detail}");
        }
    }

    private static async Task ChangeStatus(WorkOrderService.WorkOrderServiceClient client)
    {
        Console.Write("WorkOrderId: ");
        var idStr = Console.ReadLine();
        Console.Write("New status (New/InProgress/Done/Cancelled): ");
        var statusStr = Console.ReadLine()?.Trim();

        if (!int.TryParse(idStr, out var id))
        {
            Console.WriteLine("Invalid id");
            return;
        }

        var status = statusStr?.ToLowerInvariant() switch
        {
            "new" => WorkOrderStatus.New,
            "inprogress" => WorkOrderStatus.InProgress,
            "done" => WorkOrderStatus.Done,
            "cancelled" => WorkOrderStatus.Cancelled,
            _ => WorkOrderStatus.Unknown
        };

        if (status == WorkOrderStatus.Unknown)
        {
            Console.WriteLine("Unknown status");
            return;
        }

        var request = new ChangeWorkOrderStatusRequest
        {
            Id = id,
            NewStatus = status
        };

        try
        {
            await client.ChangeWorkOrderStatusAsync(request);
            Console.WriteLine("Status changed");
        }
        catch (Grpc.Core.RpcException ex)
        {
            Console.WriteLine($"gRPC error: {ex.StatusCode} - {ex.Status.Detail}");
        }
    }

    private static async Task DeleteWorkOrder(WorkOrderService.WorkOrderServiceClient client)
    {
        Console.Write("WorkOrderId: ");
        var idStr = Console.ReadLine();
        if (!int.TryParse(idStr, out var id))
        {
            Console.WriteLine("Invalid id");
            return;
        }

        try
        {
            await client.DeleteWorkOrderAsync(new DeleteWorkOrderRequest { Id = id });
            Console.WriteLine("Deleted");
        }
        catch (Grpc.Core.RpcException ex)
        {
            Console.WriteLine($"gRPC error: {ex.StatusCode} - {ex.Status.Detail}");
        }
    }
}
