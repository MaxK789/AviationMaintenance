using Aviation.Maintenance.Grpc;
using Grpc.Core;
using Grpc.Net.Client;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Aviation.Client.Console (gRPC)");

        // адрес тот же, что ты указал в WebApi для WorkOrderServiceClient
        var grpcAddress =
            args.FirstOrDefault(a => a.StartsWith("--grpc=", StringComparison.OrdinalIgnoreCase))?.Split('=', 2)[1]
            ?? Environment.GetEnvironmentVariable("GRPC_WORKORDERS_URL")
            ?? "http://localhost:5004";

        var apiKey =
            args.FirstOrDefault(a => a.StartsWith("--apikey=", StringComparison.OrdinalIgnoreCase))?.Split('=', 2)[1]
            ?? Environment.GetEnvironmentVariable("API_KEY")
            ?? "dev-super-secret-key";

        var headers = new Metadata { { "x-api-key", apiKey } };

        using var channel = GrpcChannel.ForAddress(grpcAddress);
        var client = new WorkOrderService.WorkOrderServiceClient(channel);

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("1) List work orders");
            Console.WriteLine("2) Create work order");
            Console.WriteLine("3) Change status");
            Console.WriteLine("4) Delete work order");
            Console.WriteLine("5) Watch work orders (stream snapshots)");
            Console.WriteLine("0) Exit");
            Console.Write("Select: ");
            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    await ListWorkOrders(client, headers);
                    break;
                case "2":
                    await CreateWorkOrder(client, headers);
                    break;
                case "3":
                    await ChangeStatus(client, headers);
                    break;
                case "4":
                    await DeleteWorkOrder(client, headers);
                    break;
                case "5":
                    await WatchWorkOrders(client, headers);
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Unknown option");
                    break;
            }
        }
    }

    private static async Task ListWorkOrders(WorkOrderService.WorkOrderServiceClient client, Metadata headers)
    {
        var response = await client.ListWorkOrdersAsync(new ListWorkOrdersRequest(), headers: headers);
        Console.WriteLine($"Total: {response.WorkOrders.Count}");
        foreach (var w in response.WorkOrders)
        {
            Console.WriteLine($"{w.Id}: AC={w.AircraftId}, {w.Title} [{w.Status}]");
        }
    }

    private static async Task CreateWorkOrder(WorkOrderService.WorkOrderServiceClient client, Metadata headers)
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

        var response = await client.CreateWorkOrderAsync(request, headers: headers);
        Console.WriteLine($"Created work order with id: {response.WorkOrder.Id}");
    }

    private static async Task ChangeStatus(WorkOrderService.WorkOrderServiceClient client, Metadata headers)
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

        await client.ChangeWorkOrderStatusAsync(request, headers: headers);
        Console.WriteLine("Status changed");
    }

    private static async Task DeleteWorkOrder(WorkOrderService.WorkOrderServiceClient client, Metadata headers)
    {
        Console.Write("WorkOrderId: ");
        var idStr = Console.ReadLine();
        if (!int.TryParse(idStr, out var id))
        {
            Console.WriteLine("Invalid id");
            return;
        }

        await client.DeleteWorkOrderAsync(new DeleteWorkOrderRequest { Id = id }, headers: headers);
        Console.WriteLine("Deleted");
    }

    private static async Task WatchWorkOrders(WorkOrderService.WorkOrderServiceClient client, Metadata headers)
    {
        Console.WriteLine("Watching work orders snapshots (3 updates)...");
        var call = client.WatchWorkOrders(
            new WatchWorkOrdersRequest { IntervalSeconds = 2 },
            headers: headers,
            deadline: DateTime.UtcNow.AddMinutes(1));

        try
        {
            var received = 0;
            while (await call.ResponseStream.MoveNext(CancellationToken.None))
            {
                var snapshot = call.ResponseStream.Current;
                Console.WriteLine($"[{snapshot.UnixTimeSeconds}] workOrders={snapshot.WorkOrders.Count}");

                received++;
                if (received >= 3)
                {
                    break;
                }
            }
        }
        finally
        {
            await call.DisposeAsync();
        }
    }
}
