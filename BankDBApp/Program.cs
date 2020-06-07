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

    // SKI Changed 27.5.2020
    public static class BankDefs
    {
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
            Console.WriteLine("BANK");
            Console.WriteLine("====");
            bool leaveBank = default;
            do
            {
                switch (GUIMainDisplay())
                {
                    case 0:
                        leaveBank = true;
                        Console.WriteLine("Leaving Bank...");
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
                    // SKI Changed 27.5.2020
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

        private static int GUIMainDisplay()
        {
            bool validResponse = false;
            int response;

            do
            {
                // SKI Changed 27.5.2020
                Console.WriteLine(@"
                           Select Activity (0 to 10)
                           0) Lopetus
                           1) Uusi Asiakas
                           2) Uusi Pankkitili
                           3) Uusi Luottotili
                           4) Siirrä tili toiselle asiakkaalle
                           5) Näytä asiakkaat
                           6) Näytä tilit
                           7) Poista asiakas
                           8) Poista tili
                           9) Tee tilitapahtumia (nosto ja talletus)
                        ");

                string guessInput = Console.ReadLine();
                // convert string to number
                validResponse = int.TryParse(guessInput, out response);

            } while (!validResponse);

            return response;
        }
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

        // SKI Changed 27.5.2020
        private static void GUICreateBankAccount()
        {
            //bool validResponse = false;
            int customerNumber = default;
            decimal creditLimit = default;
            decimal currentSaldo = default;


            // SKI Changed 27.5.2020
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

        // SKI Changed 27.5.2020
        private static void GUICreateCreditAccount()
        {
            // SKI
            int customerNumber = default;
            decimal creditLimit = default;
            decimal currentSaldo = default;

            // SKI get customer and set saldo for new account
            GUIAskCustomerNumberAndSaldo(out customerNumber, out currentSaldo);

            // SKI set credit limit for new account
            GUIAskSaldo("CreditLimitSaldo", out creditLimit);
            
            // SKI create the credit account
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
        
        // SKI Changed 27.5.2020
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
                Console.Write(" Tilisi ovat: ");
                
                foreach (accounts iter2 in context.accounts)
                {
                    if (iter2.customer_number.Equals(iter.Account))
                    {
                        string accountType = iter2.account_type.ToString();

                        // SKI Check the type and set the right one to be shown
                        if (accountType.Equals(BankDefs.BankAccount.ToString()))
                        {
                            accountType = "Pankki";
                        }
                        else
                        {
                            accountType = "Luotto";
                        }

                        Console.Write(iter2.account_number + " (" + accountType + "), ");
                    }
                }

                Console.WriteLine("");
            }
            Console.WriteLine(""); // SKI
            Console.WriteLine("Press Key to continue");
            Console.ReadLine();
        }
        private static void GUIShowAccount()
        {
            Console.WriteLine(@"
                           Account List:
            ");
            
            foreach (accounts iter in context.accounts)
            {
                Console.Write("Tili " + iter.account_number + " (");
                Console.Write("Nimi " + iter.account_name + ", ");
                Console.Write("Saldo " + iter.account_saldo + ", ");
                Console.Write("Limiitti " + iter.credit_limit + "), "); // SKI
                Console.WriteLine(""); // SKI
            }

            Console.WriteLine("");

            Console.WriteLine("Press Key to continue");
            Console.ReadLine();
        }

        // SKI Changed 27.5.2020
        // Method for asking customer number and saldo
        private static void GUIAskCustomerNumberAndSaldo(out int customerNumber, out decimal currentSaldo)
        {
            // Show customers
            GUIShowCustomer();

            // Choose customer
            GUIAskCustomerOrAccountNumber("AccountCreation", out customerNumber);
            
            // Set saldo
            GUIAskSaldo("AccountSaldo", out currentSaldo);

        }

        // SKI Changed 27.5.2020
        // Method for asking customer number or account number
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

        // SKI Changed 27.5.2020
        // Method for asking saldo
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
                // convert string to number                
                validResponse = decimal.TryParse(guessInput, out saldo);

            } while (!validResponse);

        }

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
                // convert string to number
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
                // convert string to number
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
        private static void GUIChangeSaldo()
        {
            GUIShowAccount();

            int response = default;
            decimal response2 = default;

            // SKI Changed 27.5.2020
            GUIAskCustomerOrAccountNumber("ChangeSaldo", out response);

            // SKI Changed 27.5.2020         
            GUIAskSaldo("ChangeSaldo", out response2);

            if (confirmInput())
            {
                accounts dummy = context.accounts.FirstOrDefault<accounts>
                    (x => x.account_number.Equals(response));

                // SKI Changed 27.5.2020
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
