using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;

namespace MONGO_CRUD_UPDATE
{
    class Program
    {

        //_client = new MongoClient("mongodb://localhost:27017");
        //_database = _client.GetDatabase("ChannelDB");
        //    _usersCollection = _database.GetCollection<User>("users");
        static void Main(string[] args)
        {
            var _client = new MongoClient("mongodb://localhost:27017");
            var _database = _client.GetDatabase("ChannelDB");
            var _students = _database.GetCollection<Student>("students");

            var filter = Builders<Student>.Filter;
            var studentIdAndCourseIdFilter = filter.And(
              filter.Eq(x => x.Id, "234dssfcv456"),
              filter.ElemMatch(x => x.Courses, c => c.Level == "Intermediate"));
            // find student with id and course id
            var student = _students.Find(studentIdAndCourseIdFilter).SingleOrDefault();

            // update with positional operator
            var update = Builders<Student>.Update;
            var courseLevelSetter = update.Set("Courses.$.Name", "Life Science");
            _students.UpdateOne(studentIdAndCourseIdFilter, courseLevelSetter);

            Console.WriteLine("Hello World!");
        }
    }

    public class Student
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public Course[] Courses { get; set; }
    }

    public class Course
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Level { get; set; }
    }
}
