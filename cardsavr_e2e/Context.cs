﻿using System;
using System.Collections.Generic;
using System.Text;

using Switch.CardSavr.Http;
using Switch.Security;

namespace cardsavr_e2e
{
    /// <summary>
    /// Context of the current test run.
    /// </summary>
    public class Context
    {
        // test env: account information.
        public static readonly string accountBaseUrl = "https://api.mbudos.cardsavr.io";
        public static readonly string accountAppID = "CardUpdatr Demo";
        public static readonly string accountStaticKey = "<REDACATED>";
        public static readonly string accountUserName = "cardupdatr_demo";
        public static readonly string accountPassword = "<REDACATED>";
        /* */

        // other resources.
        public static readonly string e2e_identifier = "c_sharp_e2e";
        public static readonly Random random = new Random();

        // properties.
        public List<MerchantSite> Sites { get; set; }
        public List<User> Users { get; set; }
        public List<User> NewUsers { get; set; }
        public string Trace { get; set; }
        public BIN Bin { get; set; }
        public bool Started { get; set; }
        public string ExecutionRole { get; set; }

        public static string GenerateBogus32BitPassword(string username)
        {
            byte[] sequence = new byte[32];
            for (byte n = 0; n < sequence.Length; ++n)
                sequence[n] = n;

            string password = Convert.ToBase64String(sequence);
            byte[] salt = Encoding.UTF8.GetBytes(username);
            return Convert.ToBase64String(HashUtil.Sha256Pbkdf2(password, salt, 5000, 32));
        }

        public Context(bool started = false)
        {
            Started = started;
        }

        public User FindUserById(int userId)
        {
            if (Users == null || Users.Count == 0)
                throw new ArgumentException("user-list not available.");

            return Users.Find(u => u.id == userId);
        }

        public User GetRandomNewUser(bool throwOnError = true)
        {
            if (NewUsers == null || NewUsers.Count == 0)
            {
                if (!throwOnError)
                    return null;
                throw new ArgumentException("no new users available.");
            }

            return NewUsers[random.Next(0, NewUsers.Count - 1)];
        }

        public MerchantSite GetRandomSite(bool throwOnError = true)
        {
            if (Sites == null || Sites.Count == 0)
            {
                if (!throwOnError)
                    return null;
                throw new ArgumentException("no merchant sites available.");
            }

            return Sites[random.Next(0, Sites.Count - 1)];
        }
    }
}
