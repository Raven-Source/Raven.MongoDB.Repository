#if MongoDB_Repository
namespace MongoDB.Repository
#else
namespace Raven.MongoDB.Repository
#endif
{
    internal static class Util
    {
        public const string PrimaryKeyName = "_id";
    }
}
