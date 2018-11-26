using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumaneSociety
{
    class UserEmployee : User
    {
        Employee employee;
        
        public override void LogIn()
        {
            if (CheckIfNewUser())
            {
                CreateNewEmployee();
                LogInPreExistingUser();
            }
            else
            {
                Console.Clear();
                LogInPreExistingUser();
            }
            RunUserMenus();
        }
        protected override void RunUserMenus()
        {
            List<string> options = new List<string>() { "What would you like to do? (select number of choice)", "1. Add animal", "2. Remove Anmial", "3. Check Animal Status",  "4. Approve Adoption", "5. Update"};
            UserInterface.DisplayUserOptions(options);
            string input = UserInterface.GetUserInput();
            RunUserInput(input);
        }
        private void RunUserInput(string input)
        {
            switch (input)
            {
                case "1":
                    AddAnimal();
                    RunUserMenus();
                    return;
                case "2":
                    RemoveAnimal();
                    RunUserMenus();
                    return;
                case "3":
                    CheckAnimalStatus();
                    RunUserMenus();
                    return;
                case "4":
                    CheckAdoptions();
                    RunUserMenus();
                    return;
                case "5":
                    UpdateOptions();
                    RunUserMenus();
                    return;
                default:
                    UserInterface.DisplayUserOptions("Input not accepted please try again");
                    RunUserMenus();
                    return;
            }
        }
        private void UpdateOptions()
        {
            Console.Clear();
            List<string> options = new List<string>() { "What would you like to do? (select number of choice)", "1. Diet Plan", "2. Import Animals From File", "3. Return to Main Menu" };
            UserInterface.DisplayUserOptions(options);
            string input = UserInterface.GetUserInput();
            switch (input)
            {
                case "1":
                    UpdateDietPlan();
                    break;
                case "2":
                    ImportFromFile();
                    return;
                case "3":
                    RunUserMenus();
                    return;
                default:
                    UserInterface.DisplayUserOptions("Input not accepted please try again");
                    UpdateOptions();
                    return;
            }
        }
        private void ImportFromFile()
        {
            Console.Clear();
            Console.WriteLine("Example: C:'\'Program Files'\'... .csv");
            string filePath = UserInterface.GetStringData("of the file you would like to import", "the path");
            try
            {
                Query.InsertAllAnimalsFromFile(filePath, employee.EmployeeId);
                Console.WriteLine("Successful file import! \nHit enter to continue.");
                Console.ReadLine();
                RunUserMenus();
            }
            catch
            {
                UserInterface.DisplayUserOptions("There was an error when attempting to import you file.");
                UpdateOptions();

            }

        }
        private void UpdateDietPlan()
        {
            Console.WriteLine("What diet plan would you like to change?");
            int amountOfDietPlans = Query.DisplayAndCountAllDietPlans();
            string userInput = Console.ReadLine();
            try
            {
                int dietPlanId = Convert.ToInt32(userInput);
                UserInterface.DisplayAboutToUpdate();
                DietPlan dietPlan = new DietPlan();
                if (amountOfDietPlans >= dietPlanId)
                {
                    dietPlan.Name = UserInterface.GetStringDataWithoutIs("set the name to", "would you like to");
                    dietPlan.FoodType = UserInterface.GetStringDataWithoutIs("set the food type to", "would you like to");
                    dietPlan.FoodAmountInCups = int.Parse(UserInterface.GetStringData("you would like to set for this diet plan", "amount of food in cups"));
                    Query.UpdateDietPlan(dietPlan, dietPlanId);
                    return;
                }
                else
                {
                    UserInterface.DisplayUserOptions("Input not accepted please try again");
                    UpdateDietPlan();
                    return;
                }
            }
            catch
            {
                UserInterface.DisplayUserOptions("Input not accepted please try again");
                UpdateDietPlan();
                return;
            }
        }

        private void CheckAdoptions()
        {
            Console.Clear();
            List<string> adoptionInfo = new List<string>();
            int counter = 1;
            var adoptions = Query.GetPendingAdoptions().ToList();
            if(adoptions.Count > 0)
            {
                foreach(Adoption adoption in adoptions)
                {
                    adoptionInfo.Add($"{counter}. {adoption.Client.FirstName} {adoption.Client.LastName}, {adoption.Animal.Name} {adoption.Animal.Category}");
                    counter++;
                }
                UserInterface.DisplayUserOptions(adoptionInfo);
                UserInterface.DisplayUserOptions("Enter the number of the adoption you would like to approve");
                int input = UserInterface.GetIntegerData();
                ApproveAdoption(adoptions[input - 1]);
            }

        }

        private void ApproveAdoption(Adoption adoption)
        {
            UserInterface.DisplayAnimalInfo(adoption.Animal);
            UserInterface.DisplayClientInfo(adoption.Client);
            UserInterface.DisplayUserOptions("Would you approve this adoption?");
            if ((bool)UserInterface.GetBitData())
            {
                Query.UpdateAdoption(true, adoption);
            }
            else
            {
                Query.UpdateAdoption(false, adoption);
            }
        }

        private void CheckAnimalStatus()
        {
            Console.Clear();            
            var animals = Query.SearchForAnimalByMultipleTraits(UserInterface.GetAnimalCriteria(), "Employee").ToList();
            if(animals.Count > 1)
            {
                UserInterface.DisplayUserOptions("Several animals found");
                UserInterface.DisplayAnimals(animals);
                UserInterface.DisplayUserOptions("Enter the ID of the animal you would like to check");
                int ID = UserInterface.GetIntegerData();
                CheckAnimalStatus(ID);
                return;
            }
            if(animals.Count == 0)
            {
                UserInterface.DisplayUserOptions("Animal not found please use different search criteria");
                return;
            }
            RunCheckMenu(animals[0]);
        }

        private void RunCheckMenu(Animal animal)
        {
            bool isFinished = false;
            Console.Clear();
            while(!isFinished){
                List<string> options = new List<string>() { "Animal found:", animal.Name, animal.Category.Name, "Would you like to:", "1. Get Info", "2. Update Info", "3. Check shots", "4. Return" };
                UserInterface.DisplayUserOptions(options);
                int input = UserInterface.GetIntegerData();
                if (input == 4)
                {
                    isFinished = true;
                    continue;
                }
                RunCheckMenuInput(input, animal);
            }
        }

        private void RunCheckMenuInput(int input, Animal animal)
        {
            
            switch (input)
            {
                case 1:
                    UserInterface.DisplayAnimalInfo(animal);
                    Console.Clear();
                    return;
                case 2:
                    UpdateAnimal(animal);
                    Console.Clear();
                    return;
                case 3:
                    CheckShots(animal);
                    Console.Clear();
                    return;
                default:
                    UserInterface.DisplayUserOptions("Input not accepted please select a menu choice");
                    return;
            }
        }

        private void CheckShots(Animal animal)
        {
            List<string> shotInfo = new List<string>();
            var shots = Query.GetShots(animal);
            foreach(AnimalShot shot in shots.ToList())
            {
                shotInfo.Add($"{shot.Shot.Name} Date: {shot.DateReceived}");
            }
            if(shotInfo.Count > 0)
            {
                UserInterface.DisplayUserOptions(shotInfo);
                if(UserInterface.GetBitData("Would you like to Update shots?"))
                {
                    Query.UpdateShot("booster", animal);
                }
            }
            else
            {
                if (UserInterface.GetBitData("Would you like to give shots?"))
                {
                    Query.GiveShot(animal);
                }
            }
            
        }

        private void UpdateAnimal(Animal animal, Dictionary<int, string> updates = null)
        {
            if(updates == null)
            {
                updates = new Dictionary<int, string>();
            }
            List<string> options = new List<string>() { "Select Updates: (Enter number and choose finished when finished)", "1. Category", "2. Name", "3. Age", "4. Demeanor", "5. Kid friendly", "6. Pet friendly", "7. Weight", "8. Diet Plan", "9. Finished" }; // Figure out today!
            UserInterface.DisplayUserOptions(options);
            string input = UserInterface.GetUserInput();
            if(input.ToLower() == "9" ||input.ToLower() == "finished")
            {
                Query.EnterAnimalUpdate(animal, updates);
            }
            else
            {
                input = input == "8" ? "9" : input;
                updates = UserInterface.EnterSearchCriteria(updates, input);
                UpdateAnimal(animal);
            }
        }

        private void CheckAnimalStatus(int iD)
        {
            Console.Clear();
            var animals = SearchForAnimal(iD).ToList();
            if (animals.Count == 0)
            {
                UserInterface.DisplayUserOptions("Animal not found please use different search criteria");
                return;
            }
            RunCheckMenu(animals[0]);
        }

        private IQueryable<Animal> SearchForAnimal(int iD)
        {
            HumaneSocietyDataContext context = new HumaneSocietyDataContext();
            var animals = (from animal in context.Animals where animal.AnimalId == iD select animal);
            return animals;
        }       

        private void RemoveAnimal()
        {
            
            var animals = Query.SearchForAnimalByMultipleTraits(UserInterface.GetAnimalCriteria(), "Employee").ToList();
            if (animals.Count > 1)
            {
                UserInterface.DisplayUserOptions("Several animals found please refine your search.");
                UserInterface.DisplayAnimals(animals);
                UserInterface.DisplayUserOptions("Press enter to continue");
                Console.ReadLine();
                return;
            }
            else if (animals.Count < 1)
            {
                UserInterface.DisplayUserOptions("Animal not found please use different search criteria");
                return;
            }
            var animal = animals[0];
            List<string> options = new List<string>() { "Animal found:", animal.Name, animal.Category.Name, "would you like to delete?" };
            if ((bool)UserInterface.GetBitData(options))
            {
                Query.RemoveAnimal(animal);
            }
        }
        private void AddAnimal()
        {
            Console.Clear();
            Animal animal = new Animal();
            string species = UserInterface.GetStringData("category", "the animal's");
            Query.AddCategoryIfNull(species);
            animal.CategoryId = Query.GetCategoryId(species);
            animal.Name = UserInterface.GetStringData("name", "the animal's");
            animal.Age = UserInterface.GetIntegerData("age", "the animal's");
            animal.Demeanor = UserInterface.GetStringData("demeanor", "the animal's");
            animal.KidFriendly = UserInterface.GetBitData("the animal", "child friendly");
            animal.PetFriendly = UserInterface.GetBitData("the animal", "pet friendly");
            animal.Gender = UserInterface.GetStringData("the animal", "the gender of the");
            animal.AdoptionStatus = "AVAILABLE";
            animal.Weight = UserInterface.GetIntegerData("the animal", "the weight of the");
            string dietPlan;
            if (UserInterface.GetBitData("Would you like create a new diet plan?"))
            {
                dietPlan = Query.CreateDietPlan();
            }
            else
            {
                dietPlan = UserInterface.GetStringData("the animal", "the diet plan of the");
            }
            animal.DietPlanId = Query.GetDietPlanId(dietPlan);
            Query.AddAnimal(animal, employee.EmployeeId);
        }
        protected override void LogInPreExistingUser()
        {
            List<string> options = new List<string>() { "Please log in", "Enter your username (CaSe SeNsItIvE)" };
            UserInterface.DisplayUserOptions(options);
            userName = UserInterface.GetUserInput();
            UserInterface.DisplayUserOptions("Enter your password (CaSe SeNsItIvE)");
            string password = UserInterface.GetUserInput();
            try
            {
                Console.Clear();
                employee = Query.EmployeeLogin(userName, password);
                UserInterface.DisplayUserOptions("Login successfull. Welcome.");
            }
            catch
            {
                Console.Clear();
                UserInterface.DisplayUserOptions("Employee not found, please try again, create a new user or contact your administrator");
                LogIn();
            }
            
        }
        private void CreateNewEmployee()
        {
            Console.Clear();
            string email = UserInterface.GetStringData("email", "your");
            int employeeNumber = int.Parse(UserInterface.GetStringData("employee number", "your"));
            try
            {
                employee = Query.RetrieveEmployeeUser(email, employeeNumber);
            }
            catch
            {
                UserInterface.DisplayUserOptions("Employee not found please contact your administrator");
                PointOfEntry.Run();
            }
            if (employee.Password != null)
            {
                UserInterface.DisplayUserOptions("User already in use please log in or contact your administrator");
                LogIn();
                return;
            }
            else
            {
                UpdateEmployeeInfo();
            }
        }

        private void UpdateEmployeeInfo()
        {
            GetUserName();
            GetPassword();
            Query.AddUsernameAndPassword(employee);
        }

        private void GetPassword()
        {
            UserInterface.DisplayUserOptions("Please enter your password: (CaSe SeNsItIvE)");
            employee.Password = UserInterface.GetUserInput();
        }

        private void GetUserName()
        {
            Console.Clear();
            string username = UserInterface.GetStringData("username", "your");
            if (Query.CheckEmployeeUserNameExist(username))
            {
                UserInterface.DisplayUserOptions("Username already in use please try another username.");
                GetUserName();
            }
            else
            {
                employee.UserName = username;
                UserInterface.DisplayUserOptions("Username successful");
            }
        }
    }
}
