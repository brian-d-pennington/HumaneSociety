using System;
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

                db.Addresses.InsertOnSubmit(newAddress);
                db.SubmitChanges();

                addressFromDb = newAddress;
            }

            // attach AddressId to clientFromDb.AddressId
            newClient.AddressId = addressFromDb.AddressId;

            db.Clients.InsertOnSubmit(newClient);

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
            if (updatedAddress == null)
            {
                Address newAddress = new Address();
                newAddress.AddressLine1 = clientAddress.AddressLine1;
                newAddress.AddressLine2 = null;
                newAddress.Zipcode = clientAddress.Zipcode;
                newAddress.USStateId = clientAddress.USStateId;

                db.Addresses.InsertOnSubmit(newAddress);
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

            if (employeeFromDb == null)
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
                SetAnimalRoom();
            }
            catch
            {
                Console.WriteLine("Error!");
            }
        }
        internal static IQueryable<Animal> SearchForAnimalByMultipleTraits(Dictionary<int, string> searchCriteria, string user)
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();
            IQueryable<Animal> animals = db.Animals;
            if (user == "Customer")
            {
                animals = animals.Where(a => a.AdoptionStatus != "ADOPTED");
            }
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
                else if (criteria.Key == 9)
                {
                    int dietPlan = GetDietPlanId(criteria.Value);
                    animals = animals.Where(a => a.DietPlanId == dietPlan);
                }
            }
            return animals;
        }

        internal static void RemoveAnimal(Animal animal)
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();

            var selectedAnimal = db.Animals.Where(a => a.AnimalId == animal.AnimalId).FirstOrDefault();
            if (selectedAnimal != null)
            {
                db.Animals.DeleteOnSubmit(selectedAnimal);
                db.SubmitChanges();
            }
        }

        internal static IQueryable<AnimalShot> GetShots(Animal animal)
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();
            IQueryable<AnimalShot> animalShots = db.AnimalShots;
            animalShots = animalShots.Where(a => a.AnimalId == animal.AnimalId);
            return animalShots;
        }
        internal static IQueryable<Adoption> GetPendingAdoptions()
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();
            IQueryable<Adoption> pendingAdoptions = db.Adoptions;
            pendingAdoptions = pendingAdoptions.Where(a => a.ApprovalStatus == "PENDING");
            return pendingAdoptions;
        }

        internal static void UpdateAdoption(bool decision, Adoption adoption)
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();
            string status = decision == true ? "APPROVED" : "PENDING";
            adoption = (from x in db.Adoptions
                        where x.AdoptionId == adoption.AdoptionId
                        select x).First();
            adoption.ApprovalStatus = status;
            if (status == "APPROVED")
            {
                adoption.PaymentCollected = true;
                UpdateRoom(adoption.AnimalId);
            }
            db.SubmitChanges();
        }
        internal static void GiveShot(Animal animal)
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();
            IQueryable<Shot> shots = db.Shots;
            //shots = shots.Where(s => s.CategoryId == animal.CategoryId);
            shots.ToList();
            foreach (Shot shot in shots)
            {
                AnimalShot animalShot = new AnimalShot();
                animalShot.ShotId = shot.ShotId;
                animalShot.AnimalId = animal.AnimalId;
                animalShot.DateReceived = DateTime.Now;
                db.AnimalShots.InsertOnSubmit(animalShot);
                db.SubmitChanges();
            }
        }
        internal static void UpdateShot(string booster, Animal animal)
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();
            IQueryable<AnimalShot> animalshots = db.AnimalShots;
            animalshots = animalshots.Where(a => a.AnimalId == animal.AnimalId);
            for (int i = 1; i < animalshots.Count() + 1; i++)
            {
                AnimalShot animalShotInstance = new AnimalShot();
                animalShotInstance = (from x in db.AnimalShots
                                      where x.ShotId == i
                                      select x).Single();
                animalShotInstance.DateReceived = DateTime.Now;
                db.SubmitChanges();
            }
        }
        internal static Animal GetAnimalByID(int ID)
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();
            return db.Animals.Where(a => a.AnimalId == ID).Single();
        }

        internal static void Adopt(Animal animal, Client client)
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();
            Adoption adoption = new Adoption();
            adoption.ClientId = client.ClientId;
            adoption.AnimalId = animal.AnimalId;
            adoption.ApprovalStatus = "PENDING";
            adoption.AdoptionFee = 75;
            adoption.PaymentCollected = false;
            db.Adoptions.InsertOnSubmit(adoption);
            db.SubmitChanges();
        }

        internal static Room GetRoom(int animalId)
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();
            return db.Rooms.Where(r => r.AnimalId == animalId).Single();
        }

        internal static void RunEmployeeQueries(Employee employee, string thisCase, int employeeNumToUpdate = 0)
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();

            switch (thisCase)
            {
                case "create":
                    db.Employees.InsertOnSubmit(employee);
                    db.SubmitChanges();
                    break;
                case "read":
                    Employee employeeToRead = db.Employees.Where(e => e.EmployeeNumber == employee.EmployeeNumber).Single();
                    UserInterface.DisplayEmployeeInfo(employeeToRead);
                    break;
                case "update":
                    Employee employeeToUpdate = new Employee();
                    employeeToUpdate = (from e in db.Employees
                                        where e.EmployeeNumber == employeeNumToUpdate
                                        select e).Single();
                    employeeToUpdate.FirstName = employee.FirstName;
                    employeeToUpdate.LastName = employee.LastName;
                    employeeToUpdate.Email = employee.Email;
                    db.SubmitChanges();
                    break;
                case "delete":
                    Employee employeeToDelete = db.Employees.Where(e => e.LastName == employee.LastName).Single();
                    db.Employees.DeleteOnSubmit(employeeToDelete);
                    db.SubmitChanges();
                    break;
                default:
                    break;
            }
        }
        internal static void EnterAnimalUpdate(Animal animal, Dictionary<int, string> updateDict)
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();
            Animal animalToUpdate = new Animal();
            animalToUpdate = (from a in db.Animals
                              where a.AnimalId == animal.AnimalId
                              select a).Single();

            foreach (KeyValuePair<int,string> update in updateDict)
            {
                if (update.Key == 1)
                {
                    int categoryId = GetCategoryId(update.Value);
                    animalToUpdate.CategoryId = categoryId;
                }else if (update.Key == 2)
                {
                    animalToUpdate.Name = update.Value;
                }
                else if (update.Key == 3)
                {
                    int age = Convert.ToInt32(update.Value);
                    animalToUpdate.Age = age;
                }
                else if (update.Key == 4)
                {
                    animalToUpdate.Demeanor = update.Value;
                }
                else if (update.Key == 5)
                {
                    bool kidFriendly = update.Value == "True" ? true : false;
                    animalToUpdate.KidFriendly = kidFriendly;
                }
                else if (update.Key == 6)
                {
                    bool petFriendly = update.Value == "True" ? true : false;
                    animalToUpdate.PetFriendly = petFriendly;
                }
                else if (update.Key == 7)
                {
                    int weight = Convert.ToInt32(update.Value);
                    animalToUpdate.Weight = weight;
                }
                else if (update.Key == 9)
                {
                    int dietPlanId = Convert.ToInt32(update.Value);
                    animalToUpdate.DietPlanId =  dietPlanId;
                }
            }
        }
        internal static void AddCategoryIfNull(string species)
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();
            IQueryable<Category> categories = db.Categories;
            foreach (Category category in categories)
            {
                if (category.Name == species)
                {
                    return;
                }
            }
            Category categoryToInsert = new Category();
            categoryToInsert.Name = species;
            db.Categories.InsertOnSubmit(categoryToInsert);
            db.SubmitChanges();
        }
        internal static void SetAnimalRoom()
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();
            IQueryable<Room> rooms = db.Rooms;
            foreach (Room room in rooms)
            {
                if (room.AnimalId == null)
                {
                    room.AnimalId = db.Animals.Max(a => a.AnimalId);
                    db.SubmitChanges();
                    return;
                }
            }
            CreateNewRoom();
        }
        internal static void CreateNewRoom()
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();
            Room room = new Room();
            room.RoomNumber = db.Rooms.Max(r => r.RoomNumber) + 1;
            db.Rooms.InsertOnSubmit(room);
            db.SubmitChanges();
            SetAnimalRoom();
        }
        internal static void UpdateRoom(int? animalId)
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();
            Room room = db.Rooms.Where(r => r.AnimalId == animalId).Single();
            room.AnimalId = null;
            db.SubmitChanges();
        }
        internal static string CreateDietPlan()
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();
            DietPlan dietPlan = new DietPlan();
            dietPlan.Name = UserInterface.GetStringData("name", "the diet plan");
            dietPlan.FoodType = UserInterface.GetStringData("food type", "the diet plan");
            dietPlan.FoodAmountInCups = UserInterface.GetIntegerData("the diet plan", "food amount in cups of");
            db.DietPlans.InsertOnSubmit(dietPlan);
            db.SubmitChanges();
            return dietPlan.Name;
        }
        internal static int DisplayAndCountAllDietPlans()
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();
            IQueryable<DietPlan> dietPlans = db.DietPlans;
            int i = 1;
            foreach (DietPlan dietPlan in dietPlans)
            {
                Console.WriteLine(i + ". " + dietPlan.Name);
                i++;
            }
            return i;
        }
        internal static void UpdateDietPlan(DietPlan dietPlan, int dietPlanIdNum)
        {
            HumaneSocietyDataContext db = new HumaneSocietyDataContext();
            DietPlan dietPlanToUpdate = db.DietPlans.Where(d => d.DietPlanId == dietPlanIdNum).Single();
            dietPlanToUpdate.Name = dietPlan.Name;
            dietPlanToUpdate.FoodType = dietPlan.FoodType;
            dietPlanToUpdate.FoodAmountInCups = dietPlan.FoodAmountInCups;
            db.SubmitChanges();
        }
    }
}