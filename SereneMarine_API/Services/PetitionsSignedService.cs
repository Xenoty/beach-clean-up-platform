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
        private readonly IMongoCollection<PetitionSigned> _petitionSignedCollection;
        private readonly IMongoCollection<Petition> _petitionCollection;

        public PetitionsSignedService(IUserDatabseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _petitionSignedCollection = database.GetCollection<PetitionSigned>(settings.PetitionsSignedCollectionName);
            _petitionCollection = database.GetCollection<Petition>(settings.PetitionsCollectionName);
        }

        public List<PetitionSigned> GetAll() => _petitionSignedCollection.Find(ps => true).ToList();

        public List<PetitionSigned> GetByPetition(string id) => _petitionSignedCollection.Find(ps => ps.petition_id == id).ToList();

        public List<PetitionSigned> GetByUser(string id) => _petitionSignedCollection.Find(ps => ps.User_Id == id).ToList();

        public PetitionSigned Create(PetitionSigned petitionSigned)
        {
            if (string.IsNullOrEmpty(petitionSigned.User_Id))
            {
                throw new AppException("User_id is required");
            }

            var query = from x in _petitionCollection.AsQueryable()
                        join y in _petitionSignedCollection.AsQueryable() on x.petition_id equals y.petition_id
                        into MatchedEvents
                        where (x.petition_id == petitionSigned.petition_id)
                        select new
                        {
                            petition_id = x.petition_id
                        };

            if (!query.Any())
            {
                throw new AppException($"Event {petitionSigned.petition_id} does not exist");
            }

            bool userHasAlreadySignedPetition = _petitionSignedCollection.Find(x => x.User_Id == petitionSigned.User_Id && x.petition_id == petitionSigned.petition_id).FirstOrDefault() != null;
            if (userHasAlreadySignedPetition)
            {
                throw new AppException("User has already participated in event");
            }

            if (petitionSigned.signed_date == default(DateTime))
            {
                throw new AppException("PetitionSigned Start Date is required");
            }

            _petitionSignedCollection.InsertOne(petitionSigned);

            return petitionSigned;
        }
        public void DeleteByPetition(string id)
        {
            PetitionSigned petitionSigned = _petitionSignedCollection.Find(ps => ps.petition_id == id).FirstOrDefault();
            if (petitionSigned != null)
            {
                _petitionSignedCollection.DeleteMany(ps => ps.petition_id == id);
            }
        }

        public void DeleteByUser(string id)
        {
            PetitionSigned petitionSigned = _petitionSignedCollection.Find(ps => ps.User_Id == id).FirstOrDefault();
            if (petitionSigned != null)
            {
                _petitionSignedCollection.DeleteMany(ps => ps.User_Id == id);
            }
        }

        public void DeleteByPetitionAndUser(string petition_id, string user_id)
        {
            PetitionSigned petitionSigned = _petitionSignedCollection.Find(ps => ps.petition_id == petition_id && ps.User_Id == user_id).FirstOrDefault();
            if (petitionSigned != null)
            {
                _petitionSignedCollection.DeleteOne(ps => ps.petition_id == petition_id && ps.User_Id == user_id);
            }
        }
    }
}