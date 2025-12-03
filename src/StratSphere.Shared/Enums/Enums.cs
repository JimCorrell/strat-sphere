namespace StratSphere.Shared.Enums;

public enum DraftMode
{
    Synchronous,
    Asynchronous
}

public enum DraftStatus
{
    Scheduled,
    InProgress,
    Paused,
    Completed,
    Cancelled
}

public enum LeagueStatus
{
    Setup,
    PreSeason,
    Active,
    PostSeason,
    OffSeason,
    Archived
}

public enum PlayerLevel
{
    Major,
    TripleA,
    DoubleA,
    SingleA,
    Rookie,
    Amateur
}

public enum Position
{
    Pitcher,
    Catcher,
    FirstBase,
    SecondBase,
    ThirdBase,
    Shortstop,
    LeftField,
    CenterField,
    RightField,
    DesignatedHitter
}

public enum SeasonPhase
{
    PreSeason,
    RegularSeason,
    Playoffs,
    WorldSeries,
    OffSeason
}

public enum TransactionType
{
    Draft,
    Trade,
    FreeAgentSigning,
    Waiver,
    Release,
    CallUp,
    SendDown
}
