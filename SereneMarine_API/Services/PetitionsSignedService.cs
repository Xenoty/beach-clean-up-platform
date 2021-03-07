using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;

namespace WebApi.Services
{
    public interface IPetitionsSignedService
    {
        List<PetitionSigned> GetAll();
        List<PetitionSigned> GetByPetition(string id);
        List<PetitionSigned> GetByUser(string id);
        PetitionSigned Create(PetitionSigned ps);
        void DeleteByPetition(string id);
        void DeleteByUser(string id);
        void DeleteByPetitionAndUser(string pet_id, string user_id);

    }
    public class PetitionsSignedService : IPetitionsSignedService
    {
        private readonly IMongoCollection<PetitionSigned> _ps;
        private readonly IMongoCollection<Petition> _pet;

        public PetitionsSignedService(IUserDatabseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _ps = database.GetCollection<PetitionSigned>(settings.PetitionsSignedCollectionName);
            _pet = database.GetCollection<Petition>(settings.PetitionsCollectionName);
        }

        public List<PetitionSigned> GetAll() => _ps.Find(ps => true).ToList();

        public List<PetitionSigned> GetByPetition(string id) => _ps.Find(ps => ps.petition_id == id).ToList();

        public List<PetitionSigned> GetByUser(string id) => _ps.Find(ps => ps.User_Id == id).ToList();

        public PetitionSigned Create(PetitionSigned ps)
        {
            // validation
            //need to assign userid from bearer token to ps.user_id
            if (string.IsNullOrEmpty(ps.User_Id))
                throw new AppException("User_id is required");

            //check if event actually exists
            //use linq standard for inner join between two collecitons
            var query = from x in _pet.AsQueryable()
                        join y in _ps.AsQueryable() on x.petition_id equals y.petition_id
                        into MatchedEvents
                        where (x.petition_id == ps.petition_id)
                        select new
                        {
                            petition_id = x.petition_id
                        };

            //see if query has any results
            if (!query.Any())
                throw new AppException($"Event {ps.petition_id} does not exist");

            // throw error if the user has already accepted an event abd event exists
            if (_ps.Find(x => x.User_Id == ps.User_Id && x.petition_id == ps.petition_id).FirstOrDefault() != null)
                throw new AppException("User has already participated in event");

            if (ps.signed_date == null || string.IsNullOrEmpty(Convert.ToString(ps.signed_date)) || ps.signed_date == default(DateTime))
                throw new AppException("PetitionSigned Start Date is required");

            _ps.InsertOne(ps);

            return ps;
        }
        public void DeleteByPetition(string id)
        {
            PetitionSigned ps = _ps.Find(ps => ps.petition_id == id).FirstOrDefault();
            if (ps != null)
            {
                _ps.DeleteMany(ps => ps.petition_id == id);
            }
        }
        public void DeleteByUser(string id)
        {
            PetitionSigned ps = _ps.Find(ps => ps.User_Id == id).FirstOrDefault();
            if (ps != null)
            {
                _ps.DeleteMany(ps => ps.User_Id == id);
            }
        }

        public void DeleteByPetitionAndUser(string petition_id, string user_id)
        {
            PetitionSigned ps = _ps.Find(ps => ps.petition_id == petition_id && ps.User_Id == user_id).FirstOrDefault();
            if (ps != null)
            {
                _ps.DeleteOne(ps => ps.petition_id == petition_id && ps.User_Id == user_id);
            }
        }
    }
}
