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
    public interface IThreadsService
    {
        List<Thread> GetAll();
        Thread GetById(string id);
        List<Thread> GetByUser(string id);
        Thread Create(Thread th);
        void Update(Thread threadParam);
        void DeleteByThread(string id);
        void DeleteByUser(string id);
    }
    public class ThreadsService : IThreadsService
    {
        private readonly IMongoCollection<Thread> _threadCollection;
        private readonly IMongoCollection<User> _userCollection;

        private ICluster _ICluster;

        public ThreadsService(IMongoClient client, IUserDatabseSettings settings)
        {
            IMongoDatabase database = client.GetDatabase(settings.DatabaseName);
            _ICluster = client.Cluster;

            _threadCollection = database.GetCollection<Thread>(settings.ThreadsCollectionName);
            _userCollection = database.GetCollection<User>(settings.UsersCollectionName);
        }

        public List<Thread> GetAll() 
        {
            if (!_ICluster.Description.State.IsConnected())
            {
                throw new AppException(AppSettings.DBDisconnectedMessage);
            }
            return _threadCollection.Find(th => true).ToList();  
        }

        public Thread GetById(string id)
        {
            if(!_ICluster.Description.State.IsConnected())
            {
                throw new AppException(AppSettings.DBDisconnectedMessage);
            }
            return _threadCollection.Find(th => th.thread_id == id).FirstOrDefault(); 
        }

        public List<Thread> GetByUser(string id)
        {
            if(!_ICluster.Description.State.IsConnected())
            {
                throw new AppException(AppSettings.DBDisconnectedMessage);
            }
            return _threadCollection.Find(th => th.User_Id == id).ToList(); 
        }

        public Thread Create(Thread thread)
        {
            if (!_ICluster.Description.State.IsConnected())
            {
                throw new AppException(AppSettings.DBDisconnectedMessage);
            }

            if (string.IsNullOrEmpty(thread.User_Id))
            {
                throw new AppException("User_id is required");
            }
                
            if (_threadCollection.Find(x => x.thread_topic == thread.thread_topic).FirstOrDefault() != null)
            {
                throw new AppException("Thread " + thread.thread_topic + " is already taken");
            }

            if (string.IsNullOrEmpty(thread.thread_topic))
            {
                throw new AppException("Thread Name is required");
            }

            if (string.IsNullOrEmpty(thread.thread_descr))
            {
                throw new AppException("Thread Description is required");
            }

            if (thread.created_date == default(DateTime))
            {
                throw new AppException("Thread Created Date is required");
            }

            User user = _userCollection.Find(u => u.User_Id == thread.User_Id).SingleOrDefault();
            if (user == null)
            {
                throw new AppException("Could not find matching user_id");
            }

            thread.author = user.FirstName + " " + user.LastName;

            _threadCollection.InsertOne(thread);

            return thread;
        }

        public void Update(Thread thread)
        {
            if (!_ICluster.Description.State.IsConnected())
            {
                throw new AppException(AppSettings.DBDisconnectedMessage);
            }

            Thread threadToUpdate = _threadCollection.Find(th => th.thread_id == thread.thread_id).SingleOrDefault();

            if (threadToUpdate == null)
            {
                throw new AppException("Thread not found");
            }

            if (!string.IsNullOrWhiteSpace(thread.thread_topic) 
                && thread.thread_topic != threadToUpdate.thread_topic)
            {
                if (_threadCollection.Find(x => x.thread_topic == thread.thread_topic).FirstOrDefault() != null)
                {
                    throw new AppException("Thread " + thread.thread_topic + " is already taken");
                }

                //assign event name to model
                threadToUpdate.thread_topic = thread.thread_topic;
            }

            if (!string.IsNullOrEmpty(thread.thread_descr))
            {
                threadToUpdate.thread_descr = thread.thread_descr;
            }

            if (thread.created_date != default(DateTime))
            {
                threadToUpdate.created_date = thread.created_date;
            }
                
            if (!string.IsNullOrEmpty(thread.author))
            {
                threadToUpdate.author = thread.author;
            }

            _threadCollection.ReplaceOne(th => th.thread_id == thread.thread_id, threadToUpdate);
        }

        public void DeleteByThread(string id)
        {
            if (!_ICluster.Description.State.IsConnected())
            {
                throw new AppException(AppSettings.DBDisconnectedMessage);
            }
            Thread threadToDelete = _threadCollection.Find(th => th.thread_id == id).FirstOrDefault();
            if (threadToDelete != null)
            {
                _threadCollection.DeleteOne(th => th.thread_id == id);
            }
        }

        public void DeleteByUser(string id)
        {
            if (!_ICluster.Description.State.IsConnected())
            {
                throw new AppException(AppSettings.DBDisconnectedMessage);
            }

            Thread threadToDelete = _threadCollection.Find(th => th.User_Id == id).FirstOrDefault();
            if (threadToDelete != null)
            {
                _threadCollection.DeleteMany(th => th.User_Id == id);
            }
        }
    }
}