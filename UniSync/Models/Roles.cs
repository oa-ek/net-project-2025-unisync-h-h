namespace UniSync.Constants
{
    public static class Roles
    {
        public const string Student = "Student";
        public const string Moderator = "Moderator";
        public const string CourseManager = "CourseManager";
        public const string NewsEditor = "NewsEditor";
        public const string Admin = "Admin";
        public const string SuperAdmin = "SuperAdmin";

        public static readonly string[] AllRoles = new[]
        {
            Student,
            Moderator,
            CourseManager,
            NewsEditor,
            Admin,
            SuperAdmin
        };
    }
}