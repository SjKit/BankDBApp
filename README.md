# BankDBApp
Bank Application using Dababase

This is a basic bank application where you can

1) create a new customer
2) create a new bank account
3) create a new credit account
4) move account to another customer
5) have a list of customers
6) have a list of accounts
7) delete customer
8) delete account
9) make account activity (like deposit and withdraw)

When you create customers and accounts, the data will be stored into database. If you make account activities or deleting, they will be updated also into database.

Exit the application by value 0.

DATABASE PART - DO THIS FIRST

You can copy below code and run it e.g. in Microsoft SQL Server Management Studio.

******************************

/* Create schema and tables into database */
  
/* Schema */

CREATE SCHEMA pankki;

go

BEGIN TRANSACTION;

/* Customer table */

CREATE TABLE pankki.customers(
customer_number INT IDENTITY (1, 1) PRIMARY KEY,
customer_first_name VARCHAR (255) NOT NULL,
customer_last_name  VARCHAR (255) NOT NULL,
account_number INT NOT NULL
);

/* Accounts table */

CREATE TABLE pankki.accounts(
account_number INT IDENTITY (1, 1) PRIMARY KEY,
account_name VARCHAR (255) NOT NULL,
account_type  INT NOT NULL,
credit_limit DECIMAL (10, 2),
account_saldo DECIMAL (10,2),
customer_number INT NOT NULL FOREIGN KEY REFERENCES pankki.customers(customer_number) ON DELETE CASCADE ON UPDATE CASCADE
);

COMMIT;

******************************

After running the code check that schema and table creations were succeeded. If succeeded, you can run the application in Visual Studio by opening BankDBApp.sln.

When database is no longer needed (you don't want to run the application any more), you can drop the tables and the schema by copying below code and running it in Microsoft SQL Server Management Studio.

******************************

/* Drop tables when needed in this order*/

drop table pankki.customers;

drop table pankki.accounts;

drop schema pankki;
