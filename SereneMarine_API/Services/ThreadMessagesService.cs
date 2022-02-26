using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;

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
        private readonly IMongoCollection<ThreadMessage> _threadMessageCollection;
        private readonly IMongoCollection<Thread> _threadCollection;
        private readonly IMongoCollection<User> _userCollection;

        public ThreadMessagesService(IMongoClient client, IUserDatabseSettings settings)
        {
            IMongoDatabase database = client.GetDatabase(settings.DatabaseName);

            _threadMessageCollection = database.GetCollection<ThreadMessage>(settings.ThreadMessagesCollectionName);
            _threadCollection = database.GetCollection<Thread>(settings.ThreadsCollectionName);
            _userCollection = database.GetCollection<User>(settings.UsersCollectionName);

        }

        public List<ThreadMessage> GetAll()
        {
            return _threadMessageCollection.Find(tm => true).ToList();
        }

        public List<ThreadMessage> GetByThread(string id)
        {

            //use linq standard for inner join between two collections
            IQueryable<ThreadMessage> query = from x in _threadMessageCollection.AsQueryable()
                                              join y in _userCollection.AsQueryable() on x.User_Id equals y.User_Id
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
            return _threadMessageCollection.Find(tm => tm.User_Id == id).ToList();
        } 

        public ThreadMessage Create(ThreadMessage threadMessage)
        {

            if (string.IsNullOrEmpty(threadMessage.User_Id))
            {
                throw new AppException("User_id is required");
            }

            if (string.IsNullOrEmpty(threadMessage.thread_id))
            {
                throw new AppException("thread_id is required");
            }

            var query = from x in _threadCollection.AsQueryable()
                        join y in _threadMessageCollection.AsQueryable() on x.thread_id equals y.thread_id
                        into MatchedEvents
                        where (x.thread_id == threadMessage.thread_id)
                        select new
                        {
                            thread_id = x.thread_id
                        };

            //see if query has any results
            if (!query.Any())
            {
                throw new AppException($"Thread {threadMessage.thread_id} does not exist");
            }

            if (string.IsNullOrEmpty(threadMessage.thread_message))
            {
                throw new AppException("Thread_message is required");
            }

            if (threadMessage.replied_date == default(DateTime))
            {
                throw new AppException("ThreadMessage reply Date is required");
            }

            _threadMessageCollection.InsertOne(threadMessage);

            return threadMessage;
        }

        public void UpdateMessage(ThreadMessage threadMessage)
        {
            ThreadMessage threadMessageToUpdate = _threadMessageCollection.Find(pet => pet.thread_message_id == threadMessage.thread_message_id).SingleOrDefault();

            if (threadMessageToUpdate == null)
            {
                throw new AppException("ThreadMessage not found");
            }

            if (!string.IsNullOrWhiteSpace(threadMessage.thread_message) 
                && threadMessage.thread_message != threadMessageToUpdate.thread_message)
            {
                // throw error if the new petition thread_message is already taken
                if (_threadMessageCollection.Find(x => x.thread_message == threadMessage.thread_message).FirstOrDefault() != null)
                {
                    throw new AppException("ThreadMessage " + threadMessage.thread_message + " is already taken");
                }

                //assign event thread_message to model
                threadMessageToUpdate.thread_message = threadMessage.thread_message;
            }

            _threadMessageCollection.ReplaceOne(pet => pet.thread_message_id == threadMessage.thread_message_id, threadMessageToUpdate);
        }
        public void DeleteByThread(string id)
        {
            ThreadMessage threadMessage = _threadMessageCollection.Find(tm => tm.thread_id == id).FirstOrDefault();
            if (threadMessage != null)
            {
                _threadMessageCollection.DeleteMany(tm => tm.thread_id == id);
            }
        }
        public void DeleteByMessage(string id)
        {
            ThreadMessage threadMessage = _threadMessageCollection.Find(tm => tm.thread_message_id == id).FirstOrDefault();
            if (threadMessage != null)
            {
                _threadMessageCollection.DeleteMany(tm => tm.thread_message_id == id);
            }
        }
        public void DeleteByUser(string id)
        {
            ThreadMessage threadMessage = _threadMessageCollection.Find(tm => tm.User_Id == id).FirstOrDefault();
            if (threadMessage != null)
            {
                _threadMessageCollection.DeleteMany(tm => tm.User_Id == id);
            }
        }
        public void DeleteByThreadAndUser(string thread_id, string user_id)
        {

            ThreadMessage threadMessage = _threadMessageCollection.Find(tm => tm.thread_id == thread_id && tm.User_Id == user_id).FirstOrDefault();
            if (threadMessage != null)
            {
                _threadMessageCollection.DeleteMany(tm => tm.thread_id == thread_id && tm.User_Id == user_id);
            }
        }
    }
}