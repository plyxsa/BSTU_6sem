namespace PlushFood.Helpers
{
    public class UserSession
    {
        private static UserSession _instance;
        public static UserSession Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UserSession();
                }
                return _instance;
            }
        }
        public bool IsGuest { get; private set; }
        public int? UserId { get; private set; }
        public string UserName { get; private set; }
        public string Role { get; private set; }

        private UserSession() { }

        public void LoginClient(int clientId, string userName)
        {
            IsGuest = false;
            UserId = clientId;
            UserName = userName;
            Role = "Client";
        }

        public void LoginAdmin(int adminId, string userName)
        {
            IsGuest = false;
            UserId = adminId;
            UserName = userName;
            Role = "Admin";
        }

        public void LoginAsGuest()
        {
            IsGuest = true;
            UserId = null;
            UserName = "Guest";
            Role = "Guest";
        }

        public void Logout()
        {
            IsGuest = false;
            UserId = null;
            UserName = null;
            Role = null;
        }
    }
}
