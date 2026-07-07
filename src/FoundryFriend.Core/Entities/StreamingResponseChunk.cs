namespace FoundryFriend.Core.Entities;

/// <summary>
/// Represents a single chunk from a streaming agent response,
/// carrying the text content and the type of update that produced it.
/// </summary>
public record StreamingResponseChunk(string Text, string UpdateType);
