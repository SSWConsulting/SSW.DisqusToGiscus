using System.Text.Json;
using DisqusToGiscusMigrator.Constants;
using DisqusToGiscusMigrator.Models;

namespace DisqusToGiscusMigrator.Helpers;

public class Checkpointer
{
    private readonly string _checkpointPath;
    private readonly JsonSerializerOptions _serializerOptions;

    public Checkpointer(string checkpointPath)
    {
        _checkpointPath = checkpointPath;
        _serializerOptions = new()
        {
            WriteIndented = true
        };
    }

    public void Checkpoint(MigrationStatus status, List<DisqusBlogPost> disqusBlogPosts)
    {
        var jsonText = JsonSerializer.Serialize(new CheckpointResult
        {
            MigrationStatus = status,
            DisqusBlogPosts = disqusBlogPosts
        }, _serializerOptions);

        File.WriteAllText(_checkpointPath, jsonText);
    }

    public CheckpointResult TryLoad()
    {
        try
        {
            var jsonText = File.ReadAllText(_checkpointPath);
            var result = JsonSerializer.Deserialize<CheckpointResult>(jsonText)!;

            return result;
        }
        catch (Exception)
        {
            return new CheckpointResult
            {
                MigrationStatus = MigrationStatus.Unparsed,
                DisqusBlogPosts = new()
            };
        }
    }

    public record CheckpointResult
    {
        public MigrationStatus MigrationStatus { get; init; }
        public List<DisqusBlogPost> DisqusBlogPosts { get; init; } = new();

        public void Deconstruct(out MigrationStatus status, out List<DisqusBlogPost> disqusBlogPosts)
        {
            status = MigrationStatus;
            disqusBlogPosts = DisqusBlogPosts;
        }
    }
}
