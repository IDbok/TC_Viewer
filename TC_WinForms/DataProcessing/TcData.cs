
namespace TC_WinForms.DataProcessing
{
    public class TcData
    {
        public string ProjectType { get; set; }
        public string ProjectNumber { get; set; }
        public string ProjectName { get; set; }
        public string? ProjectDescription { get; set; }
        public string ProjectAuthorName { get; set; }
        public string ProjectAuthorSurname { get; set; }
        public string ProjectAuthorEmail { get; set; }
        public string ProjectAuthorPhone { get; set; }
        public string ProjectDateCreated { get; set; }
        public string ProjectVersion { get; set; }

        //public TcData(string projectName, string? projectDescription = null)
        //{
        //    ProjectNumber = new Random().Next(1000, 9999).ToString();
        //    ProjectName = projectName;
        //    ProjectDescription = projectDescription;
        //    ProjectAuthorName = Authorizator.AuthUserName();
        //    ProjectAuthorSurname = Authorizator.AuthUserSurname();
        //    ProjectAuthorEmail = Authorizator.AuthUserEmail();
        //    ProjectDateCreated = DateTime.Now.ToString("dd.MM.yyyy");
        //    ProjectVersion = "0.0.0.1";
        //}
        //public TcData()
        //{
        //    ProjectNumber = new Random().Next(1000, 9999).ToString();
        //    ProjectName = "NoName Project";
        //    ProjectDescription = null;
        //    ProjectAuthorName = Authorizator.AuthUserName();
        //    ProjectAuthorSurname = Authorizator.AuthUserSurname();
        //    ProjectAuthorEmail = Authorizator.AuthUserEmail();
        //    ProjectDateCreated = DateTime.Now.ToString("dd.MM.yyyy");
        //    ProjectVersion = "0.0.0.1";
        //}
        //public override string ToString()
        //{
        //    // todo - serialize to json
        //    return $"Project {ProjectType}:\n" +
        //        $"\tNumber: {ProjectNumber} \n" +
        //        $"\tName: {ProjectName} \n" +
        //        $"\tDescription: {ProjectDescription} \n" +
        //        $"\tCreation date: {ProjectDateCreated}" +
        //        $"Project Author:\n" +
        //        $"\tName: {ProjectAuthorName} \n" +
        //        $"\t Surmane: {ProjectAuthorSurname} \n" +
        //        $"\tEmail: {ProjectAuthorEmail}\n";
        //}
    }
}
