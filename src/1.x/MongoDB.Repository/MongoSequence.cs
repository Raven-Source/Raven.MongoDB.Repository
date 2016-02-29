
namespace MongoDB.Repository
{

    public class MongoSequence
    {
        public MongoSequence()
        {
            this.Sequence = "Sequence";
            this.CollectionName = "CollectionName";
            this.IncrementID = "IncrementID";
        }

        public MongoSequence(string sequence, string collectionName, string incrementID)
        {
            this.Sequence = sequence;
            this.CollectionName = collectionName;
            this.IncrementID = incrementID;
        }

        public string CollectionName { get; set; }

        public string IncrementID { get; set; }

        public string Sequence { get; set; }
    }
}

