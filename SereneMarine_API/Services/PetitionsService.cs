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
        private readonly IMongoCollection<Petition> _petitionCollection;
        private readonly IMongoCollection<PetitionSigned> _petitionSignedCollection;

        private ICluster _ICluster;

        public PetitionsSevice(IMongoClient client, IUserDatabseSettings settings)
        {
            IMongoDatabase database = client.GetDatabase(settings.DatabaseName);
            _ICluster = client.Cluster;

            _petitionCollection = database.GetCollection<Petition>(settings.PetitionsCollectionName);
            _petitionSignedCollection = database.GetCollection<PetitionSigned>(settings.PetitionsSignedCollectionName);
        }

        public List<Petition> GetAll()
        {
            if (!_ICluster.Description.State.IsConnected())
            {
                throw new AppException(AppSettings.DBDisconnectedMessage);
            }

            return _petitionCollection.Find(x => true).ToList();
        }

        public Petition GetById(string id)
        {
            if (!_ICluster.Description.State.IsConnected())
            {
                throw new AppException(AppSettings.DBDisconnectedMessage);
            }

            return _petitionCollection.Find(pet => pet.petition_id == id).FirstOrDefault();
        }

        public List<Petition> GetByCompletion(bool isComplete)
        {
            if (!_ICluster.Description.State.IsConnected())
            {
                throw new AppException(AppSettings.DBDisconnectedMessage);
            }

            return _petitionCollection.Find(pet => pet.completed == isComplete).ToList();
        }

        public Petition GetByUser(string id)
        {
            if (!_ICluster.Description.State.IsConnected())
            {
                throw new AppException(AppSettings.DBDisconnectedMessage);
            }

            return _petitionCollection.Find(pet => pet.User_Id == id).FirstOrDefault();
        }

        public Petition Create(Petition petition)
        {
            if (!_ICluster.Description.State.IsConnected())
            {
                throw new AppException(AppSettings.DBDisconnectedMessage);
            }

            //need to assign userid from bearer token to pet.user_id
            if (string.IsNullOrEmpty(petition.User_Id))
            {
                throw new AppException("User_id is required");
            }

            // throw error if the new username is already taken
            if (_petitionCollection.Find(x => x.name == petition.name).FirstOrDefault() != null)
            {
                throw new AppException("Petition " + petition.name + " is already taken");
            }

            if (string.IsNullOrEmpty(petition.name))
            {
                throw new AppException("Petition Name is required");
            }

            if (string.IsNullOrEmpty(petition.description))
            {
                throw new AppException("Petition Description is required");
            }

            if (petition.created_date == default(DateTime))
            {
                throw new AppException("Petition Start Date is required");
            }

            if (petition.required_signatures == default(int) || petition.required_signatures == 0)
            {
                throw new AppException("Max attendance is required");
            }

            _petitionCollection.InsertOne(petition);

            return petition;
        }

        public void Update(Petition petition)
        {
            if (!_ICluster.Description.State.IsConnected())
            {
                throw new AppException(AppSettings.DBDisconnectedMessage);
            }

            Petition petitionToUpdate = _petitionCollection.Find(pet => pet.petition_id == petition.petition_id).SingleOrDefault();

            if (petitionToUpdate == null)
            {
                throw new AppException("Petition not found");
            }

            // update event name if it has changed
            if (!string.IsNullOrWhiteSpace(petition.name))
            {
                // throw error if the new petition name is already taken
                if (_petitionCollection.Find(x => x.name == petition.name).FirstOrDefault() != null)
                {
                    throw new AppException("Petition " + petition.name + " is already taken");
                }

                //assign event name to model
                petitionToUpdate.name = petition.name;
            }

            if (!string.IsNullOrEmpty(petition.description))
            {
                petitionToUpdate.description = petition.description;
            }

            if (petition.required_signatures != 0)
            {
                petitionToUpdate.required_signatures = petition.required_signatures;
            }

            if (petition.completed != petitionToUpdate.completed)
            {
                petitionToUpdate.completed = petition.completed;
            }

            _petitionCollection.ReplaceOne(pet => pet.petition_id == petition.petition_id, petitionToUpdate);
        }

        public void Delete(string id)
        {
            if (!_ICluster.Description.State.IsConnected())
            {
                throw new AppException(AppSettings.DBDisconnectedMessage);
            }

            Petition pet = _petitionCollection.Find(pet => pet.petition_id == id).FirstOrDefault();
            if (pet != null)
            {
                _petitionCollection.DeleteOne(pet => pet.petition_id == id);
            }
        }
    }
}