using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC_WinForms.WinForms;
using TcModels.Models;
using TcModels.Models.Interfaces;
using TcModels.Models.TcContent;

namespace TC_WinForms.DataProcessing
{
    public static class DataProcessing
    {
        public static bool addNewTC(Win7_1_TCs form)
        {
            var newObject = form.dbCon.AddNewObjAndReturnIt(CreateTCWithRandomName());

            form.newCard = newObject;
            return true;

        }
        public static Staff addNewStaff()
        {
            var dbCon = new DbConnector();
            var newObject = dbCon.AddNewObjAndReturnIt<Staff>(CreateStaffWithRandomName());

            return newObject;

        }
        public static T addNewObject<T>(Form form, ref T newObj) where T : class, IModelStructure, new()
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
                NetworkVoltage = 0,
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
        private static T CreateObjectWithRandomName<T>() where T : class, IModelStructure, new()
        {
            // get random number with 10 nubbers for new id
            Random rnd = new Random();
            int random = rnd.Next(1000000, 9999999);

            var newObject = new T()
            {
                Name = $"New Object - {random}",
                Type = "",
                Unit = "",
                Price = 0
            };

            return newObject;

        }
    }

    

}
