namespace Game.Entities.Character
{
    //Ceiling detection methods;
    //'OnlyCheckFirstContact' - Only check the very first collision contact. This option is slightly faster but less accurate than the other two options.
    //'CheckAllContacts' - Check all contact points and register a ceiling hit as long as just one contact qualifies.
    //'CheckAverageOfAllContacts' - Calculate an average surface normal to check against.
    public enum CeilingDetectionMethod
    {
        OnlyCheckFirstContact,
        CheckAllContacts,
        CheckAverageOfAllContacts
    }
}