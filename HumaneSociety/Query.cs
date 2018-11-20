﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumaneSociety
{
    public static class Query
    {

        internal static List<USState> GetStates()
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();

            List<USState> allStates = db.USStates.ToList();

            return allStates;
        }

        internal static Client GetClient(string userName, string password)
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();

            Client client = db.Clients.Where(c => c.UserName == userName && c.Password == password).Single();

            return client;
        }

        internal static List<Client> GetClients()
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();

            List<Client> allClients = db.Clients.ToList();

            return allClients;
        }

        internal static void AddNewClient(string firstName, string lastName, string username, string password, string email, string streetAddress, int zipCode, int stateId)
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();

            Client newClient = new Client();

            newClient.FirstName = firstName;
            newClient.LastName = lastName;
            newClient.UserName = username;
            newClient.Password = password;
            newClient.Email = email;

            Address addressFromDb = db.Addresses.Where(a => a.AddressLine1 == streetAddress && a.Zipcode == zipCode && a.USStateId == stateId).FirstOrDefault();

            // if the address isn't found in the Db, create and insert it
            if (addressFromDb == null)
            {
                Address newAddress = new Address();
                newAddress.AddressLine1 = streetAddress;
                newAddress.AddressLine2 = null;
                newAddress.Zipcode = zipCode;
                newAddress.USStateId = stateId;

                db.Addresses.Add(newAddress);
                db.SubmitChanges();

                addressFromDb = newAddress;
            }

            // attach AddressId to clientFromDb.AddressId
            newClient.AddressId = addressFromDb.AddressId;

            db.Clients.Add(newClient);

            db.SubmitChanges();
        }

        internal static void UpdateClient(Client clientWithUpdates)
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();

            // find corresponding Client from Db
            Client clientFromDb = db.Clients.Where(c => c.ClientId == clientWithUpdates.ClientId).Single();

            // update clientFromDb information with the values on clientWithUpdates (aside from address)
            clientFromDb.FirstName = clientWithUpdates.FirstName;
            clientFromDb.LastName = clientWithUpdates.LastName;
            clientFromDb.UserName = clientWithUpdates.UserName;
            clientFromDb.Password = clientWithUpdates.Password;
            clientFromDb.Email = clientWithUpdates.Email;

            // get address object from clientWithUpdates
            Address clientAddress = clientWithUpdates.Address;

            // look for existing Address in Db (null will be returned if the address isn't already in the Db
            Address updatedAddress = db.Addresses.Where(a => a.AddressLine1 == clientAddress.AddressLine1 && a.USStateId == clientAddress.USStateId && a.Zipcode == clientAddress.Zipcode).FirstOrDefault();

            // if the address isn't found in the Db, create and insert it
            if(updatedAddress == null)
            {
                Address newAddress = new Address();
                newAddress.AddressLine1 = clientAddress.AddressLine1;
                newAddress.AddressLine2 = null;
                newAddress.Zipcode = clientAddress.Zipcode;
                newAddress.USStateId = clientAddress.USStateId;

                db.Addresses.Add(newAddress);
                db.SubmitChanges();

                updatedAddress = newAddress;
            }

            // attach AddressId to clientFromDb.AddressId
            clientFromDb.AddressId = updatedAddress.AddressId;
            
            // submit changes
            db.SubmitChanges();
        }

        internal static Employee RetrieveEmployeeUser(string email, int employeeNumber)
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();

            Employee employeeFromDb = db.Employees.Where(e => e.Email == email && e.EmployeeNumber == employeeNumber).FirstOrDefault();

            if(employeeFromDb == null)
            {
                throw new NullReferenceException();            
            }
            else
            {
                return employeeFromDb;
            }            
        }

        internal static Employee EmployeeLogin(string userName, string password)
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();

            Employee employeeFromDb = db.Employees.Where(e => e.UserName == userName && e.Password == password).FirstOrDefault();

            return employeeFromDb;
        }

        internal static bool CheckEmployeeUserNameExist(string userName)
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();

            Employee employeeWithUserName = db.Employees.Where(e => e.UserName == userName).FirstOrDefault();

            return employeeWithUserName == null;
        }

        internal static void AddUsernameAndPassword(Employee employee)
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();

            Employee employeeFromDb = db.Employees.Where(e => e.EmployeeId == employee.EmployeeId).FirstOrDefault();

            employeeFromDb.UserName = employee.UserName;
            employeeFromDb.Password = employee.Password;

            db.SubmitChanges();
        }

        internal static int GetCategoryId(string species)
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();

            return db.Categories.Where(c => c.Name == species).Select(c => c.CategoryId).Single();
        }

        internal static int GetDietPlanId(string dietPlan)
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();

            return db.DietPlans.Where(d => d.Name == dietPlan).Select(d => d.DietPlanId).Single();
        }

        internal static void AddAnimal(Animal animal)
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();

            db.Animals.InsertOnSubmit(animal);
            try
            {
                db.SubmitChanges();
            }
            catch
            {
                Console.WriteLine("Error!");
            }
        }
        internal static IQueryable<Animal> SearchForAnimalByMultipleTraits(Dictionary<int, string> searchCriteria)
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();
            IQueryable<Animal> animals = db.Animals;
            foreach (KeyValuePair<int, string> criteria in searchCriteria)
            {

                if (criteria.Key == 1)
                {
                    int categoryId = GetCategoryId(criteria.Value);
                    animals = animals.Where(c => c.CategoryId == categoryId);
                }
                else if (criteria.Key == 2)
                {
                    animals = animals.Where(n => n.Name == criteria.Value);
                }
                else if (criteria.Key == 3)
                {
                    int age = Convert.ToInt32(criteria.Value);
                    animals = animals.Where(a => a.Age == age);
                }
                else if (criteria.Key == 4)
                {
                    animals = animals.Where(d => d.Demeanor == criteria.Value);
                }
                else if (criteria.Key == 5)
                {
                    bool kidFriendly = criteria.Value == "True" ? true : false;
                    animals = animals.Where(k => k.KidFriendly == kidFriendly);
                }
                else if (criteria.Key == 6)
                {
                    bool petFriendly = criteria.Value == "True" ? true : false;
                    animals = animals.Where(p => p.PetFriendly == petFriendly);
                }
                else if (criteria.Key == 7)
                {
                    int weight = Convert.ToInt32(criteria.Value);
                    animals = animals.Where(w => w.Weight == weight);
                }
                else if (criteria.Key == 8)
                {
                    int animalId = Convert.ToInt32(criteria.Value);
                    animals = animals.Where(a => a.AnimalId == animalId);
                }

            }
            return animals;
        }
    }
}