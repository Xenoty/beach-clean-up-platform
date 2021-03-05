using AutoMapper;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
        private readonly IMongoCollection<Thread> _threads;
        private readonly IMongoCollection<User> _users;

        public ThreadsService(IUserDatabseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _threads = database.GetCollection<Thread>(settings.ThreadsCollectionName);
            _users = database.GetCollection<User>(settings.UsersCollectionName);
        }

        //public List<Thread> GetAll()
        //{
        //    //need to add if statement if author name is missing
        //    var th = _threads.Find(th => true).ToList();
        //    //use linq standard for inner join between two collecitons
        //    //var query = from x in _threads.AsQueryable()
        //    //            join y in _users.AsQueryable() on x.User_Id equals y.User_Id
        //    //            select new Thread()
        //    //            {
        //    //                thread_id = x.thread_id,
        //    //                User_Id = x.User_Id,
        //    //                thread_topic = x.thread_topic,
        //    //                thread_descr = x.thread_descr,
        //    //                created_date = x.created_date,
        //    //                thread_closed = x.thread_closed,
        //    //                author = y.FirstName + " " + y.LastName
        //    //            };

        //    return th;
        //}
        public List<Thread> GetAll() =>
        _threads.Find(th => true).ToList();

        public Thread GetById(string id)
        {
            return _threads.Find<Thread>(th => th.thread_id == id).FirstOrDefault();
        }

        public List<Thread> GetByUser(string id)
        {
            return _threads.Find<Thread>(th => th.User_Id == id).ToList();
        }

        public Thread Create(Thread th)
        {
            // validation
            //need to assign userid from bearer token to th.user_id
            if (string.IsNullOrEmpty(th.User_Id))
                throw new AppException("User_id is required");
            //check if event name has been taken
            // throw error if the new username is already taken
            if (_threads.Find<Thread>(x => x.thread_topic == th.thread_topic).FirstOrDefault() != null)
                throw new AppException("Thread " + th.thread_topic + " is already taken");

            if (string.IsNullOrEmpty(th.thread_topic))
                throw new AppException("Thread Name is required");

            if (string.IsNullOrEmpty(th.thread_descr))
                throw new AppException("Thread Description is required");

            if (th.created_date == null || string.IsNullOrEmpty(Convert.ToString(th.created_date)))
                throw new AppException("Thread Created Date is required");

            if (string.IsNullOrEmpty(th.thread_closed.ToString()) || th.thread_closed == null)
                throw new AppException("Thread_closed is required");

            //need to insert author name for ease of access in web end
            var query = (from x in _threads.AsQueryable()
                         join y in _users.AsQueryable() on x.User_Id equals y.User_Id
                         select new Thread()
                         {
                             author = y.FirstName + " " + y.LastName
                         }).FirstOrDefault();

            if (string.IsNullOrEmpty(query.author))
                throw new AppException($"Error binding user_id and names");

            th.author = query.author;

            //var query2 = (from y in _users.AsQueryable()
            //             where y.User_Id == th.User_Id
            //             select new UserModel { 
            //                User_Id = y.User_Id,
            //                FirstName = y.FirstName,
            //                LastName = y.LastName,
            //                Email_address = y.Email_address,
            //                Role = y.Role,
            //                Username = y.Username,
            //                ContactNo = y.ContactNo,
            //                Address = y.Address,
            //                Joined = y.Joined
            //             }).FirstOrDefault();


            //if (query2 == null)
            //    throw new AppException($"Error binding user_id and names");

            //th.user = query2;

            _threads.InsertOne(th);

            return th;
        }

        public void Update(Thread threadParam)
        {
            var th = _threads.Find<Thread>(th => th.thread_id == threadParam.thread_id).SingleOrDefault();

            if (th == null)
                throw new AppException("Thread not found");

            // update event name if it has changed
            if (!string.IsNullOrWhiteSpace(threadParam.thread_topic) && threadParam.thread_topic != th.thread_topic)
            {
                // throw error if the new event name is already taken
                if (_threads.Find<Thread>(x => x.thread_topic == threadParam.thread_topic).FirstOrDefault() != null)
                    throw new AppException("Thread " + threadParam.thread_topic + " is already taken");

                //assign event name to model
                th.thread_topic = threadParam.thread_topic;
            }

            // update th properties if provided
            if (!string.IsNullOrEmpty(threadParam.thread_descr))
                th.thread_descr = threadParam.thread_descr;

            if (!string.IsNullOrWhiteSpace(threadParam.created_date.ToString()) && threadParam.created_date != default(DateTime))
                th.created_date = threadParam.created_date;

            if (!string.IsNullOrEmpty(threadParam.author))
                th.author = threadParam.author;

            _threads.ReplaceOne(th => th.thread_id == threadParam.thread_id, th);
        }

        public void DeleteByThread(string id)
        {
            var th = _threads.Find<Thread>(th => th.thread_id == id).FirstOrDefault();
            if (th != null)
            {
                _threads.DeleteOne(th => th.thread_id == id);
            }
        }

        public void DeleteByUser(string id)
        {
            var th = _threads.Find<Thread>(th => th.User_Id == id).FirstOrDefault();
            if (th != null)
            {
                _threads.DeleteMany(th => th.User_Id == id);
            }
        }


    }
}
