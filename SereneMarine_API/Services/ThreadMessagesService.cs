using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;
using MongoDB.Driver;
using System.Runtime.CompilerServices;
using WebApi.Models.ThreadMessages;
using System.Threading.Tasks.Dataflow;

namespace WebApi.Services
{
    public interface IThreadMessagesService
    {
        List<ThreadMessage> GetAll();
        List<ThreadMessage> GetByThread(string id);
        List<ThreadMessage> GetByUser(string id);
        ThreadMessage Create(ThreadMessage tm);
        void UpdateMessage(ThreadMessage tmParam);
        void DeleteByThread(string id);
        void DeleteByMessage(string id);
        void DeleteByUser(string id);
        void DeleteByThreadAndUser(string thread_id, string user_id);

    }
    public class ThreadMessagesService : IThreadMessagesService
    {
        private readonly IMongoCollection<ThreadMessage> _tm;
        private readonly IMongoCollection<Thread> _threads;
        private readonly IMongoCollection<User> _users;


        public ThreadMessagesService(IUserDatabseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _tm = database.GetCollection<ThreadMessage>(settings.ThreadMessagesCollectionName);
            _threads = database.GetCollection<Thread>(settings.ThreadsCollectionName);
            _users = database.GetCollection<User>(settings.UsersCollectionName);

        }

        public List<ThreadMessage> GetAll() =>
       _tm.Find(tm => true).ToList();

        public List<ThreadMessage> GetByThread(string id)
        {
            //use linq standard for inner join between two collecitons
            var query = from x in _tm.AsQueryable()
                        join y in _users.AsQueryable() on x.User_Id equals y.User_Id
                        where x.thread_id == id
                        select new ThreadMessage()
                        {
                            Id = x.Id,
                            thread_message_id = x.thread_message_id,
                            thread_id = x.thread_id,
                            User_Id = x.User_Id,
                            thread_message = x.thread_message,
                            replied_date = x.replied_date,
                            Name = y.FirstName + " " + y.LastName
                        };

            return query.ToList();
        }

        public List<ThreadMessage> GetByUser(string id)
        {
            return _tm.Find<ThreadMessage>(tm => tm.User_Id == id).ToList();
        }

        public ThreadMessage Create(ThreadMessage tm)
        {
            // validation
            //need to assign userid from bearer token to tm.user_id
            if (string.IsNullOrEmpty(tm.User_Id))
                throw new AppException("User_id is required"); 
            
            if (string.IsNullOrEmpty(tm.thread_id))
                throw new AppException("thread_id is required");

            //check if event actually exists
            //use linq standard for inner join between two collecitons
            var query = from x in _threads.AsQueryable()
                        join y in _tm.AsQueryable() on x.thread_id equals y.thread_id
                        into MatchedEvents
                        where (x.thread_id == tm.thread_id)
                        select new
                        {
                            thread_id = x.thread_id
                        };

            //see if query has any results
            if (!query.Any())
                throw new AppException($"Thread {tm.thread_id} does not exist");

            if (string.IsNullOrEmpty(tm.thread_message))
                throw new AppException("Thread_message is required");

            if (tm.replied_date == null || string.IsNullOrEmpty(Convert.ToString(tm.replied_date)) || tm.replied_date == default(DateTime))
                throw new AppException("ThreadMessage reply Date is required");

            _tm.InsertOne(tm);

            return tm;
        }

        public void UpdateMessage(ThreadMessage tmParam)
        {
            var pet = _tm.Find<ThreadMessage>(pet => pet.thread_message_id == tmParam.thread_message_id).SingleOrDefault();

            if (pet == null)
                throw new AppException("ThreadMessage not found");

            // update message if it has changed
            if (!string.IsNullOrWhiteSpace(tmParam.thread_message) && tmParam.thread_message != pet.thread_message)
            {
                // throw error if the new petition thread_message is already taken
                if (_tm.Find<ThreadMessage>(x => x.thread_message == tmParam.thread_message).FirstOrDefault() != null)
                    throw new AppException("ThreadMessage " + tmParam.thread_message + " is already taken");

                //assign event thread_message to model
                pet.thread_message = tmParam.thread_message;
            }
            _tm.ReplaceOne(pet => pet.thread_message_id == tmParam.thread_message_id, pet);
        }
        public void DeleteByThread(string id)
        {
            var tm = _tm.Find<ThreadMessage>(tm => tm.thread_id == id).FirstOrDefault();
            if (tm != null)
            {
                _tm.DeleteMany(tm => tm.thread_id == id);
            }
        }
        public void DeleteByMessage(string id)
        {
            var tm = _tm.Find<ThreadMessage>(tm => tm.thread_message_id == id).FirstOrDefault();
            if (tm != null)
            {
                _tm.DeleteMany(tm => tm.thread_message_id == id);
            }
        }
        public void DeleteByUser(string id)
        {
            var tm = _tm.Find<ThreadMessage>(tm => tm.User_Id == id).FirstOrDefault();
            if (tm != null)
            {
                _tm.DeleteMany(tm => tm.User_Id == id);
            }
        }
        public void DeleteByThreadAndUser(string thread_id, string user_id)
        {
            var tm = _tm.Find<ThreadMessage>(tm => tm.thread_id == thread_id && tm.User_Id == user_id).FirstOrDefault();
            if (tm != null)
            {
                _tm.DeleteMany(tm => tm.thread_id == thread_id && tm.User_Id == user_id);
            }
        }

    }
}
