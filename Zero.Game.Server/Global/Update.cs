namespace Zero.Game.Server
{
    public static class Update
    {
        public static ulong Id { get; internal set; }
        public static bool IsViewUpdate => (Id % ServerDomain.Options.UpdatesPerViewUpdate) == 0;
    }
}
