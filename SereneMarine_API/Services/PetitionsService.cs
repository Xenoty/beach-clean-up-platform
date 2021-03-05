using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;

namespace WebApi.Services
{
    public interface IPetitionService
    {
        List<Petition> GetAll();
        Petition GetById(string id);
        List<Petition> GetByCompletion(bool val);
        Petition GetByUser(string id);
        Petition Create(Petition pet);
        void Update(Petition pet);
        void Delete(string id);
    }

    public class PetitionsSevice : IPetitionService
    {
        private readonly IMongoCollection<Petition> _petitions;
        private readonly IMongoCollection<PetitionSigned> _ps;

        public PetitionsSevice(IUserDatabseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _petitions = database.GetCollection<Petition>(settings.PetitionsCollectionName);
            _ps = database.GetCollection<PetitionSigned>(settings.PetitionsSignedCollectionName);
        }

        public List<Petition> GetAll()
        {
            var pt = _petitions.Find(x => true).ToList();

            var signatures = (from x in _petitions.AsQueryable()
                       join y in _ps.AsQueryable() on x.petition_id equals y.petition_id
                       select y).ToList();

            //for loop is x2 faster than linq
            for (int i = 0; i < pt.Count; i++)
            {
                int count = 0;
                for (int j = 0; j < signatures.Count; j++)
                {
                    if (pt[i].petition_id == signatures[j].petition_id)
                    {
                        count++;
                    }
                }
                pt[i].current_signatures = count;
            }

            return pt;
        }

        public Petition GetById(string id)
        {
            var pt = _petitions.Find<Petition>(pet => pet.petition_id == id).FirstOrDefault();
            var signatures = _ps.Find(x => x.petition_id == id).ToList().Count();
            pt.current_signatures = signatures;
            return pt;
        }
        public List<Petition> GetByCompletion(bool val)
        {
            return _petitions.Find<Petition>(pet => pet.completed == val).ToList();
        }

        public Petition GetByUser(string id)
        {
            return _petitions.Find<Petition>(pet => pet.User_Id == id).FirstOrDefault();
        }

        public Petition Create(Petition pet)
        {
            // validation
            //need to assign userid from bearer token to pet.user_id
            if (string.IsNullOrEmpty(pet.User_Id))
                throw new AppException("User_id is required");
            //check if event name has been taken
            // throw error if the new username is already taken
            if (_petitions.Find<Petition>(x => x.name == pet.name).FirstOrDefault() != null)
                throw new AppException("Petition " + pet.name + " is already taken");

            if (string.IsNullOrEmpty(pet.name))
                throw new AppException("Petition Name is required");

            if (string.IsNullOrEmpty(pet.description))
                throw new AppException("Petition Description is required");

            if (pet.created_date == null || string.IsNullOrEmpty(Convert.ToString(pet.created_date)))
                throw new AppException("Petition Start Date is required");

            if (string.IsNullOrEmpty(pet.required_signatures.ToString()) || pet.required_signatures == 0)
                throw new AppException("Max attendance is required");

            _petitions.InsertOne(pet);

            return pet;
        }

        public void Update(Petition petParam)
        {
            var pet = _petitions.Find<Petition>(pet => pet.petition_id == petParam.petition_id).SingleOrDefault();

            if (pet == null)
                throw new AppException("Petition not found");

            // update event name if it has changed
            if (!string.IsNullOrWhiteSpace(petParam.name) && petParam.name != pet.name)
            {
                // throw error if the new petition name is already taken
                if (_petitions.Find<Petition>(x => x.name == petParam.name).FirstOrDefault() != null)
                    throw new AppException("Petition " + petParam.name + " is already taken");

                //assign event name to model
                pet.name = petParam.name;
            }

            // update pet properties if provided
            if (!string.IsNullOrEmpty(petParam.description))
                pet.description = petParam.description;

            if (!string.IsNullOrWhiteSpace(petParam.required_signatures.ToString()) && petParam.required_signatures != 0)
                pet.required_signatures = petParam.required_signatures;

            if (petParam.completed != pet.completed)
                pet.completed = petParam.completed;

            _petitions.ReplaceOne(pet => pet.petition_id == petParam.petition_id, pet);
        }

        public void Delete(string id)
        {
            var pet = _petitions.Find<Petition>(pet => pet.petition_id == id).FirstOrDefault();
            if (pet != null)
            {
                _petitions.DeleteOne(pet => pet.petition_id == id);
            }
        }

    }

}
