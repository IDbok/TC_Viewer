using TcModels.Models;
using TcModels.Models.Interfaces;
using TcModels.Models.TcContent;

namespace TC_WinForms.DataProcessing
{
    public static class DataProcessing
    {
        public static async void AddNewTC()
        {
            var dbCon = new DbConnector();
        }
        public static Staff AddNewStaff()
        {
            var dbCon = new DbConnector();
            var newObject = dbCon.AddNewObjAndReturnIt(CreateStaffWithRandomName());

            return newObject;

        }
        public static T AddNewObject<T>() where T : class, IModelStructure, IClassifaerable, new()
        {
            var dbCon = new DbConnector();
            var newObject = dbCon.AddNewObjAndReturnIt<T>(CreateObjectWithRandomName<T>());

            return newObject;

        }
        private static TechnologicalCard CreateTCWithRandomName()
        {
            // get random number with 10 nubbers for new id
            Random rnd = new Random();
            int random = rnd.Next(1000000, 9999999);

            var newObject = new TechnologicalCard()
            {
                Article = "",
                Version = "0.0.0.0",
                Name = $"New Object - {random}",
                Type = "",
                NetworkVoltage = "0",
                IsCompleted = false
            };
            
            return newObject;

        }
        private static Staff CreateStaffWithRandomName()
        {
            // get random number with 10 nubbers for new id
            Random rnd = new Random();
            int random = rnd.Next(1000000, 9999999);

            var newObject = new Staff()
            {
                Name = $"New Object - {random}",
                Type = "",
                Functions = "",
                Qualification = ""
            };

            return newObject;

        }
        private static T CreateObjectWithRandomName<T>() where T : class, IModelStructure, IClassifaerable, new()
        {
            // get random number with 10 nubbers for new id
            Random rnd = new Random();
            int random = rnd.Next(1000000, 9999999);

            var newObject = new T()
            {
                Name = $"New Object - {random}",
                Type = "",
                Unit = "",
                Price = 0,
                ClassifierCode = "",
            };

            return newObject;

        }


    }

    

}
