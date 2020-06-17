using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
namespace BankDBApp
{

    public static class BankDefs
    {
        // Account types
        public const int BankAccount = 1;
        public const int CreditAccount = 2;

    }

    class Program
    {
        public static PankkiEntities context;
        
        static void Main(string[] args)
        {
            context = new PankkiEntities();
            
            // App title
            Console.WriteLine("                           BANK APPLICATION");
            Console.WriteLine("                           ================");

            bool leaveBank = default;

            do
            {
                switch (GUIMainDisplay())
                {
                    case 0:
                        leaveBank = true;
                        Console.WriteLine("Leaving Bank...");
                        Console.WriteLine("Press <return> to exit");
                        break;
                    case 1:
                        GUICreateCustomer();
                        break;
                    case 2:
                        GUICreateBankAccount();
                        break;
                    case 3:
                        GUICreateCreditAccount();
                        break;
                    case 4:
                        GUIMoveAccountToOtherCustomer();
                        break;
                    case 5:
                        GUIShowCustomer();
                        break;
                    case 6:
                        GUIShowAccount();
                        break;
                    case 7:
                        GUIDeleteCustomer();
                        break;
                    case 8:
                        GUIDeleteAccount();
                        break;
                    case 9:
                        GUIChangeSaldo();
                        break;
                    default:
                        break;
                }
            } while (!leaveBank);

            // end program
            Console.ReadLine();
        }

        // Method for displaying the Main menu
        private static int GUIMainDisplay()
        {
            bool validResponse = false;
            int response;

            do
            {
                Console.WriteLine(@"
                           Select Activity (0 to 10)
                           0) Exit
                           1) New customer
                           2) New Bank Account
                           3) New Credit Bank Account
                           4) Move account to the other customer
                           5) Show customers
                           6) Show accounts
                           7) Delete customer
                           8) Delete account
                           9) Make withdrawal or deposit
                        ");

                string guessInput = Console.ReadLine();
                // convert string to number
                validResponse = int.TryParse(guessInput, out response);

            } while (!validResponse);

            return response;
        }

        // Method for creating customer
        // First and last name asked.
        // After confirming customer is created in database.
        private static void GUICreateCustomer()
        {
            Console.WriteLine(@"
                           Customer First Name?
            ");

            string firstInput = Console.ReadLine();

            Console.WriteLine(@"
                           Customer Family Name?
            ");

            string familyInput = Console.ReadLine();

            if (confirmInput())
            {
                var newCustomer = new customers()
                {
                    customer_first_name = firstInput,
                    customer_last_name = familyInput,
                };

                context.customers.Add(newCustomer);
                context.SaveChanges();
            }
        }

        // Method for creating bank account
        private static void GUICreateBankAccount()
        {
            int customerNumber = default;
            decimal creditLimit = default;
            decimal currentSaldo = default;

            // Ask customer and saldo for new account
            GUIAskCustomerNumberAndSaldo(out customerNumber, out currentSaldo);

            if (confirmInput())
            {
                var henkilö = context.customers.FirstOrDefault<customers>
                    (x => x.customer_number.Equals(customerNumber));

                var newAccount = new accounts()
                {
                    account_name = henkilö.customer_last_name,
                    account_type = BankDefs.BankAccount,
                    account_saldo = currentSaldo,
                    credit_limit = creditLimit,
                    customer_number = customerNumber
                };

                context.accounts.Add(newAccount);
                context.SaveChanges();
            }
        }

        // Method for creating account
        private static void GUICreateCreditAccount()
        {
            int customerNumber = default;
            decimal creditLimit = default;
            decimal currentSaldo = default;

            // Get customer and set saldo for new account
            GUIAskCustomerNumberAndSaldo(out customerNumber, out currentSaldo);

            // Set credit limit for new account
            GUIAskSaldo("CreditLimitSaldo", out creditLimit);
            
            // Create the credit account
            if (confirmInput())
            {
                var henkilö = context.customers.FirstOrDefault<customers>
                    (x => x.customer_number.Equals(customerNumber));

                var newAccount = new accounts()
                {
                    account_name = henkilö.customer_last_name,
                    account_type = BankDefs.CreditAccount,
                    account_saldo = currentSaldo,
                    credit_limit = creditLimit,
                    customer_number = customerNumber
                };

                context.accounts.Add(newAccount);
                context.SaveChanges();
            }

        }
        
        // Method for moving account to the other customer
        // Showing the Account List which account is going to be moved.
        // Showing the Customer List to whom the account is going to be moved.
        // After confirming account is moved to new owner and saved in database.
        private static void GUIMoveAccountToOtherCustomer()
        {
            int response = default;
            int response2 = default;
            
            GUIShowAccount();
            GUIShowCustomer();
            
            // Get account
            GUIAskCustomerOrAccountNumber("AccountToBeTransferred", out response);
            // Get new owner
            GUIAskCustomerOrAccountNumber("AccountNewOwner", out response2);

            // Confirmed - do transferring
            if (confirmInput())
            {
                /*var accountToBeTransferred = context.accounts.FirstOrDefault<accounts>
                    (x => x.account_number.Equals(response));*/

                // Above can be used also, below is another way
                accounts accountToBeTransferred = context.accounts.Find(response);

                var oldOwner = context.customers.FirstOrDefault<customers>
                    (x => x.customer_number.Equals(accountToBeTransferred.customer_number));

                var newOwner = context.customers.FirstOrDefault<customers>
                (x => x.customer_number.Equals(response2));

                // TODO Should it be checked that account and new owner is found?
                // If they are found, the transfer can be done.
                accountToBeTransferred.customer_number = newOwner.customer_number;
                accountToBeTransferred.account_name = newOwner.customer_last_name;
                context.SaveChanges();

                Console.WriteLine("Account " + accountToBeTransferred.account_number + " (Saldo: " 
                    + accountToBeTransferred.account_saldo + ") has transferred. Old owner: "
                    + oldOwner.customer_number + " (" + oldOwner.customer_first_name + " " + oldOwner.customer_last_name
                    + "), New owner: " + newOwner.customer_number + " (" + newOwner.customer_first_name
                    + " " + newOwner.customer_last_name + ")");
            }

        }

        // Method for showing Customer List
        private static void GUIShowCustomer()
        {
            Console.WriteLine(@"
                           Customer List:
            ");
            var list =
                from customer in context.customers
                select new
                {
                    Customer = customer.customer_number +
                    " " + customer.customer_first_name +
                    " " + customer.customer_last_name,
                    Account = customer.customer_number
                };
            foreach (var iter in list)
            {
                Console.Write("  " + iter.Customer);
                Console.Write(" Your accounts are: ");
                
                foreach (accounts iter2 in context.accounts)
                {
                    if (iter2.customer_number.Equals(iter.Account))
                    {
                        string accountType = iter2.account_type.ToString();

                        // Check the type and set the right one to be shown
                        if (accountType.Equals(BankDefs.BankAccount.ToString()))
                        {
                            accountType = "Bank";
                        }
                        else
                        {
                            accountType = "Credit";
                        }

                        Console.Write(iter2.account_number + " (" + accountType + "), ");
                    }
                }

                Console.WriteLine("");
            }
            Console.WriteLine("");
            Console.WriteLine("Press Key to continue");
            Console.ReadLine();
        }
        
        // Method for showing Account List
        private static void GUIShowAccount()
        {
            Console.WriteLine(@"
                           Account List:
            ");
            
            foreach (accounts iter in context.accounts)
            {
                Console.Write("Account " + iter.account_number + " (");
                Console.Write("Name " + iter.account_name + ", ");
                Console.Write("Saldo " + iter.account_saldo + ", ");
                Console.Write("Limit " + iter.credit_limit + "), ");
                Console.WriteLine("");
            }

            Console.WriteLine("");

            Console.WriteLine("Press Key to continue");
            Console.ReadLine();
        }

        // Method for asking customer number and saldo
        // Parameters
        // out: customer number given by user, saldo given by user
        private static void GUIAskCustomerNumberAndSaldo(out int customerNumber, out decimal currentSaldo)
        {
            // Show customers
            GUIShowCustomer();

            // Choose customer
            GUIAskCustomerOrAccountNumber("AccountCreation", out customerNumber);
            
            // Set saldo
            GUIAskSaldo("AccountSaldo", out currentSaldo);

        }

        // Method for asking customer number or account number
        // Parameters
        // in: what kind of question would be asked
        // out: user's input for number (customer or account number)
        private static void GUIAskCustomerOrAccountNumber(string question, out int outNumber)
        {
            bool validResponse = false;
            string questionClause = "Something went wrong, give number 1000";

            switch (question)
            {
                case "AccountCreation":
                    questionClause = @"Who gets new account (customer number)?";
                    break;
                case "AccountNewOwner":
                    questionClause = @"Select New owner (customer number)?";
                    break;
                case "AccountToBeTransferred":
                    questionClause = @"Select Account number to be transferred another owner?";
                    break;
                case "ChangeSaldo":
                    questionClause = @"Select Account (account number)?";
                    break;
                default:
                    break;
            }

            do
            {
                Console.WriteLine(questionClause);

                string guessInput = Console.ReadLine();
                // convert string to number
                validResponse = int.TryParse(guessInput, out outNumber);

            } while (!validResponse);
        }

        // Method for asking saldo
        // Parameters 
        // in: what kind of question would be asked
        // out: user's input for saldo
        private static void GUIAskSaldo(string question, out decimal saldo)
        {
            bool validResponse = false;
            string questionClause = "Something went wrong, give number 1000";

            switch (question)
            {
                case "AccountSaldo":
                    questionClause = @"Set saldo";
                    break;
                case "ChangeSaldo":
                    questionClause = @"Initial change in saldo (negative for withdrawal)?";
                    break;
                case "CreditLimitSaldo":
                    questionClause = @"Set credit limit?";
                    break;
                default:
                    break;
            }

            do
            {
                Console.WriteLine(questionClause);

                string guessInput = Console.ReadLine();
                // Convert string to number                
                validResponse = decimal.TryParse(guessInput, out saldo);

            } while (!validResponse);

        }

        // Method for deleting customer
        // When customer has accounts, deleting is not allowed.
        private static void GUIDeleteCustomer()
        {
            GUIShowCustomer();

            bool validResponse = false;
            int response;

            do
            {
                Console.WriteLine(@"
                           Select Customernumber to be deleted
                        ");

                string guessInput = Console.ReadLine();
                // Convert string to number
                validResponse = int.TryParse(guessInput, out response);
            } while (!validResponse);

            if (confirmInput())
            {
                var dummy = context.customers.FirstOrDefault<customers>
                    (x => x.customer_number.Equals(response));

                var dummy2 = context.accounts.FirstOrDefault<accounts>
                    (x => x.customer_number.Equals(response));

                if (dummy2 is null)
                {
                    context.customers.Remove(dummy);
                    context.SaveChanges();
                }
                else
                {
                    Console.WriteLine("Cannot remove customer with accounts!");
                }              
            }
        }
        
        // Method for deleting account
        private static void GUIDeleteAccount()
        {
            GUIShowAccount();

            bool validResponse = false;
            int response;

            do
            {
                Console.WriteLine(@"
                           Select Account number to be deleted
                        ");

                string guessInput = Console.ReadLine();
                // Convert string to number
                validResponse = int.TryParse(guessInput, out response);
            } while (!validResponse);
            
            if (confirmInput())
            {
                var dummy = context.accounts.FirstOrDefault<accounts>
                    (x => x.account_number.Equals(response));

                context.accounts.Remove(dummy);
                context.SaveChanges();
            }
        }

        // Method for changing saldo
        // When account is limit account and there is not enough limit,
        // changing is not allowed.
        private static void GUIChangeSaldo()
        {
            GUIShowAccount();

            int response = default;
            decimal response2 = default;

            GUIAskCustomerOrAccountNumber("ChangeSaldo", out response);
        
            GUIAskSaldo("ChangeSaldo", out response2);

            if (confirmInput())
            {
                accounts dummy = context.accounts.FirstOrDefault<accounts>
                    (x => x.account_number.Equals(response));

                if (dummy.account_type.Equals(BankDefs.CreditAccount))
                {
                    decimal? chandedSaldo = dummy.account_saldo + response2;

                    if (chandedSaldo >= 0)
                    {
                        dummy.account_saldo += response2;

                        context.SaveChanges();

                        Console.WriteLine($"Account { response} saldo changed ...");
                    }
                    else if (chandedSaldo >= -dummy.credit_limit)
                    {
                        dummy.account_saldo += response2;

                        context.SaveChanges();

                        Console.WriteLine($"Account { response} saldo using credit now ...");
                    }
                    else
                    {
                        decimal? limitLeft = dummy.account_saldo + dummy.credit_limit;

                        Console.WriteLine($"Account { response} dont have enough limit, your limit is {limitLeft}");
                    }                    
                }
                else
                {
                    dummy.account_saldo += response2;

                    context.SaveChanges();

                    Console.WriteLine($"Account { response} saldo changed ...");
                }
            }
        }

        // Method for asking input confirm
        private static bool confirmInput()
        {
            bool response = default;

            Console.WriteLine("Confirm Y/N?");
            string confirmInput = Console.ReadLine();

            if (confirmInput.ToUpper() == "Y")
            {
                return true;
            }

            return response;
        }
    }
}
