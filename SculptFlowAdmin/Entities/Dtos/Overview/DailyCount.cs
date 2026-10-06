namespace SculptFlowAdmin.Entities.Dtos.Overview;

public record DailyCount(DateOnly Day, int Inbound, int Ai, int Staff, int Failed);
