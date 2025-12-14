using Aviation.WebApi.GraphQL.Types;
using Google.Protobuf.WellKnownTypes;

using WorkOrderModel = Aviation.Maintenance.Grpc.WorkOrderModel;

using DomainStatus = Aviation.Maintenance.Domain.Enums.WorkOrderStatus;
using DomainPriority = Aviation.Maintenance.Domain.Enums.WorkOrderPriority;

using ProtoStatus = Aviation.Maintenance.Grpc.WorkOrderStatus;
using ProtoPriority = Aviation.Maintenance.Grpc.WorkOrderPriority;

namespace Aviation.WebApi.GraphQL.Mappers;

public static class WorkOrderGrpcMapper
{
    public static WorkOrderGql ToGql(this WorkOrderModel model) => new()
    {
        Id = model.Id,
        AircraftId = model.AircraftId,
        Title = model.Title,
        Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description,
        Priority = ToDomainPriority(model.Priority),
        PlannedStart = ToNullableDateTime(model.PlannedStart),
        PlannedEnd = ToNullableDateTime(model.PlannedEnd),
        Status = ToDomainStatus(model.Status)
    };

    public static ProtoStatus ToProtoStatus(DomainStatus status) =>
        status switch
        {
            DomainStatus.New => ProtoStatus.New,
            DomainStatus.InProgress => ProtoStatus.InProgress,
            DomainStatus.Done => ProtoStatus.Done,
            DomainStatus.Cancelled => ProtoStatus.Cancelled,
            _ => ProtoStatus.Unknown
        };

    public static ProtoPriority ToProtoPriority(DomainPriority priority) =>
        priority switch
        {
            DomainPriority.Low => ProtoPriority.Low,
            DomainPriority.Medium => ProtoPriority.Medium,
            DomainPriority.High => ProtoPriority.High,
            _ => ProtoPriority.Unknown
        };

    private static DomainStatus ToDomainStatus(ProtoStatus status) =>
        status switch
        {
            ProtoStatus.New => DomainStatus.New,
            ProtoStatus.InProgress => DomainStatus.InProgress,
            ProtoStatus.Done => DomainStatus.Done,
            ProtoStatus.Cancelled => DomainStatus.Cancelled,
            _ => DomainStatus.New
        };

    private static DomainPriority ToDomainPriority(ProtoPriority priority) =>
        priority switch
        {
            ProtoPriority.Low => DomainPriority.Low,
            ProtoPriority.Medium => DomainPriority.Medium,
            ProtoPriority.High => DomainPriority.High,
            _ => DomainPriority.Medium
        };

    public static Timestamp FromNullableDateTime(DateTime? dt) =>
        dt.HasValue
            ? Timestamp.FromDateTime(DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc))
            : Timestamp.FromDateTime(DateTime.SpecifyKind(DateTime.UnixEpoch, DateTimeKind.Utc));

    private static DateTime? ToNullableDateTime(Timestamp ts)
    {
        var dt = ts.ToDateTime();
        return dt == DateTime.UnixEpoch ? (DateTime?)null : dt;
    }
}
