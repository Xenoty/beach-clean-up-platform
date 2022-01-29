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
    public interface IUserService
    {
        User Authenticate(string username, string password);
        List<User> GetAll();
        User GetById(string id);
        User Create(User user, string password);
        void Update(User user, string password = null);
        void Delete(string id);
    }

    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _userCollection;
        private ICluster _ICluster;

        public UserService(IMongoClient client, IUserDatabseSettings settings)
        {
            IMongoDatabase database = client.GetDatabase(settings.DatabaseName);
            _ICluster = client.Cluster;

            _userCollection = database.GetCollection<User>(settings.UsersCollectionName);
        }

        public User Authenticate(string email, string password)
        {
            if (!_ICluster.Description.State.IsConnected())
            {
                throw new AppException(AppSettings.DBDisconnectedMessage);
            }

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            User user = _userCollection.Find(x => x.Email_address == email).FirstOrDefault();

            // check if username exists
            if (user == null)
            {
                return null;
            }

            // check if password is correct
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                return null;
            }

            // authentication successful
            return user;
        }

        //get users as list and not as IEnumberable
        public List<User> GetAll()
        {
            if (!_ICluster.Description.State.IsConnected())
            {
                throw new AppException(AppSettings.DBDisconnectedMessage);
            }
            return _userCollection.Find(user => true).ToList();
        }
        

        public User GetById(string id)
        {
            if (!_ICluster.Description.State.IsConnected())
            {
                throw new AppException(AppSettings.DBDisconnectedMessage);
            }
            return _userCollection.Find(user => user.User_Id == id).FirstOrDefault();                
        }
                

        public User Create(User user, string password)
        {
            if (!_ICluster.Description.State.IsConnected())
            {
                throw new AppException(AppSettings.DBDisconnectedMessage);
            }

            // validation
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new AppException("Password is required");
            }

            // throw error if the new username is already taken
            if (!string.IsNullOrEmpty(user.Username))
            {
                if (_userCollection.Find(x => x.Username == user.Username).Any())
                {
                    throw new AppException("Username '" + user.Username + "' is already taken");
                }
            }

            // throw error if the new email is already taken
            if (_userCollection.Find(x => x.Email_address == user.Email_address).Any())
            {
                throw new AppException("Email '" + user.Email_address + "' is already taken");
            }

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _userCollection.InsertOne(user);

            return user;
        }

        public void Update(User userParam, string password = null)
        {
            if (!_ICluster.Description.State.IsConnected())
            {
                throw new AppException(AppSettings.DBDisconnectedMessage);
            }

            User user = _userCollection.Find(user => user.User_Id == userParam.User_Id).SingleOrDefault();

            if (user == null)
            {
                throw new AppException("User not found");
            }

            // update username if it has changed
            if (!string.IsNullOrWhiteSpace(userParam.Username) && userParam.Username != user.Username)
            {
                // throw error if the new username is already taken
                if (_userCollection.Find(x => x.Username == userParam.Username).FirstOrDefault() != null)
                {
                    throw new AppException("Username '" + userParam.Username + "' is already taken");
                }

                // throw error if the new email is already taken
                if (_userCollection.Find(x => x.Email_address == userParam.Email_address).FirstOrDefault() != null)
                {
                    throw new AppException("Email '" + userParam.Email_address + "' is already taken");
                }

                user.Username = userParam.Username;
            }

            if (!string.IsNullOrWhiteSpace(userParam.FirstName))
            {
                user.FirstName = userParam.FirstName;
            }

            if (!string.IsNullOrWhiteSpace(userParam.LastName))
            {
                user.LastName = userParam.LastName;
            }

            if (!string.IsNullOrWhiteSpace(userParam.Role))
            {
                user.Role = userParam.Role;
            }

            if (!string.IsNullOrWhiteSpace(userParam.ContactNo.ToString()) || userParam.ContactNo != default(int))
            {
                user.ContactNo = userParam.ContactNo;
            }

            if (!string.IsNullOrWhiteSpace(userParam.Address))
            {
                user.Address = userParam.Address;
            }

            // update password if provided
            if (!string.IsNullOrWhiteSpace(password))
            {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash(password, out passwordHash, out passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            _userCollection.ReplaceOne(user => user.User_Id == userParam.User_Id, user);
        }

        public void Delete(string id)
        {
            if (!_ICluster.Description.State.IsConnected())
            {
                throw new AppException(AppSettings.DBDisconnectedMessage);
            }

            User user = _userCollection.Find(user => user.User_Id == id).FirstOrDefault();
            if (user != null)
            {
                _userCollection.DeleteOne(user => user.User_Id == id);
            }
        }

        #region Private Methods

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            using (System.Security.Cryptography.HMACSHA512 hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            }

            if (storedHash.Length != 64)
            {
                throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            }

            if (storedSalt.Length != 128)
            {
                throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");
            }

            using (System.Security.Cryptography.HMACSHA512 hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                byte[] computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }

        #endregion
    }
}