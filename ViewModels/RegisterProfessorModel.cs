namespace InstrukcijeDotNet.ViewModels
{
    public class RegisterProfessorModel
    {
        public string name { get; set; }

        public string surname { get; set; }

        public string email { get; set; }

        public string password { get; set; }
        public string confirmPassword { get; set; }
        public string? profilePictureUrl { get; set; }

        public int instructionsCount { get; set; }

        public string[]? subjects { get; set; }
    }
}
