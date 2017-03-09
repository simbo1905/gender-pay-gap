namespace GenderPayGap.Models.SqlDatabase
{
    public enum UserStatuses:byte
    {
        Unknown=0,
        New=1,
        Suspended=2,
        Active=3,
        Retired = 4,
    }
    public enum OrganisationStatuses : byte
    {
        Unknown = 0,
        New = 1,
        Suspended = 2,
        Active = 3,
        Retired = 4,
        Pending = 5,
    }

    public enum AddressStatuses : byte
    {
        Unknown = 0,
        New = 1,
        Suspended = 2,
        Active = 3,
        Retired = 4,
        Pending
    }

    public enum ReturnStatuses : byte
    {
        Unknown = 0,
        Draft = 1,
        Suspended = 2,
        Submitted = 3,
        Retired=4,
    }

    public enum SectorTypes : int
    {
        Unknown = 0,
        Private = 1,
        Public = 2,
    }

    public enum OrgTypes : int
    {
        Unknown = 0,
        PrivateCompany = 1,
        PublicCompany = 2,
        LimitedLiabilityPartnership=3,
        Charity = 4,
        Government = 5
    }


}
