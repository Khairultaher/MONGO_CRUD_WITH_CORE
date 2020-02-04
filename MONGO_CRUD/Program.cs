using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MONGO_CRUD
{
    class Program
    {
        static async Task Main(string[] args)
        {
            UsersRepository usersRepository = new UsersRepository();
            User user = new User
            {
                //{
                //    "_id" : ObjectId("59ce6b34f48f171624840b05"),
                //	"name" : "Nikola",
                //	"blog" : "rubikscode.net",
                //	"age" : 30,
                //	"location" : "Beograd"
                //    "Likes": [
                //	            {"userId":"59ce6b34f48f171624840b04","name";"Nikolaa"}
                //				{"userId":"59ce6b34f48f171624840b05","name";"Nikolaaa"}
                //	         ]	
                //}

                Id = new ObjectId().ToString(),
                Name = "Nikola",
                Blog = "rubikscode.net",
                Age = 20,
                Location = "Beograd",
                Likes = new Like[]
                { 
                    new Like{ UserId = "59ce6b34f48f171624840b04", Name = "A"},
                    new Like{ UserId = "59ce6b34f48f171624840b05", Name = "B"},
                    new Like{ UserId = "59ce6b34f48f171624840b05", Name = "C"}
                }
            };

            var likes = new List<Like>
                {
                    new Like{ UserId = "59ce6b34f48f171624840b04", Name = "C"},
                    new Like{ UserId = "59ce6b34f48f171624840b05", Name = "D"}
                };
            //await usersRepository.InsertUser(user);

            //Like like = new Like { UserId = "59ce6b34f48f171624840b07", Name = "D" };
            //var dd = usersRepository.AddLikes("5e39300c9e11ba7eb84b5f9b", like);


            usersRepository.UpdateLike("000000000000000000000000", "59ce6b34f48f171624840b04", "AA");

            var data = await usersRepository.GetAllUsers();


            Console.WriteLine("Press any key to continue...");
            Console.ReadLine();
        }
    }

    public class Like
    {
        [BsonElement("userId")]
        public string UserId { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }
    }
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("blog")]
        public string Blog { get; set; }

        [BsonElement("age")]
        public int Age { get; set; }

        [BsonElement("location")]
        public string Location { get; set; }

        public Like[] Likes { get; set; }
    }

    public class UsersRepository
    {
        private IMongoClient _client;
        private IMongoDatabase _database;
        private IMongoCollection<User> _usersCollection;

        public UsersRepository()
        {
            _client = new MongoClient("mongodb://localhost:27017");
            _database = _client.GetDatabase("ChannelDB");
            _usersCollection = _database.GetCollection<User>("users");
        }

        public async Task InsertUser(User user)
        {
            await _usersCollection.InsertOneAsync(user);
        }

        // Update inside a embaded document
        public void UpdateLike(string userId, string lileUserId, string val)
        {
            var _client = new MongoClient("mongodb://localhost:27017");
            var _database = _client.GetDatabase("ChannelDB");
            var _students = _database.GetCollection<User>("users");

            var filter = Builders<User>.Filter;
            var likeFilter = filter.And(
              filter.Eq(x => x.Id, userId),
              filter.ElemMatch(x => x.Likes, c => c.UserId == lileUserId));

            // find student with id and course id
            var student = _students.Find(likeFilter).SingleOrDefault();

            // update with positional operator
            var update = Builders<User>.Update;

            var likeUpdate = update.Set("Likes.$.name", val);
            _students.UpdateOne(likeFilter, likeUpdate);
        }

        // insert into a embaded document.
        // Insert new doc into nested doc.
        public async Task AddLikes(string productId, Like like)
        {
            var filter = Builders<User>.Filter
                                       .And(Builders<User>.Filter.Where(x => x.Id == productId));

            var update = Builders<User>.Update.Push("Likes", like);
            await _usersCollection.FindOneAndUpdateAsync(filter, update);
        }

        public async Task<List<User>> GetAllUsers()
        {
            return await _usersCollection.Find(new BsonDocument()).ToListAsync();
        }

        public async Task<List<User>> GetUsersByField(string fieldName, string fieldValue)
        {
            var filter = Builders<User>.Filter.Eq(fieldName, fieldValue);
            var result = await _usersCollection.Find(filter).ToListAsync();

            return result;
        }

        public async Task<List<User>> GetUsers(int startingFrom, int count)
        {
            var result = await _usersCollection.Find(new BsonDocument())
            .Skip(startingFrom)
            .Limit(count)
            .ToListAsync();

            return result;
        }

        public async Task<bool> UpdateUser(ObjectId id, string udateFieldName, string updateFieldValue)
        {
            var filter = Builders<User>.Filter.Eq("_id", id);
            var update = Builders<User>.Update.Set(udateFieldName, updateFieldValue);

            var result = await _usersCollection.UpdateOneAsync(filter, update);

            return result.ModifiedCount != 0;
        }

        public async Task<bool> DeleteUserById(ObjectId id)
        {
            var filter = Builders<User>.Filter.Eq("_id", id);
            var result = await _usersCollection.DeleteOneAsync(filter);
            return result.DeletedCount != 0;
        }

        public async Task<long> DeleteAllUsers()
        {
            var filter = new BsonDocument();
            var result = await _usersCollection.DeleteManyAsync(filter);
            return result.DeletedCount;
        }

        //public async Task CreateIndexOnCollection(IMongoCollection<BsonDocument> collection, string field)
        //{
        //    var keys = Builders<BsonDocument>.IndexKeys.Ascending(field);
        //    await collection.Indexes.CreateOneAsync(keys);
        //}

        //public async Task CreateIndexOnNameField()
        //{
        //    var keys = Builders<User>.IndexKeys.Ascending(x => x.Name);
        //    await _usersCollection.Indexes.CreateOneAsync(keys);
        //}
    }
}
