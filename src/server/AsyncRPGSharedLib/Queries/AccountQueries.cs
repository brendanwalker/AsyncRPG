using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Security.Cryptography;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Database;

namespace AsyncRPGSharedLib.Queries
{
    public class AccountQueries
    {
        private const int k_salted_password_bytes = 20;
        private const int k_salt_bytes = 24;
        private const int k_hash_iterations = 4;

        public static void CreateAccount(
            AsyncRPGDataContext context,
            string username,
            string clientHashedPassword,
            string emailAddress,
            out string emailVerificationString)
        {
            // Salt and re-hash the given password
            byte[] saltBytes = null;
            string saltString = CreateRandomSalt(out saltBytes);
            string saltedPassword = SaltAndHashPassword(clientHashedPassword, saltBytes);

            emailVerificationString = CreateNonDeterministicRandomString();

            // Actually create an account entry in the database
            {
                Accounts account = new Accounts
                {
                    CreationDate = DateTime.Now,
                    OpsLevel = (int)DatabaseConstants.OpsLevel.player,
                    UserName = username,
                    PasswordHash = saltedPassword,
                    Salt = saltString,
                    EmailAddress = emailAddress,
                    EmailVerificationKey = emailVerificationString
                };

                context.Accounts.InsertOnSubmit(account);
                context.SubmitChanges();
            }
        }

        public static void CreateAccountNoEmailVerify(
            AsyncRPGDataContext context,
            string username,
            string clientHashedPassword,
            string emailAddress,
            DatabaseConstants.OpsLevel opsLevel)
        {
            // Then Salt and re-hash like the client password
            byte[] saltBytes = null;
            string saltString = CreateRandomSalt(out saltBytes);
            string saltedPassword = SaltAndHashPassword(clientHashedPassword, saltBytes);

            // Actually create an account entry in the database
            {
                Accounts account = new Accounts
                {
                    CreationDate = DateTime.Now,
                    OpsLevel = (int)opsLevel,
                    UserName = username,
                    PasswordHash = saltedPassword,
                    Salt = saltString,
                    EmailAddress = emailAddress,
                    EmailVerificationKey = ""
                };

                context.Accounts.InsertOnSubmit(account);
                context.SubmitChanges();
            }
        }

        public static bool VerifyUsernameAvailable(
            AsyncRPGDataContext context,
            string username)
        {
            return (from a in context.Accounts where a.UserName == username select a).Count() == 0;
        }

        // Called directly by the accounts Web service
        public static bool VerifiedEmailAddress(
            string connection_string,
            string username,
            string key,
            out string result)
        {
            bool success = true;

            result = SuccessMessages.GENERAL_SUCCESS;

            using (AsyncRPGDataContext context = new AsyncRPGDataContext(connection_string))
            {
                try
                {
                    VerifiedEmailAddress(context, username, key);
                }
                catch (System.Exception)
                {
                    success = false;
                    result = ErrorMessages.DB_ERROR + "(Failed to verify e-mail address)";
                }
            }

            return success;
        }

        public static void VerifiedEmailAddress(
            AsyncRPGDataContext context,
            string username,
            string key)
        {
            Accounts accounts = (from a in context.Accounts where a.UserName == username select a).Single();

            accounts.EmailVerificationKey = "";

            context.SubmitChanges();
        }

        public static bool VerifyCredentials(
            AsyncRPGDataContext context,
            string username, 
            string password, 
            out int accountId,
            out string emailAddress,
            out int opsLevel,
            out string resultCode)
        {
            bool success = false;

            var query=
                (from a in context.Accounts
                where a.UserName == username
                select a).SingleOrDefault();

            accountId = -1;
            emailAddress = "";
            resultCode = "";
            opsLevel = 0;

            if (query != null)
            {
                bool emailVerified = query.EmailVerificationKey.Length == 0;

                if (emailVerified)
                {
                    byte[] saltBytes = Convert.FromBase64String(query.Salt);
                    string saltedPassword = SaltAndHashPassword(password, saltBytes);

                    if (query.PasswordHash == saltedPassword)
                    {
                        accountId = query.AccountID;
                        emailAddress = query.EmailVerificationKey;
                        opsLevel = query.OpsLevel;
                        resultCode = SuccessMessages.GENERAL_SUCCESS;
                        success = true;
                    }
                    else
                    {
                        resultCode = ErrorMessages.INVALID_AUTHENTICATION;
                        success = false;
                    }
                }
                else
                {
                    resultCode = ErrorMessages.UNVERIFIED_EMAIL;
                    success = false;
                }
            }
            else
            {
                resultCode = ErrorMessages.INVALID_USERNAME;
                success = false;
            }

            return success;
        }

        public static DatabaseConstants.OpsLevel GetAccountOpsLevel(
            string connection_string,
            int account_id,
            out string result)
        {
            DatabaseConstants.OpsLevel ops_level = DatabaseConstants.OpsLevel.invalid;

            result = SuccessMessages.GENERAL_SUCCESS;

            using (AsyncRPGDataContext context = new AsyncRPGDataContext(connection_string))
            {
                try
                {
                    ops_level= GetAccountOpsLevel(context, account_id);
                }
                catch (System.Exception)
                {
                    result = ErrorMessages.DB_ERROR + "(Failed to get account ops level)";
                }
            }

            return ops_level;
        }

        public static DatabaseConstants.OpsLevel GetAccountOpsLevel(
            AsyncRPGDataContext context,
            int account_id)
        {
            return (DatabaseConstants.OpsLevel)(from a in context.Accounts where a.AccountID == account_id select a.OpsLevel).Single();
        }

        // Called directly by SetAccountOpsLevel web service call
        public static bool SetAccountOpsLevel(
            string connection_string,
            int account_id,
            DatabaseConstants.OpsLevel ops_level,
            out string result)
        {
            bool success= false;

            result = SuccessMessages.GENERAL_SUCCESS;

            using (AsyncRPGDataContext context = new AsyncRPGDataContext(connection_string))
            {
                try
                {
                    SetAccountOpsLevel(context, account_id, ops_level);
                    success = true;
                }
                catch (System.Exception)
                {
                    result = ErrorMessages.DB_ERROR + "(Failed to set account ops level)";
                }
            }

            return success;
        }

        public static void SetAccountOpsLevel(
            AsyncRPGDataContext context,
            int account_id,
            DatabaseConstants.OpsLevel ops_level)
        {
            Accounts account = (from a in context.Accounts where a.AccountID == account_id select a).Single();

            account.OpsLevel = (int)ops_level;

            context.Accounts.InsertOnSubmit(account);
            context.SubmitChanges();
        }

        public static string CreateNonDeterministicRandomString()
        {
            const Int32 k_random_bytes = 8;
            byte[] randomBytes = new byte[k_random_bytes];

            try
            {
                RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();

                random.GetNonZeroBytes(randomBytes);
            }
            catch (System.Exception)
            {
                Random random = new Random((int)DateTime.Now.Ticks);

                random.NextBytes(randomBytes);
            }

            return BitConverter.ToString(randomBytes);
        }

        public static string CreateRandomSalt(out byte[] saltBytes)
        {
            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            
            saltBytes = new byte[k_salt_bytes];
            rngCsp.GetBytes(saltBytes);

            return Convert.ToBase64String(saltBytes);
        }

        public static string SaltAndHashPassword(
            string plainText,
            byte[] saltBytes)
        {
            // Convert plain text into a byte array.
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            // Hash the password + salt together
            Rfc2898DeriveBytes hash = new Rfc2898DeriveBytes(plainText, saltBytes, k_hash_iterations);
            byte[] hashBytes = hash.GetBytes(k_salted_password_bytes);

            // Convert result into a base64-encoded string.
            string resultHashValue = Convert.ToBase64String(hashBytes);

            // Return the result.
            return resultHashValue;
        }

        // This is what the client should do to the password before sending it to us
        // That way we don't send private information over the wire
        public static string ClientPasswordHash(string plaintextPassword)
        {
            SHA256 encryptor = SHA256Managed.Create();
            byte[] passwordBytes = System.Text.Encoding.ASCII.GetBytes(plaintextPassword);
            byte[] encryptedPasswordBytes = encryptor.ComputeHash(passwordBytes);
            string encryptedPassword = BitConverter.ToString(encryptedPasswordBytes).Replace("-", "").ToLower();

            return encryptedPassword;
        }
    }
}