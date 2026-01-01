using Google.Protobuf.WellKnownTypes;
using Aviation.Maintenance.Grpc;
using DomainStatus = Aviation.Maintenance.Domain.Enums.WorkOrderStatus;
using DomainPriority = Aviation.Maintenance.Domain.Enums.WorkOrderPriority;

namespace Aviation.Maintenance.Grpc.Mapping;

public static class WorkOrderMappings
{
    public static DomainStatus ToDomainStatus(this WorkOrderStatus status) =>
        status switch
        {
            WorkOrderStatus.New => DomainStatus.New,
            WorkOrderStatus.InProgress => DomainStatus.InProgress,
            WorkOrderStatus.Done => DomainStatus.Done,
            WorkOrderStatus.Cancelled => DomainStatus.Cancelled,
            _ => DomainStatus.New
        };

    public static WorkOrderStatus ToProtoStatus(this DomainStatus status) =>
        status switch
        {
            DomainStatus.New => WorkOrderStatus.New,
            DomainStatus.InProgress => WorkOrderStatus.InProgress,
            DomainStatus.Done => WorkOrderStatus.Done,
            DomainStatus.Cancelled => WorkOrderStatus.Cancelled,
            _ => WorkOrderStatus.Unknown
        };

    public static DomainPriority ToDomainPriority(this WorkOrderPriority priority) =>
        priority switch
        {
            WorkOrderPriority.Low => DomainPriority.Low,
            WorkOrderPriority.Medium => DomainPriority.Medium,
            WorkOrderPriority.High => DomainPriority.High,
            _ => DomainPriority.Medium
        };

    public static WorkOrderPriority ToProtoPriority(this DomainPriority priority) =>
        priority switch
        {
            DomainPriority.Low => WorkOrderPriority.Low,
            DomainPriority.Medium => WorkOrderPriority.Medium,
            DomainPriority.High => WorkOrderPriority.High,
            _ => WorkOrderPriority.Unknown
        };

    public static Timestamp ToTimestampOrDefault(this DateTime? dt) =>
        dt.HasValue
            ? Timestamp.FromDateTime(DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc))
            : Timestamp.FromDateTime(DateTime.SpecifyKind(DateTime.UnixEpoch, DateTimeKind.Utc));

    public static DateTime? ToNullableDateTime(this Timestamp? ts)
    {
        if (ts is null)
            return null;

        var dt = ts.ToDateTime();
        return dt == DateTime.UnixEpoch ? null : dt;
    }
}
