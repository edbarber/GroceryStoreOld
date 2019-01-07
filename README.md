# GroceryStore

To use this project you need to have the database created by executing CreateGroceryStoreDb. Make sure the connection string matches the database you have created locally in appsettings.json.

The models in the project are generated using the scaffolding command referenced here 
https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/powershell

Make sure to delete OnConfiguring inside the dbcontext file as it's not necessary whenever your run the command.
