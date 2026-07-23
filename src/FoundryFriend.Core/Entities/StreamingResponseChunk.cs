namespace FoundryFriend.Core.Entities;

/// <summary>
/// Represents a single chunk from a streaming agent response,
/// carrying the text content and the type of update that produced it.
/// </summary>
/// <param name="Text">The text content carried by this chunk.</param>
/// <param name="UpdateType">The type of update that produced this chunk (e.g., message delta, run status).</param>
public record StreamingResponseChunk(string Text, string UpdateType);
