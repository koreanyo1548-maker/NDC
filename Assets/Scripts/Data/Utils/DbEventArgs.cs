using System;
using Data.DbUser;

namespace Data.Utils
{
    public class DbEventArgs : EventArgs
    {
        public DbEventArgs(int id)
        {
            Id = id;
        }

        public int Id { get; set; }
    }
}