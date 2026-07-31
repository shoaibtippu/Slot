namespace Slot.Application.Ports.In.Grounds;

public record GroundImageUploadRequest(Stream Content, string FileName, string ContentType);
public record GroundImageOrderRequest(Guid ImageId, int DisplayOrder);