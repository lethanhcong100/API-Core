using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Helpers
{
    public static class ExtensionMethods
    {
        public static IEnumerable<User> WithoutPasswords(this IEnumerable<User> users)
        {
            return users.Select(x => x.WithoutPassword());
        }

        public static User WithoutPassword(this User user)
        {
            user.Password = null;
           
            return user;
        }

        public static IEnumerable<User> WithoutToken(this IEnumerable<User> users)
        {
            return users.Select(x => x.WithoutToken());
        }

        public static User WithoutToken(this User user)
        {
            user.Password = null;
            user.Token = null;
            return user;
        }

    }
}
