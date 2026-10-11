namespace ProductCompass.Application.Analysis;

// Contract only. No provider or analysis endpoint is enabled in the foundation.
// The application must load all signals through an authorized organization scope before calling a provider.
public sealed record SignalForAnalysis(Guid Id, string Text);
public sealed record ProposedNeed(string Title, string Rationale, IReadOnlyList<Guid> SourceSignalIds);
public interface INeedAnalysisProvider
{
    Task<IReadOnlyList<ProposedNeed>> ProposeAsync(IReadOnlyList<SignalForAnalysis> signals, CancellationToken ct);
}
