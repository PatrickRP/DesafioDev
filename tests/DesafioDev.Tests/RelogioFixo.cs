namespace DesafioDev.Tests;

/// <summary>TimeProvider congelado em UTC — torna "hoje" determinístico nos testes.</summary>
internal sealed class RelogioFixo(DateTimeOffset agora) : TimeProvider
{
    public RelogioFixo(int ano, int mes, int dia) : this(new DateTimeOffset(ano, mes, dia, 12, 0, 0, TimeSpan.Zero)) { }

    public override DateTimeOffset GetUtcNow() => agora;
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
